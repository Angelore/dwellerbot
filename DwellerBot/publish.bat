@echo off
set "publishpath=D:\Projects\DwellerBot"
mkdir %publishpath%
echo Publishing to %publishpath%

cd bin\debug\
@echo on
copy /Y DwellerBot.exe %publishpath%
copy /Y DwellerBot.exe.config %publishpath%
copy /Y HtmlAgilityPack.dll %publishpath%
copy /Y Microsoft.Recognizers.Definitions.dll %publishpath%
copy /Y Microsoft.Recognizers.Text.dll %publishpath%
copy /Y Microsoft.Recognizers.Text.Number.dll %publishpath%
copy /Y Microsoft.Recognizers.Text.NumberWithUnit.dll %publishpath%
copy /Y Newtonsoft.Json.dll %publishpath%
copy /Y Serilog.dll %publishpath%
copy /Y Serilog.FullNetFx.dll %publishpath%
copy /Y Serilog.Sinks.Console.dll %publishpath%
copy /Y Serilog.Sinks.ColoredConsole.dll %publishpath%
copy /Y System.Collections.Immutable.dll %publishpath%
copy /Y System.ValueTuple.dll %publishpath%
copy /Y System.Net.Http.Extensions.dll %publishpath%
copy /Y System.Net.Http.Formatting.dll %publishpath%
copy /Y System.Net.Http.Primitives.dll %publishpath%
copy /Y Telegram.Bot.dll %publishpath%
copy /Y Resources\changelog.json %publishpath%\Resources
@echo off
echo Done

pause