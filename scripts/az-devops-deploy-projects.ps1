[CmdletBinding()]
param (
  [string] $organizationFile,
  [string] $projectsFolder,
  [string] $keyVaultSubscription = 'sopi-mpn-02',
  [string] $keyVaultName = 'sopi-secrets'
)

$global:LASTEXITCODE = 0 

$json = Get-Content -Path $organizationFile
# Write-Host $json
$data = ConvertFrom-Json -InputObject ([System.string]::Concat( $json ))
# Write-Host $data
if ($LASTEXITCODE -ne 0) { throw 'error' }


$org = $data.Organization
Write-Host "Organization: $org"

Write-Host '=========================================================================================='

$projectfiles = Get-ChildItem -Path $projectsFolder

Push-Location $PSScriptRoot


foreach ($projectfile in $projectfiles) {
  Write-Host "Project File: " $projectfile.FullName

  $json1 = Get-Content -Path $projectfile.FullName
  # Write-Host $json1
  $project = ConvertFrom-Json -InputObject ([System.string]::Concat( $json1 ))

  Write-Host '=========================================================================================='
  Write-Host "Project: $($project.name)"

  .\az-devops-project.ps1 -org $org -name $project.name -description $project.description
  if ($LASTEXITCODE -ne 0) { 
    Pop-Location
    throw 'error' 
  }

  if ($project.members) {   
    Write-Host "  Project: $($project.name) - Team"
    .\az-devops-group-membership.ps1 -org $org -project $project.name -pattern "*$($project.name) Team" -members $project.members
    if ($LASTEXITCODE -ne 0) { 
      Pop-Location
      throw 'error' 
    }
  }

  if ($project.admins) {   
    Write-Host "  Project: $($project.name) - Project Administrators"
    .\az-devops-group-membership.ps1 -org $org -project $project.name -pattern "*Project Administrators" -members $project.admins
    if ($LASTEXITCODE -ne 0) { 
      Pop-Location
      throw 'error' 
    }
  }
  if ($project.repos) {   
    Write-Host "  Project: $($project.name) - Repositories"
    .\az-devops-repos.ps1 -org $org -project $project.name -repos $project.repos
    if ($LASTEXITCODE -ne 0) { 
      Pop-Location
      throw 'error' 
    }
  }

  if ($project.pipelines) {     
    Write-Host "  Project: $($project.name) - Pipelines"
    .\az-devops-pipelines.ps1 -org $org -project $project.name -pips $project.pipelines
    if ($LASTEXITCODE -ne 0) { 
      Pop-Location
      throw 'error' 
    }
  }
  
  if ($project.endpoints) {     
    Write-Host "  Project: $($project.name) - Endpoints"
    .\az-devops-service-endpoints-kv.ps1 -org $org -project $project.name -endpoints $project.endpoints -keyVaultSubscription $keyVaultSubscription -keyVaultName $keyVaultName
    if ($LASTEXITCODE -ne 0) { 
      Pop-Location
      throw 'error' 
    }
  }
  
}


Pop-Location


