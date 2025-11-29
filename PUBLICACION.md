# Guía de Publicación - Sistema de Inventarios
## Aplicación Interna (Sin Firma)

---

## ?? ANDROID (APK)

### Publicar APK para pruebas internas:

```powershell
dotnet publish prueba\prueba.csproj -f net10.0-android -c Release
```

**Ubicación del APK:** 
`prueba\bin\Release\net10.0-android\publish\com.tuempresa.inventarios-Signed.apk`

### Instalación:
1. Copia el APK al dispositivo Android
2. Habilita "Instalar aplicaciones de origen desconocido"
3. Toca el archivo APK para instalar

---

## ?? iOS (IPA)

### Publicar IPA (requiere Mac):

```powershell
dotnet publish prueba\prueba.csproj -f net10.0-ios -c Release /p:RuntimeIdentifier=ios-arm64
```

**Ubicación del IPA:** 
`prueba\bin\Release\net10.0-ios\ios-arm64\publish\`

### Notas iOS:
- **Requiere Mac** con Xcode instalado
- Para distribución interna sin App Store, necesitas:
  - Apple Developer Account (99 USD/año)
  - Certificado de distribución Ad-Hoc o Enterprise
  - Provisioning Profile

### Alternativa para pruebas rápidas (solo en Mac):
```bash
# Ejecutar en simulador iOS
dotnet build prueba\prueba.csproj -f net10.0-ios -c Debug
# Luego desde Visual Studio for Mac: Run > Debug
```

---

## ?? WINDOWS (MSIX)

### Publicar paquete MSIX para Windows:

```powershell
dotnet publish prueba\prueba.csproj -f net10.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64
```

**Ubicación del MSIX:** 
`prueba\bin\Release\net10.0-windows10.0.19041.0\win10-x64\publish\`

### Instalación en Windows (sin firma):

1. **Habilitar el Modo de Desarrollador:**
   - Abrir Configuración > Actualización y seguridad > Para desarrolladores
   - Activar "Modo de desarrollador"

2. **Instalar el paquete:**
   - Doble clic en el archivo `.msix`
   - O usar PowerShell:
     ```powershell
     Add-AppxPackage -Path "ruta\al\archivo.msix"
     ```

### Crear instalador sin empaquetar (alternativa más simple):

```powershell
dotnet publish prueba\prueba.csproj -f net10.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None -p:RuntimeIdentifier=win10-x64
```

**Ubicación:** `prueba\bin\Release\net10.0-windows10.0.19041.0\win10-x64\publish\`

Ejecutable: `prueba.exe` (copiar toda la carpeta publish para distribuir)

---

## ?? PUBLICAR TODAS LAS PLATAFORMAS A LA VEZ

### Script PowerShell para publicar todo:

```powershell
# Android
Write-Host "Publicando Android..." -ForegroundColor Green
dotnet publish prueba\prueba.csproj -f net10.0-android -c Release

# Windows (MSIX)
Write-Host "Publicando Windows..." -ForegroundColor Green
dotnet publish prueba\prueba.csproj -f net10.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64

# iOS (Solo si estás en Mac)
if ($IsMacOS) {
    Write-Host "Publicando iOS..." -ForegroundColor Green
    dotnet publish prueba\prueba.csproj -f net10.0-ios -c Release /p:RuntimeIdentifier=ios-arm64
}

Write-Host "`n? Publicación completada!" -ForegroundColor Cyan
Write-Host "Android APK: prueba\bin\Release\net10.0-android\publish\" -ForegroundColor Yellow
Write-Host "Windows MSIX: prueba\bin\Release\net10.0-windows10.0.19041.0\win10-x64\publish\" -ForegroundColor Yellow
if ($IsMacOS) {
    Write-Host "iOS IPA: prueba\bin\Release\net10.0-ios\ios-arm64\publish\" -ForegroundColor Yellow
}
```

---

## ?? REQUISITOS POR PLATAFORMA

### Android:
- ? Windows, Mac o Linux
- ? Sin requisitos especiales para APK de prueba

### iOS:
- ? Requiere **Mac** con Xcode
- ? Apple Developer Account (para dispositivos físicos)
- ? Simulador funciona sin cuenta

### Windows:
- ? Requiere **Windows 10/11**
- ? Modo desarrollador para instalar sin firma
- ? No requiere certificado para uso interno

---

## ?? CONFIGURACIÓN ACTUAL

El proyecto está configurado con:
- ? Múltiples plataformas: Android, iOS, Windows
- ? Sin firma requerida (uso interno)
- ? Modo Release optimizado
- ? Compilación correcta en todas las plataformas

---

## ?? DISTRIBUCIÓN INTERNA

### Android:
- Compartir APK por email, USB o servidor web interno
- Los usuarios deben permitir fuentes desconocidas

### Windows:
- Compartir carpeta publish completa (con .exe)
- O compartir archivo MSIX (requiere modo desarrollador)
- Considerar crear un instalador con WiX o Inno Setup

### iOS:
- Usar **TestFlight** (requiere Apple Developer)
- O usar **distribución Ad-Hoc** (hasta 100 dispositivos)
- O usar **Apple Business Manager** (empresas)

---

## ?? RECOMENDACIONES

1. **Versionado:** Actualiza `ApplicationDisplayVersion` y `ApplicationVersion` en el .csproj antes de cada publicación

2. **Pruebas:** Prueba en dispositivos reales de cada plataforma antes de distribuir

3. **Base de datos:** SQLite se incluye automáticamente en el paquete

4. **Actualizaciones:** Para apps internas, considera implementar un sistema de notificación de actualizaciones

5. **Logs:** En producción, considera agregar un sistema de logging remoto

---

## ?? SOLUCIÓN DE PROBLEMAS

### Android: "App not installed"
- Desinstala versión anterior
- Verifica permisos de instalación

### Windows: "No se puede instalar"
- Activa el Modo de Desarrollador
- Verifica que sea Windows 10 versión 1809 o superior

### iOS: No compila
- Asegúrate de estar en Mac
- Instala Xcode desde App Store
- Ejecuta: `xcode-select --install`

---

**Versión del documento:** 1.0  
**Fecha:** 2024  
**App:** Sistema de Inventarios v1.0
