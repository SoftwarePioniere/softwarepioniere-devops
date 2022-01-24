[CmdletBinding()]
param (
  [string] $serviceprincipalsFile,
  [string] $keyVaultSubscription = 'sopi-mpn-02',
  [string] $keyVaultName = 'sopi-secrets'
)

$json = Get-Content -Path $serviceprincipalsFile
$data = ConvertFrom-Json -InputObject ([System.string]::Concat( $json ))
# Write-Host $data

Push-Location $PSScriptRoot

$sub = $data[0]
foreach ($sub in $data) {
  Write-Host '=========================================================================================='

  $subscription = $sub.sub
  Write-Host  "Subscription: " $subscription

  az account set --subscription $subscription
  if ($LASTEXITCODE -ne 0) { 
    Pop-Location
    throw 'error' 
  }

  $cursub = az account show --query name --output tsv
  if ($cursub -ne $subscription) {
  
    Write-Host "Login Please !!!"
    $LASTEXITCODE = 0
    az login
    az account set --subscription $subscription
    if ($LASTEXITCODE -ne 0) {
      Pop-Location
      throw 'error' 
    }

    $cursub = az account show --query name --output tsv
    if ($cursub -ne $subscription) {
    
      Pop-Location
      throw 'not sub match'
    }
  }
  else {
    Write-Host  "  Subscription OK:" $subscription
  }

  $princs = $data.principals
  .\az-aad-service-principals-2.ps1 -subscription $subscription -princs $princs
  if ($LASTEXITCODE -ne 0) { 
    Pop-Location
    throw 'error' 
  }
}

foreach ($sub in $data) {
  Write-Host '=========================================================================================='
  $subscription = $sub.sub
  Write-Host  "Subscription: " $subscription

  foreach ($pri in $data.principals) {
    $name = $pri.name
    Write-Host "Principal Name: $name" 

    $subfile = "secret-$($subscription).json"
    Write-Host "   Subscription Secret File: $subfile"
    $spfile = "secret-$($name).json"
    Write-Host "   Principal Secret File: $spfile"

    if ( (Test-Path $subfile) -and (Test-Path $spfile)) {
      Write-Host " Writing To KeyVault"

      $sp = Get-Content -Path $spfile | ConvertFrom-Json
      $acc = Get-Content -Path $subfile | ConvertFrom-Json

      $key = "$($name)-principal-id"
      $value = $sp.appId
      az keyvault secret set --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription

      $key = "$($name)-principal-secret"
      $value = $sp.password
      az keyvault secret set --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription

      $key = "$($name)-subscription-id"
      $value = $acc.id
      az keyvault secret set --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription

      $key = "$($name)-subscription-name"
      $value = $acc.name
      az keyvault secret set --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription

      $key = "$($name)-tenant-id"
      $value = $sp.tenant
      az keyvault secret set --vault-name $keyVaultName --name $key --value $value --subscription $keyVaultSubscription

      if ($LASTEXITCODE -ne 0) {
        Pop-Location
        throw 'error' 
      }
    }
    else {
      Write-Host " One File not found"
    }
  }
}
Pop-Location