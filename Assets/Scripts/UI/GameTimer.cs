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
    public Canvas canvasPuntuacionJuego; // Canvas con el contador "0/5"
    public Image panelOscuridad;         // Panel Image que se vuelve negro
    public GameObject panelResultadoFinal; // GameObject que contiene la imagen de corazones y el texto

    [Header("Visualización del Timer")]
    // **NUEVA REFERENCIA:** Asigna tu objeto TextMeshProUGUI aquí en el Inspector
    public TextMeshProUGUI textoTimer; 

    private bool juegoTerminado = false;

    void Start()
    {
        // Aseguramos que el panel de resultado esté oculto al inicio
        if (panelResultadoFinal != null)
        {
            panelResultadoFinal.SetActive(false);
        }

        // Asegurarse de que el panel de oscuridad esté transparente
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

        // Calcula el tiempo transcurrido
        tiempoTranscurrido = Time.time - tiempoInicio;

        // **CÁLCULO DEL TIEMPO RESTANTE**
        float tiempoRestante = tiempoTotal - tiempoTranscurrido;
        // Aseguramos que el tiempo restante nunca sea negativo
        tiempoRestante = Mathf.Max(0f, tiempoRestante); 

        // **ACTUALIZACIÓN DEL TEXTO EN PANTALLA**
        if (textoTimer != null)
        {
            // Convierte los segundos restantes a formato "MM:SS" (Minutos:Segundos)
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }

        // --- LÓGICA DE OSCURIDAD ---
        // 't' es la proporción de tiempo transcurrido (de 0.0 a 1.0)
        float t = tiempoTranscurrido / tiempoTotal;
        t = Mathf.Clamp01(t); // Asegura que esté entre 0 y 1

        if (panelOscuridad != null)
        {
            // El componente alfa (t) controla la opacidad, volviéndose más oscuro a medida que 't' aumenta
            panelOscuridad.color = new Color(0, 0, 0, t);
        }

        // 5. Verificar si el tiempo se acabó
        if (tiempoTranscurrido >= tiempoTotal)
        {
            TerminarPartida();
        }
    }

    void TerminarPartida()
    {
        juegoTerminado = true;
        
        // 1. Asegurarse de que esté 100% oscuro
        if (panelOscuridad != null)
        {
            panelOscuridad.color = new Color(0, 0, 0, 1f);
        }

        // 2. Ocultar el contador de puntuación del juego ("0/5")
        if (canvasPuntuacionJuego != null) 
        {
            canvasPuntuacionJuego.gameObject.SetActive(false); 
        }

        // Ocultar el texto del timer al finalizar
         if (textoTimer != null) 
        {
            textoTimer.gameObject.SetActive(false); 
        }

        // 3. Mostrar el PanelResultadoFinal
        if (panelResultadoFinal != null)
        {
            panelResultadoFinal.SetActive(true);
            
            // 4. Actualizar el texto del resultado (usando la variable estática)
            TextMeshProUGUI textoFinal = panelResultadoFinal.GetComponentInChildren<TextMeshProUGUI>();
            if (textoFinal != null)
            {
                 // Nota: Esto asume que tienes acceso a la clase ZonaDeEntrega
                 textoFinal.text = "Recolectaron " + ZonaDeEntrega.aciertosFinales + "/5 Objetos";
            }
        }
        
        // 5. Pausar el juego 
        Time.timeScale = 0f;
    }
}