[CmdletBinding()]
param (
  [string] $project,
  [string] $pattern,
  [array] $members
)

Write-Host '=============================='
Write-Host 'Group Membership'
Write-Host '=============================='

Write-Host "Project: $project" 

$global:LASTEXITCODE = 0  
if ($project) {
  Write-Host "  Loading Project Groups"
  $groups = (az devops security group list --scope project --project $project --detect --output json) | ConvertFrom-Json
  $groups = $groups.graphGroups
  # $groups | ConvertTo-Json | Out-File "$($project).groups.json"
} 
else {
  Write-Host "  Loading Organization Groups"
  $groups = (az devops security group list --scope organization --detect --output json) | ConvertFrom-Json
  $groups = $groups.graphGroups
  # $groups | ConvertTo-Json | Out-File "organization.groups.json"
}
if ($LASTEXITCODE -ne 0) { throw 'error' }


Write-Host "   Searching Group $pattern"
$group = $groups | Where-Object -Property principalName -like $pattern  | Select-Object -first 1 
  
if ($group) {
  Write-Host "    Group found: $($group.principalName)"

  Write-Host '      Loading Memberships into Array'
  $memberships = (az devops security group membership list --id  $group.descriptor --detect --output json) | ConvertFrom-Json

  $groupMembers = @()

  [array]$membershipItems = ($memberships | Get-Member -MemberType NoteProperty).Name
  foreach ($mi in $membershipItems) {
    $x = $memberships.$mi;
    Write-Verbose $x
    if ($x.origin -eq 'aad' -and $x.subjectKind -eq 'user') {
      $groupMembers += $x
    }
  }
  Write-Host "        $($groupMembers.Length) Users in group found"
  # $groupMembers | ConvertTo-Json | Out-File "group.members.json"

  Write-Host '      Checking Members to Add'
  foreach ($m in $members) {
    Write-Host "        Checking $($m)"
    $exi = $groupMembers | Where-Object -Property mailAddress -eq $m | Select-Object -first 1
    if ($exi ) {
      Write-Host "          User $($m) already in group"
    }
    else {     
      Write-Host "        Adding Member: $m"
      $global:LASTEXITCODE = 0
      az devops security group membership add --group-id $group.descriptor --member-id $m --detect
      if ($LASTEXITCODE -ne 0) { throw 'error' }
    }
  }

  Write-Host '      Checking Members to Delete'
  foreach ($u in $groupMembers) {
    Write-Host "        Checking $($u.mailAddress)"
    if (-not $members.Contains($u.mailAddress)) {    
      Write-Host "          Removing member $($u.mailAddress)"
      $global:LASTEXITCODE = 0
      az devops security group membership remove --group-id $group.descriptor --member-id $u.descriptor --yes --detect
      if ($LASTEXITCODE -ne 0) { throw 'error' }
    }
  }


}
