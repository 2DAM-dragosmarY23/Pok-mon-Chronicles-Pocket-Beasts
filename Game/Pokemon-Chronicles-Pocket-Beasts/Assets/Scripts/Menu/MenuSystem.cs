using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuSystem : MonoBehaviour
{

    [Header("Campos de Login")]
    public TMP_InputField nicknameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public Button registerButton;
    public TMP_Text errorMessage;


    [Header("URL del backend")]
    public string apiBase = "http://127.0.0.1:5000/api/auth";

    // Métodos de los botones
    public void StartGame()
    {
        // Iniciar login primero
        StartCoroutine(LoginCoroutine());
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void RegisterUser()
    {
        StartCoroutine(RegisterCoroutine());
    }

    // Coroutine de login
    IEnumerator LoginCoroutine()
    {
        string username = nicknameField?.text ?? "";
        string password = passwordField?.text ?? "";

        if (errorMessage != null)
            errorMessage.text = "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Rellena usuario y contraseña.");
            yield break;
        }

        var dto = new LoginDTO { username = username, password = password };
        string json = JsonUtility.ToJson(dto);

        using (UnityWebRequest req = new UnityWebRequest(apiBase + "/login", "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            // Timeout 
            req.timeout = 10;

            yield return req.SendWebRequest();

            // Información de depuración
            Debug.Log($"REQUEST -> {req.method} {req.url}");
            Debug.Log($"RESULT -> {req.result}  responseCode={req.responseCode}  error='{req.error}'");

            string responseText = req.downloadHandler?.text;
            Debug.Log("BODY: " + (string.IsNullOrEmpty(responseText) ? "<empty>" : responseText));

            // Manejo de errores
            if (req.result == UnityWebRequest.Result.ConnectionError ||
                req.result == UnityWebRequest.Result.ProtocolError ||
                req.result == UnityWebRequest.Result.DataProcessingError)
            {
                // Mostrar mensaje visual
                if (errorMessage != null)
                {
                    if (req.responseCode == 401)
                        errorMessage.text = "Usuario o contraseña incorrectos";
                    else
                        errorMessage.text = "Error de conexión con el servidor";
                }

                Debug.LogError($"Error en la petición. Código HTTP: {req.responseCode}. Error: {req.error}");
                yield break;
            }

            // Si todo OK
            try
            {
                TokenResponse token = JsonUtility.FromJson<TokenResponse>(responseText);
                if (token == null || string.IsNullOrEmpty(token.token))
                {
                    Debug.LogError("Respuesta inválida del servidor (no contiene token).");
                    yield break;
                }

                PlayerPrefs.SetString("jwt", token.token);
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save();

                Debug.Log("Login correcto. Token guardado.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error parseando respuesta: " + ex);
            }
        }
    }

    // Coroutine de registro
    IEnumerator RegisterCoroutine()
    {
        // Limpia mensaje previo
        if (errorMessage != null) errorMessage.text = "";

        string username = nicknameField?.text?.Trim() ?? "";
        string email = emailField?.text?.Trim() ?? "";
        string password = passwordField?.text ?? "";

        // Validaciones cliente
        if (string.IsNullOrEmpty(username))
        {
            if (errorMessage != null) errorMessage.text = "El nombre de usuario no puede estar vacío.";
            yield break;
        }

        if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
        {
            if (errorMessage != null) errorMessage.text = "Introduce un correo válido (ej. usuario@dominio.com).";
            yield break;
        }

        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            if (errorMessage != null) errorMessage.text = "La contraseña debe tener al menos 6 caracteres.";
            yield break;
        }

        // Desactivar botón para evitar dobles envíos
        if (registerButton != null) registerButton.interactable = false;

        RegisterDTO dto = new RegisterDTO
        {
            username = username,
            email = email,
            password = password
        };

        string json = JsonUtility.ToJson(dto);

        using (UnityWebRequest req = new UnityWebRequest(apiBase + "/register", "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 10;

            yield return req.SendWebRequest();

            // Debug (opcional)
            Debug.Log($"REGISTER -> {req.method} {req.url} result={req.result} code={req.responseCode} err='{req.error}'");
            string resp = req.downloadHandler != null ? req.downloadHandler.text : "";

            // Manejo de errores de red
            if (req.result == UnityWebRequest.Result.ConnectionError ||
                req.result == UnityWebRequest.Result.DataProcessingError)
            {
                if (errorMessage != null) errorMessage.text = "Error de conexión al registrar. Intenta más tarde.";
                Debug.LogError("Register connection error: " + req.error);
                if (registerButton != null) registerButton.interactable = true;
                yield break;
            }

            // Si hay respuesta 4xx/5xx, intenta mostrar el mensaje del servidor
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                // Intentamos parsear { "message": "..." }
                MessageResponse serverMsg = null;
                try
                {
                    serverMsg = JsonUtility.FromJson<MessageResponse>(resp);
                }
                catch { /* parseo fallido */ }

                if (serverMsg != null && !string.IsNullOrEmpty(serverMsg.message))
                {
                    errorMessage.color = Color.red;
                    errorMessage.text = "Registro fallido: " + serverMsg.message;
                }
                else
                {
                    if (req.responseCode == 409)
                        errorMessage.text = "El nombre de usuario ya está registrado.";
                    else
                        errorMessage.text = "Registro fallido. Código: " + req.responseCode;
                }

                if (registerButton != null) registerButton.interactable = true;
                yield break;
            }

            // OK (2xx)
            if (req.responseCode >= 200 && req.responseCode < 300)
            {
                if (errorMessage != null) errorMessage.color = Color.blue;
                if (errorMessage != null) errorMessage.text = "Registro correcto. Ya puedes iniciar sesión.";
                Debug.Log("Registro correcto: " + resp);
            }
            else
            {
                // Caso inesperado
                if (errorMessage != null) errorMessage.text = "Error inesperado en el registro.";
                Debug.LogWarning("Register unexpected response: " + req.responseCode + " " + resp);
            }
        }

        // Reactivar botón
        if (registerButton != null) registerButton.interactable = true;
    }


    // Clases auxiliares
    [System.Serializable]
    public class LoginDTO
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class TokenResponse
    {
        public string token;
    }

    [System.Serializable]
    public class RegisterDTO
    {
        public string username;
        public string email;
        public string password;
    }

    [System.Serializable]
    public class MessageResponse
    {
        public string message;
    }

    // Validación simple: comprueba @ y . y longitud mínima.
    // Si quieres una validación más estricta, usar regex.
    bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        if (email.Length < 5) return false;
        if (!email.Contains("@")) return false;
        if (!email.Contains(".")) return false;
        // @ no puede ser el primero ni el último
        int at = email.IndexOf('@');
        if (at <= 0 || at >= email.Length - 1) return false;
        return true;
    }

}
