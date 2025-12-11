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
    
    // Nota: Asumo que ZonaDeEntregaManager es una clase existente
    private ZonaDeEntregaManager manager; 
    
    [Header("Referencias de UI y Visuales")]
    [Tooltip("El script que controla el visual de la Ventana. Necesario solo para la función DetenerTiempo().")]
    public VisualTimerController visualTimerController; 
    
    [Header("Visualización del Timer")]
    [Tooltip("El componente de texto que muestra el tiempo (00:00).")]
    public TextMeshProUGUI textoTimer; 
    
    [Header("HUD del Timer")]
    [Tooltip("Objeto padre del TextMeshPro y el fondo del temporizador para ocultar al inicio.")]
    public GameObject timerRootHUD; 

    private bool juegoTerminado = false;
    private bool isGameCounting = false; 
    
    // Propiedad calculada para el tiempo transcurrido
    private float tiempoTranscurrido => Time.time - tiempoInicio; 

    void Start()
    {
        // El temporizador DEBE empezar oculto y detenido.
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
    /// Llamado por el Manager al terminar la fase de memorización para empezar la cuenta.
    /// </summary>
    public void StartGame()
    {
        if (juegoTerminado || isGameCounting) return; 

        tiempoInicio = Time.time; 
        
        if (timerRootHUD != null)
        {
            timerRootHUD.SetActive(true);
        }

        isGameCounting = true; 
        Debug.Log("GameTimer: ¡Tiempo de juego iniciado!");
    }

    void Update()
    {
        if (juegoTerminado || !isGameCounting) 
        {
            return;
        }

        float tiempoRestante = GetTimeRemaining(); 

        // 1. LÓGICA DE FIN DE TIEMPO
        if (tiempoRestante <= 0f)
        {
            DetenerTiempo(); 
            
            if (manager != null)
            {
                manager.FinalizeGame(true); // Se acabó el tiempo
            }
            return;
        }
        
        // 2. ACTUALIZACIÓN DE UI (Digital)
        if (textoTimer != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    /// <summary>
    /// Devuelve el progreso del tiempo, de 1.0 (lleno/inicio) a 0.0 (vacío/fin).
    /// </summary>
    public float GetTimeProgress()
    {
        if (!isGameCounting || juegoTerminado) return 1f; // Si no cuenta, se considera lleno
        
        // Retorna el progreso que va disminuyendo con el tiempo.
        float progress = 1f - (tiempoTranscurrido / tiempoTotal);
        return Mathf.Clamp01(progress);
    }

    /// <summary>
    /// Devuelve el tiempo restante en segundos.
    /// </summary>
    public float GetTimeRemaining()
    {
        if (!isGameCounting) return tiempoTotal; 
        
        float tiempoRestante = tiempoTotal - tiempoTranscurrido;
        return Mathf.Max(0f, tiempoRestante);
    }
    
    /// <summary>
    /// Resta una cantidad de tiempo al contador (Penalización COOP).
    /// </summary>
    public void AplicarPenalizacion(float cantidad)
    {
        if (!juegoTerminado && isGameCounting)
        { 
            // Sumar al tiempo de inicio es equivalente a restar tiempo restante
            tiempoInicio += cantidad;  
            Debug.Log("¡Penalización! Restados " + cantidad + " segundos.");
        }
    }

    /// <summary>
    /// Detiene el tiempo y congela el estado.
    /// </summary>
    public void DetenerTiempo()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        isGameCounting = false; 
        
        if (timerRootHUD != null) 
        {
             timerRootHUD.SetActive(false);
        }
        
        // El GameTimer solo le dice al visual que se congele en el último sprite
        if (visualTimerController != null)
        {
            visualTimerController.StopVisuals();
        }
    }
}