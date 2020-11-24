@echo off
set "publishpath=D:\Projects\DwellerBot"
mkdir %publishpath%
echo Publishing to %publishpath%

cd bin\debug\net5.0
@echo on
xcopy /y /d DwellerBot.exe %publishpath%
xcopy /y /d Resources\changelog.json %publishpath%\Resources\
xcopy /y /d  "*.dll" %publishpath%
xcopy /y /d  "*.json" %publishpath%
@echo off
echo Done

pause