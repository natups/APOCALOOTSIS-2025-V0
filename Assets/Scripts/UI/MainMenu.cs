using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Configuración de Paneles")]
    [Tooltip("Arrastra aquí el objeto padre que contiene tus botones del menú (Jugar, Ajustes, etc)")]
    public GameObject panelMenuPrincipal;

    [Tooltip("Arrastra aquí el Panel que tiene la imagen de los controles")]
    public GameObject panelControles;

    private void OnEnable()
    {
        // Nos aseguramos de que al iniciar, el menú se vea y los controles estén ocultos
        if(panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if(panelControles != null) panelControles.SetActive(false);

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

    // --- MÉTODOS DE NAVEGACIÓN (Jugar/Salir) ---

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
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    // --- NUEVOS MÉTODOS PARA CONTROLES ---

    public void AbrirControles()
    {
        // 1. Ocultamos los botones del menú para que no molesten
        if(panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        
        // 2. Mostramos la imagen de los controles
        if(panelControles != null) panelControles.SetActive(true);
    }

    public void CerrarControles()
    {
        // 1. Ocultamos los controles
        if(panelControles != null) panelControles.SetActive(false);

        // 2. Volvemos a mostrar el menú principal
        if(panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
    }
}