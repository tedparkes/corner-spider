param(
    [Parameter(Mandatory = $true)][string]$Tag,
    [Parameter(Mandatory = $true)][string]$PackageDirectory,
    [Parameter(Mandatory = $true)][string]$NotesFile,
    [string[]]$Gallery = @(),
    [string]$GhPath = 'gh',
    [switch]$Publish,
    [string]$NativeLog
)

$ErrorActionPreference = 'Stop'
$repo = 'tedparkes/corner-spider'
if ($Tag -notmatch '^v([0-9]+\.[0-9]+)\.[0-9]+-alpha\.([0-9]+)$') {
    throw 'Supply a Corner Spider alpha tag such as v1.1.0-alpha.13.'
}
$title = "Corner Spider $($Matches[1]) Alpha $($Matches[2])"
$version = $Tag.Substring(1)
$env:GH_PROMPT_DISABLED = '1'

function Invoke-GitHub {
    $output = & $GhPath @args
    if ($LASTEXITCODE -ne 0) { throw "GitHub CLI command failed: $($args[0]) $($args[1])" }
    $output
}

$identity = Invoke-GitHub api user --jq .login
if ($identity -cne 'tedparkes') { throw 'Sign in to the personal tedparkes GitHub account first.' }
$commit = git -C $PSScriptRoot rev-parse "$Tag^{}"
if ($LASTEXITCODE -ne 0) { throw "Create and push the reviewed $Tag first." }

$notesPath = (Resolve-Path -LiteralPath $NotesFile).Path
$notes = [IO.File]::ReadAllText($notesPath).Trim().Replace("`r`n", "`n")
if ($notes -notmatch '(?m)^## Full changelog\s*$' -or
    $notes -notmatch "(?m)^## $([regex]::Escape($version))\s*$") {
    throw 'Review the release notes and include the full changelog for the tagged version.'
}

$assets = [ordered]@{}
$packageNames = @("CornerSpider-$Tag-Windows.zip", "CornerSpider-$Tag-Source.zip", 'SHA256SUMS.txt', 'CHANGELOG.md')
$assetPaths = @($packageNames | ForEach-Object { Join-Path $PackageDirectory $_ }) + $Gallery
foreach ($path in $assetPaths) {
    $file = Get-Item -LiteralPath $path
    if ($file.PSIsContainer -or $assets.Contains($file.Name)) { throw "Invalid or duplicate asset: $path" }
    $assets[$file.Name] = @{
        Path = $file.FullName
        Size = $file.Length
        Hash = (Get-FileHash -LiteralPath $file.FullName).Hash.ToLowerInvariant()
    }
}
foreach ($name in $packageNames[0..1]) {
    $line = "$($assets[$name].Hash)  $name"
    if ($line -cnotin (Get-Content -LiteralPath $assets['SHA256SUMS.txt'].Path)) {
        throw "Package checksum mismatch: $name"
    }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($assets[$name].Path)
    try {
        if (!$zip.Entries.Count -or @($zip.Entries | Where-Object {
            !$_.FullName.StartsWith('CornerSpider/', [StringComparison]::Ordinal) -or $_.FullName.Contains('\')
        }).Count) { throw "Unsupported ZIP entry paths: $name" }
    } finally { $zip.Dispose() }
}

function Read-VerifiedRelease {
    $release = Invoke-GitHub release view $Tag --repo $repo --json 'url,isDraft,isPrerelease,tagName,assets,body' | ConvertFrom-Json
    if ($release.tagName -cne $Tag -or !$release.isPrerelease -or
        $release.body.Trim().Replace("`r`n", "`n") -cne $notes -or $release.assets.Count -ne $assets.Count) {
        throw "GitHub release details differ from the reviewed inputs for $Tag."
    }
    foreach ($asset in $release.assets) {
        $expected = $assets[$asset.name]
        if (!$expected -or $asset.state -ne 'uploaded' -or $asset.size -ne $expected.Size -or
            $asset.digest -cne "sha256:$($expected.Hash)") {
            throw "GitHub asset verification failed: $($asset.name)"
        }
    }
    $release
}

$existing = & $GhPath release view $Tag --repo $repo --json tagName 2>&1
if ($LASTEXITCODE -ne 0) {
    if (($existing -join "`n") -notmatch '\brelease not found\b') { throw "Could not look up $Tag on GitHub." }
    $uploads = @($assets.Values | ForEach-Object { $_.Path })
    Invoke-GitHub release create $Tag @uploads --repo $repo --verify-tag --draft --prerelease --latest=false --title $title --notes-file $notesPath | Out-Null
}
$release = Read-VerifiedRelease
if ($Publish -and $release.isDraft) {
    if (!$NativeLog) { throw 'Supply the native self-test log from the extracted Windows ZIP to publish.' }
    $log = Get-Content -LiteralPath $NativeLog -Raw
    if ($log -notmatch "\ACorner Spider $([regex]::Escape($version)) Windows runtime checks" -or
        $log -notmatch '(?m)^RESULT: PASS \(0 failures\)') { throw 'The native self-test did not pass for this version.' }

    $runs = @(Invoke-GitHub run list --repo $repo --workflow build.yml --branch $Tag --commit $commit --limit 1 --json 'databaseId,conclusion,status' | ConvertFrom-Json)
    if (!$runs.Count) { throw "The GitHub Windows build for $Tag has not started yet." }
    $run = $runs[0]
    if ($run.status -ne 'completed') {
        Invoke-GitHub run watch $run.databaseId --repo $repo --exit-status --interval 5 | Out-Host
    } elseif ($run.conclusion -ne 'success') {
        throw "The GitHub Windows build for $Tag did not pass."
    }

    $stable = Invoke-GitHub api "repos/$repo/releases/latest" --jq .tag_name
    Invoke-GitHub release edit $Tag --repo $repo --draft=false --prerelease --latest=false | Out-Null
    $release = Read-VerifiedRelease
    if ($release.isDraft) { throw "The release is still a draft: $Tag" }
    if ((Invoke-GitHub api "repos/$repo/releases/latest" --jq .tag_name) -cne $stable) {
        throw 'The latest stable release changed during publication.'
    }
}
$release
