using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Controla la pantalla de fin de juego en modo VERSUS.
// Muestra quién ganó, los puntajes de cada jugador
// y gestiona los botones de navegación.
public class EndGameScreenVsUI : MonoBehaviour
{
    // =========================
    // PANEL PRINCIPAL
    // =========================

    // Panel que contiene todo el UI de fin de partida
    // Debe comenzar desactivado
    public GameObject endScreenPanel;

    // =========================
    // TEXTOS
    // =========================

    // Texto que indica quién ganó la partida
    public TextMeshProUGUI winLoseText;

    // Texto que muestra los objetos recolectados por cada jugador
    public TextMeshProUGUI collectedObjectsText;

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

        // Asignación de listeners de los botones
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButton);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnBackToMenuButton);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(OnControlsButton);
    }

    // =========================
    // MOSTRAR / OCULTAR UI
    // =========================

    // Muestra la pantalla de fin de juego con los resultados
    public void ShowEndScreenVs(string winnerText, int player1Score, int player2Score)
    {
        if (endScreenPanel == null) return;

        // Activar el panel principal
        endScreenPanel.SetActive(true);

        // Mostrar quién ganó
        if (winLoseText != null)
            winLoseText.text = winnerText;

        // Mostrar los puntajes de ambos jugadores
        if (collectedObjectsText != null)
            collectedObjectsText.text =
                $"Jugador 1: {player1Score} / 5\nJugador 2: {player2Score} / 5";
    }

    // Oculta la pantalla de fin de juego
    public void HideEndScreen()
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
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
