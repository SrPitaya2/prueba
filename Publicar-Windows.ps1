# ?? Script de Publicación para Windows
# Sistema de Inventarios
# Solución alternativa cuando Visual Studio no muestra opción de Windows

param(
    [string]$OutputPath = "C:\Publicaciones\SistemaInventarios"
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "  PUBLICADOR WINDOWS" -ForegroundColor Cyan
Write-Host "  Sistema de Inventarios" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

# Verificar que estamos en el directorio correcto
if (-not (Test-Path "prueba\prueba.csproj")) {
    Write-Host "? Error: No se encuentra el archivo prueba.csproj" -ForegroundColor Red
    Write-Host "   Asegúrate de ejecutar este script desde la raíz del proyecto" -ForegroundColor Yellow
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host "?? Directorio de salida: $OutputPath`n" -ForegroundColor Yellow

# Paso 1: Compilar directamente (sin restore con runtime)
Write-Host "?? Paso 1/3: Compilando proyecto para Windows..." -ForegroundColor Cyan
$buildResult = & dotnet build prueba\prueba.csproj -f net10.0-windows10.0.19041.0 -c Release 2>&1 | Out-String

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Compilación exitosa`n" -ForegroundColor Green
} else {
    Write-Host "   ? Error en la compilación" -ForegroundColor Red
    Write-Host ($buildResult | Select-String "error") -ForegroundColor Red
    Read-Host "`nPresiona Enter para salir"
    exit 1
}

# Paso 2: Buscar los archivos compilados
Write-Host "?? Paso 2/3: Buscando archivos compilados..." -ForegroundColor Cyan

# Buscar en todas las posibles ubicaciones
$possiblePaths = @(
    "prueba\bin\Release\net10.0-windows10.0.19041.0\win-x64",
    "prueba\bin\Release\net10.0-windows10.0.19041.0",
    "prueba\bin\x64\Release\net10.0-windows10.0.19041.0\win-x64",
    "prueba\bin\x64\Release\net10.0-windows10.0.19041.0"
)

$sourcePath = $null
foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $sourcePath = $path
        Write-Host "   ? Archivos encontrados en: $path`n" -ForegroundColor Green
        break
    }
}

if (-not $sourcePath) {
    Write-Host "   ? No se encontraron los archivos compilados" -ForegroundColor Red
    Write-Host "`n   Rutas buscadas:" -ForegroundColor Yellow
    foreach ($path in $possiblePaths) {
        Write-Host "   - $path" -ForegroundColor Gray
    }
    Read-Host "`nPresiona Enter para salir"
    exit 1
}

# Paso 3: Preparar carpeta de publicación
Write-Host "?? Paso 3/3: Preparando publicación..." -ForegroundColor Cyan

$publishPath = "$sourcePath\publish"

# Crear/limpiar directorio de publicación
if (Test-Path $publishPath) {
    Remove-Item $publishPath -Recurse -Force
}
New-Item -ItemType Directory -Path $publishPath -Force | Out-Null

# Copiar TODOS los archivos y carpetas necesarios
Write-Host "   Copiando todos los archivos necesarios..." -ForegroundColor Gray

# Copiar todos los archivos del directorio raíz
Copy-Item -Path "$sourcePath\*" -Destination $publishPath -Include "*.exe","*.dll","*.json","*.config" -Force

# Copiar subdirectorios importantes (recursivamente)
$subdirsToCheck = @("runtimes", "assets", "resources", "lib")
foreach ($subdir in $subdirsToCheck) {
    $subdirPath = Join-Path $sourcePath $subdir
    if (Test-Path $subdirPath) {
        Write-Host "   ? Copiando carpeta: $subdir" -ForegroundColor Gray
        Copy-Item -Path $subdirPath -Destination $publishPath -Recurse -Force
    }
}

