using SQLite;

namespace prueba.Models
{
    public class MovimientoStock
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ProductoId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string? TipoMovimiento { get; set; } // "Entrada" o "Salida"

        public int Cantidad { get; set; }

        public int StockAnterior { get; set; }

        public int StockNuevo { get; set; }

        public string? Motivo { get; set; } // "Compra", "Venta", "Daño", etc.

        public string? Notas { get; set; }
    }
}
