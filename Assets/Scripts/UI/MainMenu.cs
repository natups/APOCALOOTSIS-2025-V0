using UnityEngine;
using UnityEngine.SceneManagement;

// El nombre de la clase debe coincidir con el nombre del archivo: MainMenu
public class MainMenu : MonoBehaviour
{
    // =========================================================================
    // 1. VARIABLES DE PANELES (ORIGINALES)
    // =========================================================================
    [Header("Configuración de Paneles")]
    [Tooltip("Arrastra aquí el objeto MainMenuUI (el que tiene los botones principales)")]
    public GameObject panelMenuPrincipal;

    [Tooltip("Arrastra aquí el Panel que tiene la imagen de los controles")]
    public GameObject panelControles;
    
    // =========================================================================
    // 2. REFERENCIAS DE BOTONES DE AUTENTICACIÓN (NUEVAS)
    // =========================================================================
    [Header("Referencias de Autenticación (UI)")]
    [Tooltip("Botón de 'Iniciar Sesión'")]
    public GameObject loginButton;
    [Tooltip("Botón de 'Registrarse'")]
    public GameObject registerButton;
    [Tooltip("Botón de 'Cerrar Sesión'.")]
    public GameObject logoutButton; 

    // =========================================================================
    // 3. START & ONENABLE
    // =========================================================================

    private void OnEnable()
    {
        // Se asegura de que el menú principal esté activo y los controles ocultos al inicio.
        if(panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if(panelControles != null) panelControles.SetActive(false);

        // Lógica de PlayerPrefs (se mantiene)
        if (!PlayerPrefs.HasKey("hasStarted"))
        {
            PlayerPrefs.SetInt("hasStarted", 0);
            PlayerPrefs.Save();
        }
        
        // IMPORTANTE: Al cargar la escena, asumimos que NO hay sesión activa 
        // hasta que el AuthController nos confirme lo contrario.
        UpdateAuthUI(false); 
    }
    
    /// <summary>
    /// Muestra u oculta los botones según el estado de la sesión.
    /// Es llamada por el AuthController al volver de la escena Authentication.
    /// </summary>
    public void UpdateAuthUI(bool isAuthenticated)
    {
        // Si está autenticado: Muestra Logout; Oculta Login/Register
        if (loginButton != null)
            loginButton.SetActive(!isAuthenticated); 
        
        if (registerButton != null)
            registerButton.SetActive(!isAuthenticated); 

        if (logoutButton != null)
            logoutButton.SetActive(isAuthenticated); 
            
        Debug.Log($"Estado UI actualizado: Logueado = {isAuthenticated}");
    }

    // =========================================================================
    // 4. MÉTODOS DE AUTENTICACIÓN Y NAVEGACIÓN
    // =========================================================================

    // FUNCIÓN ASIGNADA A LOS BOTONES 'INICIAR SESIÓN' y 'REGISTRARSE'
    public void LoadAuthenticationScene(string panelName)
    {
        PlayerPrefs.SetString("InitialPanel", panelName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Authentication"); 
    }
    
    /// <summary>
    /// ASIGNAR AL BOTÓN 'CERRAR SESIÓN'.
    /// Pide al AuthController que ejecute el logout.
    /// </summary>
    public void LogoutClicked()
    {
        Debug.Log("Solicitando cierre de sesión y volviendo a la escena de autenticación.");
        
        // 1. Establece un indicador de que el Logout es requerido.
        // El AuthController lo leerá al inicio y llamará a RequestLogout().
        PlayerPrefs.SetInt("RequestLogout", 1);
        PlayerPrefs.Save();
        
        // 2. Carga la escena de autenticación.
        SceneManager.LoadScene("Authentication");
    }

    // --- MÉTODOS DE NAVEGACIÓN DE ESCENAS (RESTO ORIGINAL) ---

    public void PlayGame()
    {
        SceneManager.LoadScene("ModeSelectMenu");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void OpenControls()
    {
        SceneManager.LoadScene("Controls");
    }
    
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

}