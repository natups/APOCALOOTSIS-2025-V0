using UnityEngine;
using UnityEngine.SceneManagement;

/// Script genérico para botones de cerrar paneles
/// Puede cerrar paneles como Controles, Ajustes, etc.
public class CloseButtons : MonoBehaviour
{
    public void CloseScene()
    {
        // Si estamos en el contexto de fin de juego
        if (EndGameScreenUI.Instance != null &&
            EndGameScreenUI.Instance.CurrentContext == EndGameScreenUI.SceneOpenContext.EndGamePanel)
        {
            EndGameScreenUI.Instance.endScreenPanel.SetActive(true);
            Time.timeScale = 0f; // Mantener pausa
        }
        else
        {
            // Venimos del MainMenu o de otra escena
            SceneManager.LoadScene("MainMenu");
        }

        // Ocultamos el panel actual
        gameObject.transform.root.gameObject.SetActive(false);
    }
}
