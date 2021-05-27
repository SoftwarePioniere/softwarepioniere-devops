[CmdletBinding()]
param (
  
  [string] $name = 'devops-orga-app-aad-user-admin',
  [string] $subscription = 'orga-app-general',
  [array]  $roles = @('Contributor')

)

Write-Host '=============================='
Write-Host 'Creating Service Principal'
Write-Host '=============================='
Write-Host "Name: $name" 


# login und subscription krams
az account set --subscription $subscription
$acc = (az account show --output json) | ConvertFrom-Json
$acc | ConvertTo-Json | Out-File "secret-$($subscription).json"


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
}

# $sp = Get-Content -Path "secret-$($name).json" | ConvertFrom-Json

if ($roles) {
  Read-Host -Prompt '     Assigning Roles.. wait until created in aad and press enter'

  foreach ($r in $roles) {
    Write-Host "      Assigning Role $r"
    az role assignment create --assignee $sp.appId --role $r --scope "/subscriptions/$($acc.id)"
  }    
}
