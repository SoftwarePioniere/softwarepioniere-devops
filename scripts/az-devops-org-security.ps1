[CmdletBinding()]
param (
  [string] $org,
  $groups = @(
    @{
      name    = 'Project Collection Administrators'
      members = @(
        'tb@softwarepioniere.de',
        'mvd@softwarepioniere.de'
      )
    }
  )
)

Write-Host '=============================='
Write-Host 'Configuring Organization Group Memberships'
Write-Host '=============================='
Write-Host "Organization: $org" 

$devOpsGroups = (az devops security group list --org $org --scope organization --output json) | ConvertFrom-Json

foreach ($item in $groups) {
  # $item = $groups[0]
  Write-Host "Group: $($item.name)"

  $pattern = '*' + $item.name + '*'
  $devOpsGroup = $devOpsGroups.graphGroups | Where-Object -Property principalName -like $pattern  | Select-Object -first 1

  if ($devOpsGroup) {
    Write-Host '  Group found'

    Write-Host '    Loading Members into Array'
    $devOpsMembership = (az devops security group membership list --org $org --id  $devOpsGroup.descriptor --output json) | ConvertFrom-Json

    $devOpsMembers = @()
    [array]$users = ($devOpsMembership | Get-Member -MemberType NoteProperty).Name
    foreach ($u in $users) {
      $x = $devOpsMembership.$u;
      Write-Verbose $x
      if ($x.origin -eq 'aad' -and $x.subjectKind -eq 'user') {
        $devOpsMembers += $devOpsMembership.$u
      }
    }
    Write-Host "      $($devOpsMembers.Length) Members found"

    Write-Host '    Checking Members to Add'
    foreach ($m in $item.members) {
      Write-Host "      Checking $($m)"
      $exi = $devOpsMembers | Where-Object -Property mailAddress -eq $m | Select-Object -first 1
      if ($exi ) {
        Write-Host "        User $($m) already in group"
      }
      else {
        $global:LASTEXITCODE = 0
        Write-Host "      Adding Member: $m"
        az devops security group membership add --org $org --group-id $devOpsGroup.descriptor --member-id $m
        if ($LASTEXITCODE -ne 0) { throw 'error' }
      }
    }

    Write-Host '    Checking Members to Delete'
    foreach ($u in $devOpsMembers) {
      Write-Host "      Checking $($u.mailAddress)"
      if (-not $item.members.Contains($u.mailAddress)) {
        $global:LASTEXITCODE = 0
        Write-Host "        Removing member $($u.mailAddress)"
        az devops security group membership remove --org $org --group-id $devOpsGroup.descriptor --member-id $u.descriptor --yes
        if ($LASTEXITCODE -ne 0) { throw 'error' }
      }
    }

  }
  else {
    Write-Host '  Group not found'
  }

}