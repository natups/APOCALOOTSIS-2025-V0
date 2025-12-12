using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador de la pantalla final.
/// Usa exactamente el panel y los textos que existen en tu escena (Win&LoseTxt y CollectedObjectsTxt)
/// </summary>
public class EndGameScreenUI : MonoBehaviour
{
    [Header("Panel principal del final")]
    public GameObject endScreenPanel;

    [Header("Textos")]
    public TextMeshProUGUI titleText;          // Win&LoseTxt
    public TextMeshProUGUI collectedText;      // CollectedObjectsTxt

    [Header("Botones")]
    public Button backToMenuBtn;
    public Button controlsBtn;
    public Button changeModeBtn;

    private void Awake()
    {
        // Asegura que al iniciar el nivel, NO esté visible
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
    }

    /// <summary>
    /// Muestra el panel final, configurando los textos según victoria o derrota.
    /// </summary>
    public void ShowEndScreen(bool won, int deliveredCount, int totalRequired)
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(true);

        // Congela el juego
        Time.timeScale = 0f;

        // ↓↓↓ TEXTOS ↓↓↓
        if (titleText != null)
        {
            titleText.text = won ? "¡Han ganado!" : "¡Han perdido!";
        }

        if (collectedText != null)
        {
            collectedText.text = $"Objetos recolectados: {deliveredCount}/{totalRequired}";
        }

        // ↓↓↓ BOTONES ↓↓↓
        if (backToMenuBtn != null)
        {
            backToMenuBtn.onClick.RemoveAllListeners();
            backToMenuBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MenuScene");
            });
        }

        if (controlsBtn != null)
        {
            controlsBtn.onClick.RemoveAllListeners();
            controlsBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("ControlsScene");
            });
        }

        if (changeModeBtn != null)
        {
            changeModeBtn.onClick.RemoveAllListeners();
            changeModeBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("ModoScene");
            });
        }
    }
}
