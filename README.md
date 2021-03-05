# softwarepioniere-devops

Software Pioniere DevOps Scripts and Tools




```powershell
# download scripts to local folder
Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/SoftwarePioniere/softwarepioniere-devops/main/scripts/download-scripts.ps1'))

# adding as submodule
git submodule add https://github.com/SoftwarePioniere/softwarepioniere-devops.git

git submodule foreach git pull origin main

```
