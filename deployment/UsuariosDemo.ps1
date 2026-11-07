param(
    [ValidateSet('CrearClaves','MostrarClaves','Provisionar')]
    [string]$Action,
    [string]$Database = 'SupermercadosGuate',
    [string]$Server = '',
    [string]$DllPath = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$privateDir = Join-Path $PSScriptRoot 'private'
$vault = Join-Path $privateDir 'usuarios-demo.dpapi.json'

function New-Password {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+','-').Replace('/','_')
}

function Unprotect-Password([string]$cipher) {
    $secure = ConvertTo-SecureString $cipher
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr); $secure.Dispose() }
}

if ($Action -eq 'CrearClaves') {
    if (Test-Path -LiteralPath $vault) { throw 'Las claves ya existen; no se sobrescriben.' }
    New-Item -ItemType Directory -Path $privateDir -Force | Out-Null
    $data = [ordered]@{}
    foreach ($role in @('Bodega','Caja')) {
        $plain = New-Password
        $secure = ConvertTo-SecureString $plain -AsPlainText -Force
        $data[$role] = ConvertFrom-SecureString $secure
        $secure.Dispose()
        $plain = $null
    }
    $data | ConvertTo-Json -Compress | Set-Content -LiteralPath $vault -Encoding UTF8 -NoNewline
    Write-Output 'Claves aleatorias cifradas localmente con DPAPI. El archivo privado está excluido de Git.'
    return
}

if (!(Test-Path -LiteralPath $vault)) { throw 'Primero crea las claves con -Action CrearClaves.' }
$data = Get-Content -LiteralPath $vault -Raw | ConvertFrom-Json
if ($Action -eq 'MostrarClaves') {
    Write-Output ('BodegaDemo: ' + (Unprotect-Password $data.Bodega))
    Write-Output ('CajaDemo: ' + (Unprotect-Password $data.Caja))
    return
}

if ($Database -ne 'SupermercadosGuate' -and !$Database.StartsWith('SupermercadosGuate_ROLES_PRUEBAS_')) {
    throw 'Destino de base de datos no permitido.'
}
if (!$DllPath) { $DllPath = Join-Path $root 'src/SupermercadoMVC/bin/RolesCheck/SupermercadoMVC.dll' }
if (!(Test-Path -LiteralPath $DllPath)) { throw 'Falta la compilación aislada RolesCheck.' }

[xml]$project = Get-Content -LiteralPath (Join-Path $root 'src/SupermercadoMVC/SupermercadoMVC.csproj')
$secretId = [string]$project.Project.PropertyGroup.UserSecretsId
$secretPath = Join-Path $env:APPDATA "Microsoft\UserSecrets\$secretId\secrets.json"
$secrets = Get-Content -LiteralPath $secretPath -Raw | ConvertFrom-Json
$builder = [System.Data.SqlClient.SqlConnectionStringBuilder]::new([string]$secrets.'ConnectionStrings:Supermercado')
$builder['Initial Catalog'] = $Database
if ($Server) {
    if ($Server -ne '127.0.0.1,1436') { throw 'Solo se admite el puerto local 1436 para la copia aislada.' }
    $builder['Data Source'] = $Server
}
$oldConnection = $env:ConnectionStrings__Supermercado
try {
    $env:ConnectionStrings__Supermercado = $builder.ConnectionString
    $payload = @{ Bodega = (Unprotect-Password $data.Bodega); Caja = (Unprotect-Password $data.Caja) } | ConvertTo-Json -Compress
    $payload | & dotnet $DllPath --provisionar-demo "--esperar-base=$Database"
    if ($LASTEXITCODE -ne 0) { throw 'Falló el aprovisionamiento; no se mostrarán las claves.' }
}
finally {
    $env:ConnectionStrings__Supermercado = $oldConnection
    $payload = $null
    $builder = $null
    $secrets = $null
}
