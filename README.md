# softwarepioniere-devops

Software Pioniere DevOps Scripts and Tools

# Packages

```
# find outdated packages
dotnet list package --outdated --highest-minor
dotnet list package --outdated

# find transitive package references that can be removed.
dotnet snitch

# update versions in file: Directory.Build.targets

# update local installed dotnet tools 
dotnet tool update minver-cli --local

# build sdk 
https://github.com/microsoft/MSBuildSdks/blob/main/src/Traversal/README.md

```   

## Use pscore scripts

```powershell
# download scripts to local folder
Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/SoftwarePioniere/softwarepioniere-devops/main/scripts/download-scripts.ps1'))

# adding as submodule
git submodule add https://github.com/SoftwarePioniere/softwarepioniere-devops.git

# updating submodule
git submodule foreach git pull origin main

# pull submodules after clone
git submodule init
git submodule update
```



## Run

```powershell

var client = Environment.GetEnvironmentVariable("servicePrincipalId");
var key = Environment.GetEnvironmentVariable("servicePrincipalKey");
var tenant = Environment.GetEnvironmentVariable("tenantId");


$env:servicePrincipalId = 'xxx'
$env:servicePrincipalKey = 'xxx'
$env:tenantId = 'xxx'

cd SoftwarePioniere.DevOps
dotnet run -- --help

dotnet run -- aad

```