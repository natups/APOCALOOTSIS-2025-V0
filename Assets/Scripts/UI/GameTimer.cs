using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    public float tiempoTotal = 60f;
    [HideInInspector] public float tiempoInicio; // Hacerla pública para la corrección del manager
    private float tiempoTranscurrido; 

    [Header("Referencias de UI")]
    public Canvas canvasPuntuacionJuego; 
    public Image panelOscuridad; 
    public GameObject panelResultadoFinal; 

    [Header("Visualización del Timer")]
    public TextMeshProUGUI textoTimer; 

    private bool juegoTerminado = false;

    void Start()
    {
        if (panelResultadoFinal != null)
        {
            panelResultadoFinal.SetActive(false);
        }

        if (panelOscuridad != null)
        {
            panelOscuridad.color = new Color(0, 0, 0, 0);
        }

        tiempoInicio = Time.time;
    }

    void Update()
    {
        if (juegoTerminado) return;

        tiempoTranscurrido = Time.time - tiempoInicio;

        float tiempoRestante = GetTimeRemaining(); 

        if (textoTimer != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }

        // LÓGICA DE OSCURIDAD
        float t = tiempoTranscurrido / tiempoTotal;
        t = Mathf.Clamp01(t); 

        if (panelOscuridad != null)
        {
            panelOscuridad.color = new Color(0, 0, 0, t);
        }

        // El Manager decide si el juego termina aquí.
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
            // Restar tiempo incrementando el 'tiempoInicio'
            tiempoInicio += cantidad; 
            Debug.Log("¡Penalización! Restados " + cantidad + " segundos.");
        }
    }

    // CRÍTICO: Método requerido por el Manager.
    public void DetenerTiempo()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        
        if (textoTimer != null) 
        {
            textoTimer.gameObject.SetActive(false); 
        }
    }
}