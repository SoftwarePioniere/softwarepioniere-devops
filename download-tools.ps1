[CmdletBinding()]
param (
  $baseUrl = 'https://raw.githubusercontent.com/SoftwarePioniere/softwarepioniere-devops/main/src/',
  $basePath =  'downloaded-tools' 
)

$items = @(
   @{
     folder = 'SoftwarePioniere.DevOps'
     files  = @(    
       'SoftwarePioniere.DevOps.csproj'
       'Program.cs'
     )
   }
)


Write-Host '=============================='
Write-Host 'Downloading Tools'
Write-Host '=============================='

if (! (Test-Path $basePath)) {
  Write-Host "  Creating Directory $basePath"
  New-Item -Path $basePath -ItemType Directory
} else {  
  Write-Host "  Recreating Directory $basePath"
  Remove-Item -Path $basePath -Recurse -Force
  New-Item -Path $basePath -ItemType Directory  
}

# $item = $items[0]

Push-Location $basePath

foreach( $item in $items) {
  $path = $item.folder
  Write-Host "  Path: $path"


  if (! (Test-Path $path)) {
    Write-Host "  Creating Directory $path"
    New-Item -Path $path -ItemType Directory
  } 

  # $s = $item.files[0]
  foreach ($s in $item.files) {
    Write-Host "  Downloading File: $s"
    $url = $baseUrl + $($item.folder) + '/' + $s
    Write-Host "    Url: $url"
  
    $output = Join-Path $path -ChildPath $s
    Write-Host "    Output: $output"
    $global:LASTEXITCODE = 0  
    Invoke-WebRequest -Uri $url -OutFile $output
    if ($LASTEXITCODE -ne 0) { throw 'error' }
  }

}

Pop-Location
  