using UnityEngine;
using UnityEngine.UI; 
using TMPro; // NECESARIA para usar TextMeshPro (TMP)
using System.Runtime.InteropServices; 
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador principal de Autenticación.
/// Utiliza el patrón Singleton y DontDestroyOnLoad para persistir el estado de la sesión.
/// Se encarga de: 1. Llamar a las funciones JavaScript de Firebase (Login, Register).
/// 2. Gestionar el estado de la sesión (IsLoggedIn).
/// 3. Mostrar/Ocultar los paneles de UI en la escena "Authentication".
/// </summary>
public class AuthController : MonoBehaviour
{
    // =======================================================================
    // 0. SINGLETON Y ESTADO PERSISTENTE
    // =======================================================================
    // Instancia Singleton para acceso global
    public static AuthController Instance { get; private set; }
    
    // Almacena el ID del usuario actual. Es null si no hay sesión iniciada.
    private static string currentUserId = null; 

    // ----------------------------------------------------------------------
    // 1. VARIABLES PÚBLICAS (Aparecen en el Inspector)
    // ----------------------------------------------------------------------
    
    [Header("Paneles de UI de Autenticación")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    
    [Header("Inputs")]
    public GameObject loginEmailInputObject; 
    public GameObject loginPasswordInputObject; 
    
    [Header("Estado y Mensajes")]
    public TMPro.TextMeshProUGUI statusText;

    // Variables privadas para los COMPONENTES InputField
    private TMP_InputField loginEmailField; 
    private TMP_InputField loginPasswordField;

    // ----------------------------------------------------------------------
    // 2. DECLARACIÓN DEL PUENTE JAVASCRIPT (.jslib)
    // ----------------------------------------------------------------------
    
    [DllImport("__Internal")]
    private static extern void RegisterUser(string email, string password, string gameObject, string successCallback, string failureCallback);

    [DllImport("__Internal")]
    private static extern void SignInUser(string email, string password, string gameObject, string successCallback, string failureCallback);
    
    [DllImport("__Internal")]
    public static extern void SignOutUser(); // Función para cerrar sesión en Firebase


    // ----------------------------------------------------------------------
    // 3. START, AWAKE, Y GESTIÓN DE ESCENAS
    // ----------------------------------------------------------------------
    
    private void Awake()
    {
        // Implementación de Singleton (solo permitimos una instancia)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // ¡CLAVE! Hacemos que este objeto persista al cambiar de escena
            DontDestroyOnLoad(gameObject);
            
            // Suscribirse a los cambios de escena para actualizar el menú
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // Se llama cuando se carga una escena (e.g., la escena "MainMenu")
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Buscamos el script MainMenuUI en la nueva escena
        MainMenuUI menuUI = FindObjectOfType<MainMenuUI>();
        if (menuUI != null)
        {
            // Forzamos la actualización de la UI del menú (mostrar/ocultar botones)
            menuUI.UpdateUIForAuthStatus();
        }
    }
    
    void Start()
    {
        // Obtenemos los componentes InputField reales
        if (loginEmailInputObject != null)
        {
            loginEmailField = loginEmailInputObject.GetComponent<TMP_InputField>();
        }
        if (loginPasswordInputObject != null)
        {
            loginPasswordField = loginPasswordInputObject.GetComponent<TMP_InputField>();
        }

        // Lógica de navegación inicial
        string initialPanel = PlayerPrefs.GetString("InitialPanel", "Login"); 
        
        if (initialPanel == "Register")
        {
            ShowRegisterPanel();
        }
        else 
        {
            ShowLoginPanel(); 
        }
        PlayerPrefs.DeleteKey("InitialPanel");
    }
    
    // =======================================================================
    // 4. FUNCIONES DE ESTADO (Accedidas por MainMenuUI)
    // =======================================================================

    /// <summary>
    /// Devuelve TRUE si hay un usuario autenticado. (Usado por MainMenuUI)
    /// </summary>
    public static bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(currentUserId);
    }
    
    // =======================================================================
    // 5. FUNCIONES DE UI Y ACCIÓN
    // =======================================================================

    public void ShowLoginPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (statusText != null) statusText.text = ""; 
    }

    public void ShowRegisterPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        if (statusText != null) statusText.text = "";
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); 
    }
    
    /// <summary>
    /// LLAMADO POR EL BOTÓN LogOutBtn DEL MENÚ PRINCIPAL
    /// </summary>
    public void Logout()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SignOutUser(); // Llama a la función JS de Firebase para cerrar sesión
        }
        // Limpiamos el estado local
        currentUserId = null; 
        Debug.Log("AuthController: Sesión cerrada. Recargando MainMenu.");
        
        // Volvemos a cargar el MainMenu para que la UI se refresque
        SceneManager.LoadScene("MainMenu"); 
    }

    public void OnLoginClicked()
    {
        if (statusText != null) statusText.text = "Iniciando Sesión...";

        if (loginEmailField != null && loginPasswordField != null)
        {
            SignInUser(
                loginEmailField.text, 
                loginPasswordField.text, 
                gameObject.name, 
                nameof(OnAuthSuccess),
                nameof(OnAuthFailure)
            );
        } else {
            if (statusText != null) statusText.text = "Error: Faltan referencias en el Inspector.";
        }
    }

    public void OnRegisterClicked()
    {
        if (statusText != null) statusText.text = "Registrando usuario...";
        
        if (registerPanel == null)
        {
            if (statusText != null) statusText.text = "Error: El panel de registro no está asignado.";
            return;
        }

        TMP_InputField[] registerInputs = registerPanel.GetComponentsInChildren<TMP_InputField>();
        
        if (registerInputs.Length >= 2)
        {
            TMP_InputField registerEmailField = registerInputs[0]; 
            TMP_InputField registerPasswordField = registerInputs[1]; 
            
            RegisterUser(
                registerEmailField.text, 
                registerPasswordField.text, 
                gameObject.name, 
                nameof(OnAuthSuccess),
                nameof(OnAuthFailure)
            );
        } else {
            if (statusText != null) statusText.text = "Error: Faltan campos de entrada en el Panel de Registro.";
        }
    }

    // ----------------------------------------------------------------------
    // 6. MANEJO DE RESPUESTA (Callback de JS)
    // ----------------------------------------------------------------------

    public void OnAuthSuccess(string userId)
    {
        // ¡CLAVE! Guardamos el ID del usuario en el estado persistente
        currentUserId = userId; 
        
        if (statusText != null)
        {
            statusText.text = $"¡Sesión Iniciada! ID: {userId}";
        }
        Debug.Log("Usuario Autenticado: " + userId + ". Cargando escena principal.");
        
        // Navegamos al menú principal
        SceneManager.LoadScene("MainMenu"); 
    }

    public void OnAuthFailure(string errorMessage)
    {
        if (statusText != null)
        {
            statusText.text = "Error: " + errorMessage;
            Debug.LogError("Error de Autenticación: " + errorMessage);
        }
    }
}