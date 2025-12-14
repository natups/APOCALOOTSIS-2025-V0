using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// Controla el panel de fin de juego.
/// Se encarga de mostrar el resultado de la partida, configurar los botones
/// y manejar el contexto para volver correctamente desde la escena de controles.
public class EndGameScreenUI : MonoBehaviour
{
    // ==============================
    // SINGLETON
    // ==============================

    // Permite acceder a este panel desde cualquier parte del juego
    public static EndGameScreenUI Instance { get; private set; }

    
    /// Define desde dónde se abrió la escena de controles,
    /// para saber a dónde volver al cerrarla.
    public enum SceneOpenContext
    {
        MainMenu,       // Se abrió desde el menú principal
        EndGamePanel    // Se abrió desde el panel de fin de juego
    }

    // ==============================
    // REFERENCIAS DE UI
    // ==============================

    [Header("Panel principal del final")]
    public GameObject endScreenPanel;

    [Header("Textos")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI collectedText;

    [Header("Botones")]
    public Button backToMenuBtn;
    public Button controlsBtn;
    public Button changeModeBtn;
    public Button retryBtn;

    // Contexto actual de apertura
    [HideInInspector]
    public SceneOpenContext CurrentContext = SceneOpenContext.MainMenu;

    private void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // El panel comienza oculto
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
    }

    /// Muestra el panel de fin de juego y configura textos y botones
    /// según el resultado de la partida.
    public void ShowEndScreen(bool won, int deliveredCount, int totalRequired)
    {
        // Activar el panel final
        if (endScreenPanel != null)
            endScreenPanel.SetActive(true);

        // Pausar el juego
        Time.timeScale = 0f;

        // ==============================
        // ACTUALIZACIÓN DE TEXTOS
        // ==============================

        if (titleText != null)
            titleText.text = won ? "¡Han ganado!" : "¡Han perdido!";

        if (collectedText != null)
            collectedText.text = $"Objetos recolectados: {deliveredCount}/{totalRequired}";

        // Configurar acciones de los botones
        SetupButtons();

        // Guardar el contexto actual
        CurrentContext = SceneOpenContext.EndGamePanel;
    }

    /// Configura los listeners de los botones del panel de fin de juego.
    private void SetupButtons()
    {
        // ==============================
        // BOTÓN: VOLVER AL MENÚ
        // ==============================
        if (backToMenuBtn != null)
        {
            backToMenuBtn.onClick.RemoveAllListeners();
            backToMenuBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            });
        }

        // ==============================
        // BOTÓN: CONTROLES
        // ==============================
        if (controlsBtn != null)
        {
            controlsBtn.onClick.RemoveAllListeners();
            controlsBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;

                // Pasar el contexto a la escena Controls
                Controls.CurrentContext =
                    (CurrentContext == SceneOpenContext.EndGamePanel)
                    ? Controls.ControlsContext.EndGamePanel
                    : Controls.ControlsContext.MainMenu;

                SceneManager.LoadScene("Controls");
            });
        }

        // ==============================
        // BOTÓN: REINTENTAR
        // ==============================
        if (retryBtn != null)
        {
            retryBtn.onClick.RemoveAllListeners();
            retryBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                ZonaDeEntregaManager.Instance.RestartGame();
            });
        }
    }

    /// Oculta el panel de fin de juego.
    public void HideEndScreen()
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);

        CurrentContext = SceneOpenContext.MainMenu;
    }

    /// Cierra la escena actual y decide a dónde volver
    /// según el contexto desde el que se abrió.
    public void CloseScene()
    {
        Time.timeScale = 1f;

        if (CurrentContext == SceneOpenContext.EndGamePanel)
        {
            // Volver a mostrar el panel final
            if (endScreenPanel != null)
                endScreenPanel.SetActive(true);
        }
        else
        {
            // Volver al menú principal
            SceneManager.LoadScene("MainMenu");
        }
    }
}
