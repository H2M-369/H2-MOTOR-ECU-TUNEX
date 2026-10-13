@echo off
setlocal
cd /d "%~dp0"
echo EcuTuneX UI patch
if not exist "Ecu-TuneX.csproj" (
  echo ERROR: Ecu-TuneX.csproj tidak ditemukan.
  echo Ekstrak ZIP langsung ke folder proyek ecu, sejajar dengan Ecu-TuneX.csproj.
  pause
  exit /b 2
)
echo Pastikan semua jendela EcuTuneX/Kline-Remap sudah ditutup.
if exist "ReadEcu.cs" del /f /q "ReadEcu.cs"
echo Read ECU stub dihapus (jika sebelumnya ada).
dotnet clean
if errorlevel 1 (
  echo dotnet clean gagal. Periksa SDK dan folder proyek.
  pause
  exit /b 1
)
dotnet build
if errorlevel 1 (
  echo BUILD GAGAL. Simpan dan kirim log error terakhir untuk koreksi berikutnya.
  pause
  exit /b 1
)
echo BUILD SUCCEEDED.
echo File hasil build: bin\Debug\net48\Ecu TuneX.exe
pause
