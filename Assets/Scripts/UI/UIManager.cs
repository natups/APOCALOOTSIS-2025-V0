using UnityEngine;
using UnityEngine.SceneManagement;

// Maneja opciones básicas de UI como el volumen general
// y la navegación de regreso al menú principal.
public class UIManager : MonoBehaviour
{
    // Nivel actual de volumen (0 a 1)
    public float volumeLevel = 1f;

    // Cantidad que se suma o resta al volumen
    public float volumeStep = 0.1f;

    // Disminuye el volumen
    public void DecreaseVolume()
    {
        volumeLevel -= volumeStep;
        SetAudioVolume();

        // Evita que baje de 0
        if (volumeLevel <= 0f) return;
    }

    // Aumenta el volumen
    public void IncreaseVolume()
    {
        volumeLevel += volumeStep;
        SetAudioVolume();

        // Evita que supere 1
        if (volumeLevel >= 1f) return;
    }

    // Aplica el volumen al AudioListener
    void SetAudioVolume()
    {
        // Fuerza el valor entre 0 y 1
        volumeLevel = Mathf.Clamp01(volumeLevel);
        AudioListener.volume = volumeLevel;
    }

    // Vuelve al menú principal
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
