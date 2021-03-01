[CmdletBinding()]
param (
  [array] $repos = @(
    @{
      name             = 'tb-sample-app2-devops'      
    }
  )
)


Write-Host '=============================='
Write-Host 'Creating Repositories'
Write-Host '=============================='

[array] $existingRepos = (az repos list --output json) | ConvertFrom-Json  

foreach ($x in $repos) {

  Write-Host "  $($x.name)" 
  $exi = $existingRepos | Where-Object -Property name -like $x.name | Select-Object -first 1
  # $exPrinc

  if ($exi) {
    Write-Host "    Repo found"
    Write-Host "   $($exi.remoteUrl)"
  }
  else {
    Write-Host "    Creating Repo"
    az repos create --name $x.name

  }
}