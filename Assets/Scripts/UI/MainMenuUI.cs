using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    // Singleton local para permitir que AuthController acceda a esta instancia.
    public static MainMenuUI Instance { get; private set; }

    [Header("Botones de Autenticación")]
    public Button LoginBtn;
    public Button RegisterBtn;
    public Button LogOutBtn;

    void Awake()
    {
        // Establecer la instancia tan pronto como se cargue el objeto.
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            // Opcional: Esto ayuda si tienes varios MainMenuUI por error.
            // Destroy(gameObject); 
        }
    }

    void Start()
    {
        // Aseguramos que el estado del UI sea correcto al inicio.
        UpdateUIForAuthStatus();
        
        // Configuramos el listener del botón Logout
        if (LogOutBtn != null)
        {
            LogOutBtn.onClick.RemoveAllListeners(); 
            
            AuthController authControllerInstance = AuthController.Instance;
            
            if (authControllerInstance != null)
            {
                // Vinculamos el botón a la función Logout del controlador
                LogOutBtn.onClick.AddListener(authControllerInstance.Logout);
            }
            else
            {
                Debug.LogError("MainMenuUI: No se encontró la instancia de AuthController para vincular el botón Logout.");
            }
        }
    }

    // Se llama cuando la escena se descarga (buena práctica)
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Función para actualizar la visibilidad de los botones
    public void UpdateUIForAuthStatus()
    {
        // Obtenemos el estado de autenticación del controlador.
        bool isLoggedIn = AuthController.IsLoggedIn; 

        // Mostrar/Ocultar botones de Login y Register
        if (LoginBtn != null) 
            LoginBtn.gameObject.SetActive(!isLoggedIn);

        if (RegisterBtn != null) 
            RegisterBtn.gameObject.SetActive(!isLoggedIn);

        // Mostrar/Ocultar botón de Logout
        if (LogOutBtn != null) 
            LogOutBtn.gameObject.SetActive(isLoggedIn);
    }
}