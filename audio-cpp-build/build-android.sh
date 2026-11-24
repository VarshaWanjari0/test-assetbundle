#!/usr/bin/env bash
set -euo pipefail
variant=${1:-cpu}
SOURCE_SHA=a2423eb37617bce38b618e5a2ea2bb124f72c7ee
work="$PWD/work-$variant"
mkdir -p "$work"
cd "$work"
git init -q src
git -C src remote add origin https://github.com/0xShug0/audio.cpp.git
git -C src fetch --depth 1 origin "$SOURCE_SHA"
git -C src checkout --detach FETCH_HEAD
# No server/frontend checkout is needed for the CLI + C API target.
ndk="$ANDROID_SDK_ROOT/ndk/27.2.12479018"
if [[ ! -f "$ndk/build/cmake/android.toolchain.cmake" ]]; then
  sdkmanager --install 'ndk;27.2.12479018' >/dev/null
fi
export ANDROID_NDK_HOME="$ndk"
tc="$ndk/toolchains/llvm/prebuilt/linux-x86_64"
# Only compile the Voice Isolator model-spec fallback, not the entire catalog.
python3 - <<'PY'
from pathlib import Path
p=Path('src/CMakeLists.txt');s=p.read_text()
a='file(GLOB AUDIOCPP_MODEL_SPEC_FILES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/model_specs/*.json")'
b='set(AUDIOCPP_MODEL_SPEC_FILES "${CMAKE_CURRENT_SOURCE_DIR}/model_specs/voice_isolator.json")'
assert a in s;s=s.replace(a,b);p.write_text(s)
# posix_spawnp is Android API 28+. Keep API 24 support without adding a shim library.
p=Path('src/app/workflow/workflow.cpp');s=p.read_text()
s=s.replace('#include <spawn.h>', '#include <spawn.h>\n#include <unistd.h>')
a='    const int spawn_rc = posix_spawnp(&pid, program.c_str(), nullptr, nullptr, argv.data(), environ);'
b='''#if defined(__ANDROID__) && __ANDROID_API__ < 28
    pid = fork();
    const int spawn_rc = pid < 0 ? errno : 0;
    if (pid == 0) {
        execvp(program.c_str(), argv.data());
        _exit(127);
    }
#else
'''+a+'''
#endif'''
assert a in s;s=s.replace(a,b);p.write_text(s)
PY
args=(
  -S src -B build -G Ninja
  -DCMAKE_TOOLCHAIN_FILE="$ndk/build/cmake/android.toolchain.cmake"
  -DANDROID_ABI=arm64-v8a -DANDROID_PLATFORM=android-24 -DANDROID_STL=c++_static
  -DCMAKE_BUILD_TYPE=Release
  '-DCMAKE_C_FLAGS_RELEASE=-O3 -DNDEBUG -mcpu=cortex-a53 -ffunction-sections -fdata-sections -fvisibility=hidden'
  '-DCMAKE_CXX_FLAGS_RELEASE=-O3 -DNDEBUG -mcpu=cortex-a53 -ffunction-sections -fdata-sections -fvisibility=hidden -fvisibility-inlines-hidden'
  '-DCMAKE_EXE_LINKER_FLAGS=-Wl,--gc-sections,--icf=safe,-z,max-page-size=16384'
  '-DCMAKE_SHARED_LINKER_FLAGS=-Wl,--gc-sections,--icf=safe,-z,max-page-size=16384'
  -DCMAKE_INTERPROCEDURAL_OPTIMIZATION=ON
  -DAUDIOCPP_VERSION=android-a53-a2423eb
  -DAUDIOCPP_MODEL_SET=custom -DAUDIOCPP_MODELS=voice_isolator
  -DAUDIOCPP_DEPLOYMENT_BUILD=ON -DAUDIOCPP_BUILD_C_API=ON
  -DAUDIOCPP_BUILD_NATIVE_MODEL_MANAGER=OFF
  -DENGINE_BUILD_TESTS=OFF -DENGINE_BUILD_EXAMPLES=OFF
  -DENGINE_ENABLE_NATIVE_CPU=OFF -DGGML_CPU_ARM_ARCH=armv8-a
  -DENGINE_ENABLE_OPENMP=ON -DGGML_OPENMP=ON
  -DENGINE_ENABLE_CUDA=OFF -DENGINE_ENABLE_HIP=OFF -DENGINE_ENABLE_METAL=OFF
  -DENGINE_ENABLE_WEBGPU=OFF -DENGINE_ENABLE_SYCL=OFF
  -DENGINE_ENABLE_VULKAN=OFF -DGGML_OPENCL=OFF
  -DGGML_CPU_KLEIDIAI=OFF
)
if [[ "$variant" == vulkan ]]; then
  # NDK ships C Vulkan headers, but ggml also needs the Vulkan-Hpp C++ headers.
  # Copy only graphics headers: never add host /usr/include to an Android build.
  mkdir -p android-vulkan-headers
  cp -a /usr/include/vulkan android-vulkan-headers/
  if [[ -d /usr/include/vk_video ]]; then cp -a /usr/include/vk_video android-vulkan-headers/; fi
  args+=( -DENGINE_ENABLE_VULKAN=ON
    -DVulkan_INCLUDE_DIR="$PWD/android-vulkan-headers"
    -DVulkan_LIBRARY="$tc/sysroot/usr/lib/aarch64-linux-android/24/libvulkan.so"
    -DVulkan_GLSLC_EXECUTABLE=/usr/bin/glslc )
