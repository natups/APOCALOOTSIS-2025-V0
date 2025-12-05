using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    public float tiempoTotal = 60f;
    private float tiempoInicio; 
    private ZonaDeEntregaManager manager; // Referencia al Manager
    
    [Header("Referencias de UI")]
    public Image panelOscuridad; // Este control DEBE ser movido a DarknessController si existe
    
    [Header("Visualización del Timer")]
    [Tooltip("El componente de texto que muestra el tiempo.")]
    public TextMeshProUGUI textoTimer; 
    
    // Campo para el objeto padre del HUD del temporizador (Texto + Fondo)
    [Header("HUD del Timer")]
    [Tooltip("Objeto padre del TextMeshPro y el fondo del temporizador para ocultar al inicio.")]
    public GameObject timerRootHUD; 

    private bool juegoTerminado = false;
    private bool isGameCounting = false; // Bandera para controlar cuándo empieza a contar

    void Start()
    {
        // El temporizador DEBE empezar oculto y detenido.
        if (timerRootHUD != null)
        {
            timerRootHUD.SetActive(false);
        }
        // Nota: Si el panelOscuridad se controla aquí, debe tener una visibilidad inicial mínima
        if (panelOscuridad != null)
        {
            panelOscuridad.color = new Color(0, 0, 0, 0);
        }
    }

    /// <summary>
    /// Asigna la referencia al Manager para poder finalizar el juego.
    /// </summary>
    public void SetManager(ZonaDeEntregaManager managerInstance)
    {
        manager = managerInstance;
    }
    
    /// <summary>
    /// Llamado por ObjectiveListUI al terminar la fase de memorización para empezar la cuenta.
    /// </summary>
    public void StartGame()
    {
        if (juegoTerminado || isGameCounting) return; // Evitar que se llame dos veces

        // 1. Establecer el punto de inicio real para el cálculo de tiempo restante.
        tiempoInicio = Time.time; 
        
        // 2. Mostrar el HUD del temporizador.
        if (timerRootHUD != null)
        {
            timerRootHUD.SetActive(true);
        }
        
        // 3. Empezar la cuenta.
        isGameCounting = true;
        Debug.Log("GameTimer: ¡Tiempo de juego iniciado!");
    }

    void Update()
    {
        // Solo ejecuta la lógica si el juego no terminó Y el contador está activo.
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
                manager.FinalizeGame(true); 
            }
            return;
        }
        
        // 2. ACTUALIZACIÓN DE UI
        if (textoTimer != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }

        // 3. LÓGICA DE OSCURIDAD: Si está aquí, la mantenemos.
        float t = tiempoTranscurrido / tiempoTotal;
        t = Mathf.Clamp01(t); 

        if (panelOscuridad != null)
        {
            panelOscuridad.color = new Color(0, 0, 0, t);
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
            tiempoInicio += cantidad; 
            Debug.Log("¡Penalización! Restados " + cantidad + " segundos.");
        }
    }

    /// <summary>
    /// Detiene el tiempo y congela el estado de la UI y la oscuridad.
    /// </summary>
    public void DetenerTiempo()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        isGameCounting = false; 
        
        // Congela la oscuridad en el estado final
        if (panelOscuridad != null)
        {
            float t = (Time.time - tiempoInicio) / tiempoTotal;
            t = Mathf.Clamp01(t); 
            panelOscuridad.color = new Color(0, 0, 0, t);
        }

        // Ocultar el HUD si ya no es necesario
        if (timerRootHUD != null) 
        {
             timerRootHUD.SetActive(false);
        }
    }
}