@echo off
echo Starting Diabetic Mobile App...
cd Diabetic.Mobile
dotnet build -f net8.0-windows10.0.19041.0
if %ERRORLEVEL% EQU 0 (
    echo Mobile app built successfully for Windows
    echo You can run it using Visual Studio or deploy to a device
) else (
    echo Build failed
)
pause