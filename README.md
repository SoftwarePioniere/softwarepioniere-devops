# softwarepioniere-devops

Software Pioniere DevOps Scripts and Tools




```powershell
# download scripts to local folder
Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/SoftwarePioniere/softwarepioniere-devops/main/scripts/download-scripts.ps1'))


# download tool to local folder
Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/SoftwarePioniere/softwarepioniere-devops/main/tools/download-tools.ps1'))


do
dotnet tool install --add-source ./microsoft.botsay/nupkg microsoft.botsay

```
