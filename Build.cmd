@echo off
setlocal
cd /d "%~dp0"
set "FX=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319"
if not exist "%FX%\csc.exe" set "FX=%WINDIR%\Microsoft.NET\Framework\v4.0.30319"
if not exist "%FX%\csc.exe" (
  echo The Windows .NET Framework 4 compiler was not found. > build.log
  echo This package needs the Windows .NET Framework 4.8 desktop components. >> build.log
  type build.log
  exit /b 1
)
echo Building Corner Spider...
"%FX%\csc.exe" /nologo /optimize+ /target:exe /out:BehaviorTests.exe SpiderWorld.cs SpeciesBehavior.cs WebEncounter.cs GuestWebPattern.cs PreyBehavior.cs NestBehavior.cs PlayBehavior.cs SpiderPose.cs PalpPose.cs SkinCatalog.cs SilkProfile.cs Settings.cs BehaviorTests.cs BehaviorRegressionTests.cs NaturalMotionTests.cs PounceTests.cs EncounterTests.cs GuestWebTests.cs PlayTests.cs BoardingTests.cs PatrolAttentionTests.cs SettingsTests.cs MorphologyTests.cs PreyTests.cs HammockTests.cs DismountTests.cs NestEntryTests.cs > build.log 2>&1
if errorlevel 1 goto failed
BehaviorTests.exe >> build.log 2>&1
if errorlevel 1 goto failed
"%FX%\csc.exe" /nologo /optimize+ /target:winexe /resource:SupportQr.png,CornerSpider.SupportQr.png /win32manifest:app.manifest /win32icon:CornerSpider.ico /out:JumpingSpider.build.exe /reference:"%FX%\WPF\WindowsBase.dll" /reference:"%FX%\WPF\PresentationCore.dll" /reference:"%FX%\WPF\PresentationFramework.dll" /reference:"%FX%\System.Xaml.dll" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll SpiderWorld.cs SpeciesBehavior.cs WebEncounter.cs GuestWebPattern.cs PreyBehavior.cs NestBehavior.cs PlayBehavior.cs SpiderPose.cs PalpPose.cs SkinCatalog.cs SilkProfile.cs Settings.cs PetArt.cs WebEncounterArt.cs PreyArt.cs MorphologyArt.cs FingerPalette.cs HammockArt.cs SilkPreview.cs PetSettingsWindow.cs SupportBanner.cs SettingsPreview.cs MouseClicks.cs CursorVeil.cs JumpingSpider.cs RuntimeTests.cs PlayPreview.cs DismountPreview.cs NestEntryPreview.cs ReleasePreview.cs SpeciesPreview.cs PreyPreview.cs EncounterPreview.cs >> build.log 2>&1
if errorlevel 1 goto failed
copy /y JumpingSpider.build.exe JumpingSpider.exe >> build.log 2>&1
if errorlevel 1 goto failed
del JumpingSpider.build.exe
echo Build and behavior checks passed.
exit /b 0
:failed
type build.log
exit /b 1
