@set /p user_input=请输入提交信息：
git add .
git commit -m '%user_input%'
git push origin master
git status
pause
