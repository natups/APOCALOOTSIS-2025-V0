using UnityEngine;
using UnityEngine.UI; 
using TMPro; // Necesario para usar TextMeshProUGUI

public class GameTimer : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    public float tiempoTotal = 60f;
    private float tiempoInicio;
    private float tiempoTranscurrido; 

    [Header("Referencias de UI")]
    public Canvas canvasPuntuacionJuego; 
    public Image panelOscuridad;         
    public GameObject panelResultadoFinal; 

    [Header("Visualización del Timer")]
    public TextMeshProUGUI textoTimer; // 

    // Variable para la penalización (ahora gestionada por ZonaDeEntrega, pero la mantenemos si GameTimer necesita usarla)
    // [HideInInspector] public float penalizacionTiempo = 5f; 

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

        // Empezar el temporizador
        tiempoInicio = Time.time;
    }

    void Update()
    {
        if (juegoTerminado) return;

        tiempoTranscurrido = Time.time - tiempoInicio;

        // CÁLCULO Y VISUALIZACIÓN DEL TIEMPO RESTANTE
        float tiempoRestante = tiempoTotal - tiempoTranscurrido;
        tiempoRestante = Mathf.Max(0f, tiempoRestante); 

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

        // Verificar si el tiempo se acabó
        if (tiempoTranscurrido >= tiempoTotal)
        {
            TerminarPartida();
        }
    }

    // Este método se llama desde ZonaDeEntrega para penalizar
    public void AplicarPenalizacion(float cantidad)
    {
        if (!juegoTerminado)
        {
            // Aumenta el tiempo transcurrido, lo que reduce el tiempo restante.
            tiempoInicio -= cantidad; 
            Debug.Log("¡Objeto Incorrecto! Penalización de " + cantidad + " segundos aplicada.");
        }
    }


    public void TerminarPartida()
    {
        juegoTerminado = true;
        
        if (panelOscuridad != null)
        {
            panelOscuridad.color = new Color(0, 0, 0, 1f);
        }

        if (canvasPuntuacionJuego != null) 
        {
            canvasPuntuacionJuego.gameObject.SetActive(false); 
        }
        
         if (textoTimer != null) 
        {
            textoTimer.gameObject.SetActive(false); 
        }

        if (panelResultadoFinal != null)
        {
            panelResultadoFinal.SetActive(true);
            
            // Llama a SendEndScreen() en ZonaDeEntrega para mostrar los resultados correctos
            // Esto es crucial para la lógica de fin de partida. Asumimos que GameTimer
            // tiene una referencia a ZonaDeEntrega si es necesario, o lo llama desde TerminarPartida.
        }
        
        Time.timeScale = 0f;
    }
}