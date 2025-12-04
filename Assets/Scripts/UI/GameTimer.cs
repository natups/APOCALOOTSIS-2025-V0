using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    public float tiempoTotal = 60f;
    [HideInInspector]
    public float tiempoRestante; 
    private bool isRunning = true;

    [Header("Referencias de UI")]
    public TextMeshProUGUI textoTimer; 
    
    [Header("Referencias de Scripts")]
    private ZonaDeEntregaManager zonaDeEntregaManager;
    // REFERENCIA AL CONTROLADOR VISUAL
    private VisualTimerController visualTimerController; 

    void Awake() 
    {
        // 1. Inicialización del Manager 
        zonaDeEntregaManager = GetComponent<ZonaDeEntregaManager>(); 
        if (zonaDeEntregaManager == null)
        {
            zonaDeEntregaManager = FindObjectOfType<ZonaDeEntregaManager>();
        }

        if (zonaDeEntregaManager == null)
        {
            Debug.LogError("GameTimer: El script 'ZonaDeEntregaManager' no se encontró.");
            enabled = false;
        }

        // 2. Inicialización del Controlador Visual
        visualTimerController = FindObjectOfType<VisualTimerController>();
        if (visualTimerController == null)
        {
            // Advertencia si la ventana visual no está
            Debug.LogWarning("GameTimer: No se encontró un VisualTimerController en la escena. La visualización de sprites del tiempo no funcionará.");
        }

        tiempoRestante = tiempoTotal;
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning || tiempoRestante <= 0)
        {
            if (tiempoRestante <= 0 && isRunning) // Se acabó el tiempo en este frame
            {
                tiempoRestante = 0;
                isRunning = false;
                if (zonaDeEntregaManager != null)
                {
                    zonaDeEntregaManager.FinalizeGame(true); 
                }
            }
            return;
        }

        // 1. Contar tiempo
        tiempoRestante -= Time.deltaTime;
        
        // 2. Actualización de UI numérica
        if (textoTimer != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60f);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }

        // 3. Comunicación a la Ventana 
        if (visualTimerController != null)
        {
            // Pasamos el porcentaje de tiempo restante (1.0 al inicio, 0.0 al final)
            float tiempoPorcentaje = tiempoRestante / tiempoTotal;
            visualTimerController.UpdateVisualTimer(tiempoPorcentaje);
        }
    }
    
    // Métodos llamados por el Manager
    public void AplicarPenalizacion(float cantidad)
    {
        if (isRunning)
        {
            tiempoRestante -= cantidad;
            if (tiempoRestante < 0)
            {
                tiempoRestante = 0;
            }
        }
    }

    public void DetenerTiempo()
    {
        isRunning = false;
        // ¡NUEVO! Asegura que la ventana muestre el estado final (vacío)
        if (visualTimerController != null)
        {
            visualTimerController.StopVisuals();
        }
    }
}