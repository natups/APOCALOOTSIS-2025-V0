using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script encargado de actualizar la interfaz de usuario del menú principal 
/// en función del estado de la sesión (logeado/no logeado).
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    // ==============================================================
    // ESTOS SON LOS CAMPOS PÚBLICOS QUE APARECEN EN EL INSPECTOR
    // ==============================================================
    [Header("UI Elements (Botones Login/Register)")]
    public GameObject loginButton; 
    public GameObject registerButton; 
    
    [Header("UI Elements (Usuario Registrado)")]
    public GameObject logoutButton;
    // ==============================================================
    
    void Start()
    {
        // Al iniciar la escena, actualizamos el estado de la UI
        UpdateUIForAuthStatus();
    }

    /// <summary>
    /// CLAVE: Actualiza la UI para ocultar/mostrar elementos dependiendo del estado de la sesión.
    /// Esta función es llamada desde el AuthController cuando hay un cambio de escena.
    /// </summary>
    public void UpdateUIForAuthStatus()
    {
        // 1. Preguntamos al AuthController por el estado de la sesión
        // Es CLAVE que el AuthController sea Singleton y Persistente para que exista aquí.
        bool isLoggedIn = AuthController.IsLoggedIn(); 

        // 2. Ocultar/Mostrar botones de autenticación (Login/Register)
        if (loginButton != null)
        {
            loginButton.SetActive(!isLoggedIn);
        }
        if (registerButton != null)
        {
            registerButton.SetActive(!isLoggedIn);
        }

        // 3. Mostrar/Ocultar botón de Cerrar Sesión (LogOutBtn)
        if (logoutButton != null)
        {
            logoutButton.SetActive(isLoggedIn);
            
            if (isLoggedIn)
            {
                // Solo si el usuario está logeado, asignamos el evento al botón de Logout
                Button btn = logoutButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners(); 
                    
                    // Buscamos la única instancia del AuthController
                    AuthController authControllerInstance = AuthController.Instance;
                    if (authControllerInstance != null)
                    {
                        // Agregamos el listener para llamar a la función Logout
                        btn.onClick.AddListener(authControllerInstance.Logout); 
                    }
                }
            }
        }
        
        Debug.Log($"MainMenuUI: Estado de sesión: {(isLoggedIn ? "Iniciada" : "Cerrada")}. UI actualizada.");
    }
}