using System;
using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;

/// <summary>
/// Repositorio que gestiona las operaciones CRUD sobre la tabla de usuarios.
/// Proporciona una capa de abstracción entre la lógica de negocio y la base de datos.
/// </summary>
public class UserRepository
{
    /// <summary>
    /// Obtiene la conexión a la base de datos desde el SQLiteManager.
    /// </summary>
    SQLiteConnection Connection
    {
        get
        {
            return SQLiteManager.Instance != null
                ? SQLiteManager.Instance.GetConnection()
                : null;
        }
    }

    /// <summary>
    /// Crea un nuevo usuario en la base de datos local si no existe.
    /// Útil para sincronizar usuarios de Firebase con la base local.
    /// </summary>
    /// <param name="uid">ID único del usuario de Firebase</param>
    /// <param name="email">Email del usuario</param>
    public void CreateIfNotExists(string uid, string email)
    {
        if (string.IsNullOrEmpty(uid) || Connection == null) return;
        var existing = GetByUid(uid);
        if (existing != null) return;

        UserModel newUser = new UserModel
        {
            Uid = uid,
            Email = email,
            Username = email.Contains("@") ? email.Split('@')[0] : uid,
            Role = "player",
            Active = 1,
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o")
        };
        Create(newUser);
    }

    /// <summary>
    /// Busca un usuario por su UID de Firebase.
    /// </summary>
    /// <returns>Usuario encontrado o null si no existe</returns>
    public UserModel GetByUid(string uid)
    {
        if (Connection == null || string.IsNullOrEmpty(uid)) return null;
        return Connection.Find<UserModel>(uid);
    }

    /// <summary>
    /// Obtiene todos los usuarios registrados en la base de datos.
    /// </summary>
    public List<UserModel> GetAll()
    {
        if (Connection == null) return new List<UserModel>();
        return Connection.Table<UserModel>().ToList();
    }

    /// <summary>
    /// Crea un nuevo usuario en la base de datos.
    /// </summary>
    public void Create(UserModel user)
    {
        if (Connection == null || user == null || string.IsNullOrEmpty(user.Uid)) return;
        user.CreatedAt = DateTime.UtcNow.ToString("o");
        user.UpdatedAt = user.CreatedAt;
        Connection.Insert(user);
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    public void Update(UserModel user)
    {
        if (Connection == null || user == null || string.IsNullOrEmpty(user.Uid)) return;
        user.UpdatedAt = DateTime.UtcNow.ToString("o");
        Connection.Update(user);
    }

    /// <summary>
    /// Elimina un usuario de la base de datos por su UID.
    /// </summary>
    public void Delete(string uid)
    {
        if (Connection == null || string.IsNullOrEmpty(uid)) return;
        Connection.Delete<UserModel>(uid);
    }

    /// <summary>
    /// Cuenta cuántos administradores activos existen en el sistema.
    /// Utilizado para prevenir la eliminación del último administrador.
    /// </summary>
    public int CountAdmins()
    {
        if (Connection == null) return 0;
        return Connection.Table<UserModel>().Where(u => u.Role == "admin" && u.Active == 1).Count();
    }
}