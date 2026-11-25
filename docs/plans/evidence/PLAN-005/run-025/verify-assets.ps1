$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../../..'))
$assets = @(
    @{ category='characters'; name='Scp999'; revision='r005' },
    @{ category='vfx'; name='Scp999Reaction'; revision='r001' },
    @{ category='vfx'; name='Scp999Trail'; revision='r001' }
)
$result = foreach ($asset in $assets) {
    $target = Join-Path $taskRoot "GodotProject/Assets/Sprites/$($asset.category)/$($asset.name)"
    foreach ($file in Get-ChildItem -LiteralPath $target -File | Where-Object { $_.Extension -in @('.png','.json') }) {
        $source = Join-Path $taskRoot "Art/assets/$($asset.category)/$($asset.name)/revisions/$($asset.revision)/export/$($file.Name)"
        $sourceHash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
        $targetHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        [pscustomobject]@{asset=$asset.name; revision=$asset.revision; file=$file.Name; source=$sourceHash; runtime=$targetHash; equal=($sourceHash -eq $targetHash)}
    }
}
$output = Join-Path $PSScriptRoot 'asset-hashes.json'
if (Test-Path -LiteralPath $output) { throw 'Use a new evidence run; hashes already exist.' }
$result | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $output -Encoding utf8
if ($result.Count -ne 10 -or ($result | Where-Object { -not $_.equal })) { throw 'Runtime asset identity failed.' }
Write-Output "ASSET_IDENTITY_PASS files=$($result.Count)"
