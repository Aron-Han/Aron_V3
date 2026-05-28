param(
    [Parameter(Mandatory = $true)]
    [string]$AssemblyInfoPath
)

$resolvedPath = Resolve-Path -LiteralPath $AssemblyInfoPath
$text = Get-Content -LiteralPath $resolvedPath -Raw

$match = [regex]::Match($text, 'AssemblyVersion\("(?<version>\d+\.\d+\.\d+\.\d+)"\)')
if (-not $match.Success) {
    throw "AssemblyVersion was not found in $resolvedPath"
}

$parts = $match.Groups['version'].Value.Split('.') | ForEach-Object { [int]$_ }

for ($index = 3; $index -ge 0; $index--) {
    $parts[$index]++

    if ($parts[$index] -lt 10 -or $index -eq 0) {
        break
    }

    $parts[$index] = 0
}

$newVersion = [string]::Join('.', $parts)

$text = [regex]::Replace(
    $text,
    'AssemblyVersion\("\d+\.\d+\.\d+\.\d+"\)',
    'AssemblyVersion("' + $newVersion + '")')

$text = [regex]::Replace(
    $text,
    'AssemblyFileVersion\("\d+\.\d+\.\d+\.\d+"\)',
    'AssemblyFileVersion("' + $newVersion + '")')

Set-Content -LiteralPath $resolvedPath -Value $text -Encoding UTF8
Write-Host "Assembly version updated to $newVersion"
