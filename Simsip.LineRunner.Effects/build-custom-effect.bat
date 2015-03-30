@ECHO OFF

REM We are expecting the visual studio item folder to be passed as the first parameter
set folderParam=%1

REM We are expecting the visual studio effect file name to be passed as the second parameter
set effectParam=%2

Rem IMPORTANT: This substring extraction is depending on the project being located at:
Rem            C:\dev3\simsip-line\Simsip.LineRunner.Effects
set folder=%folderParam:~47,-3%

Rem IMPORTANT: This substring extraction is depending on the effect file naming structure:
Rem            <effect>-core.fx
Rem            <effect>-dx09.fx
Rem            <effect>-dx11.fx
Rem            Example:
Rem            Deferred1Scene-core.fx
Rem            Deferred1Scene-dx09.fx
Rem            Deferred1Scene-dx11.fx
set effect=%effectParam:~1,-6%

ECHO.
ECHO Clean effect: %effect%
ECHO ======================
del /Q mgfxo\DX09\%folder%\%effect%.mgfxo
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX11\%folder%\%effect%.mgfxo
IF ERRORLEVEL 1 GOTO Fail

ECHO.
ECHO Build effect: %effect%
ECHO ======================

ECHO.
ECHO DX09 build for effect: %effect%
ECHO -------------------------------
..\Tools\2MGFX-Monogame\2MGFX.exe %folder%\%effect%-dx09.fx mgfxo\DX09\%folder%\%effect%.mgfxo /DEBUG 
IF ERRORLEVEL 1 GOTO Fail

ECHO.
ECHO DX11 build for effect: %effect%
ECHO -------------------------------
..\Tools\2MGFX-Monogame\2MGFX.exe %folder%\%effect%-dx11.fx mgfxo\DX11\%folder%\%effect%.mgfxo /DEBUG /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.


GOTO Success

:Fail
Echo.
Echo Build effect failed for effect: %effect%
Exit

:Success
Echo.
Echo Build effect succeeded for effect: %effect%
