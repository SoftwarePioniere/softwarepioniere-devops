[CmdletBinding()]
param (
  $extensions = @( 
    @{
      extensionid = 'custom-terraform-tasks'
      publisherid = 'ms-devlabs'
    }, 
    @{
      extensionid = 'printAllVariables'
      publisherid = 'ShaykiAbramczyk'
    }
  )
)

Write-Host '=============================='
Write-Host 'Installing Azure Devops Organization Extension'
Write-Host '=============================='

# Write-Host $extensions

foreach ($item in $extensions) {

  Write-Host "Extension: $($item.extensionid)" 
  $ext = (az devops extension show --extension-id $item.extensionid --publisher-id $item.publisherid --output json) | ConvertFrom-Json
  $global:LASTEXITCODE = 0  
  if ($ext) {
    # Write-Host "  Updating..."
  }
  else {
    Write-Host "  Creating.."
    $cmd = "    az devops extension install --extension-id $($item.extensionid) --publisher-id $($item.publisherid)"
    Write-Host $cmd 
    az devops extension install --extension-id $item.extensionid --publisher-id $item.publisherid
  }  
  if ($LASTEXITCODE -ne 0) { throw 'error' }

}
