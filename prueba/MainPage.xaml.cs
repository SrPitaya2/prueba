using prueba.Services;
using System.Diagnostics;

namespace prueba
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public MainPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try
            {
                await VerificarConfiguracionAsync();
                await MostrarAlertasAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR OnAppearing: {ex.GetType().Name} - {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }

                await DisplayAlert("Error de Inicializacion", 
                    $"Tipo: {ex.GetType().Name}\nMensaje: {ex.Message}\n\nRevisa la ventana de salida para mas detalles.", 
                    "OK");
            }
        }

        private async Task VerificarConfiguracionAsync()
        {
            try
            {
                Debug.WriteLine("Iniciando verificacion de configuracion...");
                var establecimiento = await _databaseService.GetEstablecimientoAsync();
                Debug.WriteLine($"Establecimiento obtenido: {establecimiento?.Nombre ?? "null"}");
                
                if (establecimiento == null)
                {
                    Debug.WriteLine("Navegando a configuracion...");
                    await Shell.Current.GoToAsync("//configuracion");
                }
                else
                {
                    EstablecimientoLabel.Text = establecimiento.Nombre;
                    Debug.WriteLine("Configuracion OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR VerificarConfiguracionAsync: {ex.Message}");
                throw;
            }
        }

        private async Task MostrarAlertasAsync()
        {
            try
            {
                Debug.WriteLine("Iniciando mostrar alertas...");
                var productosStockBajo = await _databaseService.GetTodosProductosStockBajoAsync();
                Debug.WriteLine($"Productos con stock bajo: {productosStockBajo.Count}");
                
                if (productosStockBajo.Count > 0)
                {
                    AlertasLabel.Text = $"ALERTA: {productosStockBajo.Count} producto(s) con stock bajo";
                    AlertasLabel.TextColor = Colors.Red;
                    AlertasLabel.FontAttributes = FontAttributes.Bold;
                }
                else
                {
                    AlertasLabel.Text = "OK - Inventario en buen estado";
                    AlertasLabel.TextColor = Colors.Green;
                }
                Debug.WriteLine("Alertas mostradas OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR MostrarAlertasAsync: {ex.Message}");
                AlertasLabel.Text = "";
            }
        }

        private async void OnDashboardClicked(object? sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//dashboard");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR OnDashboardClicked: {ex.Message}");
                await DisplayAlert("Error", $"No se pudo navegar: {ex.Message}", "OK");
            }
        }

        private async void OnAreasClicked(object? sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//areas");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR OnAreasClicked: {ex.Message}");
                await DisplayAlert("Error", $"No se pudo navegar: {ex.Message}", "OK");
            }
        }

        private async void OnReportesClicked(object? sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("reportes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR OnReportesClicked: {ex.Message}");
                await DisplayAlert("Error", $"No se pudo navegar: {ex.Message}", "OK");
            }
        }
    }
}
