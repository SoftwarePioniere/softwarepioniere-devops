[CmdletBinding()]
param (
  $baseUrl = 'https://raw.githubusercontent.com/SoftwarePioniere/softwarepioniere-devops/main/scripts/',
  $path   = 'downloaded-scripts',
  $scripts = @(
    'az-devops-extensions.ps1',
    'az-devops-org-security.ps1',
    'az-devops-pipelines.ps1',
    'az-devops-projects.ps1',
    'az-devops-repos.ps1',
    'az-devops-service-principals.ps1'
  )
)

Write-Host '=============================='
Write-Host 'Downloading Scripts'
Write-Host '=============================='

if (! (Test-Path $path)) {
  Write-Host "  Creating Directory $path"
  New-Item -Path $path -ItemType Directory
} else {  
  Write-Host "  Recreating Directory $path"
  Remove-Item -Path $path -Recurse -Force
  New-Item -Path $path -ItemType Directory  
}

foreach ($s in $scripts) {
  Write-Host "  Downloading Script: $s"
  $url = $baseUrl + $s
  Write-Host "    Url: $url"
  $output = Join-Path $path -ChildPath $s
  Write-Host "    Output: $output"
  $global:LASTEXITCODE = 0  
  Invoke-WebRequest -Uri $url -OutFile $output
  if ($LASTEXITCODE -ne 0) { throw 'error' }
}
