"""Create and validate an audio.cpp-compatible Q8_0 download from the original Inworld checkpoint."""
import os
os.environ['OMP_NUM_THREADS']='4'
import sys,json,hashlib,subprocess,time,collections
from pathlib import Path
import numpy as np,soundfile as sf
from scipy.signal import resample_poly
from gguf import GGUFReader,GGMLQuantizationType
root=Path.cwd();src=root/'source';b=root/'build-native/bin';dist=root/'dist-q8';dist.mkdir(exist_ok=True)
checkpoint=root/'original/denoiser.safetensors';f32=root/'original/voice-isolator-offline-f32.gguf';q8=dist/'voice-isolator-offline-q8_0.gguf'
def digest(p):
 with p.open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
assert digest(checkpoint)=='647335a6aeb4dc706b895532c209d51e0e84a4849d17b339f00d4392a050ae53'
subprocess.run([sys.executable,str(src/'tests/voice_isolator/convert_gguf.py'),'--source',str(checkpoint),'--output',str(f32),'--type','f32','--audiocpp-gguf',str(b/'audiocpp_gguf')],check=True)
# Same public source fixture for both models. No user's recording is uploaded.
x,sr=sf.read(src/'assets/resources/sample_16k.wav',dtype='float32')
if x.ndim>1:x=x.mean(1)
x=x[:4*sr];x=resample_poly(x,48000//np.gcd(sr,48000),sr//np.gcd(sr,48000)).astype('float32')
rng=np.random.default_rng(2026);x=x+0.015*rng.standard_normal(len(x)).astype('float32');sf.write(root/'test.wav',x,48000,subtype='FLOAT')
orig=GGUFReader(str(f32));names={t.name for t in orig.tensors};shape_map={t.name:list(map(int,t.shape)) for t in orig.tensors}
# Quantize aligned 2D .weight tensors with the upstream policy. Explicitly include
# the attention in_proj_weight tensors that don't end in '.weight'. Keep embeddings
# F32; vectors, state rates and non-aligned kernels stay F32 under upstream policy.
base=[str(b/'audiocpp_gguf'),'--input',str(f32),'--output',str(q8),'--type','q8_0','--family','voice_isolator','--overwrite','--keep-type','weights/band_embedding*=f32']
for name in sorted(names):
 if 'embed' in name.lower() or 'codebook' in name.lower():
  base+=['--keep-type',name+'=f32']
 if name.endswith('.self_attn.in_proj_weight'):
  base+=['--keep-type',name+'=q8_0']
metadata_keys=set(orig.fields)
def convert(conservative=False):
 cmd=base.copy()
 if conservative:cmd+=['--keep-type','weights/encoders.*=f32','--keep-type','weights/denoise_decoder.*=f32']
 print('Conversion profile:', 'conservative' if conservative else 'eligible-linear-Q8',flush=True)
 subprocess.run(cmd,check=True)
def render(model,output):
 cmd=[str(b/'audiocpp_cli'),'--task','s2s','--family','voice_isolator','--model',str(model),'--backend','cpu','--threads','4','--session-option','voice_isolator.weight_type=f32','--audio',str(root/'test.wav'),'--out',str(output)]
 start=time.perf_counter();subprocess.run(cmd,check=True);elapsed=time.perf_counter()-start;y,s=sf.read(output,dtype='float32');assert s==48000 and y.size and np.isfinite(y).all();return y,elapsed
convert();ref,tr=render(f32,root/'reference.wav');y,tq=render(q8,root/'quantized.wav')
def compare(ref,y):
 assert ref.shape==y.shape
 err=np.asarray(ref,dtype='float64')-np.asarray(y,dtype='float64');power=float(np.mean(ref.astype('float64')**2));assert power>1e-10
 return {'snr_vs_f32_output_db':float(10*np.log10(power/(np.mean(err**2)+1e-30))),'max_abs_sample_error':float(np.max(np.abs(err))),'waveform_correlation':float(np.corrcoef(ref.reshape(-1),y.reshape(-1))[0,1])}
quality=compare(ref,y);profile='eligible-linear-Q8'
if quality['snr_vs_f32_output_db']<30:
 convert(conservative=True);y,tq=render(q8,root/'quantized.wav');quality=compare(ref,y);profile='Q8-backbone-F32-band-heads'
assert quality['snr_vs_f32_output_db']>=25,quality
out=GGUFReader(str(q8));assert {t.name for t in out.tensors}==names
for t in out.tensors:assert list(map(int,t.shape))==shape_map[t.name]
assert metadata_keys.issubset(set(out.fields)), 'Conversion lost metadata keys'
counts=collections.Counter(GGMLQuantizationType(t.tensor_type).name for t in out.tensors);assert counts['Q8_0']>0
subprocess.run([str(b/'audiocpp_gguf'),'--inspect',str(q8)],stdout=(dist/'model-inspection.txt').open('w'),check=True)
report={'source_repository':'https://huggingface.co/inworld/voice-isolator','source_revision':'9b0285b3fe8403aa8e2f1dbb2165a80a4294c970','checkpoint_sha256':digest(checkpoint),'checkpoint_file':checkpoint.name,'source_file':f32.name,'source_sha256':digest(f32),'source_bytes':f32.stat().st_size,'output_file':q8.name,'output_sha256':digest(q8),'output_bytes':q8.stat().st_size,'size_reduction_percent':100*(1-q8.stat().st_size/f32.stat().st_size),'quantization_profile':profile,'tensor_types':dict(counts),'tensor_count':len(names),'source_tensor_names_shapes_and_metadata_keys_preserved':True,'test_audio':'first four seconds of pinned audio.cpp assets/resources/sample_16k.wav + deterministic noise, 48 kHz mono','test_output_samples':int(ref.size),'f32_render_seconds':tr,'q8_render_seconds':tq,'quality':quality,'runtime_source_commit':'a2423eb37617bce38b618e5a2ea2bb124f72c7ee','runtime_note':'The existing Voice Isolator loads projection weights into F32 on Cortex-A53. Q8 reduces disk/download size; it is not proof of native INT8 execution, lower runtime RAM or faster inference. Smoke test ran on x86 CPU, not Android.'}
(dist/'quantization-report.json').write_text(json.dumps(report,indent=2));(dist/(q8.name+'.sha256')).write_text(report['output_sha256']+'  '+q8.name+'\n')
(dist/'README.txt').write_text('Voice Isolator offline Q8_0 for audio.cpp\n\nUse this GGUF with the Android CPU/Vulkan package already supplied, replacing only --model. This is NOT a CrispASR GGUF.\n\nQuantization: '+profile+'. Mixed precision: not every tensor is Q8; vectors/state and incompatible kernels retain floating point.\n\nDownload size reduction is not an inference-speed or RAM-reduction claim. Existing phone runtime normally expands projection weights to F32.\n\nTensor names/shapes and original metadata keys were checked. Both source and Q8 ran on a short public test sample on x86 CPU. See JSON for numeric comparison; this is not a listening test, phone benchmark or proof of quality on your recording. No user audio was uploaded.\n\nWeights: Apache-2.0, Inworld AI. Converted directly from the original Inworld safetensors checkpoint with the pinned audio.cpp converter. Artifact retention: 30 days; download before expiry.\n')
print(json.dumps(report,indent=2),flush=True)
