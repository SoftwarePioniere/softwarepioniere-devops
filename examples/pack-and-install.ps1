
$root = (Get-Item -Path (Get-Location)).Parent
Write-Host "Root: $root"

$artifacts = Join-Path -Path $root -ChildPath 'artifacts'
Write-Host "Artifacts: $artifacts"

Push-Location $root
./build.ps1

$ver = (dotnet minver)
Write-Host "Version: $ver"

Pop-Location

dotnet new tool-manifest --force
dotnet tool install --add-source $artifacts sopi-devops --version $ver --local

dotnet sopi-devops --help