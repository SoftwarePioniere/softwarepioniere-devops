[Cmdletbinding()]
Param(  
  [switch] $skipPublish,    
  [switch] $skipRestore
)


dotnet tool restore

$ver = (dotnet minver)
Write-Host "Version: $ver"

$artifacts = Join-Path -Path (Get-Location) -ChildPath 'artifacts'
Write-Host "Artifacts: $artifacts"

$nugetfeed = Join-Path -Path $artifacts -ChildPath 'nugetfeed'
Write-Host "nugetfeed: $nugetfeed"

If (Test-Path -Path $artifacts) {
  Write-Host "Deleting Artifacts"
  Remove-Item $artifacts -recurse -force 
}
New-Item $artifacts -ItemType Directory
  
@{ Version = $ver } | ConvertTo-Json | Out-File (Join-Path -Path $artifacts -ChildPath 'version.json')
  
if (! (Test-Path -Path $nugetfeed )) {
  New-Item $nugetfeed -ItemType Directory
}


if ($skipRestore) {
  Write-Host "skipping restore"
}
else {
  Write-Host "Starting restore"
  dotnet restore dirs.proj -v m --nologo
}

Write-Host "Starting build"
dotnet build dirs.proj --configuration Release -v m --nologo --no-restore

Write-Host "Starting Tests"
dotnet test dirs.proj --configuration Release --no-build --logger:trx --no-build --no-restore

Write-Host "Starting Pack"
dotnet pack dirs.proj --configuration Release -v m --nologo --no-restore --output $artifacts


# if ($skipPublish) {
#   Write-Host "skipping publish"
# }
# else {
  
#   $p = "sopi-devops.$ver.nupkg"
#   $file = Join-Path -Path $artifacts -ChildPath $p
  
#   Write-Host "Nuget Publish: $file"
#   dotnet nuget push $file --source $nugetfeed

# }