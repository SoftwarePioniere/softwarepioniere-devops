[CmdletBinding()]
param (
  [string] $org,  
  [array] $users
)

Write-Host '=============================='
Write-Host 'Organization Users'
Write-Host '=============================='

Write-Host "Organization: $org" 

$global:LASTEXITCODE = 0  

Write-Host "  Loading Existing Users"
$exiusers = ((az devops user list --org $org --output json) | ConvertFrom-Json).items
# $eu=$exiusers[0]
$exiusermails = @();
foreach ($eu in $exiusers) {
  # Write-Host "Existing User: $eu"
  # Write-Host "Existing User.user: $($eu.user)"
  Write-Host "    Existing User.user: $($eu.user.mailAddress)"
  $exiusermails += $eu.user.mailAddress
}
# Write-Host $exiusermails

Write-Host "  Checking Users"
# $u=$orgdata.Users[0]
foreach ($u in $orgdata.Users) {
  $exi = $exiusermails.Contains($u)
  Write-Host "    User: $u, exists: $exi"
  if (!$exi) {    
    Write-Host "      Adding User: $u"
    az devops user add --email-id $u --org $org --send-email-invite false --license-type stakeholder --detect false --output json
    if ($LASTEXITCODE -ne 0) { throw 'error' }    
  }
}


Write-Host '  Checking Users to Delete'
foreach ($eu in $exiusers) {
  $eumail = $eu.user.mailAddress
  Write-Host "    Existing User.user: $eumail"
  if (-not $users.Contains($eumail)) {    
    Write-Host "      Removing member $($eumail)"
    $global:LASTEXITCODE = 0
    az devops user remove --user $eu.id --org $org --yes
    if ($LASTEXITCODE -ne 0) { throw 'error' }
  }

}