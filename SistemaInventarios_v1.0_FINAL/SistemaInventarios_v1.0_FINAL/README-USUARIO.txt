# ?? Sistema de Inventarios - Guía de Instalación

## Bienvenido al Sistema de Inventarios

Esta aplicación te permite gestionar inventarios de múltiples establecimientos con control de stock en tiempo real.

---

## ? Requisitos del Sistema

- **Sistema Operativo**: Windows 10 (versión 1809 o superior) o Windows 11
- **Espacio en disco**: 300 MB mínimo
- **Memoria RAM**: 2 GB mínimo (4 GB recomendado)
- **NO se requiere instalar .NET** (ya está incluido)

---

## ?? Instalación

### Opción A: Carpeta Portátil (Sin instalación)

1. **Descomprime** el archivo ZIP descargado
2. Abre la carpeta descomprimida
3. **Doble clic** en `prueba.exe`
4. ¡Listo! La aplicación se ejecutará

### Opción B: Archivo Único

1. Copia `prueba.exe` a cualquier carpeta
2. **Doble clic** en el archivo
3. ¡Listo!

---

## ?? Primera Ejecución

La primera vez que abras la aplicación, Windows puede mostrar un mensaje de protección:

```
Windows protegió su PC
```

**Esto es NORMAL**. Sigue estos pasos:

1. Clic en **"Más información"**
2. Clic en **"Ejecutar de todas formas"**

Esto solo aparecerá la primera vez.

---

## ?? Características Principales

### ? Gestión Completa
- Múltiples establecimientos
- Áreas personalizadas por establecimiento
- Productos con stock en tiempo real
- Unidades de medida personalizables

### ? Control de Stock
- Entradas y salidas con motivos
- Alertas de stock bajo
- Historial completo de movimientos
- Notas y comentarios

### ? Reportes y Exportación
- Exportar a Excel
- Reportes de inventario
- Historial de movimientos
- Dashboard con estadísticas

---

## ??? ¿Dónde se guardan mis datos?

La base de datos se crea automáticamente en:

```
C:\Users\[TuUsuario]\AppData\Local\prueba\inventario.db
```

**IMPORTANTE**: 
- Esta carpeta es privada para cada usuario de Windows
- Haz respaldos periódicos copiando el archivo `inventario.db`

---

## ?? Cómo hacer respaldos

### Método 1: Manual
1. Abre el Explorador de Windows
2. Escribe en la barra de direcciones: `%LOCALAPPDATA%\prueba`
3. Copia el archivo `inventario.db` a una USB o carpeta segura

### Método 2: Desde la App
1. Abre la aplicación
2. Ve a **Configuración**
3. (Próximamente: función de respaldo automático)

---

## ?? Actualización

Cuando haya una nueva versión:

1. Cierra la aplicación actual
2. Descarga la nueva versión
3. Reemplaza los archivos antiguos
4. **Tus datos NO se borrarán** (están en otra carpeta)

---

## ?? Solución de Problemas

### "La aplicación no abre"
- **Causa**: Antivirus bloqueando el archivo
- **Solución**: Agrega una excepción en tu antivirus

### "Error al guardar datos"
- **Causa**: Sin permisos de escritura
- **Solución**: Ejecuta como administrador (clic derecho > Ejecutar como administrador)

### "Se ve muy pequeño en pantalla 4K"
- **Causa**: Escalado de Windows
- **Solución**: Clic derecho en prueba.exe > Propiedades > Compatibilidad > Cambiar configuración elevada de PPP

### "Faltan archivos DLL"
- **Causa**: No se copió toda la carpeta
- **Solución**: Vuelve a descomprimir el ZIP completo

---

## ?? Soporte

Para reportar problemas o sugerencias:
- **Email**: [tu-email@ejemplo.com]
- **Teléfono**: [tu-teléfono]

---

## ?? Guía Rápida de Uso

### Primer Uso - Configuración Inicial

1. **Crear Establecimiento**
   - Abre la app
   - Clic en "Crear Establecimiento"
   - Ingresa nombre y dirección

2. **Crear Áreas**
   - Entra al establecimiento
   - Clic en "Agregar Área"
   - Ejemplos: Almacén, Cocina, Bodega, etc.

3. **Agregar Productos**
   - Entra a un área
   - Clic en "+"
   - Ingresa nombre, stock inicial y unidad de medida

### Uso Diario

#### Registrar Entrada de Stock
1. Selecciona el producto
2. Clic en "Entrada"
3. Ingresa cantidad y motivo
4. Guarda

#### Registrar Salida de Stock
1. Selecciona el producto
2. Clic en "Salida"
3. Ingresa cantidad y motivo
4. Guarda

#### Ver Historial
1. Clic en "Historial" desde cualquier área o producto
2. Exporta a Excel si necesitas

#### Generar Reportes
1. Ve a "Reportes"
2. Selecciona tipo de reporte
3. Exporta a Excel

---

## ?? Personalización

La aplicación se adapta al tema de Windows:
- **Modo claro** en el día
- **Modo oscuro** en la noche (si Windows lo tiene activado)

---

## ?? Seguridad y Privacidad

- ? Todos los datos se guardan **localmente** en tu PC
- ? NO se envía información a internet
- ? NO requiere registro ni cuenta
- ? Tus datos son 100% privados

---

## ?? Capacidad

La aplicación puede manejar:
- ? Múltiples establecimientos (ilimitados)
- ? Áreas por establecimiento (ilimitadas)
- ? Miles de productos
- ? Millones de movimientos de stock

---

## ? Rendimiento

- Inicio rápido: < 3 segundos
- Respuesta inmediata en operaciones
- Base de datos SQLite optimizada
- Sin conexión a internet requerida

---

## ?? Versión

**Versión actual**: 1.0  
**Fecha de lanzamiento**: 2024  
**Plataforma**: Windows 10/11 (64-bit)

---

## ?? Licencia

© 2024 [Tu Empresa]  
Todos los derechos reservados.

Esta aplicación es para uso interno de la empresa.

---

## ? Próximas Características

- [ ] Respaldo automático en la nube
- [ ] Múltiples usuarios con permisos
- [ ] Escaneo de códigos de barras
- [ ] Sincronización entre dispositivos
- [ ] Versión móvil (Android/iOS)
- [ ] Reportes personalizados

---

**¡Gracias por usar Sistema de Inventarios!** ??
