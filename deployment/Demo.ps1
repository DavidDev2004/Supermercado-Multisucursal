param(
    [ValidateSet('Start', 'Stop', 'Status', 'ConfigureToken', 'PrepareBasic', 'ShowAccess')]
    [string]$Action = 'Start'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$private = Join-Path $PSScriptRoot 'private'
$appDir = Join-Path $root 'src\SupermercadoMVC'
$appDll = Join-Path $appDir 'bin\Release\net9.0\SupermercadoMVC.dll'
$tokenFile = Join-Path $private 'ngrok-token.xml'
$credentialFile = Join-Path $private 'basic-auth.xml'
$policyFile = Join-Path $private 'ngrok-policy-basic.yaml'
$appPidFile = Join-Path $private 'app.pid'
$tunnelName = 'supermercado-demo-ngrok'
$appPort = 5197
New-Item -ItemType Directory -Path $private -Force | Out-Null

function Get-LocalHttpStatus {
    try {
        $response = Invoke-WebRequest -Uri "http://127.0.0.1:$appPort/acceso" -TimeoutSec 5 -MaximumRedirection 0
        return [int]$response.StatusCode
    } catch {
        return 0
    }
}

function Get-TunnelUrl {
    $lines = @(docker logs --tail 150 $tunnelName 2>&1)
    $match = [regex]::Match(($lines -join "`n"), 'https://[a-zA-Z0-9.-]+\.ngrok(?:-free)?\.(?:app|dev)')
    if ($match.Success) { return $match.Value }
    return $null
}

switch ($Action) {
    'ConfigureToken' {
        $token = Read-Host 'Pega aquí el authtoken de ngrok (entrada oculta; nunca en el chat)' -AsSecureString
        if ($token.Length -eq 0) { throw 'El authtoken está vacío.' }
        $token | Export-Clixml -LiteralPath $tokenFile
        Write-Host 'Authtoken guardado cifrado para este usuario de Windows.'
        break
    }
    'PrepareBasic' {
        if (Test-Path -LiteralPath $credentialFile) {
            throw 'Las credenciales Basic Auth ya existen; no se reemplazarán automáticamente.'
        }
        $random = [Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
        $password = [Convert]::ToBase64String($random).TrimEnd('=').Replace('+', '-').Replace('/', '_')
        $credential = [pscredential]::new('profesor', (ConvertTo-SecureString $password -AsPlainText -Force))
        try {
            $credential | Export-Clixml -LiteralPath $credentialFile
        } catch {
            if (Test-Path -LiteralPath $credentialFile) { Remove-Item -LiteralPath $credentialFile -Force }
            throw 'No se pudo cifrar la credencial con el perfil de Windows.'
        } finally {
            $password = $null
        }
        Write-Host 'Credenciales de demostración generadas y guardadas cifradas para este usuario de Windows.'
        Write-Host 'Para verlas localmente cuando las necesites, usa -Action ShowAccess.'
        break
    }
    'ShowAccess' {
        if (-not (Test-Path -LiteralPath $credentialFile)) { throw 'Faltan las credenciales Basic Auth.' }
        $credential = Import-Clixml -LiteralPath $credentialFile
        if ($credential -isnot [pscredential]) { throw 'El archivo de credenciales no es válido.' }
        Write-Host "Usuario: $($credential.UserName)"
        Write-Host "Contraseña: $($credential.GetNetworkCredential().Password)"
        Write-Host 'Comparte estos datos con el profesor por un medio privado. No los publiques en GitHub.'
        break
    }
    'Stop' {
        $running = @(docker ps -a --filter "name=^/$tunnelName$" --format '{{.Names}}')
        if ($running -contains $tunnelName) {
            docker rm -f $tunnelName | Out-Null
            if ($LASTEXITCODE -ne 0) { throw 'No se pudo detener el túnel; la política Basic Auth permanece activa.' }
        }
        if (Test-Path -LiteralPath $appPidFile) {
            $savedPid = [int](Get-Content -LiteralPath $appPidFile -Raw)
            $process = Get-Process -Id $savedPid -ErrorAction SilentlyContinue
            if ($process -and $process.ProcessName -eq 'dotnet') { Stop-Process -Id $savedPid }
            Remove-Item -LiteralPath $appPidFile -Force
        }
        if (Test-Path -LiteralPath $policyFile) { Remove-Item -LiteralPath $policyFile -Force }
        Write-Host 'Enlace HTTPS detenido. SQL Server permanece privado y disponible localmente.'
        break
    }
    'Status' {
        $localStatus = Get-LocalHttpStatus
        $running = @(docker ps --filter "name=^/$tunnelName$" --format '{{.Names}}')
        Write-Host "Aplicación local 127.0.0.1:$appPort/acceso: HTTP $localStatus"
        if ($running -contains $tunnelName) {
            $url = Get-TunnelUrl
            Write-Host "Túnel: activo; URL: $url"
        } else { Write-Host 'Túnel: detenido' }
        break
    }
    'Start' {
        if (-not (Test-Path -LiteralPath $credentialFile)) { throw 'Faltan las credenciales Basic Auth. Usa -Action PrepareBasic.' }
        if (-not (Test-Path -LiteralPath $tokenFile)) { throw 'Falta el authtoken local. Usa -Action ConfigureToken.' }
        $sql = @(docker ps --filter 'name=^/sqlserver_basedatos$' --format '{{.Names}}')
        if ($sql -notcontains 'sqlserver_basedatos') { docker start sqlserver_basedatos | Out-Null }
        if ((Get-LocalHttpStatus) -ne 200) {
            if (-not (Test-Path -LiteralPath $appDll)) { throw "Falta la compilación Release: $appDll" }
            $projectFile = Join-Path $appDir 'SupermercadoMVC.csproj'
            [xml]$project = Get-Content -LiteralPath $projectFile -Raw
            $secretId = [string]$project.Project.PropertyGroup.UserSecretsId
            $secretPath = Join-Path $env:APPDATA "Microsoft\UserSecrets\$secretId\secrets.json"
            if (-not (Test-Path -LiteralPath $secretPath)) { throw 'No se encuentra la configuración SQL local en User Secrets.' }
            $secrets = Get-Content -LiteralPath $secretPath -Raw | ConvertFrom-Json
            $connection = [string]$secrets.'ConnectionStrings:Supermercado'
            if (-not $connection) { throw 'Falta ConnectionStrings:Supermercado en User Secrets.' }
            $oldConnection = $env:ConnectionStrings__Supermercado
            $oldAspnet = $env:ASPNETCORE_ENVIRONMENT
            $oldDotnet = $env:DOTNET_ENVIRONMENT
            $oldUrls = $env:ASPNETCORE_URLS
            try {
                $env:ConnectionStrings__Supermercado = $connection
                $env:ASPNETCORE_ENVIRONMENT = 'Production'
                $env:DOTNET_ENVIRONMENT = 'Production'
                $env:ASPNETCORE_URLS = "http://127.0.0.1:$appPort"
                $appProcess = Start-Process -FilePath 'dotnet' -ArgumentList "`"$appDll`"" -WorkingDirectory $appDir -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $private 'app.stdout.log') -RedirectStandardError (Join-Path $private 'app.stderr.log')
                [System.IO.File]::WriteAllText($appPidFile, [string]$appProcess.Id)
            } finally {
                $env:ConnectionStrings__Supermercado = $oldConnection
                $env:ASPNETCORE_ENVIRONMENT = $oldAspnet
                $env:DOTNET_ENVIRONMENT = $oldDotnet
                $env:ASPNETCORE_URLS = $oldUrls
                $connection = $null
            }
            $ready = $false
            for ($i = 0; $i -lt 20; $i++) {
                Start-Sleep -Seconds 1
                if ((Get-LocalHttpStatus) -eq 200) { $ready = $true; break }
            }
            if (-not $ready) { throw "La aplicación no respondió en $appPort. Revisa deployment/private/app.stderr.log." }
        }
        $credential = Import-Clixml -LiteralPath $credentialFile
        if ($credential -isnot [pscredential] -or $credential.UserName -ne 'profesor') {
            throw 'Las credenciales Basic Auth no son válidas.'
        }
        $password = $credential.GetNetworkCredential().Password
        if ($password.Length -lt 24 -or $password.Length -gt 128 -or $password -notmatch '^[A-Za-z0-9_-]+$') {
            throw 'La contraseña Basic Auth no cumple los requisitos de seguridad.'
        }
        $policy = @"
on_http_request:
  - actions:
      - type: basic-auth
        config:
          realm: "Supermercado GT"
          credentials:
            - "profesor:$password"
          enforce: true
      - type: remove-headers
        config:
          headers:
            - authorization
"@
        [System.IO.File]::WriteAllText($policyFile, $policy, [System.Text.UTF8Encoding]::new($false))
        $password = $null
        $policy = $null
        $credential = $null

        $rollbackName = "$tunnelName-rollback"
        $rollbackExists = @(docker ps -a --filter "name=^/$rollbackName$" --format '{{.Names}}')
        if ($rollbackExists -contains $rollbackName) { throw 'Existe un contenedor de recuperación anterior; revisa Docker antes de iniciar.' }
        $existing = @(docker ps -a --filter "name=^/$tunnelName$" --format '{{.Names}}')
        $hadTunnel = $existing -contains $tunnelName
        if ($hadTunnel) {
            docker stop $tunnelName | Out-Null
            if ($LASTEXITCODE -ne 0) { throw 'No se pudo detener el túnel anterior; continúa protegido.' }
            docker rename $tunnelName $rollbackName | Out-Null
            if ($LASTEXITCODE -ne 0) {
                docker start $tunnelName | Out-Null
                throw 'No se pudo reservar el túnel anterior para recuperación.'
            }
        }
        $secureToken = Import-Clixml -LiteralPath $tokenFile
        $plainToken = [System.Net.NetworkCredential]::new('', $secureToken).Password
        try {
            $env:NGROK_AUTHTOKEN = $plainToken
            $plainToken = $null
            docker run -d --name $tunnelName --restart no -e NGROK_AUTHTOKEN --mount "type=bind,source=$policyFile,target=/etc/ngrok/policy.yaml,readonly" ngrok/ngrok:latest http "http://host.docker.internal:$appPort" --traffic-policy-file /etc/ngrok/policy.yaml --log stdout | Out-Null
            if ($LASTEXITCODE -ne 0) { throw 'ngrok no aceptó la nueva política Basic Auth.' }
            $url = $null
            for ($i = 0; $i -lt 20; $i++) {
                Start-Sleep -Seconds 1
                $url = Get-TunnelUrl
                $newTunnel = @(docker ps --filter "name=^/$tunnelName$" --format '{{.Names}}')
                if ($url -and $newTunnel -contains $tunnelName) { break }
            }
            if (-not $url -or $newTunnel -notcontains $tunnelName) { throw 'ngrok no publicó el enlace con Basic Auth.' }
        } catch {
            docker rm -f $tunnelName 2>$null | Out-Null
            if ($hadTunnel) {
                docker rename $rollbackName $tunnelName | Out-Null
                docker start $tunnelName | Out-Null
            }
            throw 'No se activó Basic Auth. Se intentó restaurar el túnel protegido anterior.'
        } finally {
            $plainToken = $null
            $env:NGROK_AUTHTOKEN = $null
        }
        if ($hadTunnel) { docker rm $rollbackName | Out-Null }
        Write-Host "Enlace HTTPS protegido: $url"
        Write-Host 'Protección externa: Basic Auth. Después se requiere el login propio de Supermercado GT.'
        break
    }
}
