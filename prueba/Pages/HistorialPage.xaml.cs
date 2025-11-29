using prueba.Models;
using prueba.Services;
using System.Text;

namespace prueba.Pages
{
    [QueryProperty(nameof(AreaId), "areaId")]
    [QueryProperty(nameof(ProductoId), "productoId")]
    public partial class HistorialPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private int _areaId;
        private int _productoId;
        private List<MovimientoStock> _movimientos = new();

        public int AreaId
        {
            get => _areaId;
            set
            {
                _areaId = value;
                Task.Run(async () => await CargarHistorialAreaAsync());
            }
        }

        public int ProductoId
        {
            get => _productoId;
            set
            {
                _productoId = value;
                Task.Run(async () => await CargarHistorialProductoAsync());
            }
        }

        public HistorialPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        private async Task CargarHistorialAreaAsync()
        {
            try
            {
                var area = await _databaseService.GetAreaAsync(_areaId);
                if (area != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        TituloLabel.Text = $"Historial - {area.Nombre}";
                    });

                    _movimientos = await _databaseService.GetMovimientosAreaAsync(_areaId);
                    await MostrarHistorial();
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Error", $"Error al cargar: {ex.Message}", "OK");
                });
            }
        }

        private async Task CargarHistorialProductoAsync()
        {
            try
            {
                var producto = await _databaseService.GetProductoAsync(_productoId);
                if (producto != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        TituloLabel.Text = $"Historial - {producto.Nombre}";
                    });

                    _movimientos = await _databaseService.GetMovimientosProductoAsync(_productoId);
                    await MostrarHistorial();
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Error", $"Error al cargar: {ex.Message}", "OK");
                });
            }
        }

        private async Task MostrarHistorial()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                HistorialContainer.Clear();
                SubtituloLabel.Text = $"{_movimientos.Count} movimientos registrados";
            });

            if (_movimientos.Count == 0)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    HistorialContainer.Add(new Label
                    {
                        Text = "No hay movimientos registrados",
                        HorizontalOptions = LayoutOptions.Center,
                        TextColor = Colors.Gray,
                        Margin = new Thickness(0, 40, 0, 0)
                    });
                });
                return;
            }

            foreach (var movimiento in _movimientos)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var frame = await CrearMovimientoFrameAsync(movimiento);
                    HistorialContainer.Add(frame);
                });
            }
        }

        private async Task<Frame> CrearMovimientoFrameAsync(MovimientoStock movimiento)
        {
            var producto = await _databaseService.GetProductoAsync(movimiento.ProductoId);
            var colorBorde = movimiento.TipoMovimiento == "Entrada" 
                ? Color.FromArgb("#51CF66") 
                : Color.FromArgb("#FF6B6B");

            var frame = new Frame
            {
                BorderColor = colorBorde,
                CornerRadius = 8,
                Padding = 12,
                HasShadow = false
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var infoStack = new VerticalStackLayout { Spacing = 3 };

            if (_areaId > 0 && producto != null)
            {
                infoStack.Add(new Label
                {
                    Text = producto.Nombre,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold
                });
            }

            var iconoTipo = movimiento.TipoMovimiento == "Entrada" ? "^" : "v";
            infoStack.Add(new Label
            {
                Text = $"{iconoTipo} {movimiento.TipoMovimiento} - {movimiento.Motivo}",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            });

            infoStack.Add(new Label
            {
                Text = $"Cantidad: {movimiento.Cantidad} | {movimiento.StockAnterior} > {movimiento.StockNuevo}",
                FontSize = 12,
                TextColor = Colors.Gray
            });

            if (!string.IsNullOrEmpty(movimiento.Notas))
            {
                infoStack.Add(new Label
                {
                    Text = $"* {movimiento.Notas}",
                    FontSize = 11,
                    TextColor = Colors.Gray,
                    FontAttributes = FontAttributes.Italic
                });
            }

            infoStack.Add(new Label
            {
                Text = movimiento.Fecha.ToString("dd/MM/yyyy HH:mm"),
                FontSize = 11,
                TextColor = Colors.Gray
            });

            Grid.SetColumn(infoStack, 0);
            grid.Add(infoStack);

            frame.Content = grid;
            return frame;
        }

        private async void OnExportarClicked(object? sender, EventArgs e)
        {
            try
            {
                if (_movimientos.Count == 0)
                {
                    await DisplayAlert("Aviso", "No hay movimientos para exportar", "OK");
                    return;
                }

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Historial");

                // Encabezado
                worksheet.Cell(1, 1).Value = "HISTORIAL DE MOVIMIENTOS";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                
                worksheet.Cell(2, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Cabeceras de tabla
                int row = 4;
                worksheet.Cell(row, 1).Value = "Fecha";
                worksheet.Cell(row, 2).Value = "Producto";
                worksheet.Cell(row, 3).Value = "Tipo";
                worksheet.Cell(row, 4).Value = "Motivo";
                worksheet.Cell(row, 5).Value = "Cantidad";
                worksheet.Cell(row, 6).Value = "Stock Anterior";
                worksheet.Cell(row, 7).Value = "Stock Nuevo";
                worksheet.Cell(row, 8).Value = "Notas";
                
                // Estilo de cabecera
                var headerRange = worksheet.Range(row, 1, row, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thick;

                row++;
                foreach (var mov in _movimientos)
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

                    // Color según tipo
                    if (mov.TipoMovimiento == "Entrada")
                    {
                        worksheet.Range(row, 1, row, 8).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
                    }
                    else
                    {
                        worksheet.Range(row, 1, row, 8).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightYellow;
                    }

                    row++;
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                // Guardar y compartir
                var fileName = $"historial_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                
                workbook.SaveAs(filePath);

                // Compartir el archivo
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Compartir Historial Excel",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al exportar: {ex.Message}", "OK");
            }
        }
    }
}
