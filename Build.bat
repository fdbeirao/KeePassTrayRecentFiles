@echo off
rmdir "..\TrayRecentFiles\obj" /S /Q
rmdir "..\TrayRecentFiles\bin" /S /Q
KeePass.exe --plgx-create "%~dp0TrayRecentFiles"
