using Firebase.Auth;
using System;
using UnityEngine;

/// <summary>
/// Servicio singleton que gestiona la autenticación con Firebase.
/// Proporciona métodos para login, registro y consulta del usuario actual.
/// Utiliza UnityMainThreadDispatcher para ejecutar callbacks en el hilo principal.
/// </summary>
public class AuthService : MonoBehaviour
{
    public static AuthService Instance { get; private set; }
    FirebaseAuth auth;

    /// <summary>
    /// Inicializa el singleton y obtiene la instancia de FirebaseAuth.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        auth = FirebaseAuth.DefaultInstance;
    }

    /// <summary>
    /// Inicia sesión con email y contraseña de forma asíncrona.
    /// Los callbacks se ejecutan en el hilo principal de Unity.
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="password">Contraseña del usuario</param>
    /// <param name="onSuccess">Callback ejecutado si el login es exitoso</param>
    /// <param name="onError">Callback ejecutado si ocurre un error</param>
    public void Login(string email, string password, Action<FirebaseUser> onSuccess, Action<string> onError)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string msg = "Error de login";
                if (task.Exception != null) msg = task.Exception.Flatten().Message;
                UnityMainThreadDispatcher.Instance().Enqueue(() => onError?.Invoke(msg));
                return;
            }

            var result = task.Result;
            UnityMainThreadDispatcher.Instance().Enqueue(() => onSuccess?.Invoke(result.User));
        });
    }

    /// <summary>
    /// Registra un nuevo usuario con email y contraseña de forma asíncrona.
    /// </summary>
    /// <param name="email">Email del nuevo usuario</param>
    /// <param name="password">Contraseña del nuevo usuario</param>
    /// <param name="onSuccess">Callback ejecutado si el registro es exitoso</param>
    /// <param name="onError">Callback ejecutado si ocurre un error</param>
    public void Register(string email, string password, Action<FirebaseUser> onSuccess, Action<string> onError)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string msg = "Error al registrar usuario";
                if (task.Exception != null) msg = task.Exception.Flatten().Message;
                UnityMainThreadDispatcher.Instance().Enqueue(() => onError?.Invoke(msg));
                return;
            }

            var result = task.Result;
            UnityMainThreadDispatcher.Instance().Enqueue(() => onSuccess?.Invoke(result.User));
        });
    }

    /// <summary>
    /// Obtiene el usuario actualmente autenticado en Firebase.
    /// </summary>
    /// <returns>Usuario de Firebase o null si no hay sesión activa</returns>
    public FirebaseUser GetCurrentUser()
    {
        return auth?.CurrentUser;
    }
}