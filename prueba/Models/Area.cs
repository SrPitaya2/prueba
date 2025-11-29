using SQLite;

namespace prueba.Models
{
    public class Area
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int EstablecimientoId { get; set; }

        public string? Nombre { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