fi
cmake "${args[@]}"
cmake --build build --target audiocpp_cli audiocpp --parallel 2
out="audio-cpp-android-arm64-$variant"
mkdir -p "$out/bin" "$out/lib" "$out/include" "$out/licenses"
cp build/bin/audiocpp_cli "$out/bin/"
cp -L build/bin/libaudiocpp.so "$out/lib/libaudiocpp.so"
# The soname is used by applications linking against the C ABI.
ln -sf libaudiocpp.so "$out/lib/libaudiocpp.so.0"
cp src/include/audiocpp.h "$out/include/"
cp src/LICENSE "$out/licenses/audio.cpp-LICENSE"
cp src/external/ggml/LICENSE "$out/licenses/ggml-LICENSE"
for path in src/external/*/LICENSE* src/external/*/COPYING*; do
  [[ -f "$path" ]] || continue
  parent=$(basename "$(dirname "$path")")
  cp "$path" "$out/licenses/${parent}-$(basename "$path")"
done
# Compiler runtime license notices for the statically linked C++ and OpenMP code.
for component in libcxx libcxxabi openmp; do
  curl -fLsS "https://raw.githubusercontent.com/llvm/llvm-project/release/18.x/$component/LICENSE.TXT" -o "$out/licenses/LLVM-$component-LICENSE.txt"
done
"$tc/bin/llvm-strip" --strip-unneeded "$out/bin/audiocpp_cli" "$out/lib/libaudiocpp.so"
# Static libc++/OpenMP are intended. Fail instead of shipping missing runtime dependencies.
"$tc/bin/llvm-readelf" -h -l -d "$out/bin/audiocpp_cli" > "$out/ELF-CLI.txt"
"$tc/bin/llvm-readelf" -h -d "$out/lib/libaudiocpp.so" > "$out/ELF-LIBRARY.txt"
grep -q AArch64 "$out/ELF-CLI.txt"
grep -q /system/bin/linker64 "$out/ELF-CLI.txt"
if grep -E 'NEEDED.*(libc\+\+_shared|libomp)' "$out/ELF-CLI.txt" "$out/ELF-LIBRARY.txt"; then
  echo 'Unexpected shared compiler runtime dependency' >&2;exit 1
fi
"$tc/bin/llvm-nm" -D "$out/lib/libaudiocpp.so" | grep ' T audiocpp_' > "$out/EXPORTED-C-API.txt"
[[ $(wc -l < "$out/EXPORTED-C-API.txt") -gt 10 ]]
cp "$GITHUB_WORKSPACE/audio-cpp-build/README-PHONE.md" "$out/README.md"
cat > "$out/run.sh" <<'SH'
#!/system/bin/sh
HERE=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
export LD_LIBRARY_PATH="$HERE/lib${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
exec "$HERE/bin/audiocpp_cli" "$@"
SH
chmod +x "$out/run.sh"
printf 'source=%s\nvariant=%s\nABI=arm64-v8a\nAPI=24\nCPU=Cortex-A53 / ARMv8-A NEON\nLTO=ON\nOpenMP=ON\nCXX-runtime=static\n' "$SOURCE_SHA" "$variant" > "$out/BUILD-INFO.txt"
git -C src diff > "$out/source-build.patch"
mkdir -p "$GITHUB_WORKSPACE/dist"
# xz is for download size only, not executable packing or runtime decompression.
tar -cJf "$GITHUB_WORKSPACE/dist/$out.tar.xz" "$out"
(cd "$GITHUB_WORKSPACE/dist" && sha256sum "$out.tar.xz" > "$out.tar.xz.sha256")
ls -lh "$GITHUB_WORKSPACE/dist/"
