####################################################
# Instalador Giga Telemetry Agent
# Autor: bruno-bueno
# Descricao: Baixa o binario direto do GitHub e instala na pasta Inicializar do usuario
####################################################

# 1. Configuracao de Seguranca para GitHub (TLS 1.2)
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# 2. Definir Caminhos
$GithubExeUrl = "https://github.com/bruno-bueno/Giga_telemetry/blob/master/Giga_telemetry.exe?raw=true"
$StartupFolder = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup"
$TargetExePath = "$StartupFolder\Giga_telemetry.exe"
$TempExePath = "$env:TEMP\Giga_telemetry_new.exe"

Write-Host "Iniciando instalacao do Giga Telemetry..." -ForegroundColor Cyan

# 3. Parar processo se estiver rodando
$process = Get-Process -Name "Giga_telemetry" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "Parando versao antiga..." -ForegroundColor Yellow
    Stop-Process -Name "Giga_telemetry" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
}

# 4. Baixar o executavel atualizado
try {
    Write-Host "Baixando versao mais recente de: $GithubExeUrl"
    Invoke-WebRequest -Uri $GithubExeUrl -OutFile $TargetExePath
    Write-Host "Download concluido com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "ERRO AO BAIXAR: $_" -ForegroundColor Red
    exit 1
}

# 5. Executar o Agente instalado
Write-Host "Iniciando o Agente..." -ForegroundColor Cyan
try {
    Start-Process -FilePath $TargetExePath
    Write-Host "Instalacao concluida! O Agente iniciara automaticamente com o Windows." -ForegroundColor Green
}
catch {
    Write-Host "Erro ao iniciar o agente: $_" -ForegroundColor Red
}
