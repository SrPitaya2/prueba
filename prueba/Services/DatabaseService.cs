using SQLite;
using prueba.Models;
using System.Diagnostics;

namespace prueba.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        public async Task InitAsync()
        {
            try
            {
                if (_database != null)
                    return;

                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "inventario.db3");
                Debug.WriteLine($"Database path: {dbPath}");
                
                _database = new SQLiteAsyncConnection(dbPath);
                
                await _database.CreateTableAsync<Usuario>();
                await _database.CreateTableAsync<Establecimiento>();
                await _database.CreateTableAsync<Area>();
                await _database.CreateTableAsync<Producto>();
                await _database.CreateTableAsync<MovimientoStock>();
                
                Debug.WriteLine("Database initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR InitAsync: {ex.GetType().Name}");
                Debug.WriteLine($"ERROR Message: {ex.Message}");
                Debug.WriteLine($"ERROR StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"INNER ERROR: {ex.InnerException.Message}");
                }
                throw new Exception($"Error al inicializar base de datos: {ex.Message}", ex);
            }
        }

        // Métodos Usuario
        public async Task<Usuario?> GetUsuarioAsync()
        {
            try
            {
                await InitAsync();
                var usuarios = await _database!.Table<Usuario>().ToListAsync();
                return usuarios.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetUsuarioAsync: {ex.Message}");
                throw new Exception($"Error al obtener usuario: {ex.Message}", ex);
            }
        }

        public async Task<int> SaveUsuarioAsync(Usuario usuario)
        {
            try
            {
                await InitAsync();

                var existente = await GetUsuarioAsync();
                if (existente != null)
                {
                    usuario.Id = existente.Id;
                    return await _database!.UpdateAsync(usuario);
                }
                else
                {
                    return await _database!.InsertAsync(usuario);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR SaveUsuarioAsync: {ex.Message}");
                throw new Exception($"Error al guardar usuario: {ex.Message}", ex);
            }
        }

        // Métodos Establecimiento
        public async Task<Establecimiento?> GetEstablecimientoAsync()
        {
            try
            {
                await InitAsync();
                var establecimientos = await _database!.Table<Establecimiento>().ToListAsync();
                return establecimientos.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetEstablecimientoAsync: {ex.Message}");
                throw new Exception($"Error al obtener establecimiento: {ex.Message}", ex);
            }
        }

        public async Task<int> SaveEstablecimientoAsync(Establecimiento establecimiento)
        {
            try
            {
                await InitAsync();
                var existente = await GetEstablecimientoAsync();
                if (existente != null)
                {
                    establecimiento.Id = existente.Id;
                    return await _database!.UpdateAsync(establecimiento);
                }
                else
                {
                    return await _database!.InsertAsync(establecimiento);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR SaveEstablecimientoAsync: {ex.Message}");
                throw new Exception($"Error al guardar establecimiento: {ex.Message}", ex);
            }
        }

        // Métodos Area
        public async Task<List<Area>> GetAreasAsync(int establecimientoId)
        {
            try
            {
                await InitAsync();
                return await _database!.Table<Area>()
                    .Where(a => a.EstablecimientoId == establecimientoId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetAreasAsync: {ex.Message}");
                return new List<Area>();
            }
        }

        public async Task<Area?> GetAreaAsync(int id)
        {
            try
            {
                await InitAsync();
                return await _database!.Table<Area>()
                    .Where(a => a.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetAreaAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveAreaAsync(Area area)
        {
            try
            {
                await InitAsync();
                if (area.Id == 0)
                {
                    return await _database!.InsertAsync(area);
                }
                else
                {
                    return await _database!.UpdateAsync(area);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR SaveAreaAsync: {ex.Message}");
                throw new Exception($"Error al guardar área: {ex.Message}", ex);
            }
        }

        public async Task<int> DeleteAreaAsync(Area area)
        {
            try
            {
                await InitAsync();
                var productos = await GetProductosAsync(area.Id);
                foreach (var producto in productos)
                {
                    await DeleteProductoAsync(producto);
                }
                return await _database!.DeleteAsync(area);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR DeleteAreaAsync: {ex.Message}");
                throw new Exception($"Error al eliminar área: {ex.Message}", ex);
            }
        }

        // Métodos Producto
        public async Task<List<Producto>> GetProductosAsync(int areaId)
        {
            try
            {
                await InitAsync();
                return await _database!.Table<Producto>()
                    .Where(p => p.AreaId == areaId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetProductosAsync: {ex.Message}");
                return new List<Producto>();
            }
        }

        public async Task<List<Producto>> BuscarProductosAsync(int areaId, string busqueda)
        {
            try
            {
                await InitAsync();
                return await _database!.Table<Producto>()
                    .Where(p => p.AreaId == areaId && p.Nombre!.ToLower().Contains(busqueda.ToLower()))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR BuscarProductosAsync: {ex.Message}");
                return new List<Producto>();
            }
        }

        public async Task<List<Producto>> GetProductosStockBajoAsync(int areaId)
        {
            try
            {
                await InitAsync();
                var productos = await GetProductosAsync(areaId);
                return productos.Where(p => p.StockBajo).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetProductosStockBajoAsync: {ex.Message}");
                return new List<Producto>();
            }
        }

        public async Task<List<Producto>> GetTodosProductosStockBajoAsync()
        {
            try
            {
                await InitAsync();
                var productos = await _database!.Table<Producto>().ToListAsync();
                return productos.Where(p => p.StockBajo).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetTodosProductosStockBajoAsync: {ex.Message}");
                return new List<Producto>();
            }
        }

        public async Task<Producto?> GetProductoAsync(int id)
        {
            try
            {
                await InitAsync();
                return await _database!.Table<Producto>()
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetProductoAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveProductoAsync(Producto producto)
        {
            try
            {
                await InitAsync();
                producto.FechaActualizacion = DateTime.Now;
                
                if (producto.Id == 0)
                {
                    return await _database!.InsertAsync(producto);
                }
                else
                {
                    return await _database!.UpdateAsync(producto);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR SaveProductoAsync: {ex.Message}");
                throw new Exception($"Error al guardar producto: {ex.Message}", ex);
            }
        }

        public async Task<int> DeleteProductoAsync(Producto producto)
        {
            try
            {
                await InitAsync();
                var movimientos = await GetMovimientosProductoAsync(producto.Id);
                foreach (var mov in movimientos)
                {
                    await _database!.DeleteAsync(mov);
                }
                return await _database!.DeleteAsync(producto);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR DeleteProductoAsync: {ex.Message}");
                throw new Exception($"Error al eliminar producto: {ex.Message}", ex);
            }
        }

        public async Task<int> ActualizarStockAsync(int productoId, int nuevoStock, string tipoMovimiento, string motivo, string? notas = null)
        {
            try
            {
                await InitAsync();
                var producto = await GetProductoAsync(productoId);
                if (producto != null)
                {
                    int stockAnterior = producto.Stock;
                    int cantidad = Math.Abs(nuevoStock - stockAnterior);

                    producto.Stock = nuevoStock;
                    producto.FechaActualizacion = DateTime.Now;
                    
                    var movimiento = new MovimientoStock
                    {
                        ProductoId = productoId,
                        Fecha = DateTime.Now,
                        TipoMovimiento = tipoMovimiento,
                        Cantidad = cantidad,
                        StockAnterior = stockAnterior,
                        StockNuevo = nuevoStock,
                        Motivo = motivo,
                        Notas = notas
                    };

                    await _database!.InsertAsync(movimiento);
                    return await _database!.UpdateAsync(producto);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR ActualizarStockAsync: {ex.Message}");
                throw new Exception($"Error al actualizar stock: {ex.Message}", ex);
            }
        }

        // Métodos MovimientoStock
        public async Task<List<MovimientoStock>> GetMovimientosProductoAsync(int productoId)
        {
            try
            {
                await InitAsync();
                return await _database!.Table<MovimientoStock>()
                    .Where(m => m.ProductoId == productoId)
                    .OrderByDescending(m => m.Fecha)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetMovimientosProductoAsync: {ex.Message}");
                return new List<MovimientoStock>();
            }
        }

        public async Task<List<MovimientoStock>> GetMovimientosAreaAsync(int areaId)
        {
            try
            {
                await InitAsync();
                var productos = await GetProductosAsync(areaId);
                var movimientos = new List<MovimientoStock>();
                
                foreach (var producto in productos)
                {
                    var movs = await GetMovimientosProductoAsync(producto.Id);
                    movimientos.AddRange(movs);
                }
                
                return movimientos.OrderByDescending(m => m.Fecha).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetMovimientosAreaAsync: {ex.Message}");
                return new List<MovimientoStock>();
            }
        }

        public async Task<List<MovimientoStock>> GetMovimientosRecientesAsync(int dias = 7)
        {
            try
            {
                await InitAsync();
                var fechaLimite = DateTime.Now.AddDays(-dias);
                return await _database!.Table<MovimientoStock>()
                    .Where(m => m.Fecha >= fechaLimite)
                    .OrderByDescending(m => m.Fecha)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetMovimientosRecientesAsync: {ex.Message}");
                return new List<MovimientoStock>();
            }
        }

        // Métodos de estadísticas
        public async Task<int> GetTotalProductosAsync()
        {
            try
            {
                await InitAsync();
                return await _database!.Table<Producto>().CountAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetTotalProductosAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalProductosAreaAsync(int areaId)
        {
            try
            {
                await InitAsync();
                return await _database!.Table<Producto>()
                    .Where(p => p.AreaId == areaId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetTotalProductosAreaAsync: {ex.Message}");
                return 0;
            }
        }
    }
}
