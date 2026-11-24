# audio.cpp Voice Isolator — Android ARM64 / Cortex-A53

This package is for Android arm64-v8a (64-bit Android), API 24 / Android 7 or newer. It is NOT an ordinary glibc ARM Linux executable. Use a current Termux installation. The CPU instruction baseline is ARMv8-A with Cortex-A53 tuning: no ARMv8.2 FP16 arithmetic, dot-product or newer-CPU requirement was deliberately added.

## Contents
- `bin/audiocpp_cli`: ready-to-run command-line program.
- `lib/libaudiocpp.so`: C ABI shared library for embedding (header in `include/`).
- `run.sh`: local launcher setting the library path.
- ELF dependency/export reports, build metadata, source-build.patch and license notices.

Only the Voice Isolator model family is compiled in. Other audio.cpp model families are intentionally unavailable. Model weights are NOT bundled. No pip, compiler, CMake, Python, OpenBLAS or separate C++ runtime installation should be necessary just to execute the CLI. WAV input avoids a dependency on external audio converters.

## Install in Termux's HOME, not shared storage

Run `termux-setup-storage` if Downloads is not accessible. Extract the archive downloaded from this chat or the private GitHub Actions artifact:

```sh
cd ~
tar -xJf ~/storage/downloads/audio-cpp-android-arm64-cpu.tar.xz
cd audio-cpp-android-arm64-cpu
chmod +x run.sh bin/audiocpp_cli
./run.sh --version
./run.sh --list-loaders
```

A GitHub Actions download is a ZIP wrapper: extract the ZIP first, then extract its `.tar.xz` file. Do not try to execute binaries directly inside Downloads or a ZIP.

Check `getprop ro.product.cpu.abilist`: it must include `arm64-v8a`. A 64-bit chip with a 32-bit Android installation is not sufficient. If the loader reports a missing symbol, save the exact error and your Android version; no on-phone execution test is claimed by the cloud build's ELF checks.

## Download one model — separately

Etherll's files are full F32 weights, not Q8/Q4. The offline file is around 256 MB uncompressed; budget that download separately. Higher download count is not evidence of higher model quality. These files use audio.cpp's packaging; do not substitute a CrispASR GGUF just because its filename contains Voice Isolator.

```sh
mkdir -p models
curl -fL --retry 3 \
  https://huggingface.co/Etherll/voice-isolator-GGUF/resolve/main/voice-isolator-offline-f32.gguf \
  -o models/voice-isolator-offline-f32.gguf
```

## Run on a short WAV first

Use a mono, 48 kHz WAV named `test.wav`. If ffmpeg is already installed, you can prepare one:

```sh
ffmpeg -ss 30 -t 8 -i ~/storage/downloads/original.mp3 -ac 1 -ar 48000 test.wav
```

Then:

```sh
./run.sh --task s2s --family voice_isolator \
  --model models/voice-isolator-offline-f32.gguf \
  --backend cpu --threads 4 --audio test.wav --out isolated.wav --log
```

After a successful test, replace `test.wav` with your full input. Compare 4, 6 and 8 threads rather than assuming 8 is always faster. Heat and memory bandwidth may dominate on these older phones. This is an optimized build, NOT a measured phone benchmark or a realtime-speed promise. `-O3`, LTO, dead-section removal and Cortex-A53 tuning are enabled; aggressive global fast-math is intentionally avoided to protect inference numerics.

## Optional experimental Vulkan build

The separate `...-vulkan.tar.xz` package contains its own CLI/library. Keep it in a different folder. CPU-only users do not need to download it.

```sh
./run.sh --list-devices
./run.sh --task s2s --family voice_isolator \
  --model /absolute/path/to/voice-isolator-offline-f32.gguf \
  --backend vulkan --threads 4 --audio /absolute/path/to/test.wav \
  --out isolated-vulkan.wav --log
```

The GPU package requires Android's `libvulkan.so` and a compatible GPU/driver feature set. A Vulkan-1.1-only stack is not made compatible by this build; the backend requires Vulkan 1.2. Neither Adreno 506 nor PowerVR GE8320 has been hardware-tested here. Use the CPU package if the GPU version fails even at startup. No proprietary GPU driver is included, replaced or installed. OpenCL is not enabled in this package: audio.cpp does not expose a qualified OpenCL Voice Isolator path in this build recipe.

## Embedding

Use `include/audiocpp.h` with `lib/libaudiocpp.so` (soname `libaudiocpp.so.0`). The library is a C ABI facade, not a Python module or Android APK. Your app must package it for arm64-v8a and supply its own model path and audio I/O. See upstream `docs/c_api.md` for API usage.

## Provenance / limitations

Source: https://github.com/0xShug0/audio.cpp
Pinned source commit: a2423eb37617bce38b618e5a2ea2bb124f72c7ee
Model: https://huggingface.co/Etherll/voice-isolator-GGUF
Build toolchain: Android NDK r27c, Clang, Android API 24; static C++ runtime, OpenMP.

Cloud compilation and ELF architecture/dependency checks are not a substitute for running the binary on your actual phone. Check the accompanying status/report for which validations actually completed. Included SHA256 files detect accidental corruption, not independent authenticity.
