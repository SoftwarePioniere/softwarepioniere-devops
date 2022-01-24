[CmdletBinding()]
param (
  [string] $org,
  [string] $project,
  [array] $repos = @(
     'tb-sample-app2-devops'
  )
)


Write-Host '=============================='
Write-Host 'Creating Repositories'
Write-Host '=============================='
Write-Host "Project: $project" 
Write-Host "Organization: $org" 


[array] $existingRepos = (az repos list --org $org --project $project --output json) | ConvertFrom-Json  

foreach ($x in $repos) {

  Write-Host "  $($x)" 
  $exi = $existingRepos | Where-Object -Property name -like $x | Select-Object -first 1
  # $exPrinc

  if ($exi) {
    Write-Host "    Repo found"
    Write-Host "   $($exi.remoteUrl)"
  }
  else {
    Write-Host "    Creating Repo"
    az repos create --org $org --project $project --name $x

  }
}