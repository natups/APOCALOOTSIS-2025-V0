using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

// Controla el menú principal del juego.
// Maneja la navegación entre paneles, escenas
// y la visualización de botones según el estado de autenticación.
public class MainMenu : MonoBehaviour
{
    // =========================
    // PANELES DEL MENÚ
    // =========================

    // Panel principal con los botones del menú
    public GameObject panelMenuPrincipal;

    // Panel que muestra la imagen o información de controles
    public GameObject panelControles;

    // =========================
    // BOTONES DE AUTENTICACIÓN
    // =========================

    // Botón para iniciar sesión
    public GameObject loginButton;

    // Botón para registrarse
    public GameObject registerButton;

    // Botón para entrar como invitado
    public GameObject guestButton;

    // Botón para cerrar sesión
    public GameObject logoutButton;

    // =========================
    // PUENTE JAVASCRIPT (Firebase)
    // =========================

    [DllImport("__Internal")]
    private static extern void SignInAnonymouslyUser(
        string gameObject,
        string successCallback,
        string failureCallback
    );

    private string currentUserId = null;

    private void OnEnable()
    {
        // Activa el menú principal y oculta el panel de controles
        if (panelMenuPrincipal != null)
            panelMenuPrincipal.SetActive(true);

        if (panelControles != null)
            panelControles.SetActive(false);

        // Inicializa la clave hasStarted si no existe
        if (!PlayerPrefs.HasKey("hasStarted"))
        {
            PlayerPrefs.SetInt("hasStarted", 0);
            PlayerPrefs.Save();
        }

        // Por defecto se asume que no hay sesión activa
        UpdateAuthUI(false);
    }

    // Actualiza la visibilidad de los botones según si el usuario está autenticado
    public void UpdateAuthUI(bool isAuthenticated)
    {
        // Login y Register solo aparecen si NO está autenticado
        if (loginButton != null)
            loginButton.SetActive(!isAuthenticated);

        if (registerButton != null)
            registerButton.SetActive(!isAuthenticated);

        if (guestButton != null)
            guestButton.SetActive(!isAuthenticated);

        // Logout solo aparece si está autenticado
        if (logoutButton != null)
            logoutButton.SetActive(isAuthenticated);
    }

    // =========================
    // ENTRAR COMO INVITADO
    // =========================

    public void PlayAsGuest()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SignInAnonymouslyUser(
                gameObject.name,
                nameof(OnGuestAuthSuccess),
                nameof(OnGuestAuthFailure)
            );
        }
        else
        {
            // Simulación en Editor
            OnGuestAuthSuccess("simulated-anonymous-user");
        }
    }

    public void OnGuestAuthSuccess(string userId)
    {
        currentUserId = userId;
        UpdateAuthUI(true);
    }

    public void OnGuestAuthFailure(string error)
    {
        Debug.LogError("Error login anónimo: " + error);
    }

    // =========================
    // AUTENTICACIÓN
    // =========================

    // Carga la escena de autenticación indicando qué panel abrir primero
    public void LoadAuthenticationScene(string panelName)
    {
        PlayerPrefs.SetString("InitialPanel", panelName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Authentication");
    }

    // Solicita el cierre de sesión y redirige a la escena de autenticación
    public void LogoutClicked()
    {
        currentUserId = null;
        UpdateAuthUI(false);

        PlayerPrefs.SetInt("RequestLogout", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Authentication");
    }

    // =========================
    // NAVEGACIÓN DE ESCENAS
    // =========================

    // Inicia el juego
    public void PlayGame()
    {
        SceneManager.LoadScene("ModeSelectMenu");
    }

    // Abre la escena de configuración
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    // Abre la escena de controles
    public void OpenControls()
    {
        SceneManager.LoadScene("Controls");
    }
}
