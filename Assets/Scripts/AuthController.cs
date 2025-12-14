using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class AuthController : MonoBehaviour
{
    // ----------------------------------------------------------------------
    // 1. VARIABLES PÚBLICAS (Inspector)
    // ----------------------------------------------------------------------

    // Paneles principales de la UI
    public GameObject loginPanel;
    public GameObject registerPanel;

    // ---------- LOGIN ----------
    public GameObject loginEmailInputObject;
    public GameObject loginPasswordInputObject;
    public TextMeshProUGUI loginStatusText; // Texto de estado (errores / éxito)

    // ---------- REGISTER ----------
    public GameObject registerEmailInputObject;
    public GameObject registerPasswordInputObject;
    public TextMeshProUGUI registerStatusText; // Texto de estado (errores / éxito)

    // Referencias internas a los TMP_InputField
    private TMP_InputField loginEmailField;
    private TMP_InputField loginPasswordField;
    private TMP_InputField registerEmailField;
    private TMP_InputField registerPasswordField;

    // ID del usuario actual
    // → null = no hay sesión iniciada
    private string currentUserId = null;

    // ----------------------------------------------------------------------
    // 2. PUENTE JAVASCRIPT (WebGL)
    // ----------------------------------------------------------------------

    // Registro de usuario (Firebase vía JS)
    [DllImport("__Internal")]
    private static extern void RegisterUser(
        string email,
        string password,
        string gameObject,
        string successCallback,
        string failureCallback
    );

    // Login de usuario
    [DllImport("__Internal")]
    private static extern void SignInUser(
        string email,
        string password,
        string gameObject,
        string successCallback,
        string failureCallback
    );

    // Logout
    [DllImport("__Internal")]
    private static extern void SignOutUser(
        string gameObject,
        string successCallback,
        string failureCallback
    );

    // ----------------------------------------------------------------------
    // 3. START – INICIALIZACIÓN GENERAL
    // ----------------------------------------------------------------------

    void Start()
    {
        // =========================
        // OBTENER INPUTFIELDS LOGIN
        // =========================
        if (loginEmailInputObject != null)
            loginEmailField = loginEmailInputObject.GetComponent<TMP_InputField>();

        if (loginPasswordInputObject != null)
            loginPasswordField = loginPasswordInputObject.GetComponent<TMP_InputField>();

        // =========================
        // OBTENER INPUTFIELDS REGISTER
        // =========================
        if (registerEmailInputObject != null)
            registerEmailField = registerEmailInputObject.GetComponent<TMP_InputField>();

        if (registerPasswordInputObject != null)
            registerPasswordField = registerPasswordInputObject.GetComponent<TMP_InputField>();

        // =========================
        // DESDE QUÉ PANEL ARRANCAMOS
        // =========================
        // Se guarda desde MainMenu para saber si entrar a Login o Register
        string initialPanel = PlayerPrefs.GetString("InitialPanel", "Login");

        if (initialPanel == "Register")
        {
            ShowRegisterPanel();
        }
        else
        {
            // Default: Login
            ShowLoginPanel();
        }

        // =========================
        // LOGOUT FORZADO DESDE MAINMENU
        // =========================
        // Si el MainMenu pidió logout, lo ejecutamos al cargar esta escena
        if (PlayerPrefs.GetInt("RequestLogout", 0) == 1)
        {
            PlayerPrefs.DeleteKey("RequestLogout");
            RequestLogout();
        }

        // Limpieza
        PlayerPrefs.DeleteKey("InitialPanel");
    }

    // ----------------------------------------------------------------------
    // 4. UTILIDAD: TEXTO DE ESTADO ACTIVO
    // ----------------------------------------------------------------------

    // Devuelve el texto correcto según el panel visible
    private TextMeshProUGUI GetActiveStatusText()
    {
        // Si estamos en Login
        if (loginPanel != null && loginPanel.activeSelf && loginStatusText != null)
            return loginStatusText;

        // Si estamos en Register
        if (registerPanel != null && registerPanel.activeSelf && registerStatusText != null)
            return registerStatusText;

        // Ningún texto válido
        return null;
    }

    // ----------------------------------------------------------------------
    // 5. CAMBIO DE PANELES
    // ----------------------------------------------------------------------

    public void ShowLoginPanel()
    {
        // Activa Login
        if (loginPanel != null) loginPanel.SetActive(true);

        // Oculta Register
        if (registerPanel != null) registerPanel.SetActive(false);

        // Limpia mensajes anteriores
        if (loginStatusText != null) loginStatusText.text = "";
    }

    public void ShowRegisterPanel()
    {
        // Activa Register
        if (registerPanel != null) registerPanel.SetActive(true);

        // Oculta Login
        if (loginPanel != null) loginPanel.SetActive(false);

        // Limpia mensajes anteriores
        if (registerStatusText != null) registerStatusText.text = "";
    }

    // ----------------------------------------------------------------------
    // 6. VOLVER AL MAIN MENU
    // ----------------------------------------------------------------------

    public void GoBackToMainMenu()
    {
        // Carga el MainMenu
        SceneManager.LoadScene("MainMenu");

        // Esperamos a que cargue para avisarle el estado de login
        SceneManager.sceneLoaded += OnMainMenuLoaded;
    }

    private void OnMainMenuLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nos desuscribimos para evitar llamadas futuras
        SceneManager.sceneLoaded -= OnMainMenuLoaded;

        // Solo actuamos si realmente es el MainMenu
        if (scene.name == "MainMenu")
        {
            MainMenu mainMenuUI = FindAnyObjectByType<MainMenu>();

            if (mainMenuUI != null)
            {
                // Si hay userId → está logueado
                mainMenuUI.UpdateAuthUI(!string.IsNullOrEmpty(currentUserId));
            }
        }
    }

    // ----------------------------------------------------------------------
    // 7. BOTONES DE LOGIN / REGISTER
    // ----------------------------------------------------------------------

    public void OnLoginClicked()
    {
        // Feedback inmediato
        if (loginStatusText != null)
            loginStatusText.text = "Iniciando sesión...";

        // Validamos que existan los campos
        if (loginEmailField != null && loginPasswordField != null)
        {
            // =========================
            // WEBGL (Firebase real)
            // =========================
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
            // =========================
            // EDITOR (simulado)
            // =========================
            else
            {
                Invoke(nameof(SimulateLoginSuccess), 1f);
            }
        }
        else
        {
            // Error de referencias
            if (loginStatusText != null)
                loginStatusText.text = "Error: Campos de Login no asignados.";
        }
    }

    public void OnRegisterClicked()
    {
        // Feedback inmediato
        if (registerStatusText != null)
            registerStatusText.text = "Registrando usuario...";

        // Validamos que existan los campos
        if (registerEmailField != null && registerPasswordField != null)
        {
            // =========================
            // WEBGL (Firebase real)
            // =========================
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
            // =========================
            // EDITOR (simulado)
            // =========================
            else
            {
                Invoke(nameof(SimulateRegisterSuccess), 1f);
            }
        }
        else
        {
            // Error de referencias
            if (registerStatusText != null)
                registerStatusText.text = "Error: Campos de Registro no asignados.";
        }
    }

    // ----------------------------------------------------------------------
    // 8. LOGOUT
    // ----------------------------------------------------------------------

    public void RequestLogout()
    {
        // Limpiamos sesión local
        currentUserId = null;

        // WebGL → Firebase real
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SignOutUser(
                gameObject.name,
                nameof(OnLogoutSuccess),
                nameof(OnAuthFailure)
            );
        }
        // Editor → simulado
        else
        {
            Invoke(nameof(SimulateLogoutSuccess), 0.5f);
        }
    }

    // ----------------------------------------------------------------------
    // 9. RESPUESTAS (CALLBACKS)
    // ----------------------------------------------------------------------

    // ===== SIMULACIONES =====
    private void SimulateLoginSuccess()
    {
        OnAuthSuccess("simulated-user-id-login");
    }

    private void SimulateRegisterSuccess()
    {
        OnAuthSuccess("simulated-user-id-register");
    }

    private void SimulateLogoutSuccess()
    {
        OnLogoutSuccess("logout");
    }

    // ===== ÉXITO LOGIN / REGISTER =====
    public void OnAuthSuccess(string userId)
    {
        // Guardamos el ID
        currentUserId = userId;

        // Mostramos mensaje en el panel activo
        TextMeshProUGUI activeStatus = GetActiveStatusText();

        if (activeStatus != null)
            activeStatus.text = "Sesión iniciada correctamente";

        Debug.Log("Usuario autenticado: " + userId);

        // Volvemos al menú principal
        GoBackToMainMenu();
    }

    // ===== ÉXITO LOGOUT =====
    public void OnLogoutSuccess(string unused)
    {
        currentUserId = null;

        Debug.Log("Sesión cerrada correctamente");

        // Volvemos a Login
        ShowLoginPanel();

        if (loginStatusText != null)
            loginStatusText.text = "Sesión cerrada. Iniciá sesión nuevamente.";
    }

    // ===== ERROR =====
    public void OnAuthFailure(string errorMessage)
    {
        currentUserId = null;

        // Mostramos error en el panel activo
        TextMeshProUGUI activeStatus = GetActiveStatusText();

        if (activeStatus != null)
        {
            activeStatus.text = "Error: " + errorMessage;
            Debug.LogError("Error de autenticación: " + errorMessage);
        }
    }
}
