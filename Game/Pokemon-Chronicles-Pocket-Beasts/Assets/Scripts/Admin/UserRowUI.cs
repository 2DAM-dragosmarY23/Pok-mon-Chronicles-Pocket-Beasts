using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Componente de interfaz que representa una fila individual de usuario en el panel de administración.
/// Permite visualizar y modificar el rol, estado activo y eliminar usuarios.
/// Implementa validaciones para prevenir la eliminación o desactivación del último administrador.
/// </summary>
public class UserRowUI : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text emailText;
    public TMP_Dropdown roleDropdown;
    public Toggle activeToggle;
    public Button deleteButton;

    private UserModel user;
    private UserRepository userRepository = new UserRepository();

    /// <summary>
    /// Configura la fila con los datos de un usuario específico.
    /// Inicializa todos los controles UI y registra los eventos de cambio.
    /// </summary>
    /// <param name="userData">Modelo de usuario a representar en esta fila</param>
    public void Setup(UserModel userData)
    {
        user = userData;

        usernameText.text = user.Username;
        emailText.text = user.Email;

        // Configurar dropdown de roles
        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(new System.Collections.Generic.List<string> { "player", "admin" });
        roleDropdown.value = user.Role == "admin" ? 1 : 0;

        activeToggle.isOn = user.Active == 1;

        // Limpiar listeners previos para evitar duplicados
        roleDropdown.onValueChanged.RemoveAllListeners();
        activeToggle.onValueChanged.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();

        // Registrar eventos de cambio
        roleDropdown.onValueChanged.AddListener(OnRoleChanged);
        activeToggle.onValueChanged.AddListener(OnActiveChanged);
        deleteButton.onClick.AddListener(DeleteUser);
    }

    /// <summary>
    /// Elimina el usuario de la base de datos.
    /// Implementa validación para prevenir la eliminación del último administrador activo del sistema.
    /// </summary>
    void DeleteUser()
    {
        int adminCount = userRepository.CountAdmins();

        bool isAdmin = user.Role == "admin" && user.Active == 1;
        bool isLastAdmin = isAdmin && adminCount <= 1;

        // Prevenir eliminación del último administrador
        if (isLastAdmin)
        {
            Debug.LogWarning("No se puede borrar el último administrador.");
            return;
        }

        userRepository.Delete(user.Uid);
        FindObjectOfType<AdminPanelController>().Reload();
    }

    /// <summary>
    /// Maneja el cambio de rol del usuario.
    /// Valida que no se esté eliminando el rol de administrador del último admin activo.
    /// Si la validación falla, revierte el cambio en el dropdown.
    /// </summary>
    /// <param name="value">Índice del rol seleccionado (0=player, 1=admin)</param>
    void OnRoleChanged(int value)
    {
        bool newIsAdmin = value == 1;
        int adminCount = userRepository.CountAdmins();

        // Verificar si se está quitando el rol admin al último administrador
        bool removingLastAdmin =
            user.Role == "admin" &&
            !newIsAdmin &&
            adminCount <= 1;

        if (removingLastAdmin)
        {
            roleDropdown.value = 1; // Revertir a admin
            Debug.LogWarning("Debe existir al menos un administrador.");
            return;
        }

        user.Role = newIsAdmin ? "admin" : "player";
    }

    /// <summary>
    /// Maneja el cambio de estado activo del usuario.
    /// Previene la desactivación del último administrador activo del sistema.
    /// Si la validación falla, revierte el cambio en el toggle.
    /// </summary>
    /// <param name="value">True si el usuario debe estar activo</param>
    void OnActiveChanged(bool value)
    {
        int adminCount = userRepository.CountAdmins();

        // Verificar si se está desactivando el último administrador
        bool disablingLastAdmin =
            user.Role == "admin" &&
            !value &&
            adminCount <= 1;

        if (disablingLastAdmin)
        {
            activeToggle.isOn = true; // Revertir a activo
            Debug.LogWarning("No se puede desactivar el último administrador.");
            return;
        }

        user.Active = value ? 1 : 0;
    }

    /// <summary>
    /// Obtiene el modelo de usuario con los valores actualizados desde los controles UI.
    /// Utilizado por el AdminPanelController al guardar cambios.
    /// </summary>
    /// <returns>Modelo de usuario con los datos modificados</returns>
    public UserModel GetUpdatedUser()
    {
        user.Role = roleDropdown.value == 1 ? "admin" : "player";
        user.Active = activeToggle.isOn ? 1 : 0;
        return user;
    }
}