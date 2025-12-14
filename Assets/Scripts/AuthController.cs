using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class AuthController : MonoBehaviour
{
    // ----------------------------------------------------------------------
    // 1. VARIABLES PÚBLICAS (Inspector)
    // ----------------------------------------------------------------------

    // Paneles principales
    public GameObject loginPanel;
    public GameObject registerPanel;

    // ---------- LOGIN ----------
    public GameObject loginEmailInputObject;
    public GameObject loginPasswordInputObject;
    public TextMeshProUGUI loginStatusText;

    // ---------- REGISTER ----------
    public GameObject registerEmailInputObject;
    public GameObject registerPasswordInputObject;
    public TextMeshProUGUI registerStatusText;

    // Referencias internas
    private TMP_InputField loginEmailField;
    private TMP_InputField loginPasswordField;
    private TMP_InputField registerEmailField;
    private TMP_InputField registerPasswordField;

    // Usuario actual (null = no logueado)
    private string currentUserId = null;

    // ----------------------------------------------------------------------
    // 2. PUENTE JAVASCRIPT (WebGL)
    // ----------------------------------------------------------------------

    [DllImport("__Internal")]
    private static extern void RegisterUser(
        string email,
        string password,
        string gameObject,
        string successCallback,
        string failureCallback
    );

    [DllImport("__Internal")]
    private static extern void SignInUser(
        string email,
        string password,
        string gameObject,
        string successCallback,
        string failureCallback
    );

    // 👉 NUEVO: Login anónimo
    [DllImport("__Internal")]
    private static extern void SignInAnonymouslyUser(
        string gameObject,
        string successCallback,
        string failureCallback
    );

    [DllImport("__Internal")]
    private static extern void SignOutUser(
        string gameObject,
        string successCallback,
        string failureCallback
    );

    // ----------------------------------------------------------------------
    // 3. START – INICIALIZACIÓN
    // ----------------------------------------------------------------------

    void Start()
    {
        // Obtener InputFields
        if (loginEmailInputObject != null)
            loginEmailField = loginEmailInputObject.GetComponent<TMP_InputField>();

        if (loginPasswordInputObject != null)
            loginPasswordField = loginPasswordInputObject.GetComponent<TMP_InputField>();

        if (registerEmailInputObject != null)
            registerEmailField = registerEmailInputObject.GetComponent<TMP_InputField>();

        if (registerPasswordInputObject != null)
            registerPasswordField = registerPasswordInputObject.GetComponent<TMP_InputField>();

        // Panel inicial
        string initialPanel = PlayerPrefs.GetString("InitialPanel", "Login");

        if (initialPanel == "Register")
            ShowRegisterPanel();
        else
            ShowLoginPanel();

        // Logout forzado desde MainMenu
        if (PlayerPrefs.GetInt("RequestLogout", 0) == 1)
        {
            PlayerPrefs.DeleteKey("RequestLogout");
            RequestLogout();
        }

        PlayerPrefs.DeleteKey("InitialPanel");
    }

    // ----------------------------------------------------------------------
    // 4. UTILIDAD: TEXTO DE ESTADO ACTIVO
    // ----------------------------------------------------------------------

    private TextMeshProUGUI GetActiveStatusText()
    {
        if (loginPanel != null && loginPanel.activeSelf)
            return loginStatusText;

        if (registerPanel != null && registerPanel.activeSelf)
            return registerStatusText;

        return null;
    }

    // ----------------------------------------------------------------------
    // 5. CAMBIO DE PANELES
    // ----------------------------------------------------------------------

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);

        if (loginStatusText != null)
            loginStatusText.text = "";
    }

    public void ShowRegisterPanel()
    {
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);

        if (registerStatusText != null)
            registerStatusText.text = "";
    }

    // ----------------------------------------------------------------------
    // 6. VOLVER AL MAIN MENU
    // ----------------------------------------------------------------------

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        SceneManager.sceneLoaded += OnMainMenuLoaded;
    }

    private void OnMainMenuLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnMainMenuLoaded;

        if (scene.name == "MainMenu")
        {
            MainMenu menu = FindAnyObjectByType<MainMenu>();
            if (menu != null)
                menu.UpdateAuthUI(!string.IsNullOrEmpty(currentUserId));
        }
    }

    // ----------------------------------------------------------------------
    // 7. BOTONES LOGIN / REGISTER / INVITADO
    // ----------------------------------------------------------------------

    public void OnLoginClicked()
    {
        if (loginStatusText != null)
            loginStatusText.text = "Iniciando sesión...";

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
        else
        {
            Invoke(nameof(SimulateLoginSuccess), 1f);
        }
    }

    public void OnRegisterClicked()
    {
        if (registerStatusText != null)
            registerStatusText.text = "Registrando usuario...";

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
        else
        {
            Invoke(nameof(SimulateRegisterSuccess), 1f);
        }
    }

    // 👉 BOTÓN "ENTRAR COMO INVITADO"
    public void OnAnonymousLoginClicked()
    {
        TextMeshProUGUI status = GetActiveStatusText();
        if (status != null)
            status.text = "Entrando como invitado...";

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SignInAnonymouslyUser(
                gameObject.name,
                nameof(OnAuthSuccess),
                nameof(OnAuthFailure)
            );
        }
        else
        {
            Invoke(nameof(SimulateAnonymousSuccess), 1f);
        }
    }

    // ----------------------------------------------------------------------
    // 8. LOGOUT
    // ----------------------------------------------------------------------

    public void RequestLogout()
    {
        currentUserId = null;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SignOutUser(
                gameObject.name,
                nameof(OnLogoutSuccess),
                nameof(OnAuthFailure)
            );
        }
        else
        {
            Invoke(nameof(SimulateLogoutSuccess), 0.5f);
        }
    }

    // ----------------------------------------------------------------------
    // 9. RESPUESTAS
    // ----------------------------------------------------------------------

    // ===== SIMULACIONES (EDITOR) =====
    private void SimulateLoginSuccess() =>
        OnAuthSuccess("simulated-login-user");

    private void SimulateRegisterSuccess() =>
        OnAuthSuccess("simulated-register-user");

    private void SimulateAnonymousSuccess() =>
        OnAuthSuccess("simulated-anonymous-user");

    private void SimulateLogoutSuccess() =>
        OnLogoutSuccess("ok");

    // ===== ÉXITO LOGIN / REGISTER / INVITADO =====
    public void OnAuthSuccess(string userId)
    {
        currentUserId = userId;

        TextMeshProUGUI status = GetActiveStatusText();
        if (status != null)
            status.text = "Sesión iniciada correctamente";

        GoBackToMainMenu();
    }

    // ===== ÉXITO LOGOUT =====
    public void OnLogoutSuccess(string unused)
    {
        currentUserId = null;
        ShowLoginPanel();

        if (loginStatusText != null)
            loginStatusText.text = "Sesión cerrada.";
    }

    // ===== ERROR =====
    public void OnAuthFailure(string errorMessage)
    {
        currentUserId = null;

        TextMeshProUGUI status = GetActiveStatusText();
        if (status != null)
            status.text = "Error: " + errorMessage;
    }
}
