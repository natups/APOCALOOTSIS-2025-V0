using UnityEngine;
using UnityEngine.UI; 
using TMPro; // NECESARIA para usar TextMeshPro (TMP)
using System.Runtime.InteropServices; 

public class AuthController : MonoBehaviour
{
    // ----------------------------------------------------------------------
    // 1. VARIABLES PÚBLICAS (Aparecen en el Inspector)
    // ----------------------------------------------------------------------
    
    // Paneles de UI
    public GameObject loginPanel;
    public GameObject registerPanel;
    
    // Inputs (Usamos GameObject para la ASIGNACIÓN en el Inspector)
    // ¡ARRATRA EL OBJETO DE ENTRADA COMPLETO AQUÍ!
    public GameObject loginEmailInputObject; // CAMBIADO a GameObject
    public GameObject loginPasswordInputObject; // CAMBIADO a GameObject
    
    // Texto de Estado (TextMeshProUGUI funciona correctamente)
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

    // ----------------------------------------------------------------------
    // 3. START Y CONTROL DE PANELES
    // ----------------------------------------------------------------------
    
    void Start()
    {
        // En Start(), OBTENEMOS los componentes reales desde los GameObjects arrastrados
        if (loginEmailInputObject != null)
        {
            loginEmailField = loginEmailInputObject.GetComponent<TMP_InputField>();
        }
        if (loginPasswordInputObject != null)
        {
            loginPasswordField = loginPasswordInputObject.GetComponent<TMP_InputField>();
        }

        // Al iniciar el juego, mostramos el panel de Login (y ocultamos Register)
        ShowLoginPanel();
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        // Limpia el texto de estado al cambiar de panel
        if (statusText != null) statusText.text = ""; 
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        if (statusText != null) statusText.text = "";
    }

    // ----------------------------------------------------------------------
    // 4. FUNCIONES LLAMADAS POR LOS BOTONES DE LA UI
    // ----------------------------------------------------------------------

    public void OnLoginClicked()
    {
        if (statusText != null) statusText.text = "Iniciando Sesión...";

        // Usamos los campos privados que se inicializaron en Start()
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
             if (statusText != null) statusText.text = "Error: Faltan referencias o componentes InputField en el Inspector.";
        }
    }

    public void OnRegisterClicked()
    {
        if (statusText != null) statusText.text = "Registrando usuario...";
        
        // Buscamos los inputs del panel de registro.
        TMP_InputField[] registerInputs = registerPanel.GetComponentsInChildren<TMP_InputField>();
        
        if (registerInputs.Length >= 2)
        {
            // Asumimos [0] es Email y [1] es Password
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
    // 5. MANEJO DE RESPUESTA
    // ----------------------------------------------------------------------

    public void OnAuthSuccess(string userId)
    {
        if (statusText != null)
        {
            statusText.text = $"¡Sesión Iniciada! ID: {userId}";
            Debug.Log("Usuario Autenticado: " + userId);
        }
    }

    public void OnAuthFailure(string errorMessage)
    {
        if (statusText != null)
        {
            // Muestra el mensaje de error de Firebase
            statusText.text = "Error: " + errorMessage;
            Debug.LogError("Error de Autenticación: " + errorMessage);
        }
    }
}