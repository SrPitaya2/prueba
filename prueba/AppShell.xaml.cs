using prueba.Pages;

namespace prueba
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            Routing.RegisterRoute("productos", typeof(ProductosPage));
            Routing.RegisterRoute("historial", typeof(HistorialPage));
            Routing.RegisterRoute("detalle-producto", typeof(DetalleProductoPage));
            Routing.RegisterRoute("reportes", typeof(ReportesPage));
        }
    }
}
