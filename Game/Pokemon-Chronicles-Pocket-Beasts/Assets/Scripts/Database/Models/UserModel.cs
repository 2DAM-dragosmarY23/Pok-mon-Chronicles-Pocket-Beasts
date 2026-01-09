using SQLite4Unity3d;

/// <summary>
/// Modelo de datos que representa un usuario en la base de datos local.
/// Sincroniza la información de autenticación con datos adicionales del juego.
/// </summary>
[Table("users")]
public class UserModel
{
    [PrimaryKey]
    public string Uid { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    // Rol del usuario: "player" o "admin"
    public string Role { get; set; }

    // Estado de activación: 1 = activo, 0 = inactivo
    public int Active { get; set; }

    public string CreatedAt { get; set; }

    public string UpdatedAt { get; set; }
}