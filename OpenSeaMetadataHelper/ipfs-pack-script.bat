@echo off
set /p "pack_dir=Enter packing directory: "
set /p "save_path=Enter save path with .car: "

echo Packing...
start /wait /b cmd /c ipfs-car --pack "%pack_dir%" --output "%save_path%" --wrapWithDirectory false
echo.

pause