param(
    [Parameter(Mandatory = $true)]
    [string]$AdminEmail,

    [Parameter(Mandatory = $true)]
    [string]$AdminFullName,

    [Parameter(Mandatory = $true)]
    [string]$AdminPassword
)

$ErrorActionPreference = 'Stop'

function Get-DockerExe {
    $docker = Get-Command docker -ErrorAction SilentlyContinue
    if ($docker) {
        return $docker.Source
    }

    $candidates = @(
        'C:\Program Files\Docker\Docker\resources\bin\docker.exe',
        'C:\Program Files\Docker\Docker\resources\docker.exe'
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw 'docker.exe was not found. Start Docker Desktop first.'
}

function Import-DotEnv {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        throw ".env file not found at $Path"
    }

    foreach ($line in Get-Content $Path) {
        if ([string]::IsNullOrWhiteSpace($line) -or $line.TrimStart().StartsWith('#')) {
            continue
        }

        $parts = $line.Split('=', 2)
        if ($parts.Count -ne 2) {
            continue
        }

        [Environment]::SetEnvironmentVariable($parts[0], $parts[1])
    }
}

function Get-PasswordHash {
    param([string]$Value)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $sha256.ComputeHash($bytes)
    }
    finally {
        $sha256.Dispose()
    }

    return ([System.BitConverter]::ToString($hashBytes)).Replace('-', '').ToLowerInvariant()
}

function Escape-SqlLiteral {
    param([string]$Value)

    return $Value.Replace("'", "''")
}

function New-TemporaryPassword {
    param([int]$Length = 14)

    $alphabet = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%*?'
    $buffer = New-Object char[] $Length
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $randomBytes = New-Object byte[] $Length
    $rng.GetBytes($randomBytes)

    for ($i = 0; $i -lt $Length; $i++) {
        $buffer[$i] = $alphabet[$randomBytes[$i] % $alphabet.Length]
    }

    return -join $buffer
}

function Invoke-SqlText {
    param(
        [string]$Query,
        [string]$Database = 'MontraTherapyDb'
    )

    $dockerExe = Get-DockerExe
    $container = 'montra-sqlserver'
    $password = [Environment]::GetEnvironmentVariable('MONTRA_DB_PASSWORD')

    if ([string]::IsNullOrWhiteSpace($password)) {
        throw 'MONTRA_DB_PASSWORD is missing. Check your .env file.'
    }

    $tmpFile = [System.IO.Path]::GetTempFileName()
    try {
        Set-Content -Path $tmpFile -Value $Query -Encoding UTF8
        $tmpFileUnix = '/tmp/' + [System.IO.Path]::GetFileName($tmpFile)

        & $dockerExe cp $tmpFile "${container}:${tmpFileUnix}" | Out-Null
        & $dockerExe exec $container /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $password -d $Database -C -b -i $tmpFileUnix
        try {
            & $dockerExe exec $container sh -c "rm -f $tmpFileUnix" 2>$null | Out-Null
        }
        catch {
        }
    }
    finally {
        if (Test-Path $tmpFile) {
            Remove-Item $tmpFile -Force
        }
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
Import-DotEnv -Path (Join-Path $repoRoot '.env')

$adminHash = Get-PasswordHash -Value $AdminPassword
$adminEmailSql = Escape-SqlLiteral -Value $AdminEmail
$adminFullNameSql = Escape-SqlLiteral -Value $AdminFullName
$therapistOnePassword = New-TemporaryPassword
$therapistTwoPassword = New-TemporaryPassword
$therapistOneHash = Get-PasswordHash -Value $therapistOnePassword
$therapistTwoHash = Get-PasswordHash -Value $therapistTwoPassword

$bootstrapSql = @"
SET NOCOUNT ON;

DECLARE @AdminUserId INT;
DECLARE @TherapistOneUserId INT;
DECLARE @TherapistTwoUserId INT;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = '$adminEmailSql')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt)
    VALUES ('$adminFullNameSql', '$adminEmailSql', '$adminHash', 'Admin', GETDATE());
END
ELSE
BEGIN
    UPDATE Users
    SET FullName = '$adminFullNameSql', PasswordHash = '$adminHash', Role = 'Admin'
    WHERE Email = '$adminEmailSql';
END;

SELECT @AdminUserId = Id FROM Users WHERE Email = '$adminEmailSql';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'linh.tran@montratherapy.vn')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt)
    VALUES ('Tran Ngoc Linh', 'linh.tran@montratherapy.vn', '$therapistOneHash', 'Therapist', GETDATE());
END;
ELSE
BEGIN
    UPDATE Users
    SET FullName = 'Tran Ngoc Linh', PasswordHash = '$therapistOneHash', Role = 'Therapist'
    WHERE Email = 'linh.tran@montratherapy.vn';
END;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'bao.nguyen@montratherapy.vn')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt)
    VALUES ('Nguyen Hoai Bao', 'bao.nguyen@montratherapy.vn', '$therapistTwoHash', 'Therapist', GETDATE());
