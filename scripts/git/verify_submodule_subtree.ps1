param(
    [string]$RepoRoot = "."
)

$submodulePath = Join-Path $RepoRoot "external/team-docs"
$subtreePath = Join-Path $RepoRoot "shared/ui"

Write-Host "Checking submodule and subtree..."

if (Test-Path $submodulePath) {
    Write-Host "OK: submodule path exists -> $submodulePath"
}
else {
    Write-Host "WARN: submodule path missing -> $submodulePath"
}

if (Test-Path $subtreePath) {
    Write-Host "OK: subtree path exists -> $subtreePath"
}
else {
    Write-Host "WARN: subtree path missing -> $subtreePath"
}
