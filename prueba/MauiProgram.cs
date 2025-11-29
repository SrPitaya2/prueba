using Microsoft.Extensions.Logging;
using prueba.Services;
using prueba.Pages;

namespace prueba
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<DatabaseService>();
            
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<PerfilPage>();
            builder.Services.AddTransient<ConfiguracionPage>();
            builder.Services.AddTransient<AreasPage>();
            builder.Services.AddTransient<ProductosPage>();
            builder.Services.AddTransient<HistorialPage>();
            builder.Services.AddTransient<DetalleProductoPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<ReportesPage>();

            return builder.Build();
        }
    }
}
