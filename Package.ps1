param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'dist'))
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    & cmd /c Build.cmd
    if ($LASTEXITCODE -ne 0) { throw 'Build or behavior tests failed.' }
    $binary = Join-Path $PSScriptRoot 'JumpingSpider.exe'
    $version = [Version](Get-Item -LiteralPath $binary).VersionInfo.FileVersion
    $releaseVersion = '{0}.{1}.{2}' -f $version.Major,$version.Minor,$version.Build
    $label = (Get-Item -LiteralPath $binary).VersionInfo.ProductVersion
    if ($label -match ('^' + [regex]::Escape($releaseVersion) + '-[0-9A-Za-z.-]+$')) { $releaseVersion = $label }
    $output = [IO.Path]::GetFullPath($OutputDirectory)
    New-Item -ItemType Directory -Path $output -Force | Out-Null
    $stage = Join-Path $output ('.stage-' + [Guid]::NewGuid().ToString('N'))
    $source = Join-Path $stage 'source\CornerSpider'
    $portable = Join-Path $stage 'portable\CornerSpider'
    New-Item -ItemType Directory -Path $source,$portable -Force | Out-Null
    $common = @('README.md','README.txt','LICENSE','CHANGELOG.md','ROADMAP.md','CONTRIBUTING.md','RELEASING.md','CornerSpider.ico','SupportQr.png')
    foreach ($file in $common) {
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination $source
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination $portable
    }
    # Include only public documentation/assets. Never collect the parent workspace.
    foreach ($destination in @($source,$portable)) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'docs') -Destination $destination -Recurse }
    foreach ($file in Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' -File) { Copy-Item -LiteralPath $file.FullName -Destination $source }
    foreach ($file in @('Build.cmd','Start-Spider.cmd','Stop-Spider.cmd','Package.ps1','app.manifest','JumpingSpider.exe.config','.gitignore','.gitattributes')) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination $source }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot '.github') -Destination $source -Recurse
    foreach ($file in @('JumpingSpider.exe','JumpingSpider.exe.config','Start-Spider.cmd','Stop-Spider.cmd')) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination $portable }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archives = @()
    foreach ($kind in @('Windows','Source')) {
        $archive = Join-Path $output ('CornerSpider-v' + $releaseVersion + '-' + $kind + '.zip')
        if (Test-Path -LiteralPath $archive) { throw "Archive already exists; choose a new output folder to preserve prior release bytes: $archive" }
        $tree = if ($kind -eq 'Windows') { $portable } else { $source }
        [IO.Compression.ZipFile]::CreateFromDirectory((Split-Path $tree),$archive,[IO.Compression.CompressionLevel]::Optimal,$false)
        $zip = [IO.Compression.ZipFile]::OpenRead($archive)
        try {
            $entries = @($zip.Entries | Where-Object { $_.Name.Length -gt 0 })
            $expected = @(Get-ChildItem -LiteralPath $tree -File -Recurse -Force)
            if ($entries.Count -ne $expected.Count) { throw 'Archive entry count mismatch.' }
            foreach ($entry in $entries) {
                $relative = $entry.FullName.Substring('CornerSpider/'.Length).Replace('/',[IO.Path]::DirectorySeparatorChar)
                $original = Join-Path $tree $relative
                $stream = $entry.Open();$sha = [Security.Cryptography.SHA256]::Create()
                try { $hash = ([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','') } finally { $stream.Dispose();$sha.Dispose() }
                if ($hash -ne (Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash) { throw "Archive content mismatch: $relative" }
            }
        } finally { $zip.Dispose() }
        $archives += $archive
    }
    $checksums = foreach ($archive in $archives) { ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + [IO.Path]::GetFileName($archive)) }
    $checksums | Set-Content -LiteralPath (Join-Path $output 'SHA256SUMS.txt') -Encoding ASCII
    # Generate release notes directly from the complete versioned changelog.
    $changelog = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'CHANGELOG.md')).Trim()
    $releaseLog = ($changelog -replace '\A# Changelog\s*', '').Trim()
    $releaseLog = ($releaseLog -replace '\A## Unreleased\s*(?=## )', '').Trim()
    $downloadRoot = 'https://github.com/tedparkes/corner-spider/releases/download/v' + $releaseVersion
    $intro = @(
        'A free jumping-spider companion for your actual Windows desktop. Twenty procedural skins, corner silk retreats, hunting, finger rides and multi-monitor exploration.'
        ''
        ('**[Download for Windows (ZIP)](' + $downloadRoot + '/CornerSpider-v' + $releaseVersion + '-Windows.zip)**')
        ''
        'Extract the entire Windows ZIP, then open **JumpingSpider.exe** inside the CornerSpider folder. Windows 10/11 with .NET Framework 4.8. No installer or administrator rights needed. Startup is opt-in.'
        ''
        'This is an unsigned community build. The pet works offline, with local saves and no telemetry. Optional support is a bundled Cash App QR. See the included verification notes for tested behavior and remaining hardware-specific limits.'
        ''
        ('[Source package](' + $downloadRoot + '/CornerSpider-v' + $releaseVersion + '-Source.zip) | [SHA-256 checksums](' + $downloadRoot + '/SHA256SUMS.txt) | [Changelog file](' + $downloadRoot + '/CHANGELOG.md)')
        ''
        '## Full changelog'
    ) -join "`r`n"
    $notes = '# Corner Spider ' + $releaseVersion + "`r`n`r`n" + $intro + "`r`n`r`n" + $releaseLog + "`r`n"
    [IO.File]::WriteAllText((Join-Path $output 'RELEASE-NOTES.md'),$notes)
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CHANGELOG.md') -Destination (Join-Path $output 'CHANGELOG.md')
    # Remove only the fresh staging folder inside the explicit output directory.
    $resolvedStage = [IO.Path]::GetFullPath($stage)
    if (!$resolvedStage.StartsWith($output.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase) -or !(Split-Path $resolvedStage -Leaf).StartsWith('.stage-')) { throw 'Unsafe staging path; refusing cleanup.' }
    Remove-Item -LiteralPath $resolvedStage -Recurse -Force
    Write-Output ('Verified release packages: ' + $output)
    $checksums
} finally { Pop-Location }
