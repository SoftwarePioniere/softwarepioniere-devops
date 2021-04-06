[CmdletBinding()]
param (
  [string] $org,
  $projects = @( 
    @{
      name        = 'project1'
      description = 'Das ist das erste Projekt'
      members     = @(
        'tb@softwarepioniere.de',
        'mvd@softwarepioniere.de'
      )
    }, 
    @{
      name        = 'project2'
      description = 'Das ist das zweite Projekt'
    }
  )
)

Write-Host '=============================='
Write-Host 'Creating Azure Devops Projects'
Write-Host '=============================='
Write-Host "Organization: $org" 

# Write-Host $extensions

foreach ($item in $projects) {

  $name = $item.name
  Write-Host "Project: $name" 
  $ext = (az devops project show --org $org --project $name --output json) | ConvertFrom-Json

  $global:LASTEXITCODE = 0  
  if ($ext) {
    # Write-Host "  Updating..."
  }
  else {
    Write-Host "  Creating.."
    $cmd = "    az devops project create --name $name"
    Write-Host $cmd 
    az devops project create --org $org --name $name --description $item.description
  }  
  if ($LASTEXITCODE -ne 0) { throw 'error' }

  $groups = (az devops security group list --org $org --scope project --project $name --output json) | ConvertFrom-Json
  # $groups | ConvertTo-Json | Out-File "$($name).groups.json"
  $pattern = "*$($name) Team";
  Write-Host "    Searching Team $pattern"
  $g = $groups.graphGroups | Where-Object -Property principalName -like $pattern  | Select-Object -first 1 
  
  if ($g) {
    Write-Host "    Team Found $pattern"

    foreach ($m in $item.members) {
      $global:LASTEXITCODE = 0  
      Write-Host "     Adding Member: $m"
      az devops security group membership add --org $org --group-id $g.descriptor --member-id $m      
      if ($LASTEXITCODE -ne 0) { throw 'error' }
    }

  }
}

# $g = $groups | Select-Object -first 1 
# $g | ConvertTo-Json

# $pn = 'project1'
# $pattern = "[$pn]\\\\$pn Team";
# $pattern