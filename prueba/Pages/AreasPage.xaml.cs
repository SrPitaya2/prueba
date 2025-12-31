using prueba.Models;
using prueba.Services;

namespace prueba.Pages
{
    public partial class AreasPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Establecimiento? _establecimiento;

        public AreasPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                _establecimiento = await _databaseService.GetEstablecimientoAsync();
                if (_establecimiento != null)
                {
                    EstablecimientoLabel.Text = _establecimiento.Nombre;
                    await CargarAreasAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar: {ex.Message}", "OK");
            }
        }

        private async Task CargarAreasAsync()
        {
            if (_establecimiento == null) return;

            AreasContainer.Clear();

            var areas = await _databaseService.GetAreasAsync(_establecimiento.Id);

            if (areas.Count == 0)
            {
                AreasContainer.Add(new Label
                {
                    Text = "No hay areas. Agrega una nueva.",
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(0, 40, 0, 0)
                });
            }
            else
            {
                foreach (var area in areas)
                {
                    var frame = new Frame
                    {
                        BorderColor = Color.FromArgb("#512BD4"),
                        CornerRadius = 10,
                        Padding = 15,
                        HasShadow = true
                    };

                    var grid = new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                            new ColumnDefinition { Width = GridLength.Auto }
                        }
                    };

                    var labelStack = new VerticalStackLayout { Spacing = 5 };
                    labelStack.Add(new Label
                    {
                        Text = area.Nombre,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold
                    });

                    var productosCount = await _databaseService.GetProductosAsync(area.Id);
                    labelStack.Add(new Label
                    {
                        Text = $"{productosCount.Count} productos",
                        TextColor = Colors.Gray,
                        FontSize = 14
                    });

                    var buttonStack = new HorizontalStackLayout { Spacing = 10 };

                    var verBtn = new Button
                    {
                        Text = "Ver",
                        BackgroundColor = Color.FromArgb("#512BD4"),
                        TextColor = Colors.White,
                        WidthRequest = 80
                    };
                    verBtn.Clicked += async (s, e) => await OnVerAreaClicked(area.Id);

                    var eliminarBtn = new Button
                    {
                        Text = "🗑️",
                        BackgroundColor = Colors.Red,
                        TextColor = Colors.White,
                        WidthRequest = 50,
                        HeightRequest = 50,
                        FontSize = 24,
                        Padding = new Thickness(0)
                    };
                    eliminarBtn.Clicked += async (s, e) => await OnEliminarAreaClicked(area);

                    buttonStack.Add(verBtn);
                    buttonStack.Add(eliminarBtn);

                    Grid.SetColumn(labelStack, 0);
                    Grid.SetColumn(buttonStack, 1);

                    grid.Add(labelStack);
                    grid.Add(buttonStack);

                    frame.Content = grid;
                    AreasContainer.Add(frame);
                }
            }
        }

        private async void OnAgregarAreaClicked(object? sender, EventArgs e)
        {
            if (_establecimiento == null) return;

            string nombre = await DisplayPromptAsync("Nueva Area", "Nombre del area:", 
                placeholder: "Ej: Almacen, Cocina, Ventas...");

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var area = new Area
                {
                    EstablecimientoId = _establecimiento.Id,
                    Nombre = nombre.Trim()
                };

                await _databaseService.SaveAreaAsync(area);
                await CargarAreasAsync();
            }
        }

        private async Task OnVerAreaClicked(int areaId)
        {
            await Shell.Current.GoToAsync($"productos?areaId={areaId}");
        }

        private async Task OnEliminarAreaClicked(Area area)
        {
            bool confirmar = await DisplayAlert("Confirmar", 
                $"Eliminar el area '{area.Nombre}' y todos sus productos?", 
                "Eliminar", "Cancelar");

            if (confirmar)
            {
                await _databaseService.DeleteAreaAsync(area);
                await CargarAreasAsync();
            }
        }
    }
}
