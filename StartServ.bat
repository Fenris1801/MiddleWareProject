cd /d "%~dp0"

start "" ".\backend\LetsGoBiking\bin\Debug\LetsGoBiking.exe"
start "" ".\backend\ProxyCache\bin\Debug\ProxyCache.exe"
start "" python -m http.server 5500 --directory ".\front"

pause