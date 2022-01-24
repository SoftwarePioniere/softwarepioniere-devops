[CmdletBinding()]
param (  
  [string] $org,
  [string] $project,
  [string] $keyVaultSubscription = 'sopi-mpn-02',
  [string] $keyVaultName = 'sopi-secrets',
  [array] $endpoints = @( @{
    
      name          = 'sopi-mpn-02'    
      principalName = 'az-devops-sopi-scheideler-mpn-02-test-2'    
    }
  ) 
)

Write-Host '=============================='
Write-Host 'Creating Service Endpoints from Keyvault'
Write-Host '=============================='
Write-Host "Project: $project" 
Write-Host "Organization: $org" 

$global:LASTEXITCODE = 0  

$ep = $endpoints[0]
foreach ($ep in $endpoints) {
  Write-Host '=========================================================================================='
  $name = $ep.name
  Write-Host "Endpoint: $name" 
  $princ = $ep.principalName
  Write-Host "ServicePrincipal: $princ" 

  $key = "$($princ)-principal-id"
  $principalId = (az keyvault secret show --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription --query 'value' --output tsv)
  if ($LASTEXITCODE -ne 0) { throw 'error' }     

  $key = "$($princ)-principal-secret"
  $principalSecret = (az keyvault secret show --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription --query 'value' --output tsv)
  if ($LASTEXITCODE -ne 0) { throw 'error' }     

  $key = "$($princ)-subscription-id"
  $subscriptionId = (az keyvault secret show --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription --query 'value' --output tsv)
  if ($LASTEXITCODE -ne 0) { throw 'error' }     

  $key = "$($princ)-subscription-name"
  $subscriptionName = (az keyvault secret show --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription --query 'value' --output tsv)
  if ($LASTEXITCODE -ne 0) { throw 'error' }     

  $key = "$($princ)-tenant-id"
  $tenantId = (az keyvault secret show --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription --query 'value' --output tsv)
    if ($LASTEXITCODE -ne 0) { throw 'error' }     
    
  $env:AZURE_DEVOPS_EXT_AZURE_RM_SERVICE_PRINCIPAL_KEY = $principalSecret
  az devops service-endpoint azurerm create --name $name --azure-rm-service-principal-id $principalId --azure-rm-subscription-id $subscriptionId --azure-rm-subscription-name $subscriptionName --azure-rm-tenant-id $tenantId --org $org --project $project 

  $env:AZURE_DEVOPS_EXT_AZURE_RM_SERVICE_PRINCIPAL_KEY = ''
  if ($LASTEXITCODE -ne 0) { throw 'error' }        

}

