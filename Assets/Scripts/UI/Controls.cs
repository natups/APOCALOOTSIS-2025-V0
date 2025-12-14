using UnityEngine;
using UnityEngine.SceneManagement;

/// Script para la escena de controles.
/// Detecta desde dónde se abrió (MainMenu o fin de juego)
/// y vuelve al lugar correcto al cerrar.
public class Controls : MonoBehaviour
{
    public enum ControlsContext
    {
        MainMenu,
        EndGamePanel
    }

    // Guardamos contexto estático para que sea accesible desde EndGameScreenUI
    public static ControlsContext CurrentContext = ControlsContext.MainMenu;

    /// Llamar desde el botón de cerrar controles
    public void CloseControls()
    {
        if (CurrentContext == ControlsContext.EndGamePanel)
        {
            // Reactivar panel de fin de juego
            if (EndGameScreenUI.Instance != null)
            {
                EndGameScreenUI.Instance.endScreenPanel.SetActive(true);
                EndGameScreenUI.Instance.CurrentContext = EndGameScreenUI.SceneOpenContext.EndGamePanel;
                Time.timeScale = 0f;
            }

            // Regresamos a la escena de juego (asumimos que está en BuildSettings)
            SceneManager.LoadScene("SampleScene"); // <- reemplazar por tu escena de juego
        }
        else
        {
            // Regresamos al MainMenu
            SceneManager.LoadScene("MainMenu");
        }
    }
}
