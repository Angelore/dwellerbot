@echo off
set "publishpath=D:\Projects\DwellerBot"
mkdir %publishpath%
echo Publishing to %publishpath%

cd bin\debug\
@echo on
copy /Y DwellerBot.exe %publishpath%
copy /Y DwellerBot.exe.config %publishpath%
copy /Y Resources\changelog.json %publishpath%\Resources
xcopy /y /d  "*.dll" %publishpath%
@echo off
echo Done

pause