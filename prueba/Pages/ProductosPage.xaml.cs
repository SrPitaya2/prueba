using prueba.Models;
using prueba.Services;
using prueba.Helpers;

namespace prueba.Pages
{
    [QueryProperty(nameof(AreaId), "areaId")]
    public partial class ProductosPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private int _areaId;
        private Area? _area;
        private bool _isInitialized = false;
        private List<Producto> _todosLosProductos = new();

        public int AreaId
        {
            get => _areaId;
            set
            {
                _areaId = value;
                _isInitialized = false;
                Task.Run(async () => await CargarDatosAsync());
            }
        }

        public ProductosPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (_isInitialized)
            {
                await CargarProductosAsync();
            }
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                _area = await _databaseService.GetAreaAsync(_areaId);
                if (_area != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        AreaLabel.Text = _area.Nombre;
                        await ActualizarEstadisticasAsync();
                    });
                    await CargarProductosAsync();
                    _isInitialized = true;
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

        private async Task ActualizarEstadisticasAsync()
        {
            var total = await _databaseService.GetTotalProductosAreaAsync(_areaId);
            var stockBajo = await _databaseService.GetProductosStockBajoAsync(_areaId);
            
            string estadisticas = $"{total} productos";
            if (stockBajo.Count > 0)
            {
                estadisticas += $" - ALERTA: {stockBajo.Count} con stock bajo";
            }
            EstadisticasLabel.Text = estadisticas;
        }

        private async void OnBusquedaTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                await MostrarProductos(_todosLosProductos);
            }
            else
            {
                var filtrados = await _databaseService.BuscarProductosAsync(_areaId, e.NewTextValue);
                await MostrarProductos(filtrados);
            }
        }

        private async void OnHistorialClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"historial?areaId={_areaId}");
        }

        private async Task CargarProductosAsync()
        {
            if (_areaId == 0) return;

            _todosLosProductos = await _databaseService.GetProductosAsync(_areaId);
            await MostrarProductos(_todosLosProductos);
            await ActualizarEstadisticasAsync();
        }

        private async Task MostrarProductos(List<Producto> productos)
        {
            await MainThread.InvokeOnMainThreadAsync(() => ProductosContainer.Clear());

            if (productos.Count == 0)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ProductosContainer.Add(new Label
                    {
                        Text = "No hay productos. Agrega uno nuevo.",
                        HorizontalOptions = LayoutOptions.Center,
                        TextColor = Colors.Gray,
                        Margin = new Thickness(0, 40, 0, 0)
                    });
                });
            }
            else
            {
                foreach (var producto in productos)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        var frame = CrearProductoFrame(producto);
                        ProductosContainer.Add(frame);
                    });
                }
            }
        }

        private Frame CrearProductoFrame(Producto producto)
        {
            var borderColor = producto.StockBajo ? Colors.Red : Color.FromArgb("#512BD4");
            
            var frame = new Frame
            {
                BorderColor = borderColor,
                CornerRadius = 10,
                Padding = 15,
                HasShadow = true
            };

            var mainGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                RowSpacing = 15
            };

            // Fila 1: Nombre, alerta y botones
            var headerGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 10
            };

            var nombreStack = new VerticalStackLayout { Spacing = 5 };
            nombreStack.Add(new Label
            {
                Text = producto.Nombre,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold
            });

            if (!string.IsNullOrEmpty(producto.Categoria))
            {
                nombreStack.Add(new Label
                {
                    Text = $"Categoria: {producto.Categoria}",
                    FontSize = 12,
                    TextColor = Colors.Gray
                });
            }

            if (producto.StockBajo)
            {
                nombreStack.Add(new Label
                {
                    Text = $"ALERTA: Stock bajo (minimo: {producto.StockMinimo})",
                    FontSize = 12,
                    TextColor = Colors.Red,
                    FontAttributes = FontAttributes.Bold
                });
            }

            // Botón VER con símbolo Unicode
            var verBtn = new Button
            {
                Text = "i",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.FromArgb("#4A90E2"),
                TextColor = Colors.White,
                WidthRequest = 50,
                HeightRequest = 50
            };
            verBtn.Clicked += async (s, e) => await OnVerDetalleClicked(producto);

            // Botón ELIMINAR con símbolo Unicode
            var eliminarBtn = new Button
            {
                Text = "?",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Colors.Red,
                TextColor = Colors.White,
                WidthRequest = 50,
                HeightRequest = 50
            };
            eliminarBtn.Clicked += async (s, e) => await OnEliminarProductoClicked(producto);

            Grid.SetColumn(nombreStack, 0);
            Grid.SetColumn(verBtn, 1);
            Grid.SetColumn(eliminarBtn, 2);

            headerGrid.Add(nombreStack);
            headerGrid.Add(verBtn);
            headerGrid.Add(eliminarBtn);

            // Fila 2: Controles de stock
            var stockGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 10
            };

            var disminuirBtn = new Button
            {
                Text = "-",
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.FromArgb("#FF6B6B"),
                TextColor = Colors.White,
                WidthRequest = 60,
                HeightRequest = 60,
                CornerRadius = 30
            };
            disminuirBtn.Clicked += async (s, e) => await OnCambiarStockClickedConMotivo(producto, -1);

            var stockStack = new VerticalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var stockLabel = new Label
            {
                Text = $"Stock ({producto.UnidadMedida ?? "Unidades"})",
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Gray
            };

            var stockEntry = new Entry
            {
                Text = producto.Stock.ToString(),
                FontSize = 32,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                WidthRequest = 120,
                Keyboard = Keyboard.Numeric
            };
            stockEntry.Completed += async (s, e) => await OnStockManualChanged(producto, stockEntry);

            stockStack.Add(stockLabel);
            stockStack.Add(stockEntry);

            var aumentarBtn = new Button
            {
                Text = "+",
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.FromArgb("#51CF66"),
                TextColor = Colors.White,
                WidthRequest = 60,
                HeightRequest = 60,
                CornerRadius = 30
            };
            aumentarBtn.Clicked += async (s, e) => await OnCambiarStockClickedConMotivo(producto, 1);

            Grid.SetColumn(disminuirBtn, 0);
            Grid.SetColumn(stockStack, 1);
            Grid.SetColumn(aumentarBtn, 2);

            stockGrid.Add(disminuirBtn);
            stockGrid.Add(stockStack);
            stockGrid.Add(aumentarBtn);

            Grid.SetRow(headerGrid, 0);
            Grid.SetRow(stockGrid, 1);

            mainGrid.Add(headerGrid);
            mainGrid.Add(stockGrid);

            frame.Content = mainGrid;
            return frame;
        }

        private async Task OnStockManualChanged(Producto producto, Entry stockEntry)
        {
            if (int.TryParse(stockEntry.Text, out int nuevoStock))
            {
                if (nuevoStock >= 0)
                {
                    if (nuevoStock != producto.Stock)
                    {
                        var motivo = await SeleccionarMotivoAsync(nuevoStock > producto.Stock);
                        if (!string.IsNullOrEmpty(motivo) && motivo != "Cancelar")
                        {
                            var tipoMovimiento = nuevoStock > producto.Stock ? "Entrada" : "Salida";
                            await _databaseService.ActualizarStockAsync(producto.Id, nuevoStock, tipoMovimiento, motivo);
                            await CargarProductosAsync();
                        }
                        else
                        {
                            // Usuario canceló, revertir el valor
                            stockEntry.Text = producto.Stock.ToString();
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error", "El stock no puede ser negativo", "OK");
                    stockEntry.Text = producto.Stock.ToString();
                }
            }
            else
            {
                stockEntry.Text = producto.Stock.ToString();
            }
        }

        private async Task OnCambiarStockClickedConMotivo(Producto producto, int cambio)
        {
            var nuevoStock = producto.Stock + cambio;
            if (nuevoStock < 0)
            {
                await DisplayAlert("Aviso", "El stock no puede ser negativo", "OK");
                return;
            }

            var motivo = await SeleccionarMotivoAsync(cambio > 0);
            if (!string.IsNullOrEmpty(motivo) && motivo != "Cancelar")
            {
                var tipoMovimiento = cambio > 0 ? "Entrada" : "Salida";
                await _databaseService.ActualizarStockAsync(producto.Id, nuevoStock, tipoMovimiento, motivo);
                await CargarProductosAsync();
            }
            // Si cancela, no hace nada (el stock queda como estaba)
        }

        private async Task<string?> SeleccionarMotivoAsync(bool esEntrada)
        {
            string[] motivosEntrada = { "Compra", "Devolucion", "Ajuste inventario", "Produccion", "Otro" };
            string[] motivosSalida = { "Venta", "Dano", "Perdida", "Consumo interno", "Merma", "Otro" };

            var motivos = esEntrada ? motivosEntrada : motivosSalida;
            var titulo = esEntrada ? "Motivo de Entrada" : "Motivo de Salida";

            return await DisplayActionSheet(titulo, "Cancelar", null, motivos);
        }

        private async Task OnVerDetalleClicked(Producto producto)
        {
            await Shell.Current.GoToAsync($"detalle-producto?productoId={producto.Id}");
        }

        private async void OnAgregarProductoClicked(object? sender, EventArgs e)
        {
            if (_areaId == 0) return;

            string? nombre = await DisplayPromptAsync("Nuevo Producto", "Nombre del producto:",
                placeholder: "Ej: Producto A, Material B...");

            if (string.IsNullOrWhiteSpace(nombre))
                return;

            // Selector de unidad de medida
            string[] unidades = { "Unidades", "Kilogramos", "Gramos", "Litros", "Mililitros", "Metros", "Cajas", "Paquetes", "Piezas" };
            var unidadSeleccionada = await DisplayActionSheet("Selecciona la unidad de medida", "Cancelar", null, unidades);
            
            if (unidadSeleccionada == "Cancelar" || string.IsNullOrEmpty(unidadSeleccionada))
                unidadSeleccionada = "Unidades";

            string? stockStr = await DisplayPromptAsync("Stock Inicial", $"Cantidad inicial en {unidadSeleccionada}:",
                placeholder: "0", keyboard: Keyboard.Numeric);

            if (!int.TryParse(stockStr, out int stock))
                stock = 0;

            if (stock < 0)
            {
                await DisplayAlert("Error", "El stock no puede ser negativo", "OK");
                return;
            }

            string? stockMinimoStr = await DisplayPromptAsync("Stock Minimo (opcional)", 
                $"Alertar cuando sea menor a (en {unidadSeleccionada}):",
                placeholder: "0", keyboard: Keyboard.Numeric);

            if (!int.TryParse(stockMinimoStr, out int stockMinimo))
                stockMinimo = 0;

            var producto = new Producto
            {
                AreaId = _areaId,
                Nombre = nombre.Trim(),
                Stock = stock,
                StockMinimo = stockMinimo,
                UnidadMedida = unidadSeleccionada
            };

            await _databaseService.SaveProductoAsync(producto);
            
            if (stock > 0)
            {
                await _databaseService.ActualizarStockAsync(producto.Id, stock, "Entrada", "Stock inicial");
            }

            await CargarProductosAsync();
        }

        private async Task OnEliminarProductoClicked(Producto producto)
        {
            bool confirmar = await DisplayAlert("Confirmar",
                $"Eliminar el producto '{producto.Nombre}'?",
                "Eliminar", "Cancelar");

            if (confirmar)
            {
                await _databaseService.DeleteProductoAsync(producto);
                await CargarProductosAsync();
            }
        }
    }
}
