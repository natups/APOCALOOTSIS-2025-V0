using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class AuthController : MonoBehaviour
{
    // CÓDIGO JS DllImport
    // Llama a la función implementada en FirebaseBridge.jslib
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void SignOutUser();
    #endif

    // Singleton y propiedades estáticas
    public static AuthController Instance;
    public static bool IsLoggedIn { get; private set; } = false;
    public static string CurrentUserId { get; private set; } = "";
    public static string CurrentUserEmail { get; private set; } = "";
    
    // Flag para verificar si el JS ha inicializado Firebase
    private bool firebaseServiceIsReady = false; 

    // -------------------------------------------------------------------
    // 1. GESTIÓN DEL SINGLETON
    // -------------------------------------------------------------------

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.name = "AuthController";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------
    // 2. FUNCIÓN DE LOGOUT (Llamada desde MainMenuUI)
    // -------------------------------------------------------------------

    public void Logout()
    {
        // 1. Llama a la función externa de JavaScript
        #if UNITY_WEBGL && !UNITY_EDITOR
        if (firebaseServiceIsReady)
        {
            Debug.Log("C#: Llamando a la función JS para cerrar sesión (a través de .jslib).");
            SignOutUser(); // Llama al JSLib
        }
        else
        {
            Debug.LogError("C#: Servicio Firebase no listo. Cerrando sesión solo localmente.");
        }
        #endif
        
        // 2. Limpiamos el estado en C# inmediatamente
        SetAuthStatus(false, "", "");
        
        // 3. Recargamos la escena del Menú Principal
        SceneManager.LoadScene("MainMenu");
    }

    // -------------------------------------------------------------------
    // 3. CALLBACK DESDE JAVASCRIPT (index.html)
    // -------------------------------------------------------------------

    public void OnFirebaseServiceReady(string message)
    {
        Debug.Log("C#: Mensaje de JS recibido: Firebase Service está listo.");
        firebaseServiceIsReady = true;
        UpdateAuthStatusUI(); 
    }
    
    // -------------------------------------------------------------------
    // 4. GESTIÓN DE ESTADO Y UI
    // -------------------------------------------------------------------

    public void SetAuthStatus(bool isLoggedIn, string userId, string userEmail)
    {
        IsLoggedIn = isLoggedIn;
        CurrentUserId = userId;
        CurrentUserEmail = userEmail;
        Debug.Log($"C#: Estado de Autenticación actualizado. Logeado: {IsLoggedIn}");
        UpdateAuthStatusUI();
    }
    
    // Función segura para actualizar la UI usando el Singleton
    public void UpdateAuthStatusUI()
    {
        // Solo actualiza la UI si existe una instancia de MainMenuUI
        MainMenuUI.Instance?.UpdateUIForAuthStatus();
    }
    
    // [Agrega aquí otras funciones como OnSignInOrRegisterSuccess, OnAuthError, etc.]
}