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
    public Image panelOscuridad; 
    
    [Header("Visualización del Timer")]
    public TextMeshProUGUI textoTimer; 

    private bool juegoTerminado = false;

    void Start()
    {
        tiempoInicio = Time.time;
    }

    /// <summary>
    /// Asigna la referencia al Manager para poder finalizar el juego.
    /// </summary>
    public void SetManager(ZonaDeEntregaManager managerInstance)
    {
        manager = managerInstance;
    }

    void Update()
    {
        if (juegoTerminado) 
        {
            // CRÍTICO: Si el juego terminó, sal del Update para que no se ejecute NADA más, ni el cálculo del tiempo ni la oscuridad.
            return;
        }

        float tiempoTranscurrido = Time.time - tiempoInicio;
        float tiempoRestante = GetTimeRemaining(); 

        // 1. LÓGICA DE FIN DE TIEMPO
        if (tiempoRestante <= 0f)
        {
            // Si el tiempo se agotó, notifica al Manager para finalizar.
            if (manager != null)
            {
                manager.FinalizeGame(true);
            }
            else
            {
                // Si el manager es null, al menos detiene la ejecución local.
                juegoTerminado = true; 
            }
            return;
        }
        
        // 2. ACTUALIZACIÓN DE UI Y OSCURIDAD
        if (textoTimer != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }

        // LÓGICA DE OSCURIDAD: Usa el porcentaje de tiempo transcurrido
        float t = tiempoTranscurrido / tiempoTotal;
        t = Mathf.Clamp01(t); 

        if (panelOscuridad != null)
        {
            // El color va de transparente (0) a completamente negro (1) a medida que 't' aumenta.
            panelOscuridad.color = new Color(0, 0, 0, t);
        }
    }

    /// <summary>
    /// Devuelve el tiempo restante en segundos.
    /// </summary>
    public float GetTimeRemaining()
    {
        float tiempoRestante = tiempoTotal - (Time.time - tiempoInicio);
        return Mathf.Max(0f, tiempoRestante);
    }
    
    /// <summary>
    /// Resta una cantidad de tiempo al contador (Penalización COOP).
    /// </summary>
    public void AplicarPenalizacion(float cantidad)
    {
        if (!juegoTerminado)
        {
            // Restar tiempo incrementando el 'tiempoInicio' para simular que pasó más tiempo.
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
        
        // CRÍTICO: Congela la oscuridad en el estado final que tenía cuando terminó el juego.
        if (panelOscuridad != null)
        {
            // Asegura que la oscuridad no siga cambiando después del final
            float t = (Time.time - tiempoInicio) / tiempoTotal;
            t = Mathf.Clamp01(t); 
            panelOscuridad.color = new Color(0, 0, 0, t);
        }

        if (textoTimer != null) 
        {
            textoTimer.gameObject.SetActive(false); 
        }
    }
}