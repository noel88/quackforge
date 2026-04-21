# QuackForge — Windows VM (Parallels) 용 BepInEx 설치 스크립트.
#
# VM 측 선제 조건:
#   1. Escape from Duckov Steam 설치 완료
#   2. PowerShell 5.1+ 또는 PowerShell 7+
#   3. 관리자 권한 불요 (사용자 공간 설치)
#
# 실행:
#   .\install-bepinex-vm.ps1
#
# Windows 11 ARM 에서 x64 덕코프를 x64 에뮬레이션으로 구동하는 것이 기본 가정.
# BepInEx_win_x64_5.4.23.5 가 x64 에뮬 내에서 doorstop 주입.

[CmdletBinding()]
param(
    [string]$DuckovDir = "",
    [string]$BepInExTag = "v5.4.23.5",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

function Write-Info($msg)  { Write-Host "[install-bepinex] $msg" -ForegroundColor Cyan }
function Write-WarnLine($msg) { Write-Host "[install-bepinex] $msg" -ForegroundColor Yellow }
function Write-Err($msg)   { Write-Host "[install-bepinex] $msg" -ForegroundColor Red }

function Resolve-DuckovDir {
    param([string]$Explicit)

    if ($Explicit -and (Test-Path $Explicit)) { return (Resolve-Path $Explicit).Path }

    $candidates = @(
        "$env:ProgramFiles (x86)\Steam\steamapps\common\Escape from Duckov",
        "$env:ProgramFiles\Steam\steamapps\common\Escape from Duckov"
    )
    # Steam library roots (libraryfolders.vdf 를 대충 파싱)
    $vdf = "$env:ProgramFiles (x86)\Steam\steamapps\libraryfolders.vdf"
    if (Test-Path $vdf) {
        $lines = Get-Content $vdf
        foreach ($line in $lines) {
            if ($line -match '"path"\s+"([^"]+)"') {
                $path = $Matches[1] -replace '\\\\', '\'
                $candidates += (Join-Path $path "steamapps\common\Escape from Duckov")
            }
        }
    }

    foreach ($p in $candidates) {
        if (Test-Path (Join-Path $p "Duckov.exe")) { return (Resolve-Path $p).Path }
    }
    throw "Duckov.exe not found. Pass -DuckovDir '<path>' explicitly."
}

$duckov = Resolve-DuckovDir -Explicit $DuckovDir
Write-Info "Duckov: $duckov"

$asset = "BepInEx_win_x64_5.4.23.5.zip"
$releaseUrl = "https://github.com/BepInEx/BepInEx/releases/download/$BepInExTag/$asset"

$existingMarker = Join-Path $duckov "BepInEx\core\BepInEx.dll"
if ((Test-Path $existingMarker) -and -not $Force) {
    Write-Info "BepInEx already present. Re-run with -Force to overwrite."
} else {
    $tmp = Join-Path $env:TEMP "quackforge-bepinex"
    New-Item -ItemType Directory -Force -Path $tmp | Out-Null
    $zip = Join-Path $tmp $asset

    Write-Info "downloading $asset"
    Invoke-WebRequest -Uri $releaseUrl -OutFile $zip -UseBasicParsing

    Write-Info "extracting into $duckov"
    Expand-Archive -Path $zip -DestinationPath $duckov -Force

    Remove-Item -Recurse -Force $tmp
}

# Windows 에서 standalone 실행 시 Steam DRM 우회 (macOS 와 동일 AppID).
$appidFile = Join-Path $duckov "steam_appid.txt"
if (-not (Test-Path $appidFile)) {
    Write-Info "writing steam_appid.txt (3167020)"
    Set-Content -Path $appidFile -Value "3167020" -NoNewline -Encoding ASCII
}

Write-Info "done. Launch Duckov once (via Steam or .\run-duckov-vm.ps1) to generate BepInEx/LogOutput.log"
Write-Info "Expected first line: 'BepInEx 5.4.23.5 - Duckov'"