# Verificar que la carpeta runtimes se copió correctamente
if (Test-Path "$sourcePath\runtimes") {
    if (-not (Test-Path "$publishPath\runtimes")) {
        Write-Host "   ??  Reintentando copiar carpeta runtimes..." -ForegroundColor Yellow
        Copy-Item -Path "$sourcePath\runtimes" -Destination "$publishPath\runtimes" -Recurse -Force
    }
    Write-Host "   ? Carpeta runtimes incluida ($((Get-ChildItem "$publishPath\runtimes" -Recurse -File).Count) archivos)" -ForegroundColor Green
}

Write-Host "   ? Archivos preparados en: $publishPath`n" -ForegroundColor Green

# Copiar a la ubicación de salida personalizada
if ($OutputPath -ne $publishPath) {
    Write-Host "   Copiando a ubicación de distribución..." -ForegroundColor Gray
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }
    Copy-Item -Path "$publishPath\*" -Destination $OutputPath -Recurse -Force
    Write-Host "   ? Archivos también copiados a: $OutputPath`n" -ForegroundColor Green
}

# Copiar README si existe
if (Test-Path "README-USUARIO.txt") {
    Copy-Item "README-USUARIO.txt" -Destination $publishPath -Force
    if ($OutputPath -ne $publishPath) {
        Copy-Item "README-USUARIO.txt" -Destination $OutputPath -Force
    }
    Write-Host "   ? README incluido`n" -ForegroundColor Green
}

# Copiar el script Iniciar.bat si no existe
$batPath = Join-Path $publishPath "Iniciar.bat"
if (-not (Test-Path $batPath)) {
    @'
@echo off
echo Iniciando Sistema de Inventarios...
echo.
cd /d "%~dp0"
if not exist "prueba.exe" (
    echo ERROR: No se encuentra prueba.exe
    pause
    exit /b 1
)
echo Ejecutando aplicacion...
start "" "prueba.exe"
timeout /t 2 /nobreak >nul
'@ | Out-File -FilePath $batPath -Encoding ASCII
    Write-Host "   ? Script Iniciar.bat creado`n" -ForegroundColor Green
}

# Resumen final
Write-Host "`n================================" -ForegroundColor Cyan
Write-Host "  ? PUBLICACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "================================`n" -ForegroundColor Cyan

Write-Host "?? Ubicaciones de los archivos:`n" -ForegroundColor Yellow

Write-Host "1??  Publicación local:" -ForegroundColor Cyan
Write-Host "   $publishPath`n" -ForegroundColor White

if ($OutputPath -ne $publishPath) {
    Write-Host "2??  Copia de distribución:" -ForegroundColor Cyan
    Write-Host "   $OutputPath`n" -ForegroundColor White
}

