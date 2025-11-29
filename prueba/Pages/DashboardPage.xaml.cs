using prueba.Models;
using prueba.Services;

namespace prueba.Pages
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public DashboardPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarDashboardAsync();
        }

        private async Task CargarDashboardAsync()
        {
            try
            {
                var establecimiento = await _databaseService.GetEstablecimientoAsync();
                if (establecimiento == null) return;

                var totalProductos = await _databaseService.GetTotalProductosAsync();
                var areas = await _databaseService.GetAreasAsync(establecimiento.Id);
                var productosStockBajo = await _databaseService.GetTodosProductosStockBajoAsync();
                var movimientosRecientes = await _databaseService.GetMovimientosRecientesAsync(7);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TotalProductosLabel.Text = $"# Total de Productos: {totalProductos}";
                    TotalAreasLabel.Text = $"= Areas: {areas.Count}";
                    
                    if (productosStockBajo.Count > 0)
                    {
                        ProductosStockBajoLabel.Text = $"! Productos con stock bajo: {productosStockBajo.Count}";
                        ProductosStockBajoLabel.TextColor = Colors.Red;
                        ProductosStockBajoLabel.FontAttributes = FontAttributes.Bold;
                    }
                    else
                    {
                        ProductosStockBajoLabel.Text = "v Todos los productos con stock adecuado";
                        ProductosStockBajoLabel.TextColor = Colors.Green;
                    }
                });

                await MostrarProductosAlertaAsync(productosStockBajo);
                await MostrarMovimientosRecientesAsync(movimientosRecientes);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar dashboard: {ex.Message}", "OK");
            }
        }

        private async Task MostrarProductosAlertaAsync(List<Producto> productos)
        {
            await MainThread.InvokeOnMainThreadAsync(() => ProductosAlertaContainer.Clear());

            if (productos.Count == 0)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ProductosAlertaContainer.Add(new Label
                    {
                        Text = "No hay productos con stock bajo",
                        TextColor = Colors.Gray,
                        HorizontalOptions = LayoutOptions.Center
                    });
                });
                return;
            }

            foreach (var producto in productos.Take(10))
            {
                var area = await _databaseService.GetAreaAsync(producto.AreaId);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var frame = new Frame
                    {
                        BorderColor = Colors.Red,
                        CornerRadius = 8,
                        Padding = 12,
                        HasShadow = false
                    };

                    var stack = new VerticalStackLayout { Spacing = 3 };
                    stack.Add(new Label
                    {
                        Text = $"! {producto.Nombre}",
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.Red
                    });
                    stack.Add(new Label
                    {
                        Text = $"Area: {area?.Nombre} | Stock: {producto.Stock} (min: {producto.StockMinimo})",
                        FontSize = 12,
                        TextColor = Colors.Gray
                    });

                    frame.Content = stack;

                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += async (s, e) => 
                    {
                        await Shell.Current.GoToAsync($"detalle-producto?productoId={producto.Id}");
                    };
                    frame.GestureRecognizers.Add(tapGesture);

                    ProductosAlertaContainer.Add(frame);
                });
            }
        }

        private async Task MostrarMovimientosRecientesAsync(List<MovimientoStock> movimientos)
        {
            await MainThread.InvokeOnMainThreadAsync(() => MovimientosRecientesContainer.Clear());

            if (movimientos.Count == 0)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MovimientosRecientesContainer.Add(new Label
                    {
                        Text = "No hay movimientos recientes",
                        TextColor = Colors.Gray,
                        HorizontalOptions = LayoutOptions.Center
                    });
                });
                return;
            }

            foreach (var movimiento in movimientos.Take(10))
            {
                var producto = await _databaseService.GetProductoAsync(movimiento.ProductoId);
                var colorBorde = movimiento.TipoMovimiento == "Entrada" 
                    ? Color.FromArgb("#51CF66") 
                    : Color.FromArgb("#FF6B6B");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var frame = new Frame
                    {
                        BorderColor = colorBorde,
                        CornerRadius = 8,
                        Padding = 10,
                        HasShadow = false
                    };

                    var stack = new VerticalStackLayout { Spacing = 2 };
                    
                    var icono = movimiento.TipoMovimiento == "Entrada" ? "^" : "v";
                    stack.Add(new Label
                    {
                        Text = $"{icono} {producto?.Nombre}",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold
                    });
                    stack.Add(new Label
                    {
                        Text = $"{movimiento.Motivo} - Cantidad: {movimiento.Cantidad}",
                        FontSize = 12,
                        TextColor = Colors.Gray
                    });
                    stack.Add(new Label
                    {
                        Text = movimiento.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        FontSize = 11,
                        TextColor = Colors.Gray
                    });

                    frame.Content = stack;
                    MovimientosRecientesContainer.Add(frame);
                });
            }
        }
    }
}
