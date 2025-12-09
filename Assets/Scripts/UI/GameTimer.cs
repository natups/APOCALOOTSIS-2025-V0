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
    private ZonaDeEntregaManager manager; 
    
    [Header("Referencias de UI y Visuales")]
    [Tooltip("El script que actualiza el sprite de la 'Ventana' visual.")]
    public VisualTimerController visualTimerController; 
    
    [Header("Visualización del Timer")]
    [Tooltip("El componente de texto que muestra el tiempo (00:00).")]
    public TextMeshProUGUI textoTimer; 
    
    [Header("HUD del Timer")]
    [Tooltip("Objeto padre del TextMeshPro y el fondo del temporizador para ocultar al inicio.")]
    public GameObject timerRootHUD; 

    private bool juegoTerminado = false;
    private bool isGameCounting = false; 

    void Start()
    {
        // El temporizador DEBE empezar oculto y detenido.
        if (timerRootHUD != null)
        {
            timerRootHUD.SetActive(false);
        }
        
        // Se asegura que el VisualTimerController esté en el estado inicial
        if (visualTimerController != null)
        {
             visualTimerController.StopVisuals(); // Muestra el sprite de tiempo agotado/inicial.
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

        float tiempoTranscurrido = Time.time - tiempoInicio;
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

        // 3. ACTUALIZACIÓN DE UI (Visual/Sprite)
        // timeProgress va de 1.0 (lleno) a 0.0 (vacío)
        float timeProgress = 1f - (tiempoTranscurrido / tiempoTotal);
        if (visualTimerController != null)
        {
            visualTimerController.UpdateVisualTimer(timeProgress);
        }
    }

    /// <summary>
    /// Devuelve el tiempo restante en segundos.
    /// </summary>
    public float GetTimeRemaining()
    {
        if (!isGameCounting) return tiempoTotal; 
        
        float tiempoRestante = tiempoTotal - (Time.time - tiempoInicio);
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
        
        // Congela el temporizador visual en el estado final
        if (visualTimerController != null)
        {
            visualTimerController.StopVisuals();
        }
    }
}