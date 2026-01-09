using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del sistema de menú inicial que gestiona login y registro de usuarios.
/// Valida las entradas, comunica con Firebase y SQLite, y coordina las transiciones de escena.
/// </summary>
public class MenuSystem : MonoBehaviour
{
    [Header("Campos de Login")]
    public TMP_InputField nicknameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_Text errorMessage;

    [Header("Escenas")]
    public string gameSceneName = "GameScene";
    public string adminSceneName = "AdminPanelScene";

    UserRepository repo = new UserRepository();

    /// <summary>
    /// Valida si una cadena tiene formato de email válido usando expresiones regulares.
    /// </summary>
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    /// <summary>
    /// Procesa el inicio de sesión del usuario.
    /// Valida las credenciales, autentica con Firebase y sincroniza con la base de datos local.
    /// Redirige a la escena apropiada según el rol del usuario.
    /// </summary>
    public void StartGame()
    {
        errorMessage.text = "";
        string email = emailField.text.Trim();
        string password = passwordField.text;
        string nickname = nicknameField.text.Trim();

        string errors = "";

        // Validación de campos
        if (string.IsNullOrEmpty(email))
            errors += "- Rellena el email\n";
        else if (!IsValidEmail(email))
            errors += "- El email no tiene un formato válido\n";

        if (string.IsNullOrEmpty(password))
            errors += "- Rellena la contraseña\n";

        if (string.IsNullOrEmpty(nickname))
            errors += "- Rellena el usuario\n";

        if (!string.IsNullOrEmpty(password) && password.Length < 6)
            errors += "- La contraseña debe tener al menos 6 caracteres\n";

        if (!string.IsNullOrEmpty(errors))
        {
            errorMessage.text = errors.TrimEnd('\n');
            return;
        }

        // Intentar login con Firebase
        AuthService.Instance.Login(email, password,
            user =>
            {
                // Sincronizar con base de datos local
                repo.CreateIfNotExists(user.UserId, user.Email);
                Debug.Log("Login correcto: " + user.UserId);

                // Determinar escena según rol
                var local = repo.GetByUid(user.UserId);
                if (local != null && local.Active == 1 && local.Role == "admin")
                    SceneManager.LoadScene(adminSceneName);
                else
                    SceneManager.LoadScene(gameSceneName);
            },
            err =>
            {
                errorMessage.text = err;
            }
        );
    }

    /// <summary>
    /// Procesa el registro de un nuevo usuario.
    /// Valida los datos, crea la cuenta en Firebase y registra al usuario en la base de datos local.
    /// </summary>
    public void RegisterUser()
    {
        errorMessage.text = "";
        string email = emailField.text.Trim();
        string password = passwordField.text;
        string nickname = nicknameField.text.Trim();

        string errors = "";

        // Validación de campos
        if (string.IsNullOrEmpty(email))
            errors += "- Rellena el email\n";
        else if (!IsValidEmail(email))
            errors += "- El email no tiene un formato válido\n";

        if (string.IsNullOrEmpty(password))
            errors += "- Rellena la contraseña\n";

        if (!string.IsNullOrEmpty(password) && password.Length < 6)
            errors += "- La contraseña debe tener al menos 6 caracteres\n";

        if (!string.IsNullOrEmpty(errors))
        {
            errorMessage.text = errors.TrimEnd('\n');
            return;
        }

        // Registrar en Firebase
        AuthService.Instance.Register(email, password,
            user =>
            {
                // Crear usuario en base de datos local
                UserModel newUser = new UserModel
                {
                    Uid = user.UserId,
                    Username = string.IsNullOrEmpty(nickname) ? email.Split('@')[0] : nickname,
                    Email = email,
                    Role = "player",
                    Active = 1,
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    UpdatedAt = DateTime.UtcNow.ToString("o")
                };

                repo.Create(newUser);
                Debug.Log("Usuario creado en SQLite: " + newUser.Username);

                errorMessage.color = Color.green;
                errorMessage.text = "Registro correcto. Ya puedes iniciar sesión.";
            },
            err =>
            {
                errorMessage.text = err;
            }
        );
    }
}