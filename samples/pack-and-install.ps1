Push-Location ..
dotnet restore
dotnet pack
$ver = dotnet minver
Pop-Location

dotnet new tool-manifest --force
dotnet tool install --add-source ./../src/SoftwarePioniere.Devops/bin/Debug SoftwarePioniere.DevOps --version $ver

dotnet sopi-devops --help