[CmdletBinding()]
param (
  [string] $org,
  [string] $project,
  [array] $creds = @(
    @{
      name             = 'sp-sopi2--tb-msdn--tb-sample-app2--contributor'
      subscriptionName = 'tb-msdn'    
      endpoint         = 'sopi2--tb-msdn--contributor'
      roles            =  @('Contributor')
    }

  )
)

# az login --tenant '74a8c6fa-684f-4b5a-b174-34428871d801' 
# az account set --subscription 'sopi-demo'

# az account list --query "[].{Name:name,SubscriptionId:id,TenantId:tenantId}" --output table

Write-Host '=============================='
Write-Host 'Creating Service Principals and Service Connections'
Write-Host '=============================='
Write-Host "Project: $project" 
Write-Host "Organization: $org" 

# $cr = $creds[0]
# $existingPrincipals | ConvertTo-Json | Out-File 'existingPrincipals.json'
# $existingPrincipals | ForEach-Object  { Write-Host $_.appDisplayName  }

# [array] $existingServiceCo#nnections = (az devops service-endpoint list --output json) | ConvertFrom-Json
# $existingPrincipals | ForEach-Object  { Write-Host $_.  }

foreach ($cr in $creds) {

  Write-Host "  $($cr.name)"

  $global:LASTEXITCODE = 0  
  az account set --subscription $cr.subscriptionName
  $acc = (az account show --output json) | ConvertFrom-Json  
  if ($LASTEXITCODE -ne 0) { throw 'error' }

  [array] $existingPrincipals = (az ad sp list --all --output json) | ConvertFrom-Json  
  if ($LASTEXITCODE -ne 0) { throw 'error' }
  $exPrinc = $existingPrincipals | Where-Object -Property appDisplayName -like $cr.name | Select-Object -first 1
  # $exPrinc


  if ($exPrinc) {
    Write-Host "    SP found"
  }
  else {
    Write-Host "    Creating SP"
    $sp = (az ad sp create-for-rbac --name $cr.name --skip-assignment --output json) | ConvertFrom-Json       
    if ($LASTEXITCODE -ne 0) { throw 'error' }

    $sp | ConvertTo-Json | Out-File "secret-$($cr.name).json" 
    # $sp = Get-Content -Path "secret-$($cr.name).json" | ConvertFrom-Json

    if ($cr.roles) {
      Read-Host -Prompt '     Assigning Roles.. wait until created in aad and press enter'

      foreach ($r in $cr.roles) {
        Write-Host "      Assigning Role $r"
        az role assignment create --assignee $sp.appId --role $r --scope "/subscriptions/$($acc.id)"
      }    
    }
    
    
    if ($sp) {      
      if ($cr.endpoint -and $sp) {    
        Write-Host "      Creating Service Endpoint"
        $env:AZURE_DEVOPS_EXT_AZURE_RM_SERVICE_PRINCIPAL_KEY = $sp.password
        az devops service-endpoint azurerm create --org $org --project $project --azure-rm-service-principal-id $sp.appId  --azure-rm-subscription-id $acc.id --azure-rm-subscription-name $acc.name --azure-rm-tenant-id $sp.tenant --name $cr.endpoint
        $env:AZURE_DEVOPS_EXT_AZURE_RM_SERVICE_PRINCIPAL_KEY = ''
        if ($LASTEXITCODE -ne 0) { throw 'error' }        
      }    
    }
  } 
}
