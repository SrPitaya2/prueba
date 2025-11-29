# ?? Sistema de Inventarios - Publicación Rápida

## ? USO RÁPIDO

### Publicar todas las plataformas (Android + Windows):
```powershell
.\Publicar.ps1
```

### Publicar solo una plataforma:
```powershell
.\Publicar.ps1 -Android   # Solo Android
.\Publicar.ps1 -Windows   # Solo Windows
.\Publicar.ps1 -iOS       # Solo iOS (requiere Mac)
```

---

## ?? PUBLICAR ANDROID

### Comando directo:
```powershell
dotnet publish prueba\prueba.csproj -f net10.0-android -c Release
```

### Resultado:
- **APK:** `prueba\bin\Release\net10.0-android\publish\com.tuempresa.inventarios-Signed.apk`
- **Tamaño:** ~52 MB

### Instalación:
1. Copia el APK al teléfono Android
2. Activa "Instalar apps de orígenes desconocidos" en Configuración
3. Abre el APK y toca "Instalar"

---

## ?? PUBLICAR WINDOWS

### Opción A: Ejecutable directo (RECOMENDADO para uso interno)
```powershell
dotnet publish prueba\prueba.csproj -f net10.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None -p:RuntimeIdentifier=win10-x64
```

**Resultado:** `prueba\bin\Release\net10.0-windows10.0.19041.0\win10-x64\publish\prueba.exe`

**Distribución:** Copia la carpeta completa `publish` y ejecuta `prueba.exe`

### Opción B: Paquete MSIX
```powershell
dotnet publish prueba\prueba.csproj -f net10.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64
```

**Instalación:** 
1. Activa "Modo de desarrollador" en Windows
2. Doble clic en el archivo `.msix`

---

## ?? PUBLICAR iOS

**?? Requiere Mac con Xcode**

```bash
dotnet publish prueba\prueba.csproj -f net10.0-ios -c Release /p:RuntimeIdentifier=ios-arm64
```

**Resultado:** `prueba\bin\Release\net10.0-ios\ios-arm64\publish\`

**Distribución:** Usar TestFlight o distribución Ad-Hoc (requiere Apple Developer Account)

---

## ? ESTADO ACTUAL

- ? Proyecto compila correctamente
- ? Android APK generado: 52 MB
- ? Configurado para Android, iOS y Windows
- ? Sin firma requerida (apps internas)
- ? Base de datos SQLite incluida

---

## ?? ARCHIVOS DE SALIDA

```
prueba\bin\Release\
??? net10.0-android\publish\
?   ??? com.tuempresa.inventarios-Signed.apk   ? Android
?
??? net10.0-windows10.0.19041.0\win10-x64\publish\
?   ??? prueba.exe                              ? Windows (EXE)
?   ??? *.msix                                  ? Windows (MSIX)
?
??? net10.0-ios\ios-arm64\publish\
    ??? *.ipa                                   ? iOS
```

---

## ?? SOLUCIÓN RÁPIDA DE PROBLEMAS

### Android: "App no instalada"
```powershell
# Desinstala la versión anterior primero
adb uninstall com.tuempresa.inventarios
```

### Windows: Error al ejecutar
- Asegúrate de copiar TODA la carpeta `publish`, no solo el .exe
- Verifica que sea Windows 10/11

### iOS: No compila
- Solo funciona en Mac
- Instala Xcode desde App Store

---

## ?? DOCUMENTACIÓN COMPLETA

Para más detalles, consulta: **[PUBLICACION.md](PUBLICACION.md)**

---

**App:** Sistema de Inventarios v1.0  
**Plataformas:** Android 5.0+, Windows 10+, iOS 15+
