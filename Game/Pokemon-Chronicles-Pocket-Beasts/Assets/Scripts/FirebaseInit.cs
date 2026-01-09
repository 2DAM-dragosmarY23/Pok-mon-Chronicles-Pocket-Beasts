using Firebase;
using Firebase.Auth;
using UnityEngine;

/// <summary>
/// Componente responsable de inicializar Firebase al arrancar el juego.
/// Verifica las dependencias y configura FirebaseAuth.
/// </summary>
public class FirebaseInit : MonoBehaviour
{
    public static FirebaseAuth Auth;

    /// <summary>
    /// Verifica y corrige las dependencias de Firebase de forma asíncrona.
    /// Debe estar presente en la escena inicial del juego.
    /// </summary>
    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase inicializado correctamente");
            }
            else
            {
                Debug.LogError("Firebase no disponible: " + task.Result);
            }
        });
    }
}