# Verificar que el EXE existe
$exePath = Join-Path $publishPath "prueba.exe"
if (Test-Path $exePath) {
    $exeSize = (Get-Item $exePath).Length / 1MB
    Write-Host "? Archivo ejecutable:" -ForegroundColor Green
    Write-Host "   prueba.exe" -ForegroundColor White
    Write-Host "   Tamaño: $([math]::Round($exeSize, 2)) MB`n" -ForegroundColor Gray
    
    # Listar archivos principales
    Write-Host "?? Contenido de la publicación:" -ForegroundColor Cyan
    $files = Get-ChildItem $publishPath -File | Sort-Object Length -Descending | Select-Object -First 10
    foreach ($file in $files) {
        $size = $file.Length / 1MB
        if ($size -gt 1) {
            Write-Host "   - $($file.Name) ($([math]::Round($size, 2)) MB)" -ForegroundColor Gray
        } else {
            $sizeKB = $file.Length / 1KB
            Write-Host "   - $($file.Name) ($([math]::Round($sizeKB, 1)) KB)" -ForegroundColor Gray
        }
    }
    
    $totalFiles = (Get-ChildItem $publishPath -Recurse -File).Count
    $totalSize = (Get-ChildItem $publishPath -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB
    
    Write-Host "`n   Total: $totalFiles archivos | $([math]::Round($totalSize, 2)) MB" -ForegroundColor Yellow
    
    # Verificar carpetas críticas
    Write-Host "`n?? Carpetas incluidas:" -ForegroundColor Cyan
    $criticalFolders = @("runtimes")
    foreach ($folder in $criticalFolders) {
        if (Test-Path (Join-Path $publishPath $folder)) {
            $folderFiles = (Get-ChildItem (Join-Path $publishPath $folder) -Recurse -File).Count
            Write-Host "   ? $folder ($folderFiles archivos)" -ForegroundColor Green
        } else {
            Write-Host "   ??  $folder (no encontrada)" -ForegroundColor Yellow
        }
    }
    
} else {
    Write-Host "??  No se encontró prueba.exe en $publishPath" -ForegroundColor Yellow
    Write-Host "   Archivos disponibles:" -ForegroundColor Gray
    Get-ChildItem $publishPath -File | ForEach-Object {
        Write-Host "   - $($_.Name)" -ForegroundColor Gray
    }
}

Write-Host "`n================================" -ForegroundColor Cyan
Write-Host "  SIGUIENTES PASOS" -ForegroundColor Yellow
Write-Host "================================`n" -ForegroundColor Cyan

Write-Host "1??  Probar la aplicación:" -ForegroundColor Cyan
if (Test-Path $exePath) {
    Write-Host "   Ejecuta: $publishPath\Iniciar.bat" -ForegroundColor White
    Write-Host "   O directamente: $exePath`n" -ForegroundColor Gray
} else {
    Write-Host "   Busca el EXE en: $publishPath`n" -ForegroundColor White
}

Write-Host "2??  Crear ZIP para distribución:" -ForegroundColor Cyan
Write-Host "   a) Opción manual:" -ForegroundColor Yellow
Write-Host "      - Clic derecho en la carpeta $publishPath" -ForegroundColor White
Write-Host "      - Enviar a > Carpeta comprimida (zip)" -ForegroundColor White
Write-Host "   b) Opción PowerShell:" -ForegroundColor Yellow
Write-Host "      Compress-Archive -Path '$publishPath\*' -DestinationPath 'SistemaInventarios_v1.0_Windows.zip'`n" -ForegroundColor White

Write-Host "3??  Distribuir:" -ForegroundColor Cyan
Write-Host "   - Comparte el ZIP" -ForegroundColor White
Write-Host "   - Incluye el README-USUARIO.txt con instrucciones`n" -ForegroundColor White

Write-Host "================================`n" -ForegroundColor Cyan

# Preguntar si desea abrir la carpeta
$openFolder = Read-Host "¿Deseas abrir la carpeta de publicación? (S/N)"
if ($openFolder -eq "S" -or $openFolder -eq "s" -or $openFolder -eq "Y" -or $openFolder -eq "y") {
    Start-Process explorer.exe -ArgumentList $publishPath
}

# Preguntar si desea crear el ZIP automáticamente
$createZip = Read-Host "`n¿Deseas crear el archivo ZIP ahora? (S/N)"
if ($createZip -eq "S" -or $createZip -eq "s" -or $createZip -eq "Y" -or $createZip -eq "y") {
    $zipName = "SistemaInventarios_v1.0_Windows_$(Get-Date -Format 'yyyyMMdd').zip"
    Write-Host "`nCreando ZIP: $zipName..." -ForegroundColor Cyan
    
    # Eliminar ZIP anterior si existe
    if (Test-Path $zipName) {
        Remove-Item $zipName -Force
    }
    
    # Crear ZIP con todo el contenido de publish
    Compress-Archive -Path "$publishPath\*" -DestinationPath $zipName -Force
    Write-Host "? ZIP creado exitosamente!" -ForegroundColor Green
    Write-Host "   Ubicación: $(Get-Location)\$zipName" -ForegroundColor White
    
    $zipSize = (Get-Item $zipName).Length / 1MB
    Write-Host "   Tamaño: $([math]::Round($zipSize, 2)) MB`n" -ForegroundColor Gray
}

Write-Host "`n? ¡Publicación completada exitosamente!" -ForegroundColor Green
Write-Host "Presiona Enter para salir..." -ForegroundColor Gray
Read-Host
