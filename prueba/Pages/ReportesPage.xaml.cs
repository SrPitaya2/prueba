using prueba.Services;
using prueba.Models;
using ClosedXML.Excel;

namespace prueba.Pages
{
    public partial class ReportesPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public ReportesPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        private async void OnInventarioActualClicked(object? sender, EventArgs e)
        {
            try
            {
                var establecimiento = await _databaseService.GetEstablecimientoAsync();
                if (establecimiento == null)
                {
                    await DisplayAlert("Error", "No hay establecimiento configurado", "OK");
                    return;
                }

                var areas = await _databaseService.GetAreasAsync(establecimiento.Id);
                
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Inventario");

                // Encabezado
                worksheet.Cell(1, 1).Value = "REPORTE DE INVENTARIO ACTUAL";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                
                worksheet.Cell(2, 1).Value = $"Establecimiento: {establecimiento.Nombre}";
                worksheet.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Cabeceras de tabla
                int row = 5;
                worksheet.Cell(row, 1).Value = "Area";
                worksheet.Cell(row, 2).Value = "Producto";
                worksheet.Cell(row, 3).Value = "Stock";
                worksheet.Cell(row, 4).Value = "Unidad";
                worksheet.Cell(row, 5).Value = "Stock Minimo";
                worksheet.Cell(row, 6).Value = "Categoria";
                worksheet.Cell(row, 7).Value = "Notas";
                
                // Estilo de cabecera
                var headerRange = worksheet.Range(row, 1, row, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

                row++;
                int totalProductos = 0;

                foreach (var area in areas)
                {
                    var productos = await _databaseService.GetProductosAsync(area.Id);
                    totalProductos += productos.Count;

                    foreach (var prod in productos)
                    {
                        worksheet.Cell(row, 1).Value = area.Nombre;
                        worksheet.Cell(row, 2).Value = prod.Nombre;
                        worksheet.Cell(row, 3).Value = prod.Stock;
                        worksheet.Cell(row, 4).Value = prod.UnidadMedida ?? "Unidades";
                        worksheet.Cell(row, 5).Value = prod.StockMinimo;
                        worksheet.Cell(row, 6).Value = prod.Categoria ?? "";
                        worksheet.Cell(row, 7).Value = prod.Notas ?? "";

                        // Resaltar productos con stock bajo
                        if (prod.StockBajo)
                        {
                            worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.LightPink;
                        }

                        row++;
                    }
                }

                // Resumen
                row++;
                worksheet.Cell(row, 1).Value = "RESUMEN";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                row++;
                worksheet.Cell(row, 1).Value = $"Total de productos: {totalProductos}";
                worksheet.Cell(row + 1, 1).Value = $"Total de areas: {areas.Count}";

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                await CompartirExcel(workbook, "inventario");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al generar reporte: {ex.Message}", "OK");
            }
        }

