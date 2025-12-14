using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ControlMusicaEscena : MonoBehaviour
{
    // ==============================
    // CONFIGURACIÓN DE LA MÚSICA
    // ==============================

    [Tooltip("Lista de canciones que se reproducirán en esta escena")]
    public AudioClip[] listaMusicaEscena;

    [Tooltip("Duración en segundos de cada canción antes de cambiar")]
    public float tiempoPorCancion = 120f;

    // ==============================
    // VARIABLES INTERNAS
    // ==============================

    private AudioSource miAudioSource;      // AudioSource de la escena
    private int indiceMusicaActual = 0;     // Índice de la canción actual
    private float temporizador;             // Controla cuándo cambiar de canción

    void Awake()
    {
        // Obtener el AudioSource del mismo GameObject
        miAudioSource = GetComponent<AudioSource>();

        // El script controla el cambio de canciones, no el AudioSource
        miAudioSource.loop = false;
        miAudioSource.playOnAwake = false;
    }

    void Start()
    {
        // ==============================
        // 1. PAUSAR MÚSICA GLOBAL (MENÚ)
        // ==============================

        if (MusicaSingleton.instance != null)
        {
            MusicaSingleton.instance.PausarMusica(true);
        }

        // ==============================
        // 2. INICIAR PLAYLIST DE LA ESCENA
        // ==============================

        if (listaMusicaEscena.Length > 0)
        {
            miAudioSource.clip = listaMusicaEscena[0];
            miAudioSource.Play();
            temporizador = tiempoPorCancion;
        }
    }

    void Update()
    {
        // Si no hay canciones configuradas, no se ejecuta la lógica
        if (listaMusicaEscena.Length == 0) return;

        // ==============================
        // CONTROL DEL TIEMPO POR CANCIÓN
        // ==============================

        temporizador -= Time.deltaTime;

        if (temporizador <= 0f)
        {
            SiguienteCancion();
        }
    }

    // ==============================
    // CAMBIO DE CANCIÓN
    // ==============================
    void SiguienteCancion()
    {
        // Avanza al siguiente índice
        indiceMusicaActual++;

        // Si llega al final, vuelve al inicio de la lista
        if (indiceMusicaActual >= listaMusicaEscena.Length)
        {
            indiceMusicaActual = 0;
        }

        // Reproduce la nueva canción
        miAudioSource.clip = listaMusicaEscena[indiceMusicaActual];
        miAudioSource.Play();

        // Reinicia el temporizador
        temporizador = tiempoPorCancion;
    }

    void OnDestroy()
    {
        // ==============================
        // REANUDAR MÚSICA GLOBAL
        // ==============================

        if (MusicaSingleton.instance != null)
        {
            MusicaSingleton.instance.PausarMusica(false);
        }
    }
}
