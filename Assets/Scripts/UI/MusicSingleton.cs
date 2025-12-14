using UnityEngine;

public class MusicaSingleton : MonoBehaviour
{
    public static MusicaSingleton instance; // Instancia pública para acceder globalmente

    private AudioSource miAudioSource; // Referencia al AudioSource

    void Awake()
    {
        // Revisamos si ya existe una instancia
        if (instance == null)
        {
            instance = this; // Asignamos esta instancia como la única
            DontDestroyOnLoad(gameObject); // No destruir este GameObject al cambiar de escena
            miAudioSource = GetComponent<AudioSource>(); // Obtenemos el AudioSource
        }
        else
        {
            // Si ya existe, destruimos este GameObject duplicado
            Destroy(gameObject);
        }
    }

    // Función para pausar o reanudar la música
    public void PausarMusica(bool pausar)
    {
        if (pausar)
        {
            miAudioSource.Pause(); // Pausa la música
        }
        else
        {
            miAudioSource.UnPause(); // Reanuda la música
        }
    }
}
