$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$envFile = Join-Path $repoRoot '.env'
$dockerCandidates = @(
    (Get-Command docker -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue),
    'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

if (-not $dockerCandidates) {
    throw 'Docker CLI was not found. Install Docker Desktop or add docker.exe to PATH.'
}

$dockerExe = @($dockerCandidates)[0]
$dockerBin = Split-Path -Parent $dockerExe

if (-not (($env:Path -split ';') -contains $dockerBin)) {
    $env:Path = "$dockerBin;$env:Path"
}

if (-not (Test-Path $envFile)) {
    throw ".env file not found at $envFile"
}

Get-Content $envFile | ForEach-Object {
    $line = $_.Trim()

    if (-not $line -or $line.StartsWith('#')) {
        return
    }

    $parts = $line.Split('=', 2)

    if ($parts.Count -ne 2) {
        return
    }

    [System.Environment]::SetEnvironmentVariable($parts[0], $parts[1])
    Set-Item -Path ("Env:" + $parts[0]) -Value $parts[1]
}

if (-not $env:MONTRA_DB_PASSWORD) {
    throw 'MONTRA_DB_PASSWORD was not loaded from .env.'
}

Push-Location $repoRoot

try {
    $composeFile = Join-Path $repoRoot 'docker-compose.yml'
    $projectFile = Join-Path $repoRoot 'Assigment2_therapy\Assigment2_therapy.csproj'

    & $dockerExe compose --env-file $envFile -f $composeFile up -d

    if ($LASTEXITCODE -ne 0) {
        throw 'docker compose up failed. Check Docker Desktop state and image pull/auth configuration.'
    }

    $maxAttempts = 30

    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        $status = & $dockerExe inspect -f "{{.State.Health.Status}}" montra-sqlserver 2>$null

        if ($LASTEXITCODE -eq 0 -and $status.Trim() -eq 'healthy') {
            break
        }

        if ($attempt -eq $maxAttempts) {
            throw 'SQL Server container did not become healthy in time.'
        }

        Start-Sleep -Seconds 5
    }

    $env:ConnectionStrings__DefaultConnection = "Server=localhost,14333;Database=MontraTherapyDb;User Id=sa;Password=$($env:MONTRA_DB_PASSWORD);TrustServerCertificate=True;MultipleActiveResultSets=true"

    dotnet run --project $projectFile
}
finally {
    Pop-Location
}
