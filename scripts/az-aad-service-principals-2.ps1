[CmdletBinding()]
param (
  
  [string] $subscription = 'sopi-mpn-02',  
  [array] $princs = @(
    @{
      name             = 'az-devops-sopi-scheideler-mpn-02-test-2'    
      subcriptionroles = @('Contributor')
      reset            = $false
    }
  )

)

$global:LASTEXITCODE = 0 

Write-Host '=============================='
Write-Host 'Creating Service Principal'
Write-Host '=============================='
Write-Host "Subscription: $subscription" 


# login und subscription krams
az account set --subscription $subscription
$acc = (az account show --output json) | ConvertFrom-Json
$acc | ConvertTo-Json | Out-File "secret-$($subscription).json"

$pri = $princs[0]
foreach ($pri in $princs) {
  Write-Host '=========================================================================================='
  $name = $pri.name
  Write-Host "Principal Name: $name" 

  # service principal
  [array] $existingPrincipals = (az ad sp list --all --output json) | ConvertFrom-Json
  $exi = $existingPrincipals | Where-Object -Property appDisplayName -like $name | Select-Object -first 1

  if ($exi) {
    Write-Host "    SP found"
  }
  else {
    Write-Host "    Creating SP"
    $sp = (az ad sp create-for-rbac --name $name --skip-assignment --output json) | ConvertFrom-Json

    if ($LASTEXITCODE -ne 0) { throw 'error' }
    $sp | ConvertTo-Json | Out-File "secret-$($name).json"

    # check created
    $created = $false

    while ($created -eq $false) {
      # check if created 
      $existingPrincipals = (az ad sp list --all --output json) | ConvertFrom-Json
      $exi = $existingPrincipals | Where-Object -Property appDisplayName -like $name | Select-Object -first 1
  
      if ($exi) {
        Write-Host "    Creating SP.. OK"
        $created = $true
      }
      else {
        Write-Host "    Creating SP.. waiting and retry"
        Start-Sleep -Seconds 2
      }
    }
  }
  # $sp = Get-Content -Path "secret-$($name).json" | ConvertFrom-Json
  
  if ($pri.reset) {
    Write-Host "Resetting Credentials"
    $sp = (az ad sp credential reset --name $name --output json) | ConvertFrom-Json

    if ($LASTEXITCODE -ne 0) { throw 'error' }
    $sp | ConvertTo-Json | Out-File "secret-$($name).json"
  }

 
  if ($pri.subcriptionroles) {
    # Read-Host -Prompt '     Assigning Subscription Scopes.. wait until created in aad and press enter'

    foreach ($r in $pri.subcriptionroles) {
      Write-Host "      Assigning Subscription Scope Role $r"
      az role assignment create --assignee $sp.appId --role $r --scope "/subscriptions/$($acc.id)"
      if ($LASTEXITCODE -ne 0) { throw 'error' }
    }    
  }

  if ($pri.scopedroles) {
    foreach ($r in $pri.scopedroles) {
      Write-Host "      Assigning Scoped $($r.role) Role $($r.scope)"
      az role assignment create --assignee $sp.appId --role $($r.role) --scope $($r.scope)
      if ($LASTEXITCODE -ne 0) { throw 'error' }
    }    
  }

}