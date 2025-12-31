using SQLite;

namespace prueba.Models
{
    public class Establecimiento
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public string? LogoPath { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
