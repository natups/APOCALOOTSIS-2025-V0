using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    public float tiempoTotal = 60f;
    private float tiempoRestante;
    private bool isRunning = true;

    // ÚNICA referencia de UI en este script: el texto del reloj.
    [Header("Referencias de UI")]
    public TextMeshProUGUI textoTimer; 
    
    // Referencia al Manager para terminar el juego
    private ZonaDeEntregaManager zonaDeEntregaManager;

    void Start()
    {
        tiempoRestante = tiempoTotal;
        // Buscamos el componente ZonaDeEntregaManager en el mismo objeto
        zonaDeEntregaManager = GetComponent<ZonaDeEntregaManager>(); 
        isRunning = true;

        if (zonaDeEntregaManager == null)
        {
            Debug.LogError("ERROR: El script 'ZonaDeEntregaManager' no se encontró en el mismo GameObject. ¡Asegúrate de que el Manager está en el mismo objeto que GameTimer!");
            enabled = false;
        }
    }

    void Update()
    {
        if (isRunning && tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            
            if (textoTimer != null)
            {
                int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
                int segundos = Mathf.FloorToInt(tiempoRestante % 60f);
                textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            }
        }
        else if (isRunning && tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            isRunning = false;
            // Terminar el juego a través del Manager
            if (zonaDeEntregaManager != null)
            {
                zonaDeEntregaManager.FinalizeGame(true); // isTimeOut = true
            }
        }
    }
    
    // El Manager llama a estos métodos
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
    }
}