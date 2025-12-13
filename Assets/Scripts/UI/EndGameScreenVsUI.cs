using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Script para controlar la pantalla de fin de juego en modo VERSUS.
/// Muestra quién ganó, los objetos recolectados por cada jugador,
/// y controla los botones de Retry, BackToMenu y Controls.
/// </summary>
public class EndGameScreenVsUI : MonoBehaviour
{
    [Header("Panel Principal")]
    public GameObject endScreenPanel; // Panel que contiene todo el UI, debe estar desactivado al inicio

    [Header("Textos de Fin")]
    public TextMeshProUGUI winLoseText; // Texto que dirá "Jugador 1 ganó", etc.
    public TextMeshProUGUI collectedObjectsText; // Texto que muestra objetos recolectados por cada jugador

    [Header("Botones")]
    public Button retryButton;
    public Button backToMenuButton;
    public Button controlsButton;

    private void Awake()
    {
        // Asegurarnos de que el panel esté oculto al inicio
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);

        // Asignar listeners a los botones
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButton);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnBackToMenuButton);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(OnControlsButton);
    }

    /// <summary>
    /// Mostrar la pantalla de fin de juego con los resultados.
    /// </summary>
    /// <param name="winnerText">Texto que indica quién ganó</param>
    /// <param name="player1Score">Cantidad de objetos correctos del jugador 1</param>
    /// <param name="player2Score">Cantidad de objetos correctos del jugador 2</param>
    public void ShowEndScreenVs(string winnerText, int player1Score, int player2Score)
    {
        if (endScreenPanel == null) return;

        endScreenPanel.SetActive(true);

        if (winLoseText != null)
            winLoseText.text = winnerText;

        if (collectedObjectsText != null)
            collectedObjectsText.text = $"Jugador 1: {player1Score} objetos\nJugador 2: {player2Score} objetos";
    }

    /// <summary>
    /// Oculta la pantalla de fin de juego
    /// </summary>
    public void HideEndScreen()
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
    }

    #region Botones
    private void OnRetryButton()
    {
        // Reinicia la misma escena
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void OnBackToMenuButton()
    {
        // Asumimos que el menú principal es la escena "MainMenu"
        SceneManager.LoadScene("MainMenu");
    }

    private void OnControlsButton()
    {
        SceneManager.LoadScene("Controls");
    }
    #endregion
}
