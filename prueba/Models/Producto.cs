using SQLite;

namespace prueba.Models
{
    public class Producto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int AreaId { get; set; }

        public string? Nombre { get; set; }

        public int Stock { get; set; }

        public int StockMinimo { get; set; } = 0;

        public string? UnidadMedida { get; set; } = "Unidades"; // Nuevo campo

        public string? ImagenRuta { get; set; }

        public string? Categoria { get; set; }

        public string? Notas { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        [Ignore]
        public bool StockBajo => Stock <= StockMinimo && StockMinimo > 0;
        
        [Ignore]
        public string StockConUnidad => $"{Stock} {UnidadMedida}";
    }
}
