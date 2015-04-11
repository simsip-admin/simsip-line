@ECHO OFF
ECHO.
ECHO Clean output folders
ECHO ====================
del /Q mgfxo\DX09\Deferred\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX11\Deferred\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX09\Shadow\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX11\Shadow\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX09\Sky\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX11\Sky\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX09\Stock\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX11\Stock\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX09\Voxeliq\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX11\Voxeliq\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX09\Voxeliq\PostProcessing\Bloom\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX11\Voxeliq\PostProcessing\Bloom\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX09\Water\*.*
IF ERRORLEVEL 1 GOTO Fail
del /Q mgfxo\DX11\Water\*.*
IF ERRORLEVEL 1 GOTO Fail

ECHO.
ECHO Deferred
ECHO ========

ECHO.
ECHO Deferred\Deferred1Scene
ECHO -----------------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Deferred\Deferred1Scene.fx mgfxo\DX09\Deferred\Deferred1Scene.mgfxo /DEBUG 
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Deferred\Deferred1Scene.fx mgfxo\DX11\Deferred\Deferred1Scene.mgfxo /DEBUG /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Deferred\Deferred2Lights
ECHO ------------------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Deferred\Deferred2Lights.fx mgfxo\DX09\Deferred\Deferred2Lights.mgfxo /DEBUG 
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Deferred\Deferred2Lights.fx mgfxo\DX11\Deferred\Deferred2Lights.mgfxo /DEBUG /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Deferred\Deferred3Final
ECHO -----------------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Deferred\Deferred3Final.fx mgfxo\DX09\Deferred\Deferred3Final.mgfxo /DEBUG 
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Deferred\Deferred3Final.fx mgfxo\DX11\Deferred\Deferred3Final.mgfxo /DEBUG /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Shadow
ECHO ======

ECHO.
ECHO Shadow\ShadowMap
ECHO ----------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Shadow\ShadowMap.fx mgfxo\DX09\Shadow\ShadowMap.mgfxo /DEBUG 
ECHO.
IF ERRORLEVEL 1 GOTO Fail
..\Tools\2MGFX-Monogame\2MGFX.exe src\Shadow\ShadowMap.fx mgfxo\DX11\Shadow\ShadowMap.mgfxo /DEBUG /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Sky
ECHO ===

ECHO.
ECHO Sky\PerlinNoise
ECHO ---------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Sky\PerlinNoise.fx mgfxo\DX09\Sky\PerlinNoise.mgfxo /DEBUG
ECHO.
IF ERRORLEVEL 1 GOTO Fail
..\Tools\2MGFX-Monogame\2MGFX.exe src\Sky\PerlinNoise.fx mgfxo\DX11\Sky\PerlinNoise.mgfxo /Debug /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Sky\SkyDome
ECHO -----------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Sky\SkyDome.fx mgfxo\DX09\Sky\SkyDome.mgfxo /DEBUG
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Sky\SkyDome.fx mgfxo\DX11\Sky\SkyDome.mgfxo /Debug /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Stock
ECHO =====

ECHO.
ECHO Stock\StockBasicEffect
ECHO ----------------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Stock\StockBasicEffect.fx mgfxo\DX09\Stock\StockBasicEffect.mgfxo /DEBUG 
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Stock\StockBasicEffect.fx mgfxo\DX11\Stock\StockBasicEffect.mgfxo /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Voxeliq
ECHO =======

ECHO.
ECHO Voxeliq\BlockEffect
ECHO -------------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\BlockEffect.fx mgfxo\DX09\Voxeliq\BlockEffect.mgfxo /DEBUG
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\BlockEffect.fx mgfxo\DX11\Voxeliq\BlockEffect.mgfxo /Debug /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Voxeliq\DualTextured
ECHO --------------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\DualTextured.fx mgfxo\DX09\Voxeliq\DualTextured.mgfxo /DEBUG
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\DualTextured.fx mgfxo\DX11\Voxeliq\DualTextured.mgfxo /Debug /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Voxeliq\PostProcessing\Bloom\BloomCombine
ECHO -----------------------------------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\PostProcessing\Bloom\BloomCombine.fx mgfxo\DX09\Voxeliq\PostProcessing\Bloom\BloomCombine.mgfxo /DEBUG
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\PostProcessing\Bloom\BloomCombine.fx mgfxo\DX11\Voxeliq\PostProcessing\Bloom\BloomCombine.mgfxo /Debug /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Voxeliq\PostProcessing\Bloom\BloomExtract
ECHO -----------------------------------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\PostProcessing\Bloom\BloomExtract.fx mgfxo\DX09\Voxeliq\PostProcessing\Bloom\BloomExtract.mgfxo /DEBUG
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\PostProcessing\Bloom\BloomExtract.fx mgfxo\DX11\Voxeliq\PostProcessing\Bloom\BloomExtract.mgfxo /Debug /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Voxeliq\PostProcessing\Bloom\GaussianBlur
ECHO -----------------------------------------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\PostProcessing\Bloom\GaussianBlur.fx mgfxo\DX09\Voxeliq\PostProcessing\Bloom\GaussianBlur.mgfxo /DEBUG
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Voxeliq\PostProcessing\Bloom\GaussianBlur.fx mgfxo\DX11\Voxeliq\PostProcessing\Bloom\GaussianBlur.mgfxo /Debug /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

ECHO.
ECHO Water
ECHO =====

ECHO.
ECHO Water\Water
ECHO -----------
..\Tools\2MGFX-Monogame\2MGFX.exe src\Water\Water.fx mgfxo\DX09\Water\Water.mgfxo /DEBUG
IF ERRORLEVEL 1 GOTO Fail
ECHO.
..\Tools\2MGFX-Monogame\2MGFX.exe src\Water\Water.fx mgfxo\DX11\Water\Water.mgfxo /Debug /Profile:DirectX_11
IF ERRORLEVEL 1 GOTO Fail
ECHO.

GOTO Success

:Fail
Echo.
Echo Build all effects failed
Exit

:Success
Echo.
Echo Build all effects succeeded