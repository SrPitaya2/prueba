# ?? Exportar Sistema de Inventarios como EXE para Windows

## ?? PROBLEMA CONOCIDO

Visual Studio puede no mostrar la opción de publicar para Windows cuando hay multi-targeting (Android, iOS, Windows).

**SÍNTOMA**: Al hacer clic derecho > Publicar, solo aparece Android.

---

## ? SOLUCIÓN: Usar Script PowerShell (RECOMENDADO)

### Pasos:

1. **Abre PowerShell** en la carpeta del proyecto:
   - En el Explorador de archivos, navega a `C:\Users\SrPitaya\source\repos\prueba\`
   - Shift + Clic derecho en espacio vacío
   - "Abrir ventana de PowerShell aquí" o "Abrir en Terminal"

2. **Ejecuta el script**:
    ```powershell
    .\Publicar-Windows.ps1
    ```

3. **Listo!** El script:
   - Limpia compilaciones anteriores
   - Restaura paquetes
   - Compila el proyecto
   - Copia los archivos necesarios
   - Te muestra la ubicación del EXE

### Resultado:
- **Ubicación**: `prueba\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\`
- **Archivo principal**: `prueba.exe`
- **Tamaño aproximado**: 150-200 MB

---

## ?? MÉTODO ALTERNATIVO: Compilar y Copiar Manual

Si el script no funciona, hazlo manualmente:

### 1. Compilar el proyecto:
```powershell
dotnet build prueba\prueba.csproj -f net10.0-windows10.0.19041.0 -c Release
```

### 2. Los archivos estarán en:
```
prueba\bin\Release\net10.0-windows10.0.19041.0\win-x64\
```

### 3. Copia TODA esa carpeta para distribuir

---

## ?? ANDROID (Ya funciona perfectamente)

```powershell
dotnet publish prueba\prueba.csproj -f net10.0-android -c Release
```

**APK ubicación**: `prueba\bin\Release\net10.0-android\publish\com.tuempresa.inventarios-Signed.apk`

---

## ?? DISTRIBUCIÓN

### Opción A: Carpeta Completa (RECOMENDADO)
1. Copia toda la carpeta `publish` (después de ejecutar el script)
2. Comprime en ZIP
3. Nombra: `SistemaInventarios_v1.0_Windows.zip`
4. Distribuye

### Contenido del ZIP:
```
SistemaInventarios_v1.0_Windows/
??? prueba.exe               ? Ejecutable principal
??? README-USUARIO.txt       ? Instrucciones
??? *.dll                    ? Bibliotecas necesarias
??? [otros archivos]
```

---

## ?? INSTALACIÓN EN COMPUTADORAS DE DESTINO

### Requisitos:
- **Windows 10** versión 1809 o superior
- **Windows 11** (cualquier versión)
- **NO requiere instalar .NET** (está incluido)

### Instalación:
1. Descomprimir el ZIP
2. Ejecutar `prueba.exe`
3. Primera ejecución: Windows Defender puede pedir confirmación (es normal)

---

## ?? SOLUCIÓN DE PROBLEMAS

### Script: "No se puede ejecutar scripts"
**Error**: `execution of scripts is disabled on this system`

**Solución**:
```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```
Luego vuelve a ejecutar el script.

### "Windows protegió su PC"
1. Clic en **"Más información"**
2. Clic en **"Ejecutar de todas formas"**
3. Esto es normal para apps sin firma digital

### "Falta archivo DLL"
- Asegúrate de copiar TODA la carpeta publish
- No copies solo el .exe

### "No se puede abrir la base de datos"
- La app creará la base de datos automáticamente en:
  - `C:\Users\[Usuario]\AppData\Local\prueba\inventario.db`

### Script falla al compilar
1. Verifica que tengas .NET 10 SDK instalado:
```powershell
dotnet --version
```
2. Si no es versión 10.x, instala .NET 10 SDK desde:
   https://dotnet.microsoft.com/download/dotnet/10.0

---

## ?? COMPARACIÓN DE MÉTODOS

| Método | Ventajas | Desventajas |
|--------|----------|-------------|
| **Script PowerShell** | ? Automático<br>? Rápido<br>? Verifica errores | ?? Requiere PowerShell |
| **Manual** | ? Control total<br>? Sin scripts | ?? Más pasos<br>?? Propenso a errores |
| **Visual Studio** | ? Interfaz gráfica | ? No disponible con multi-targeting |

---

## ?? SCRIPT AVANZADO (Opcional)

Para crear un ZIP automáticamente, modifica `Publicar-Windows.ps1`:

```powershell
# Al final del script, agrega:
$zipPath = "SistemaInventarios_v1.0_Windows.zip"
Compress-Archive -Path $publishPath -DestinationPath $zipPath -Force
Write-Host "? ZIP creado: $zipPath" -ForegroundColor Green
```

---

## ?? CHECKLIST ANTES DE DISTRIBUIR

- [ ] Ejecutar `.\Publicar-Windows.ps1` sin errores
- [ ] Probar `prueba.exe` en tu PC
- [ ] Probar en otra PC sin Visual Studio
- [ ] Verificar que se crea la base de datos
- [ ] Probar todas las funcionalidades:
  - [ ] Crear establecimiento
  - [ ] Crear áreas
  - [ ] Agregar productos
  - [ ] Movimientos de stock (entrada/salida)
  - [ ] Exportar a Excel
  - [ ] Generar reportes
- [ ] Incluir `README-USUARIO.txt` en el ZIP
- [ ] Nombrar el ZIP descriptivamente

---

## ?? PRÓXIMOS PASOS (OPCIONAL)

### Crear Instalador Profesional

Si necesitas un instalador `.exe` profesional:

1. **Instala Inno Setup**: https://jrsoftware.org/isdl.php
2. **Crea el script de instalación** (disponible en el proyecto)
3. **Genera `Setup.exe`** con:
   - Instalación guiada
   - Acceso directo en escritorio
   - Entrada en el menú inicio
   - Desinstalador automático

---

## ?? RESUMEN RÁPIDO

### Para Windows:
```powershell
# 1. Abre PowerShell en la carpeta del proyecto
cd C:\Users\SrPitaya\source\repos\prueba\

# 2. Ejecuta el script
.\Publicar-Windows.ps1

# 3. Los archivos estarán en:
# prueba\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\
```

### Para Android:
```powershell
# Funciona perfectamente con:
dotnet publish prueba\prueba.csproj -f net10.0-android -c Release

# APK en:
# prueba\bin\Release\net10.0-android\publish\com.tuempresa.inventarios-Signed.apk
```

---

## ?? NOTAS TÉCNICAS

### ¿Por qué Visual Studio no muestra Windows?

El problema es que Visual Studio tiene dificultades con proyectos multi-targeting (Android + iOS + Windows). El sistema de publicación de VS está diseñado principalmente para proyectos de una sola plataforma.

### ¿Por qué no funciona `dotnet publish` para Windows?

.NET 10 está en Preview y tiene un bug conocido donde intenta usar el runtime de Mono (para móviles) en lugar del runtime nativo de Windows. El script de PowerShell usa `dotnet build` que sí funciona correctamente.

### ¿Cuándo se solucionará?

Cuando .NET 10 salga de Preview (versión estable), tanto Visual Studio como `dotnet publish` deberían funcionar correctamente.

---

**Versión:** 2.0  
**Última actualización:** 2024  
**App:** Sistema de Inventarios - Multiplataforma  
**Método recomendado**: Script PowerShell `Publicar-Windows.ps1`
