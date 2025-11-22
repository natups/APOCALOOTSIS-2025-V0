using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices; 

public class AuthController : MonoBehaviour
{
    // ----------------------------------------------------------------------
    // 1. VARIABLES PÚBLICAS (Aparecen en el Inspector)
    // ----------------------------------------------------------------------
    
    // Paneles
    public GameObject loginPanel;
    public GameObject registerPanel;
    
    // Input Fields (¡Usamos GameObject para que puedas arrastrar el objeto!)
    // Recuerda arrastrar los inputs que están dentro del LoginPanel
    public GameObject emailInputObject; 
    public GameObject passwordInputObject;
    
    // Texto de Estado
    public Text statusText;

    // Variables internas para el componente InputField
    private InputField loginEmailField;
    private InputField loginPasswordField;


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
        // Al inicio, buscamos el componente InputField en los GameObjects que arrastraste.
        loginEmailField = emailInputObject.GetComponent<InputField>();
        loginPasswordField = passwordInputObject.GetComponent<InputField>();

        // Esto asegura que al inicio, solo se muestre el panel de Login
        ShowLoginPanel();
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        statusText.text = ""; 
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        statusText.text = "";
    }

    // ----------------------------------------------------------------------
    // 4. FUNCIONES LLAMADAS POR LOS BOTONES DE LA UI
    // ----------------------------------------------------------------------

    public void OnLoginClicked()
    {
        statusText.text = "Iniciando Sesión...";

        // Usamos los componentes InputField que encontramos en Start()
        SignInUser(
            loginEmailField.text, 
            loginPasswordField.text, 
            gameObject.name, 
            nameof(OnAuthSuccess),
            nameof(OnAuthFailure)
        );
    }

    public void OnRegisterClicked()
    {
        statusText.text = "Registrando usuario...";
        
        // ¡IMPORTANTE! Buscamos los Inputs directamente dentro del RegisterPanel.
        // Asumimos que el [0] es el Email y el [1] es el Password
        InputField regEmailInput = registerPanel.GetComponentsInChildren<InputField>()[0]; 
        InputField regPassInput = registerPanel.GetComponentsInChildren<InputField>()[1]; 
        
        RegisterUser(
            regEmailInput.text, 
            regPassInput.text, 
            gameObject.name, 
            nameof(OnAuthSuccess),
            nameof(OnAuthFailure)
        );
    }

    // ----------------------------------------------------------------------
    // 5. MANEJO DE RESPUESTA
    // ----------------------------------------------------------------------

    public void OnAuthSuccess(string userId)
    {
        statusText.text = $"¡Sesión Iniciada! ID: {userId}";
        Debug.Log("Usuario Autenticado: " + userId);
        
        // Aquí puedes cargar la escena del juego
    }

    public void OnAuthFailure(string errorMessage)
    {
        statusText.text = "Error: " + errorMessage;
        Debug.LogError("Error de Autenticación: " + errorMessage);
    }
}