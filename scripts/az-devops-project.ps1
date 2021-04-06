[CmdletBinding()]
param (
  [string] $org,
  $name        = 'project-test3',
  $description = 'Das ist ein Test Projekt'
)

Write-Host '=============================='
Write-Host 'Creating Azure Devops Project'
Write-Host '=============================='
Write-Host "Organization: $org" 
Write-Host "Project: $name" 

$global:LASTEXITCODE = 0  
Write-Host "  Loading Projects"
[array] $existingProjects = (az devops project list --org $org --detect --output json) | ConvertFrom-Json
$existingProjects = $existingProjects.value
if ($LASTEXITCODE -ne 0) { throw 'error' }

$pr = $existingProjects | Where-Object -Property name -like $name | Select-Object -first 1

if ($pr) {
  Write-Host "  Project Found..." 
}
else {
  Write-Host "  Creating Project.."
  
  $global:LASTEXITCODE = 0  
  $pr = (az devops project create --org $org --name $name --description $description --detect) | ConvertFrom-Json
  if ($LASTEXITCODE -ne 0) { throw 'error' }
}  


