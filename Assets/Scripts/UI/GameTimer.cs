using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Controla el tiempo de la partida.
// Maneja la cuenta regresiva, el HUD del timer
// y avisa al manager cuando el tiempo se termina.
public class GameTimer : MonoBehaviour
{
    // =========================
    // CONFIGURACIÓN DEL TIEMPO
    // =========================

    // Tiempo total de la partida en segundos
    public float tiempoTotal = 60f;

    // Momento en el que se inició el contador
    private float tiempoInicio;

    // Referencia al manager principal de la partida
    private ZonaDeEntregaManager manager;

    // =========================
    // REFERENCIAS VISUALES
    // =========================

    // Controlador visual del timer (barras, efectos, etc.)
    public VisualTimerController visualTimerController;

    // Texto que muestra el tiempo restante en formato MM:SS
    public TextMeshProUGUI textoTimer;

    // Objeto raíz del HUD del timer
    public GameObject timerRootHUD;

    // =========================
    // ESTADOS INTERNOS
    // =========================

    // Indica si el juego ya terminó
    private bool juegoTerminado = false;

    // Indica si el tiempo está contando
    private bool isGameCounting = false;

    // Tiempo transcurrido desde que comenzó la partida
    private float tiempoTranscurrido => Time.time - tiempoInicio;

    void Start()
    {
        // El HUD del timer debe comenzar oculto
        if (timerRootHUD != null)
        {
            timerRootHUD.SetActive(false);
        }
    }

    // Asigna la referencia al manager de la partida
    public void SetManager(ZonaDeEntregaManager managerInstance)
    {
        manager = managerInstance;
    }

    // Inicia la cuenta regresiva del juego
    public void StartGame()
    {
        // Evita reiniciar el timer si ya terminó o ya está contando
        if (juegoTerminado || isGameCounting) return;

        // Guarda el tiempo de inicio
        tiempoInicio = Time.time;

        // Activa el HUD del timer
        if (timerRootHUD != null)
            timerRootHUD.SetActive(true);

        // Marca que el tiempo comenzó a correr
        isGameCounting = true;
    }

    void Update()
    {
        // Si el juego terminó o el timer no está activo, no hace nada
        if (juegoTerminado || !isGameCounting) return;

        // Obtiene el tiempo restante
        float tiempoRestante = GetTimeRemaining();

        // Si el tiempo llegó a cero, finaliza la partida
        if (tiempoRestante <= 0f)
        {
            DetenerTiempo();
            manager?.FinalizeGame(true); // Finaliza por tiempo agotado
            return;
        }

        // Actualiza el texto del timer en pantalla
        if (textoTimer != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    // Devuelve el progreso del tiempo
    // 1.0 = recién iniciado / 0.0 = tiempo agotado
    public float GetTimeProgress()
    {
        if (!isGameCounting || juegoTerminado) return 1f;

        float progress = 1f - (tiempoTranscurrido / tiempoTotal);
        return Mathf.Clamp01(progress);
    }

    // Devuelve el tiempo restante en segundos
    public float GetTimeRemaining()
    {
        if (!isGameCounting) return tiempoTotal;
        return Mathf.Max(0f, tiempoTotal - tiempoTranscurrido);
    }

    // Detiene el tiempo y apaga el HUD
    public void DetenerTiempo()
    {
        // Evita ejecutar la lógica más de una vez
        if (juegoTerminado) return;

        juegoTerminado = true;
        isGameCounting = false;

        // Oculta el HUD del timer
        if (timerRootHUD != null)
            timerRootHUD.SetActive(false);

        // Detiene los efectos visuales del timer
        visualTimerController?.StopVisuals();
    }
}
