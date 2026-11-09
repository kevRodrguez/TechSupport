using TechSupport.API.Models;

namespace TechSupport.API.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Usuarios.Any())
            return;

        db.Usuarios.AddRange(
            new Usuario
            {
                NombreUsuario = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Rol = Rol.Administrador
            },
            new Usuario
            {
                NombreUsuario = "tecnico",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tecnico123!"),
                Rol = Rol.Tecnico
            },
            new Usuario
            {
                NombreUsuario = "usuario",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Usuario123!"),
                Rol = Rol.Usuario
            });

        db.SaveChanges();
    }
}
