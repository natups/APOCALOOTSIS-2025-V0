using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using System.Runtime.InteropServices; 
using UnityEngine.SceneManagement;

public class AuthController : MonoBehaviour
{
    // ----------------------------------------------------------------------
    // 1. VARIABLES PÚBLICAS (Aparecen en el Inspector)
    // ----------------------------------------------------------------------
    
    [Header("Panel References")]
    // Paneles de UI
    public GameObject loginPanel;
    public GameObject registerPanel;
    
    [Header("Login UI Elements")]
    // Inputs y Status Text del panel de Login
    public GameObject loginEmailInputObject; 
    public GameObject loginPasswordInputObject; 
    public TMPro.TextMeshProUGUI loginStatusText; // StatusText del Login
    
    [Header("Register UI Elements")] 
    // Inputs y Status Text del panel de Registro
    public GameObject registerEmailInputObject; 
    public GameObject registerPasswordInputObject; 
    public TMPro.TextMeshProUGUI registerStatusText; // StatusText del Register

    // Variables privadas para los COMPONENTES InputField
    private TMP_InputField loginEmailField; 
    private TMP_InputField loginPasswordField;
    private TMP_InputField registerEmailField; 
    private TMP_InputField registerPasswordField; 

    // VARIABLE CLAVE: Almacena el ID del usuario actual. Null si no está logueado.
    private string currentUserId = null; 

    // ----------------------------------------------------------------------
    // 2. DECLARACIÓN DEL PUENTE JAVASCRIPT (.jslib)
    // ----------------------------------------------------------------------
    
    [DllImport("__Internal")]
    private static extern void RegisterUser(string email, string password, string gameObject, string successCallback, string failureCallback);

    [DllImport("__Internal")]
    private static extern void SignInUser(string email, string password, string gameObject, string successCallback, string failureCallback);
    
    // NUEVO: Para Cerrar Sesión
    [DllImport("__Internal")]
    private static extern void SignOutUser(string gameObject, string successCallback, string failureCallback);

    // ----------------------------------------------------------------------
    // 3. START Y CONTROL DE PANELES
    // ----------------------------------------------------------------------
    
    void Start()
    {
        // --- Inicialización de Inputs de Login ---
        if (loginEmailInputObject != null)
        {
            loginEmailField = loginEmailInputObject.GetComponent<TMP_InputField>();
        }
        if (loginPasswordInputObject != null)
        {
            loginPasswordField = loginPasswordInputObject.GetComponent<TMP_InputField>();
        }
        
        // --- Inicialización de Inputs de Registro (NUEVO y CLAVE) ---
        if (registerEmailInputObject != null)
        {
            registerEmailField = registerEmailInputObject.GetComponent<TMP_InputField>();
        }
        if (registerPasswordInputObject != null)
        {
            registerPasswordField = registerPasswordInputObject.GetComponent<TMP_InputField>();
        }


        // ===================================================================
        // NAVEGACIÓN DESDE MAIN MENU
        // ===================================================================
        string initialPanel = PlayerPrefs.GetString("InitialPanel", "Login"); 

        if (initialPanel == "Register")
        {
            ShowRegisterPanel();
        }
        else // Si es "Login" o cualquier otro valor (por defecto)
        {
            ShowLoginPanel(); 
        }

        // Si venimos de un Logout, lo ejecutamos aquí antes de limpiar la preferencia
        if (PlayerPrefs.GetInt("RequestLogout", 0) == 1)
        {
            PlayerPrefs.DeleteKey("RequestLogout"); 
            RequestLogout();
        }
        
        PlayerPrefs.DeleteKey("InitialPanel");
        // ===================================================================
    }
    
    // Función auxiliar para obtener el campo de texto de estado activo
    private TMPro.TextMeshProUGUI GetActiveStatusText()
    {
        if (loginPanel != null && loginPanel.activeSelf && loginStatusText != null)
        {
            return loginStatusText;
        }
        if (registerPanel != null && registerPanel.activeSelf && registerStatusText != null)
        {
            return registerStatusText;
        }
        return null; // Devuelve null si no hay un texto de estado activo válido
    }

