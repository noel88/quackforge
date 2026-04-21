# QuackForge — Mac 빌드 출력과 VM BepInEx/plugins/QuackForge 심볼릭 링크 연결.
#
# Parallels "Share Mac folders with Windows" 활성화 시 Mac 홈 디렉토리는
# \\Mac\Home\ 으로 VM 안에서 접근 가능. 이 스크립트는:
#
#   1. Mac 빌드 출력 경로 (default:
#        \\Mac\Home\Desktop\Project\side-project\quackforge\src\QuackForge.Loader\bin\Release)
#      가 VM 에서 접근 가능한지 확인
#   2. Duckov 의 BepInEx\plugins\QuackForge 를 이 경로에 심볼릭 링크
#   3. Mac 측에서 재빌드만 하면 VM 측 덕코프 재실행만으로 반영
#
# 전제:
#   - 관리자 PowerShell (mklink 또는 New-Item -ItemType SymbolicLink 권한 필요)
#   - Developer Mode 활성화 시 일반 권한으로도 가능
#   - UNC 경로 심링크가 불안정하면 -MapDrive 로 Z: 드라이브 매핑 후 재시도
#
# 실행 예:
#   .\link-plugin-vm.ps1
#   .\link-plugin-vm.ps1 -MapDrive Z

[CmdletBinding()]
param(
    [string]$DuckovDir = "",
    [string]$MacBuildUNC = "\\Mac\Home\Desktop\Project\side-project\quackforge\src\QuackForge.Loader\bin\Release",
    [string]$MapDrive = "",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

function Write-Info($msg) { Write-Host "[link-plugin] $msg" -ForegroundColor Cyan }
function Write-Err($msg)  { Write-Host "[link-plugin] $msg" -ForegroundColor Red }

function Resolve-DuckovDir {
    param([string]$Explicit)
    if ($Explicit -and (Test-Path $Explicit)) { return (Resolve-Path $Explicit).Path }
    $candidates = @(
        "$env:ProgramFiles (x86)\Steam\steamapps\common\Escape from Duckov",
        "$env:ProgramFiles\Steam\steamapps\common\Escape from Duckov"
    )
    foreach ($p in $candidates) { if (Test-Path (Join-Path $p "Duckov.exe")) { return (Resolve-Path $p).Path } }
    throw "Duckov.exe not found. Pass -DuckovDir '<path>' explicitly."
}

$duckov = Resolve-DuckovDir -Explicit $DuckovDir
$pluginDir = Join-Path $duckov "BepInEx\plugins\QuackForge"
Write-Info "plugin target: $pluginDir"

if ($MapDrive) {
    $drive = "${MapDrive}:"
    Write-Info "mapping $drive to $MacBuildUNC"
    net use $drive /delete 2>$null | Out-Null
    net use $drive $MacBuildUNC
    $source = $drive
} else {
    $source = $MacBuildUNC
}

if (-not (Test-Path $source)) {
    Write-Err "source not reachable: $source"
    Write-Err "Verify Parallels > Options > Sharing > Share Mac folders with Windows is ON,"
    Write-Err "and the Mac path exists: $source"
    exit 1
}

if ((Test-Path $pluginDir) -and -not $Force) {
    Write-Err "$pluginDir already exists. Re-run with -Force to replace."
    exit 1
}

if (Test-Path $pluginDir) { Remove-Item -Recurse -Force $pluginDir }
New-Item -ItemType Directory -Force -Path (Split-Path $pluginDir -Parent) | Out-Null

Write-Info "creating directory symlink: $pluginDir -> $source"
New-Item -ItemType SymbolicLink -Path $pluginDir -Target $source | Out-Null

Write-Info "done. Rebuild on Mac (dotnet build -c Release) and re-launch Duckov in the VM."
Write-Info "Expected: BepInEx\LogOutput.log shows '[Data.Weapons] weapon registry ready...'"
