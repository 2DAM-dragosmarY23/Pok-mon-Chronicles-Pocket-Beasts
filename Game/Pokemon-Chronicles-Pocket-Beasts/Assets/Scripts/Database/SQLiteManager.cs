using System.IO;
using SQLite4Unity3d;
using UnityEngine;

/// <summary>
/// Singleton que gestiona la conexión con la base de datos SQLite local.
/// Se encarga de crear y mantener la conexión persistente durante toda la ejecución del juego.
/// </summary>
public class SQLiteManager : MonoBehaviour
{
    public static SQLiteManager Instance { get; private set; }

    SQLiteConnection connection;

    /// <summary>
    /// Inicializa el singleton y establece la conexión con la base de datos.
    /// Crea el archivo de base de datos en el directorio persistente de Unity si no existe.
    /// </summary>
    void Awake()
    {
        bool canInitialize = true;

        // Implementación del patrón Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            canInitialize = false;
        }

        if (canInitialize)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("SQLiteManager inicializado en: " + Application.persistentDataPath);

            // Establecer conexión con la base de datos
            string dbPath = Path.Combine(Application.persistentDataPath, "pokemon.db");
            connection = new SQLiteConnection(
                dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
            );

            CreateTables();
        }
    }

    /// <summary>
    /// Crea las tablas necesarias en la base de datos si no existen.
    /// </summary>
    void CreateTables()
    {
        connection.CreateTable<UserModel>();
    }

    /// <summary>
    /// Proporciona acceso a la conexión de base de datos para los repositorios.
    /// </summary>
    /// <returns>Conexión SQLite activa</returns>
    public SQLiteConnection GetConnection()
    {
        return connection;
    }
}