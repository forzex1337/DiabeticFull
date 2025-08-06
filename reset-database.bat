@echo off
echo Resetting database...
cd Diabetic
dotnet ef database drop -f
dotnet ef database update
echo Database reset complete!
pause