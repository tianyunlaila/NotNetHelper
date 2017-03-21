echo off
for %%i in ("%cd%") do set "name=%%~ni"
git init
git remote add origin git@github.com:tianyunlaila/%name%.git
git add .
git commit -m "first"
git push -u origin master
git status