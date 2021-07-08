# sopi-dotnet-devops

Softwarepioniere DevOps-Tools .NET Application

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