@echo off
echo Starting all Diabetic Management services...
echo.
echo This will open multiple command windows:
echo - API Server (port 5000/5001)
echo - Web Application (port 5002/5003)
echo - Mobile Build Status
echo.
echo Press any key to continue...
pause

echo Starting API Server...
start cmd /c "cd Diabetic && echo Starting Diabetic API Server... && dotnet run"

timeout /t 3 /nobreak > nul

echo Starting Web Application...
start cmd /c "cd Diabetic.Web && echo Starting Diabetic Web App... && dotnet run"

timeout /t 2 /nobreak > nul

echo Building Mobile App...
start cmd /c "cd Diabetic.Mobile && echo Building Diabetic Mobile App... && dotnet build -f net8.0-windows10.0.19041.0 && echo Mobile app built successfully! && pause"

echo.
echo All services started! 
echo - API will be available at: https://localhost:7030 or http://localhost:5278
echo - Web App will be available at: https://localhost:7058 or http://localhost:5294
echo - Mobile app has been built and is ready for deployment
echo.
echo Press any key to exit this launcher...
pause