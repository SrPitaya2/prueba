using prueba.Models;
using prueba.Services;
using System.Diagnostics;

namespace prueba.Pages
{
    public partial class ConfiguracionPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Establecimiento? _establecimientoActual;
        private string? _tempLogoPath;

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
                    SubtituloLabel.Text = "Modifica los datos de tu establecimiento";
                    GuardarBtn.Text = "Guardar Cambios";

                    if (!string.IsNullOrEmpty(_establecimientoActual.LogoPath))
                    {
                        LogoPreview.Source = ImageSource.FromFile(_establecimientoActual.LogoPath);
                    }
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

        private async void OnSeleccionarLogoClicked(object? sender, EventArgs e)
        {
            try
            {
                var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecciona un logo PNG"
                });

                if (result != null)
                {
                    // Validar extensin (opcional, el MediaPicker ya ayuda)
                    if (!result.FileName.ToLower().EndsWith(".png") && !result.FileName.ToLower().EndsWith(".jpg") && !result.FileName.ToLower().EndsWith(".jpeg"))
                    {
                        await DisplayAlert("Aviso", "Se recomienda usar un formato PNG", "OK");
                    }

                    // Guardar en directorio local para persistencia
                    var localPath = Path.Combine(FileSystem.AppDataDirectory, $"logo_{DateTime.Now.Ticks}_{result.FileName}");
                    
                    using (var stream = await result.OpenReadAsync())
                    using (var newStream = File.OpenWrite(localPath))
                    {
                        await stream.CopyToAsync(newStream);
                    }

                    _tempLogoPath = localPath;
                    LogoPreview.Source = ImageSource.FromFile(localPath);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al seleccionar imagen: {ex.Message}", "OK");
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

                var establecimiento = _establecimientoActual ?? new Establecimiento();
                establecimiento.Nombre = NombreEntry.Text.Trim();
                
                if (!string.IsNullOrEmpty(_tempLogoPath))
                {
                    establecimiento.LogoPath = _tempLogoPath;
                }

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
