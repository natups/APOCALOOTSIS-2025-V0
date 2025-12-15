using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Controla la pantalla de fin de juego en modo VERSUS.
// Muestra quién ganó y gestiona los botones de navegación.
public class EndGameScreenVsUI : MonoBehaviour
{
    // =========================
    // ENUMERACIÓN DE RESULTADOS
    // =========================
    // Nota: Esta enumeración es usada por ZonaDeEntregaManager
    public enum GameResult
    {
        Player1Wins,
        Player2Wins,
        Draw
    }

    // =========================
    // PANEL PRINCIPAL
    // =========================

    // Panel que contiene todo el UI de fin de partida
    // Debe comenzar desactivado
    public GameObject endScreenPanel;

    // =========================
    // TEXTOS DE RESULTADO (EXPLÍCITAMENTE PUBLIC)
    // =========================

    // Textos separados para la traducción. Deben asignarse en el Inspector.
    public TextMeshProUGUI Player1WinsTxt; // Texto para: Jugador 1 Gana
    public TextMeshProUGUI Player2WinsTxt; // Texto para: Jugador 2 Gana
    public TextMeshProUGUI DrawTxt;        // Texto para: Empate

    // NOTA: Se eliminaron winLoseText y collectedObjectsText.

    // =========================
    // BOTONES
    // =========================

    public Button retryButton;
    public Button backToMenuButton;
    public Button controlsButton;

    private void Awake()
    {
        // Aseguramos que el panel esté oculto al iniciar la escena
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
            
        // Aseguramos que los textos de resultado estén ocultos al iniciar
        SetResultTextsActive(false, false, false);

        // Asignación de listeners de los botones
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButton);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnBackToMenuButton);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(OnControlsButton);
    }
    
    // Método auxiliar para activar/desactivar los textos
    private void SetResultTextsActive(bool p1Active, bool p2Active, bool drawActive)
    {
        // Activamos/desactivamos los gameObjects de los textos
        if (Player1WinsTxt != null) Player1WinsTxt.gameObject.SetActive(p1Active);
        if (Player2WinsTxt != null) Player2WinsTxt.gameObject.SetActive(p2Active);
        if (DrawTxt != null) DrawTxt.gameObject.SetActive(drawActive);
    }

    // =========================
    // MOSTRAR / OCULTAR UI
    // =========================

    // Muestra la pantalla de fin de juego, activando el texto de resultado correcto
    public void ShowEndScreenVs(GameResult result)
    {
        if (endScreenPanel == null) return;

        // Activar el panel principal
        endScreenPanel.SetActive(true);

        // Mostrar el texto de resultado correspondiente
        switch (result)
        {
            case GameResult.Player1Wins:
                SetResultTextsActive(true, false, false);
                break;
            case GameResult.Player2Wins:
                SetResultTextsActive(false, true, false);
                break;
            case GameResult.Draw:
                SetResultTextsActive(false, false, true);
                break;
        }
    }

    // Oculta la pantalla de fin de juego
    public void HideEndScreen()
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
            
        // Ocultamos todos los textos de resultado al ocultar la pantalla
        SetResultTextsActive(false, false, false);
    }

    // =========================
    // BOTONES
    // =========================

    // Reinicia la escena actual
    private void OnRetryButton()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // Vuelve al menú principal
    private void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Abre la escena de controles
    private void OnControlsButton()
    {
        SceneManager.LoadScene("Controls");
    }
}