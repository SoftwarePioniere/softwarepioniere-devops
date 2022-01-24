[CmdletBinding()]
param (
  [string] $organizationFile
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

Push-Location $PSScriptRoot

Write-Host "Project Collection Administrators"
.\az-devops-group-membership.ps1 -org $org -pattern "*Project Collection Administrators*" -members $data.CollectionAdmins
if ($LASTEXITCODE -ne 0) { 
  Pop-Location
  throw 'error' 
}

Write-Host '=========================================================================================='

Write-Host "Organization Extensions"
.\az-devops-extensions.ps1 -org $org -extensions $data.Extensions
if ($LASTEXITCODE -ne 0) {
  Pop-Location
  throw 'error' 
}

Pop-Location


