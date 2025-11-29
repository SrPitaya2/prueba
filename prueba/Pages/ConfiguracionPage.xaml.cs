using prueba.Models;
using prueba.Services;

namespace prueba.Pages
{
    public partial class ConfiguracionPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Establecimiento? _establecimientoActual;

        public ConfiguracionPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarEstablecimientoAsync();
        }

        private async Task CargarEstablecimientoAsync()
        {
            try
            {
                _establecimientoActual = await _databaseService.GetEstablecimientoAsync();
                
                if (_establecimientoActual != null)
                {
                    NombreEntry.Text = _establecimientoActual.Nombre;
                    TituloLabel.Text = "Editar Establecimiento";
                    SubtituloLabel.Text = "Modifica el nombre de tu establecimiento";
                    GuardarBtn.Text = "Guardar Cambios";
                }
                else
                {
                    TituloLabel.Text = "Bienvenido a Inventarios";
                    SubtituloLabel.Text = "Configura tu establecimiento";
                    GuardarBtn.Text = "Comenzar";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar: {ex.Message}", "OK");
            }
        }

        private async void OnGuardarClicked(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NombreEntry.Text))
                {
                    await DisplayAlert("Validacion", "El nombre del establecimiento es obligatorio", "OK");
                    return;
                }

                var establecimiento = new Establecimiento
                {
                    Nombre = NombreEntry.Text.Trim()
                };

                await _databaseService.SaveEstablecimientoAsync(establecimiento);

                string mensaje = _establecimientoActual != null 
                    ? "Cambios guardados correctamente" 
                    : "Establecimiento creado correctamente";
                
                await DisplayAlert("Exito", mensaje, "OK");
                await Shell.Current.GoToAsync("//main");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
            }
        }
    }
}
