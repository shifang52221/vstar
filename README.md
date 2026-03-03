# EVplayer2Crack
EV加密播放器2 反录屏、截图破解
（这段话编辑于2024年9月，此项目为2022年编写的代码，当前版本可能不适用，仅作存档学习使用）

## 原理 
APIHOOK 两个函数 SetWindowDisplayAffinity 和 OpenProcess 

## 使用方法
1.下载源码编译，或者下载Release中的成品  
2.然后安装 EVPlayer2(https://www.ieway.cn/evplayer2.html)
3.运行EVPlayer2 （进程名为 EVPlayer2.exe）  
4.打开我的软件，提示  
注入成功，破解成功 即可进行截图，录屏

## VStart Next (Windows Prototype)
This repository now also contains a new launcher prototype at src/VStartNext.App.

### Quick Verification
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
- dotnet build VStartNext.sln -c Release
- powershell -ExecutionPolicy Bypass -File scripts/verify.ps1