END;
ELSE
BEGIN
    UPDATE Users
    SET FullName = 'Nguyen Hoai Bao', PasswordHash = '$therapistTwoHash', Role = 'Therapist'
    WHERE Email = 'bao.nguyen@montratherapy.vn';
END;

SELECT @TherapistOneUserId = Id FROM Users WHERE Email = 'linh.tran@montratherapy.vn';
SELECT @TherapistTwoUserId = Id FROM Users WHERE Email = 'bao.nguyen@montratherapy.vn';

IF NOT EXISTS (SELECT 1 FROM Therapists WHERE UserId = @TherapistOneUserId)
BEGIN
    INSERT INTO Therapists (UserId, Name, Specialty, Bio, Phone, IsActive)
    VALUES (@TherapistOneUserId, 'Tran Ngoc Linh', 'Functional Capacity Assessment', 'Occupational therapist focused on complex FCA, assistive technology planning, and community participation outcomes.', '0901555001', 1);
END
ELSE
BEGIN
    UPDATE Therapists
    SET Name = 'Tran Ngoc Linh', Specialty = 'Functional Capacity Assessment', Bio = 'Occupational therapist focused on complex FCA, assistive technology planning, and community participation outcomes.', Phone = '0901555001', IsActive = 1
    WHERE UserId = @TherapistOneUserId;
END;

IF NOT EXISTS (SELECT 1 FROM Therapists WHERE UserId = @TherapistTwoUserId)
BEGIN
    INSERT INTO Therapists (UserId, Name, Specialty, Bio, Phone, IsActive)
    VALUES (@TherapistTwoUserId, 'Nguyen Hoai Bao', 'Home Modifications & AT', 'Clinician specializing in home access, equipment recommendations, and discharge transition planning.', '0901555002', 1);
END
ELSE
BEGIN
    UPDATE Therapists
    SET Name = 'Nguyen Hoai Bao', Specialty = 'Home Modifications & AT', Bio = 'Clinician specializing in home access, equipment recommendations, and discharge transition planning.', Phone = '0901555002', IsActive = 1
    WHERE UserId = @TherapistTwoUserId;
END;

IF NOT EXISTS (SELECT 1 FROM TherapyServices WHERE Name = 'Functional Capacity Assessment')
BEGIN
    INSERT INTO TherapyServices (Name, Description, Price, DurationMinutes, IsActive)
    VALUES ('Functional Capacity Assessment', 'Comprehensive functional assessment with clinical interview, observation, and funding-ready reporting.', 1800000, 120, 1);
END
ELSE
BEGIN
    UPDATE TherapyServices
    SET Description = 'Comprehensive functional assessment with clinical interview, observation, and funding-ready reporting.', Price = 1800000, DurationMinutes = 120, IsActive = 1
    WHERE Name = 'Functional Capacity Assessment';
END;

IF NOT EXISTS (SELECT 1 FROM TherapyServices WHERE Name = 'Assistive Technology Assessment')
BEGIN
    INSERT INTO TherapyServices (Name, Description, Price, DurationMinutes, IsActive)
    VALUES ('Assistive Technology Assessment', 'Assessment for equipment selection, trials, justification, and implementation planning.', 1450000, 90, 1);
END
ELSE
BEGIN
    UPDATE TherapyServices
    SET Description = 'Assessment for equipment selection, trials, justification, and implementation planning.', Price = 1450000, DurationMinutes = 90, IsActive = 1
    WHERE Name = 'Assistive Technology Assessment';
END;

IF NOT EXISTS (SELECT 1 FROM TherapyServices WHERE Name = 'Home Modifications Assessment')
BEGIN
    INSERT INTO TherapyServices (Name, Description, Price, DurationMinutes, IsActive)
    VALUES ('Home Modifications Assessment', 'Clinical home access review with modification recommendations and supporting report.', 1600000, 120, 1);
END
ELSE
BEGIN
    UPDATE TherapyServices
    SET Description = 'Clinical home access review with modification recommendations and supporting report.', Price = 1600000, DurationMinutes = 120, IsActive = 1
    WHERE Name = 'Home Modifications Assessment';
END;

SELECT 'Users' AS Category, FullName AS [Name], Email AS [Value] FROM Users WHERE Email IN ('$adminEmailSql', 'linh.tran@montratherapy.vn', 'bao.nguyen@montratherapy.vn')
UNION ALL
SELECT 'Therapists', Name, Specialty FROM Therapists WHERE UserId IN (@TherapistOneUserId, @TherapistTwoUserId)
UNION ALL
SELECT 'Services', Name, CAST(Price AS nvarchar(50)) FROM TherapyServices WHERE Name IN ('Functional Capacity Assessment', 'Assistive Technology Assessment', 'Home Modifications Assessment');
"@

Invoke-SqlText -Query $bootstrapSql

Write-Host ''
Write-Host 'Bootstrap completed successfully.' -ForegroundColor Green
Write-Host "Admin login: $AdminEmail / $AdminPassword"
Write-Host "Therapist login: linh.tran@montratherapy.vn / $therapistOnePassword"
Write-Host "Therapist login: bao.nguyen@montratherapy.vn / $therapistTwoPassword"
Write-Host 'Change these passwords after first login.' -ForegroundColor Yellow