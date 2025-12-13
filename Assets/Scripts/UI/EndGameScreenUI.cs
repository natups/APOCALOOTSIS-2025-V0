using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del panel de fin de juego.
/// Permite mostrar resultados, actualizar botones y mantener contexto
/// para volver correctamente desde la escena de controles.
/// </summary>
public class EndGameScreenUI : MonoBehaviour
{
    // Singleton para acceso global
    public static EndGameScreenUI Instance { get; private set; }

    /// <summary>
    /// Enum que define desde dónde se abrió la escena de controles
    /// para saber a dónde volver al cerrarla.
    /// </summary>
    public enum SceneOpenContext
    {
        MainMenu,       // La escena controles fue abierta desde el menú principal
        EndGamePanel    // La escena controles fue abierta desde el panel de fin de juego
    }

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

    [HideInInspector]
    public SceneOpenContext CurrentContext = SceneOpenContext.MainMenu;

    private void Awake()
    {
        // Singleton pattern: asegurarse de que solo haya una instancia
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Ocultar el panel al iniciar
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
    }

    /// <summary>
    /// Muestra el panel final y configura textos y botones
    /// según si se ganó o se perdió.
    /// </summary>
    public void ShowEndScreen(bool won, int deliveredCount, int totalRequired)
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(true);

        // Pausar el tiempo mientras se muestra el panel
        Time.timeScale = 0f;

        // Actualizar textos
        if (titleText != null)
            titleText.text = won ? "¡Han ganado!" : "¡Han perdido!";

        if (collectedText != null)
            collectedText.text = $"Objetos recolectados: {deliveredCount}/{totalRequired}";

        // Configurar listeners de los botones
        SetupButtons();

        // Guardar contexto para saber a dónde volver desde controles
        CurrentContext = SceneOpenContext.EndGamePanel;
    }

    /// <summary>
    /// Configura los listeners de los botones del panel de fin de juego
    /// </summary>
    private void SetupButtons()
    {
        // Botón volver al menú principal
        if (backToMenuBtn != null)
        {
            backToMenuBtn.onClick.RemoveAllListeners();
            backToMenuBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            });
        }

        // Botón abrir controles
        if (controlsBtn != null)
        {
            controlsBtn.onClick.RemoveAllListeners();
            controlsBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;

                // Pasamos el contexto a la escena Controls
                Controls.CurrentContext = (CurrentContext == SceneOpenContext.EndGamePanel)
                    ? Controls.ControlsContext.EndGamePanel
                    : Controls.ControlsContext.MainMenu;

                SceneManager.LoadScene("Controls");
            });
        }


        // Botón reiniciar juego
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

    /// <summary>
    /// Oculta el panel de fin de juego
    /// </summary>
    public void HideEndScreen()
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);

        CurrentContext = SceneOpenContext.MainMenu;
    }

    /// <summary>
    /// Método que se puede llamar desde botones de "cerrar"
    /// Decide a dónde volver según contexto.
    /// </summary>
    public void CloseScene()
    {
        Time.timeScale = 1f;

        if (CurrentContext == SceneOpenContext.EndGamePanel)
        {
            // Mostrar el panel de fin de juego de nuevo
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
