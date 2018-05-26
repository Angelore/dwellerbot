@echo off
set "outputpath=D:\Projects\DwellerBot\Resources\Images"
set "sourcepath=D:\Pictures\Internets"
mkdir %outputpath%
echo Copuing images to %outputpath%

@echo on
xcopy %sourcepath%\Reaction_images %outputpath%\Reaction_images /Y /I
xcopy %sourcepath%\Reaction_images\Macro %outputpath%\Reaction_images\Macro /Y /I
xcopy %sourcepath%\Vidya %outputpath%\Vidya /Y /I
xcopy %sourcepath%\Grafics_cat %outputpath%\Grafics_cat /Y /I
xcopy %sourcepath%\Memes\Bait %outputpath%\Memes\Bait /Y /I
xcopy %sourcepath%\Memes\Pacha %outputpath%\Memes\Pacha /Y /I
xcopy %sourcepath%\Memes\PeKa %outputpath%\Memes\PeKa /Y /I
@echo off
echo Done

pause