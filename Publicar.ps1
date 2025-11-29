# Script de Publicación Multiplataforma
# Sistema de Inventarios

param(
    [switch]$Android,
    [switch]$Windows,
    [switch]$iOS,
    [switch]$All
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "  Sistema de Inventarios" -ForegroundColor Cyan
Write-Host "  Publicación Multiplataforma" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

# Si no se especifica ninguna plataforma, publicar todas
if (-not ($Android -or $Windows -or $iOS)) {
    $All = $true
}

$ErrorCount = 0
$SuccessCount = 0

# Función para mostrar resultado
function Show-Result {
    param($Platform, $Success, $Path)
    if ($Success) {
        Write-Host "? $Platform publicado exitosamente" -ForegroundColor Green
        Write-Host "   Ubicación: $Path`n" -ForegroundColor Yellow
        $script:SuccessCount++
    } else {
        Write-Host "? Error al publicar $Platform" -ForegroundColor Red
        $script:ErrorCount++
    }
}

# ANDROID
if ($Android -or $All) {
    Write-Host "?? Publicando Android..." -ForegroundColor Cyan
    try {
        dotnet publish prueba\prueba.csproj -f net10.0-android -c Release 2>&1 | Out-Null
        $path = "prueba\bin\Release\net10.0-android\publish\"
        Show-Result "Android" $true $path
    } catch {
        Show-Result "Android" $false ""
    }
}

# WINDOWS
if ($Windows -or $All) {
    Write-Host "?? Publicando Windows..." -ForegroundColor Cyan
    try {
        # Opción 1: MSIX (requiere modo desarrollador)
        dotnet publish prueba\prueba.csproj -f net10.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64 2>&1 | Out-Null
        $path = "prueba\bin\Release\net10.0-windows10.0.19041.0\win10-x64\publish\"
        Show-Result "Windows" $true $path
        
        # Opción 2: EXE sin empaquetar (más fácil de distribuir)
        Write-Host "?? Creando versión EXE sin empaquetar..." -ForegroundColor Cyan
        dotnet publish prueba\prueba.csproj -f net10.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None -p:RuntimeIdentifier=win10-x64 2>&1 | Out-Null
        $pathExe = "prueba\bin\Release\net10.0-windows10.0.19041.0\win10-x64\publish\"
        Write-Host "? Windows EXE publicado" -ForegroundColor Green
        Write-Host "   Ubicación: $pathExe`n" -ForegroundColor Yellow
    } catch {
        Show-Result "Windows" $false ""
    }
}

# iOS
if ($iOS -or $All) {
    if ($IsMacOS) {
        Write-Host "?? Publicando iOS..." -ForegroundColor Cyan
        try {
            dotnet publish prueba\prueba.csproj -f net10.0-ios -c Release /p:RuntimeIdentifier=ios-arm64 2>&1 | Out-Null
            $path = "prueba\bin\Release\net10.0-ios\ios-arm64\publish\"
            Show-Result "iOS" $true $path
        } catch {
            Show-Result "iOS" $false ""
        }
    } else {
        Write-Host "??  iOS requiere Mac con Xcode. Omitiendo..." -ForegroundColor Yellow
    }
}

# Resumen
Write-Host "`n================================" -ForegroundColor Cyan
Write-Host "  RESUMEN DE PUBLICACIÓN" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "? Exitosos: $SuccessCount" -ForegroundColor Green
Write-Host "? Errores: $ErrorCount" -ForegroundColor Red

if ($SuccessCount -gt 0) {
    Write-Host "`n?? ARCHIVOS GENERADOS:" -ForegroundColor Cyan
    
    if ($Android -or $All) {
        if (Test-Path "prueba\bin\Release\net10.0-android\publish\") {
            Write-Host "`n?? Android:" -ForegroundColor Yellow
            Get-ChildItem "prueba\bin\Release\net10.0-android\publish\*.apk" | ForEach-Object {
                Write-Host "   - $($_.Name)" -ForegroundColor White
                Write-Host "     Tamaño: $([math]::Round($_.Length / 1MB, 2)) MB" -ForegroundColor Gray
            }
        }
    }
    
    if ($Windows -or $All) {
        if (Test-Path "prueba\bin\Release\net10.0-windows10.0.19041.0\win10-x64\publish\") {
            Write-Host "`n?? Windows:" -ForegroundColor Yellow
            $winPath = "prueba\bin\Release\net10.0-windows10.0.19041.0\win10-x64\publish\"
            if (Test-Path "$winPath\*.msix") {
                Get-ChildItem "$winPath\*.msix" | ForEach-Object {
                    Write-Host "   - $($_.Name)" -ForegroundColor White
                }
            }
            if (Test-Path "$winPath\prueba.exe") {
                Write-Host "   - prueba.exe (ejecutable directo)" -ForegroundColor White
            }
        }
    }
    
    if (($iOS -or $All) -and $IsMacOS) {
        if (Test-Path "prueba\bin\Release\net10.0-ios\ios-arm64\publish\") {
            Write-Host "`n?? iOS:" -ForegroundColor Yellow
            Get-ChildItem "prueba\bin\Release\net10.0-ios\ios-arm64\publish\*.ipa" -ErrorAction SilentlyContinue | ForEach-Object {
                Write-Host "   - $($_.Name)" -ForegroundColor White
            }
        }
    }
}

Write-Host "`n================================`n" -ForegroundColor Cyan

# Instrucciones
if ($SuccessCount -gt 0) {
    Write-Host "?? SIGUIENTES PASOS:" -ForegroundColor Cyan
    Write-Host ""
    
    if ($Android -or $All) {
        Write-Host "?? Android:" -ForegroundColor Yellow
        Write-Host "   1. Copia el APK al dispositivo" -ForegroundColor White
        Write-Host "   2. Habilita 'Fuentes desconocidas' en Configuración" -ForegroundColor White
        Write-Host "   3. Instala el APK" -ForegroundColor White
        Write-Host ""
    }
    
    if ($Windows -or $All) {
        Write-Host "?? Windows:" -ForegroundColor Yellow
        Write-Host "   Opción A (EXE):" -ForegroundColor White
        Write-Host "   - Copia la carpeta 'publish' completa" -ForegroundColor White
        Write-Host "   - Ejecuta prueba.exe" -ForegroundColor White
        Write-Host ""
        Write-Host "   Opción B (MSIX):" -ForegroundColor White
        Write-Host "   1. Activa el Modo de Desarrollador en Windows" -ForegroundColor White
        Write-Host "   2. Doble clic en el archivo .msix" -ForegroundColor White
        Write-Host ""
    }
    
    if (($iOS -or $All) -and $IsMacOS) {
        Write-Host "?? iOS:" -ForegroundColor Yellow
        Write-Host "   1. Usa TestFlight o distribución Ad-Hoc" -ForegroundColor White
        Write-Host "   2. Requiere Apple Developer Account" -ForegroundColor White
        Write-Host ""
    }
}

Write-Host "Para más información, consulta PUBLICACION.md" -ForegroundColor Gray
