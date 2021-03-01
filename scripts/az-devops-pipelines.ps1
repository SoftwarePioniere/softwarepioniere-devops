[CmdletBinding()]
param (
  [array] $pips = @(
    @{
      name  = 'sopidemo-azure-ad'      
      branch = 'main'
      description = 'sopidemo Azure AD einrichten'
      repository = 'softwarepioniere-demo'
      yamlpath = 'azure-pipelines.yml'
    }
  )
)


Write-Host '=============================='
Write-Host 'Creating Pipelines'
Write-Host '=============================='

[array] $existingPipelines = (az pipelines  list --output json) | ConvertFrom-Json  
$x = $pips[0]

foreach ($x in $pips) {

  Write-Host "  $($x.name)" 
  $exi = $existingPipelines | Where-Object -Property name -like $x.name | Select-Object -first 1
  # $exPrinc

  if ($exi) {
    Write-Host "    Pipeline found"
    Write-Host "   $($exi.remoteUrl)"
  }
  else {
    Write-Host "    Creating Pipeline"
    # az repos create --name $x.name
    az pipelines create --name $x.name --branch $x.branch --description $x.description --repository $x.repository --yaml-path $x.yamlpath --skip-first-run true --repository-type tfsgit
  }
}