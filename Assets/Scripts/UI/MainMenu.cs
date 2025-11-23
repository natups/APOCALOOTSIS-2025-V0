using UnityEngine;
using UnityEngine.SceneManagement;

// El nombre de la clase debe coincidir con el nombre del archivo: MainMenu
public class MainMenu : MonoBehaviour
{
    [Header("Configuración de Paneles")]
    [Tooltip("Arrastra aquí el objeto MainMenuUI (el que tiene los botones principales)")]
    public GameObject panelMenuPrincipal;

    [Tooltip("Arrastra aquí el Panel que tiene la imagen de los controles")]
    public GameObject panelControles;

    private void OnEnable()
    {
        // Se asegura de que el menú principal esté activo y los controles ocultos al inicio.
        if(panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if(panelControles != null) panelControles.SetActive(false);

        // Lógica de PlayerPrefs (se mantiene por si la necesitas)
        if (!PlayerPrefs.HasKey("hasStarted"))
        {
            PlayerPrefs.SetInt("hasStarted", 0);
            PlayerPrefs.Save();
            Debug.Log("Primera vez que se inicia el juego");
        }
        else
        {
            Debug.Log("No es la primera vez que se inicia el juego");
        }
    }

    // --- MÉTODOS DE NAVEGACIÓN DE ESCENAS ---

    // Esta función es llamada por los botones de Login y Registro.
    // Es PUBLIC y VOID, por eso aparece en el Inspector.
    public void LoadAuthenticationScene()
    {
        // Esto carga la escena con el nombre exacto "Authentication"
        SceneManager.LoadScene("Authentication"); 
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("ModeSelectMenu");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }
    
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // --- MÉTODOS DE PANELES (Controles) ---

    public void AbrirControles()
    {
        if(panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if(panelControles != null) panelControles.SetActive(true);
    }

    public void CerrarControles()
    {
        if(panelControles != null) panelControles.SetActive(false);
        if(panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
    }
}