using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controla el menú de configuración de audio.
// Permite modificar el volumen general y volver al menú principal.
public class SettingsMenu : MonoBehaviour
{
    // Mezclador de audio donde se controla el volumen general
    public AudioMixer audioMixer;

    // Slider usado para ajustar el volumen
    public Slider volumeSlider;

    // Botón para volver al menú principal
    public Button backToMenuButton;

    void Start()
    {
        // Recupera el volumen guardado previamente
        if (PlayerPrefs.HasKey("volume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("volume");
            volumeSlider.value = savedVolume;
            audioMixer.SetFloat("volume", savedVolume);
        }

        // Oculta el botón si el menú ya es el MainMenu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            backToMenuButton.gameObject.SetActive(false);
        }
        else
        {
            backToMenuButton.gameObject.SetActive(true);
        }
    }

    // Cambia el volumen del AudioMixer y guarda la preferencia
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
        PlayerPrefs.SetFloat("volume", volume);
    }

    // Vuelve al menú principal
    public void BackToMenu()
    {
        PlayerPrefs.SetInt("hasStarted", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }
}
