using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;

namespace SistemaVentas.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
    public DbSet<Producto> Productos => Set<Producto>();
    // Alias para compatibilidad con código que usa .Producto
    public DbSet<Producto> Producto => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relaciones y configuraciones
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Precarga de Categorías
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Limpieza" },
            new Categoria { Id = 2, Nombre = "Abarrotes" },
            new Categoria { Id = 3, Nombre = "Bebidas" },
            new Categoria { Id = 4, Nombre = "Lácteos" },
            new Categoria { Id = 5, Nombre = "Snacks y Golosinas" }
        );

        // Precarga de Clientes (Andy 16565432 como en la imagen)
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, Nombre = "Andy", Apellido = "", Telefono = "16565432", Email = "andy@correo.com" },
            new Cliente { Id = 2, Nombre = "María", Apellido = "Flores", Telefono = "71234567", Email = "maria.flores@correo.com" },
            new Cliente { Id = 3, Nombre = "Carlos", Apellido = "Pérez", Telefono = "78912345", Email = "carlos.perez@correo.com" },
            new Cliente { Id = 4, Nombre = "Cliente Mostrador", Apellido = "General", Telefono = "00000000", Email = "general@mostrador.com" }
        );

        // Precarga de Productos
        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Jabón Rexona 125g", Precio = 8.00m, Stock = 100, StockMinimo = 10, CategoriaId = 1 },
            new Producto { Id = 2, Nombre = "Arroz Grano de Oro 1kg", Precio = 16.00m, Stock = 100, StockMinimo = 15, CategoriaId = 2 },
            new Producto { Id = 3, Nombre = "Coca Cola 2L Retornable", Precio = 13.00m, Stock = 100, StockMinimo = 10, CategoriaId = 3 },
            new Producto { Id = 4, Nombre = "Aceite Fino 1L", Precio = 12.50m, Stock = 80, StockMinimo = 10, CategoriaId = 2 },
            new Producto { Id = 5, Nombre = "Detergente OMO 800g", Precio = 15.00m, Stock = 60, StockMinimo = 8, CategoriaId = 1 },
            new Producto { Id = 6, Nombre = "Leche PIL 1L", Precio = 6.50m, Stock = 90, StockMinimo = 12, CategoriaId = 4 },
            new Producto { Id = 7, Nombre = "Galletas Mabel's Wafer", Precio = 3.50m, Stock = 120, StockMinimo = 20, CategoriaId = 5 }
        );
    }
}