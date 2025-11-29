using prueba.Models;
using prueba.Services;

namespace prueba.Pages
{
    [QueryProperty(nameof(ProductoId), "productoId")]
    public partial class DetalleProductoPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private int _productoId;
        private Producto? _producto;

        public int ProductoId
        {
            get => _productoId;
            set
            {
                _productoId = value;
                Task.Run(async () => await CargarProductoAsync());
            }
        }

        public DetalleProductoPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        private async Task CargarProductoAsync()
        {
            try
            {
                _producto = await _databaseService.GetProductoAsync(_productoId);
                if (_producto != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        NombreEntry.Text = _producto.Nombre;
                        CategoriaEntry.Text = _producto.Categoria;
                        UnidadMedidaEntry.Text = _producto.UnidadMedida ?? "Unidades";
                        StockMinimoEntry.Text = _producto.StockMinimo.ToString();
                        NotasEditor.Text = _producto.Notas;
                        StockLabel.Text = _producto.Stock.ToString();
                        UnidadLabel.Text = _producto.UnidadMedida ?? "Unidades";
                        UltimaActualizacionLabel.Text = $"Actualizado: {_producto.FechaActualizacion:dd/MM/yyyy HH:mm}";

                        if (_producto.StockBajo)
                        {
                            StockLabel.TextColor = Colors.Red;
                        }
                    });
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

        private async void OnGuardarClicked(object? sender, EventArgs e)
        {
            try
            {
                if (_producto == null) return;

                if (string.IsNullOrWhiteSpace(NombreEntry.Text))
                {
                    await DisplayAlert("Validación", "El nombre es obligatorio", "OK");
                    return;
                }

                if (!int.TryParse(StockMinimoEntry.Text, out int stockMinimo) || stockMinimo < 0)
                {
                    await DisplayAlert("Validación", "El stock mínimo debe ser un número mayor o igual a 0", "OK");
                    return;
                }

                _producto.Nombre = NombreEntry.Text.Trim();
                _producto.Categoria = CategoriaEntry.Text?.Trim();
                _producto.UnidadMedida = UnidadMedidaEntry.Text?.Trim() ?? "Unidades";
                _producto.StockMinimo = stockMinimo;
                _producto.Notas = NotasEditor.Text?.Trim();

                await _databaseService.SaveProductoAsync(_producto);
                await DisplayAlert("Éxito", "Producto actualizado correctamente", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
            }
        }

        private async void OnHistorialClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"historial?productoId={_productoId}");
        }
    }
}