        private async void OnStockBajoClicked(object? sender, EventArgs e)
        {
            try
            {
                var establecimiento = await _databaseService.GetEstablecimientoAsync();
                if (establecimiento == null) return;

                var productosStockBajo = await _databaseService.GetTodosProductosStockBajoAsync();
                
                if (productosStockBajo.Count == 0)
                {
                    await DisplayAlert("Aviso", "No hay productos con stock bajo", "OK");
                    return;
                }

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Stock Bajo");

                // Encabezado
                worksheet.Cell(1, 1).Value = "PRODUCTOS CON STOCK BAJO";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.Red;
                
                worksheet.Cell(2, 1).Value = $"Establecimiento: {establecimiento.Nombre}";
                worksheet.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Cabeceras
                int row = 5;
                worksheet.Cell(row, 1).Value = "Area";
                worksheet.Cell(row, 2).Value = "Producto";
                worksheet.Cell(row, 3).Value = "Stock Actual";
                worksheet.Cell(row, 4).Value = "Unidad";
                worksheet.Cell(row, 5).Value = "Stock Minimo";
                worksheet.Cell(row, 6).Value = "Faltante";
                
                var headerRange = worksheet.Range(row, 1, row, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.Red;
                headerRange.Style.Font.FontColor = XLColor.White;

                row++;
                foreach (var prod in productosStockBajo)
                {
                    var area = await _databaseService.GetAreaAsync(prod.AreaId);
                    int diferencia = prod.StockMinimo - prod.Stock;
                    
                    worksheet.Cell(row, 1).Value = area?.Nombre;
                    worksheet.Cell(row, 2).Value = prod.Nombre;
                    worksheet.Cell(row, 3).Value = prod.Stock;
                    worksheet.Cell(row, 4).Value = prod.UnidadMedida ?? "Unidades";
                    worksheet.Cell(row, 5).Value = prod.StockMinimo;
                    worksheet.Cell(row, 6).Value = diferencia;
                    
                    worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.LightPink;
                    row++;
                }

                row++;
                worksheet.Cell(row, 1).Value = $"Total: {productosStockBajo.Count} productos";
                worksheet.Cell(row, 1).Style.Font.Bold = true;

                worksheet.Columns().AdjustToContents();
                await CompartirExcel(workbook, "stock_bajo");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al generar reporte: {ex.Message}", "OK");
            }
        }

        private async void OnMovimientosClicked(object? sender, EventArgs e)
        {
            try
            {
                var movimientos = await _databaseService.GetMovimientosRecientesAsync(30);
                
                if (movimientos.Count == 0)
                {
                    await DisplayAlert("Aviso", "No hay movimientos registrados", "OK");
                    return;
                }

                var establecimiento = await _databaseService.GetEstablecimientoAsync();
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Movimientos");

                // Encabezado
                worksheet.Cell(1, 1).Value = "HISTORIAL DE MOVIMIENTOS (30 DIAS)";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                
                worksheet.Cell(2, 1).Value = $"Establecimiento: {establecimiento?.Nombre}";
                worksheet.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Cabeceras
                int row = 5;
                worksheet.Cell(row, 1).Value = "Fecha";
                worksheet.Cell(row, 2).Value = "Producto";
                worksheet.Cell(row, 3).Value = "Tipo";
                worksheet.Cell(row, 4).Value = "Motivo";
                worksheet.Cell(row, 5).Value = "Cantidad";
                worksheet.Cell(row, 6).Value = "Stock Anterior";
                worksheet.Cell(row, 7).Value = "Stock Nuevo";
                worksheet.Cell(row, 8).Value = "Notas";
                
                var headerRange = worksheet.Range(row, 1, row, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                row++;
                foreach (var mov in movimientos)
                {
                    var producto = await _databaseService.GetProductoAsync(mov.ProductoId);
                    
                    worksheet.Cell(row, 1).Value = mov.Fecha.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 2).Value = producto?.Nombre;
                    worksheet.Cell(row, 3).Value = mov.TipoMovimiento;
                    worksheet.Cell(row, 4).Value = mov.Motivo;
                    worksheet.Cell(row, 5).Value = mov.Cantidad;
                    worksheet.Cell(row, 6).Value = mov.StockAnterior;
                    worksheet.Cell(row, 7).Value = mov.StockNuevo;
                    worksheet.Cell(row, 8).Value = mov.Notas ?? "";

                    // Color según tipo de movimiento
                    if (mov.TipoMovimiento == "Entrada")
                    {
                        worksheet.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    }
                    else
                    {
                        worksheet.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;
                    }

                    row++;
                }

                row++;
                worksheet.Cell(row, 1).Value = $"Total: {movimientos.Count} movimientos";
                worksheet.Cell(row, 1).Style.Font.Bold = true;

                worksheet.Columns().AdjustToContents();
                await CompartirExcel(workbook, "movimientos");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al generar reporte: {ex.Message}", "OK");
            }
        }

        private async void OnReporteCompletoClicked(object? sender, EventArgs e)
        {
            try
            {
                var establecimiento = await _databaseService.GetEstablecimientoAsync();
                if (establecimiento == null) return;

                using var workbook = new XLWorkbook();

                // Hoja 1: Inventario
                var wsInventario = workbook.Worksheets.Add("Inventario");
                await GenerarHojaInventario(wsInventario, establecimiento);

                // Hoja 2: Stock Bajo
                var wsStockBajo = workbook.Worksheets.Add("Stock Bajo");
                await GenerarHojaStockBajo(wsStockBajo, establecimiento);

                // Hoja 3: Movimientos
                var wsMovimientos = workbook.Worksheets.Add("Movimientos");
                await GenerarHojaMovimientos(wsMovimientos);

                // Hoja 4: Resumen
                var wsResumen = workbook.Worksheets.Add("Resumen");
                await GenerarHojaResumen(wsResumen, establecimiento);

                await CompartirExcel(workbook, "completo");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al generar reporte: {ex.Message}", "OK");
            }
        }

        private async Task GenerarHojaInventario(IXLWorksheet ws, Establecimiento establecimiento)
        {
            ws.Cell(1, 1).Value = "INVENTARIO ACTUAL";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;

            int row = 3;
            ws.Cell(row, 1).Value = "Area";
            ws.Cell(row, 2).Value = "Producto";
            ws.Cell(row, 3).Value = "Stock";
            ws.Cell(row, 4).Value = "Unidad";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightBlue;

            row++;
            var areas = await _databaseService.GetAreasAsync(establecimiento.Id);
            foreach (var area in areas)
            {
                var productos = await _databaseService.GetProductosAsync(area.Id);
                foreach (var prod in productos)
                {
                    ws.Cell(row, 1).Value = area.Nombre;
                    ws.Cell(row, 2).Value = prod.Nombre;
                    ws.Cell(row, 3).Value = prod.Stock;
                    ws.Cell(row, 4).Value = prod.UnidadMedida ?? "Unidades";
                    if (prod.StockBajo) ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightPink;
                    row++;
                }
            }
            ws.Columns().AdjustToContents();
        }

        private async Task GenerarHojaStockBajo(IXLWorksheet ws, Establecimiento establecimiento)
        {
            ws.Cell(1, 1).Value = "PRODUCTOS CON STOCK BAJO";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Font.FontColor = XLColor.Red;

            int row = 3;
            ws.Cell(row, 1).Value = "Area";
            ws.Cell(row, 2).Value = "Producto";
            ws.Cell(row, 3).Value = "Stock";
            ws.Cell(row, 4).Value = "Minimo";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.Red;
            ws.Range(row, 1, row, 4).Style.Font.FontColor = XLColor.White;

            row++;
            var productos = await _databaseService.GetTodosProductosStockBajoAsync();
            foreach (var prod in productos)
            {
                var area = await _databaseService.GetAreaAsync(prod.AreaId);
                ws.Cell(row, 1).Value = area?.Nombre;
                ws.Cell(row, 2).Value = prod.Nombre;
                ws.Cell(row, 3).Value = prod.Stock;
                ws.Cell(row, 4).Value = prod.StockMinimo;
                ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightPink;
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private async Task GenerarHojaMovimientos(IXLWorksheet ws)
        {
            ws.Cell(1, 1).Value = "MOVIMIENTOS RECIENTES (7 DIAS)";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;

            int row = 3;
            ws.Cell(row, 1).Value = "Fecha";
            ws.Cell(row, 2).Value = "Producto";
            ws.Cell(row, 3).Value = "Tipo";
            ws.Cell(row, 4).Value = "Motivo";
            ws.Cell(row, 5).Value = "Cantidad";
            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightBlue;

            row++;
            var movimientos = await _databaseService.GetMovimientosRecientesAsync(7);
            foreach (var mov in movimientos.Take(50))
            {
                var producto = await _databaseService.GetProductoAsync(mov.ProductoId);
                ws.Cell(row, 1).Value = mov.Fecha.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(row, 2).Value = producto?.Nombre;
                ws.Cell(row, 3).Value = mov.TipoMovimiento;
                ws.Cell(row, 4).Value = mov.Motivo;
                ws.Cell(row, 5).Value = mov.Cantidad;
                
                if (mov.TipoMovimiento == "Entrada")
                    ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightGreen;
                else
                    ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightYellow;
                
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private async Task GenerarHojaResumen(IXLWorksheet ws, Establecimiento establecimiento)
        {
            ws.Cell(1, 1).Value = "RESUMEN EJECUTIVO";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;

            ws.Cell(3, 1).Value = "Establecimiento:";
            ws.Cell(3, 2).Value = establecimiento.Nombre;
            ws.Cell(4, 1).Value = "Fecha de reporte:";
            ws.Cell(4, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            var areas = await _databaseService.GetAreasAsync(establecimiento.Id);
            var totalProductos = await _databaseService.GetTotalProductosAsync();
            var stockBajo = await _databaseService.GetTodosProductosStockBajoAsync();
            var movimientos = await _databaseService.GetMovimientosRecientesAsync(7);

            int row = 6;
            ws.Cell(row, 1).Value = "Total de Areas:";
            ws.Cell(row, 2).Value = areas.Count;
            row++;
            ws.Cell(row, 1).Value = "Total de Productos:";
            ws.Cell(row, 2).Value = totalProductos;
            row++;
            ws.Cell(row, 1).Value = "Productos con Stock Bajo:";
            ws.Cell(row, 2).Value = stockBajo.Count;
            ws.Cell(row, 2).Style.Font.FontColor = stockBajo.Count > 0 ? XLColor.Red : XLColor.Green;
            row++;
            ws.Cell(row, 1).Value = "Movimientos (7 dias):";
            ws.Cell(row, 2).Value = movimientos.Count;

            ws.Range(1, 1, row, 2).Style.Font.FontSize = 12;
            ws.Columns().AdjustToContents();
        }

        private async Task CompartirExcel(XLWorkbook workbook, string tipo)
        {
            try
            {
                var fileName = $"reporte_{tipo}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                
                workbook.SaveAs(filePath);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Compartir Reporte Excel",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al compartir: {ex.Message}", "OK");
            }
        }
    }
}
