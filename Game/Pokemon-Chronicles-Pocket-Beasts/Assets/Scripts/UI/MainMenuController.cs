using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador del menú principal que determina qué panel mostrar según el usuario actual.
/// Evalúa el estado de autenticación y el rol del usuario para decidir la vista apropiada.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject playerPanel;
    public GameObject adminPanel;

    UserRepository userRepository;

    /// <summary>
    /// Inicializa el controlador y evalúa el usuario actual tras un breve delay.
    /// </summary>
    IEnumerator Start()
    {
        userRepository = new UserRepository();
        yield return new WaitForSeconds(0.1f);
        EvaluateUser();
    }

    /// <summary>
    /// Determina qué panel mostrar basándose en el usuario de Firebase y su información local.
    /// Verifica autenticación, existencia en base de datos local, estado activo y rol.
    /// </summary>
    void EvaluateUser()
    {
        var firebaseUser = AuthService.Instance.GetCurrentUser();
        bool hasFirebaseUser = firebaseUser != null;

        if (!hasFirebaseUser)
        {
            ShowLogin();
        }
        else
        {
            var localUser = userRepository.GetByUid(firebaseUser.UserId);
            bool hasLocalUser = localUser != null && localUser.Active == 1;

            if (!hasLocalUser)
            {
                ShowLogin();
            }
            else
            {
                bool isAdmin = localUser.Role == "admin";

                if (isAdmin)
                {
                    ShowAdmin();
                }
                else
                {
                    ShowPlayer();
                }
            }
        }
    }

    /// <summary>
    /// Muestra el panel de login y oculta los demás.
    /// </summary>
    void ShowLogin()
    {
        loginPanel.SetActive(true);
        playerPanel.SetActive(false);
        adminPanel.SetActive(false);
    }

    /// <summary>
    /// Muestra el panel de jugador y oculta los demás.
    /// </summary>
    void ShowPlayer()
    {
        loginPanel.SetActive(false);
        playerPanel.SetActive(true);
        adminPanel.SetActive(false);
    }

    /// <summary>
    /// Muestra el panel de administrador y oculta los demás.
    /// </summary>
    void ShowAdmin()
    {
        loginPanel.SetActive(false);
        playerPanel.SetActive(false);
        adminPanel.SetActive(true);
    }
}