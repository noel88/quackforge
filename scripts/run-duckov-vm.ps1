# QuackForge — Windows VM 에서 Duckov + BepInEx 실행.
#
# 기본은 Steam 경유 (덕코프를 Steam 라이브러리에서 직접 실행). BepInEx 의
# winhttp.dll 이 이미 설치되어 있다면 Steam 실행만으로 doorstop 이 주입됨.
# steam_appid.txt 는 Steam 없이 직접 실행도 허용 (디버그용).
#
# 실행:
#   .\run-duckov-vm.ps1            # Steam 경유 (권장)
#   .\run-duckov-vm.ps1 -Direct    # Duckov.exe 직접 실행
#
# 실행 후 BepInEx/LogOutput.log 를 tail.

[CmdletBinding()]
param(
    [string]$DuckovDir = "",
    [switch]$Direct,
    [int]$TailSeconds = 30
)

$ErrorActionPreference = "Stop"

function Write-Info($msg) { Write-Host "[run-duckov] $msg" -ForegroundColor Cyan }
function Write-Err($msg)  { Write-Host "[run-duckov] $msg" -ForegroundColor Red }

function Resolve-DuckovDir {
    param([string]$Explicit)
    if ($Explicit -and (Test-Path $Explicit)) { return (Resolve-Path $Explicit).Path }
    $candidates = @(
        "$env:ProgramFiles (x86)\Steam\steamapps\common\Escape from Duckov",
        "$env:ProgramFiles\Steam\steamapps\common\Escape from Duckov"
    )
    $vdf = "$env:ProgramFiles (x86)\Steam\steamapps\libraryfolders.vdf"
    if (Test-Path $vdf) {
        foreach ($line in Get-Content $vdf) {
            if ($line -match '"path"\s+"([^"]+)"') {
                $candidates += (Join-Path ($Matches[1] -replace '\\\\', '\') "steamapps\common\Escape from Duckov")
            }
        }
    }
    foreach ($p in $candidates) { if (Test-Path (Join-Path $p "Duckov.exe")) { return (Resolve-Path $p).Path } }
    throw "Duckov.exe not found. Pass -DuckovDir '<path>' explicitly."
}

$duckov = Resolve-DuckovDir -Explicit $DuckovDir
$bepinexCore = Join-Path $duckov "BepInEx\core\BepInEx.dll"
$log = Join-Path $duckov "BepInEx\LogOutput.log"

if (-not (Test-Path $bepinexCore)) {
    Write-Err "BepInEx not installed. Run install-bepinex-vm.ps1 first."
    exit 1
}

if (Test-Path $log) { Remove-Item $log -Force }

if ($Direct) {
    Write-Info "launching Duckov.exe directly"
    Start-Process -FilePath (Join-Path $duckov "Duckov.exe") -WorkingDirectory $duckov
} else {
    Write-Info "launching via Steam (steam://rungameid/3167020)"
    Start-Process "steam://rungameid/3167020"
}

Write-Info "waiting $TailSeconds s for log to appear..."
$deadline = (Get-Date).AddSeconds($TailSeconds)
while (-not (Test-Path $log) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 500 }

if (Test-Path $log) {
    Write-Info "--- $log (head) ---"
    Get-Content $log -TotalCount 20
} else {
    Write-Err "LogOutput.log not produced within ${TailSeconds}s."
    Write-Err "Check Duckov is installed and Steam is logged in (if not using -Direct)."
    exit 2
}
