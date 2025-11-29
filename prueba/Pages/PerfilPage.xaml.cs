using prueba.Models;
using prueba.Services;

namespace prueba.Pages
{
    public partial class PerfilPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Usuario? _usuarioActual;

        public PerfilPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarPerfilAsync();
        }

        private async Task CargarPerfilAsync()
        {
            try
            {
                _usuarioActual = await _databaseService.GetUsuarioAsync();

                if (_usuarioActual != null)
                {
                    NombreEntry.Text = _usuarioActual.Nombre;
                    ApellidoEntry.Text = _usuarioActual.Apellido;
                    EmailEntry.Text = _usuarioActual.Email;
                    TelefonoEntry.Text = _usuarioActual.Telefono;
                    MensajeLabel.Text = $"Perfil cargado (Creado: {_usuarioActual.FechaCreacion:dd/MM/yyyy})";
                }
                else
                {
                    MensajeLabel.Text = "Crea tu perfil nuevo";
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
                    await DisplayAlert("Validación", "El nombre es obligatorio", "OK");
                    return;
                }

                var usuario = new Usuario
                {
                    Nombre = NombreEntry.Text?.Trim(),
                    Apellido = ApellidoEntry.Text?.Trim(),
                    Email = EmailEntry.Text?.Trim(),
                    Telefono = TelefonoEntry.Text?.Trim(),
                    FechaCreacion = _usuarioActual?.FechaCreacion ?? DateTime.Now
                };

                await _databaseService.SaveUsuarioAsync(usuario);

                MensajeLabel.Text = "? Perfil guardado exitosamente!";
                MensajeLabel.TextColor = Colors.Green;

                await Task.Delay(2000);
                await CargarPerfilAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
            }
        }
    }
}
