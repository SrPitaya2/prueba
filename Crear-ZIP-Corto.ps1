# Script para crear ZIP con rutas cortas
# Evita el error 0x80010135 (ruta demasiado larga)

Write-Host "?? Creando versión con rutas cortas..." -ForegroundColor Cyan

$tempDir = "SistemaInventarios_Temp"
$sourcePath = "prueba\bin\Release\net10.0-windows10.0.19041.0"

# Limpiar directorio temporal
if (Test-Path $tempDir) {
    Remove-Item $tempDir -Recurse -Force
}
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

Write-Host "Copiando archivos..." -ForegroundColor Yellow

# Copiar todos los archivos del directorio raíz
Get-ChildItem $sourcePath -File | ForEach-Object {
    Copy-Item $_.FullName -Destination $tempDir -Force
}

# Copiar solo la carpeta runtimes (la más importante)
if (Test-Path "$sourcePath\runtimes") {
    Copy-Item "$sourcePath\runtimes" -Destination "$tempDir\runtimes" -Recurse -Force
    Write-Host "? Carpeta runtimes copiada" -ForegroundColor Green
}

# Agregar README
if (Test-Path "README-USUARIO.txt") {
    Copy-Item "README-USUARIO.txt" -Destination $tempDir -Force
}

# Crear script de inicio
@'
@echo off
title Sistema de Inventarios
echo Iniciando Sistema de Inventarios...
echo.
start "" "prueba.exe"
exit
'@ | Out-File -FilePath "$tempDir\INICIAR.bat" -Encoding ASCII

Write-Host "`nCreando ZIP..." -ForegroundColor Yellow

$zipName = "SistemaInventarios_v1.0_FINAL.zip"
if (Test-Path $zipName) {
    Remove-Item $zipName -Force
}

Compress-Archive -Path "$tempDir\*" -DestinationPath $zipName -Force

$zipSize = (Get-Item $zipName).Length / 1MB
Write-Host "`n? ZIP creado exitosamente!" -ForegroundColor Green
Write-Host "   Archivo: $zipName" -ForegroundColor White
Write-Host "   Tamaño: $([math]::Round($zipSize, 2)) MB" -ForegroundColor Gray

# Limpiar temporal
Remove-Item $tempDir -Recurse -Force

Write-Host "`n?? INSTRUCCIONES:" -ForegroundColor Cyan
Write-Host "1. Descomprime el ZIP en C:\App\ (ruta corta)" -ForegroundColor White
Write-Host "2. Ejecuta INICIAR.bat" -ForegroundColor White
Write-Host "`n??  IMPORTANTE: No descomprimas en rutas largas como:" -ForegroundColor Yellow
Write-Host "   C:\Users\...\Downloads\...\...\..." -ForegroundColor Gray
Write-Host "   Mejor usa: C:\App\ o C:\Inventario\" -ForegroundColor Green

Write-Host "`nPresiona Enter para salir..." -ForegroundColor Gray
Read-Host
