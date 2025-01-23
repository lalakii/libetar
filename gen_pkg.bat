@echo off
cd /d "%~dp0"
DEL /Q *.nupkg >NUL 2>&1
nuget > NUL 2>&1
if %ERRORLEVEL%==0 (
    nuget pack libetar.csproj -Properties Configuration=Release
)
