using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del panel de administración que permite gestionar usuarios del sistema.
/// Carga todos los usuarios de la base de datos y coordina las operaciones de modificación.
/// Proporciona una interfaz para cambiar roles, estados y eliminar usuarios.
/// </summary>
public class AdminPanelController : MonoBehaviour
{
    public Transform contentParent;
    public GameObject userRowPrefab;

    UserRepository repo = new UserRepository();
    List<UserRowUI> rows = new List<UserRowUI>();

    /// <summary>
    /// Carga la lista de usuarios al iniciar el panel.
    /// </summary>
    void Start()
    {
        Reload();
    }

    /// <summary>
    /// Recarga la lista completa de usuarios desde la base de datos.
    /// Destruye todas las filas existentes y genera nuevas instancias con los datos actualizados.
    /// Este método se invoca tras cualquier operación que modifique los datos de usuarios.
    /// </summary>
    public void Reload()
    {
        // Limpiar filas existentes
        rows.Clear();
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        var users = repo.GetAll();
        if (users == null || users.Count == 0) return;

        // Instanciar una fila por cada usuario
        foreach (var u in users)
        {
            var go = Instantiate(userRowPrefab, contentParent);
            var ui = go.GetComponent<UserRowUI>();
            ui.Setup(u);
            rows.Add(ui);
        }
    }

    /// <summary>
    /// Persiste en la base de datos todos los cambios realizados en las filas de usuario.
    /// Itera sobre cada fila, obtiene los datos modificados y los actualiza en la base de datos.
    /// Finaliza recargando la vista para reflejar los cambios guardados.
    /// </summary>
    public void SaveChanges()
    {
        foreach (var r in rows)
        {
            var updated = r.GetUpdatedUser();
            repo.Update(updated);
        }
        Reload();
    }

    /// <summary>
    /// Retorna al menú principal del juego.
    /// Permite al administrador salir del panel de gestión.
    /// </summary>
    public void GoBackToMenu()
    {
        SceneManager.LoadScene("MenuInicio");
    }
}