[CmdletBinding()]
param (  
  [string] $org,
  [string] $project,
  [string] $principal     = 'devops-orga-app-aad-user-admin',
  [string] $subscription  = 'orga-app-general',
  [string] $endpoint      = 'orga-app-aad-user-admin'

)

Write-Host '=============================='
Write-Host 'Creating Service Endpoint'
Write-Host '=============================='
Write-Host "Project: $project" 
Write-Host "Organization: $org" 
Write-Host "Endpoint: $endpoint" 

$global:LASTEXITCODE = 0  
$sp = Get-Content -Path "secret-$($principal).json" | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'error' }   

$acc = Get-Content -Path "secret-$($subscription).json" | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'error' }   

$env:AZURE_DEVOPS_EXT_AZURE_RM_SERVICE_PRINCIPAL_KEY = $sp.password
az devops service-endpoint azurerm create --name $endpoint --azure-rm-service-principal-id $sp.appId --azure-rm-subscription-id $acc.id --azure-rm-subscription-name $acc.name --azure-rm-tenant-id $sp.tenant --org $org --project $project 
$env:AZURE_DEVOPS_EXT_AZURE_RM_SERVICE_PRINCIPAL_KEY = ''
if ($LASTEXITCODE -ne 0) { throw 'error' }        
