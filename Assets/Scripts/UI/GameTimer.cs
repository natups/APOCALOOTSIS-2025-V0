using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la cuenta regresiva del juego, el estado del tiempo y las penalizaciones.
/// </summary>
public class GameTimer : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    public float tiempoTotal = 60f;
    private float tiempoInicio;

    // Referencia al manager de la partida
    private ZonaDeEntregaManager manager;

    [Header("Referencias de UI y Visuales")]
    public VisualTimerController visualTimerController;

    [Header("Visualización del Timer")]
    public TextMeshProUGUI textoTimer;

    [Header("HUD del Timer")]
    public GameObject timerRootHUD;

    private bool juegoTerminado = false;
    private bool isGameCounting = false;

    // Tiempo transcurrido desde el inicio
    private float tiempoTranscurrido => Time.time - tiempoInicio;

    void Start()
    {
        // El temporizador DEBE empezar oculto
        if (timerRootHUD != null)
        {
            timerRootHUD.SetActive(false);
        }
    }

    /// <summary>
    /// Asigna la referencia al Manager.
    /// </summary>
    public void SetManager(ZonaDeEntregaManager managerInstance)
    {
        manager = managerInstance;
    }

    /// <summary>
    /// Inicia la cuenta regresiva
    /// </summary>
    public void StartGame()
    {
        if (juegoTerminado || isGameCounting) return;

        tiempoInicio = Time.time;

        if (timerRootHUD != null)
            timerRootHUD.SetActive(true);

        isGameCounting = true;
        Debug.Log("GameTimer: ¡Tiempo de juego iniciado!");
    }

    void Update()
    {
        if (juegoTerminado || !isGameCounting) return;

        float tiempoRestante = GetTimeRemaining();

        // Fin del tiempo
        if (tiempoRestante <= 0f)
        {
            DetenerTiempo();
            manager?.FinalizeGame(true); // Se acabó el tiempo
            return;
        }

        // Actualizar UI
        if (textoTimer != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    /// <summary>
    /// Progreso del tiempo de 1.0 (inicio) a 0.0 (fin)
    /// </summary>
    public float GetTimeProgress()
    {
        if (!isGameCounting || juegoTerminado) return 1f;
        float progress = 1f - (tiempoTranscurrido / tiempoTotal);
        return Mathf.Clamp01(progress);
    }

    /// <summary>
    /// Devuelve el tiempo restante en segundos
    /// </summary>
    public float GetTimeRemaining()
    {
        if (!isGameCounting) return tiempoTotal;
        return Mathf.Max(0f, tiempoTotal - tiempoTranscurrido);
    }

    /// <summary>
    /// Detiene el tiempo y el HUD
    /// </summary>
    public void DetenerTiempo()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        isGameCounting = false;

        if (timerRootHUD != null)
            timerRootHUD.SetActive(false);

        visualTimerController?.StopVisuals();
    }
}
