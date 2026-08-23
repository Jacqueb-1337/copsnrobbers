$ErrorActionPreference = 'Stop'
$log = Join-Path $PSScriptRoot 'host-repro.log'
$done = Join-Path $PSScriptRoot 'host-repro.done'
Remove-Item $log,$done -Force -ErrorAction SilentlyContinue
try {
  $ninja = (Get-Command ninja.exe -ErrorAction Stop).Source
  $clang = (Get-Command clang.exe -ErrorAction Stop).Source
  $clangxx = (Get-Command clang++.exe -ErrorAction Stop).Source
  "NINJA=$ninja`nCLANG=$clang`nCLANGXX=$clangxx" | Out-File $log -Encoding utf8
  Remove-Item (Join-Path $PSScriptRoot 'build-host') -Recurse -Force -ErrorAction SilentlyContinue
  & cmake -S $PSScriptRoot -B (Join-Path $PSScriptRoot 'build-host') -G Ninja `
    "-DCMAKE_MAKE_PROGRAM=$ninja" `
    "-DCMAKE_C_COMPILER=$clang" `
    "-DCMAKE_CXX_COMPILER=$clangxx" `
    -DCMAKE_BUILD_TYPE=Release `
    "-DBoost_INCLUDE_DIR=$PSScriptRoot\third_party\boost_1_85_0" `
    -DDYNARMIC_USE_BUNDLED_EXTERNALS=ON `
    -DDYNARMIC_TESTS=OFF *>> $log
  if ($LASTEXITCODE -ne 0) { throw "cmake configure failed: $LASTEXITCODE" }
  & cmake --build (Join-Path $PSScriptRoot 'build-host') --target cnr64-a32-poc -j 8 *>> $log
  if ($LASTEXITCODE -ne 0) { throw "host build failed: $LASTEXITCODE" }
  & (Join-Path $PSScriptRoot 'build-host\cnr64-a32-poc.exe') `
    (Join-Path $PSScriptRoot '..\APK_Build_Active\apk_source\lib\armeabi-v7a\libmain.so') `
    (Join-Path $PSScriptRoot '..\APK_Build_Active\apk_source\lib\armeabi-v7a\libunity.so') *>> $log
  "EXIT=$LASTEXITCODE" | Out-File $log -Append -Encoding utf8
} catch {
  "ERROR=$($_.Exception.Message)" | Out-File $log -Append -Encoding utf8
} finally {
  Set-Content $done 'done'
}