    public void ShowLoginPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (registerPanel != null) registerPanel.SetActive(false);
        // Limpia el texto de estado al cambiar de panel
        if (loginStatusText != null) loginStatusText.text = ""; 
    }

    public void ShowRegisterPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        // Limpia el texto de estado al cambiar de panel
        if (registerStatusText != null) registerStatusText.text = ""; 
    }

    // --- MÉTODO DE NAVEGACIÓN DE ESCENA ---

    // 1. FUNCIÓN PARA VOLVER AL MENÚ PRINCIPAL
    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); 
        
        // Nos suscribimos para notificar al MainMenu CUANDO la escena se cargue
        SceneManager.sceneLoaded += OnMainMenuLoaded;
    }
    
    private void OnMainMenuLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cancelamos la suscripción para que no se llame en otras cargas
        SceneManager.sceneLoaded -= OnMainMenuLoaded;
        
        // Solo si estamos en el MainMenu
        if (scene.name == "MainMenu")
        {
            MainMenu mainMenuUI = FindAnyObjectByType<MainMenu>();
            if (mainMenuUI != null)
            {
                // Notificamos al MainMenu si hay un usuario logueado o no
                mainMenuUI.UpdateAuthUI(!string.IsNullOrEmpty(currentUserId));
            }
        }
    }


    // ----------------------------------------------------------------------
    // 4. FUNCIONES LLAMADAS POR LOS BOTONES DE LA UI
    // ----------------------------------------------------------------------

    public void OnLoginClicked()
    {
        if (loginStatusText != null) loginStatusText.text = "Iniciando Sesión..."; // Usa el texto de Login

        if (loginEmailField != null && loginPasswordField != null)
        {
            // Ejecución en WebGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SignInUser(
                    loginEmailField.text, 
                    loginPasswordField.text, 
                    gameObject.name, 
                    nameof(OnAuthSuccess),
                    nameof(OnAuthFailure)
                );
            }
            // Simulación para el Editor de Unity
            else
            {
                Invoke(nameof(SimulateLoginSuccess), 1.0f);
            }
        } else {
             if (loginStatusText != null) loginStatusText.text = "Error: Faltan referencias de campos de Login.";
        }
    }

    public void OnRegisterClicked()
    {
        if (registerStatusText != null) registerStatusText.text = "Registrando usuario..."; // Usa el texto de Register

        // Usamos los campos de registro inicializados en Start()
        if (registerEmailField != null && registerPasswordField != null)
        {
            // Ejecución en WebGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                RegisterUser(
                    registerEmailField.text, 
                    registerPasswordField.text, 
                    gameObject.name, 
                    nameof(OnAuthSuccess), 
                    nameof(OnAuthFailure)
                );
            }
            // Simulación para el Editor de Unity
            else
            {
                 Invoke(nameof(SimulateRegisterSuccess), 1.0f);
            }
        } else {
            if (registerStatusText != null) registerStatusText.text = "Error: Faltan referencias de campos de Registro.";
        }
    }
    
    // Inicia el proceso de Logout (Llamado al cargar la escena de Auth desde MainMenu)
    public void RequestLogout()
    {
        currentUserId = null; 

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SignOutUser(gameObject.name, nameof(OnLogoutSuccess), nameof(OnAuthFailure));
        }
        else
        {
            Invoke(nameof(SimulateLogoutSuccess), 0.5f);
        }
    }


    // ----------------------------------------------------------------------
    // 5. MANEJO DE RESPUESTA
    // ----------------------------------------------------------------------

    // Simulación para el Editor
    private void SimulateLoginSuccess() { OnAuthSuccess("simulated-user-id-123"); }
    private void SimulateRegisterSuccess() { OnAuthSuccess("simulated-user-id-456"); }
    private void SimulateLogoutSuccess() { OnLogoutSuccess("Logout successful (Simulated)"); }


    public void OnAuthSuccess(string userId)
    {
        currentUserId = userId; 
        
        // Usa el status text del panel que estaba activo al hacer clic
        TMPro.TextMeshProUGUI activeStatus = GetActiveStatusText();
        
        if (activeStatus != null)
        {
            activeStatus.text = $"¡Sesión Iniciada! ID: {userId}";
            Debug.Log("Usuario Autenticado: " + userId);
        }
        
        // Si hay éxito, vamos al menú principal
        GoBackToMainMenu();
    }

    // Callback de éxito de Logout
    public void OnLogoutSuccess(string unused)
    {
        currentUserId = null; 
        Debug.Log("Sesión cerrada correctamente.");
        
        // Volvemos a la vista de Login y mostramos un mensaje de éxito allí
        ShowLoginPanel(); 
        
        if (loginStatusText != null) 
        {
            loginStatusText.text = "Sesión cerrada correctamente. Inicia sesión de nuevo.";
        }
    }

    public void OnAuthFailure(string errorMessage)
    {
        currentUserId = null;

        // Usa el campo de estado que esté activo en el momento del error
        TMPro.TextMeshProUGUI activeStatus = GetActiveStatusText();

        if (activeStatus != null)
        {
            activeStatus.text = "Error: " + errorMessage;
            Debug.LogError("Error de Autenticación: " + errorMessage);
        }
    }
}