using UnityEngine;
using TMPro; 
using System.Collections.Generic;
using System.Linq; 

[RequireComponent(typeof(Collider2D))]
public class ZonaDeEntrega : MonoBehaviour
{
    // --- IMPORTANTE: Mantener en estático para que otros scripts (como GameTimer) puedan usarlo ---
    public static int aciertosFinales = 0; 

    [Header("Configuración de la Zona")]
    public int capacidadMaxima = 5; 
    public int cantidadObjetivos = 5; 

    [Header("Visualización UI")]
    public TextMeshProUGUI textoAciertos; 
    public TextMeshProUGUI textoTotal; 
    public TextMeshProUGUI p1ScoreText; 
    public TextMeshProUGUI p2ScoreText; 

    [Header("Referencias de Juego")]
    [SerializeField] private GameTimer gameTimer; 

    [Header("Lista de Objetivos")]
    [HideInInspector] public HashSet<GameObject> objetosObjetivoActuales = new HashSet<GameObject>(); 
    
    [Header("Penalización COOP")]
    public float penalizacionTiempo = 5f; 

    // Variables internas de puntuación y estado
    private List<GameObject> objetosEnZona = new List<GameObject>(); 
    public int player1Count = 0; 
    public int player2Count = 0; 
    public bool coopGame = true; 
    
    // Referencias para la pantalla final (End Screen)
    [SerializeField] private GameObject endScreenUI; // Panel que se activa al final
    [SerializeField] private TMP_Text player1TextoFinal;
    [SerializeField] private TMP_Text player2TextoFinal;
    [SerializeField] private TMP_Text whoWins;
    int totalCount = 0;

    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger) 
        {
            col.isTrigger = true;
        }
        
        aciertosFinales = 0;
        ActualizarUI();
    }

    // Esta función se activa AUTOMÁTICAMENTE cuando un collider se activa dentro de la zona
    private void OnTriggerEnter2D(Collider2D other)
    {
        int playerID = 0;
        
        if (other.gameObject.name == "Player 1") playerID = 1;
        else if (other.gameObject.name == "Player 2") playerID = 2;

        if (playerID != 0)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null && pc.isHolding)
            {
                GameObject objetoSostenido = pc.heldObject;
                pc.DropObject(); 
                EvaluarObjetoEntregado(objetoSostenido, playerID);
            }
        }
    }

    // Método central para procesar la entrega de un objeto
    private void EvaluarObjetoEntregado(GameObject objetoSostenido, int playerID)
    {
        if (objetoSostenido == null || objetosEnZona.Contains(objetoSostenido))
        {
            return; 
        }
        
        bool esCorrecto = objetosObjetivoActuales.Contains(objetoSostenido);

        if (esCorrecto)
        {
            if (playerID == 1) player1Count++;
            else if (playerID == 2) player2Count++;
            
            objetosEnZona.Add(objetoSostenido);
        }
        else // ¡Es incorrecto!
        {
            if (coopGame && gameTimer != null)
            {
                // Penalización de tiempo en COOP
                gameTimer.AplicarPenalizacion(penalizacionTiempo); 
            }
            
            objetoSostenido.SetActive(false);
            
            ActualizarUI(); 
            return; 
        }

        objetoSostenido.SetActive(false);
        ActualizarUI();

        // Verificar la condición de victoria COOP
        if (coopGame)
        {
            totalCount = player1Count + player2Count;
            if (aciertosFinales >= objetosObjetivoActuales.Count && gameTimer != null)
            {
                Debug.Log("¡Victoria COOP! Misión Cumplida.");
                gameTimer.TerminarPartida(); 
                SendEndScreen();
            }
        } 
        else 
        {
            // Lógica de victoria VERSUS
        }
    }

    private void ActualizarUI()
    {
        int aciertos = 0;
        
        foreach (GameObject obj in objetosEnZona)
        {
             if (objetosObjetivoActuales.Contains(obj))
             {
                 aciertos++;
             }
        }

        aciertosFinales = aciertos;

        // Actualización de la UI basada en el modo
        if (coopGame)
        {
             if (textoAciertos != null)
             {
                 textoAciertos.text = "Objetivos: " + aciertosFinales + " / " + cantidadObjetivos;
             }
             if (textoTotal != null) textoTotal.text = ""; 
        }
        else // Modo VERSUS
        {
             if (p1ScoreText != null) p1ScoreText.text = "P1: " + player1Count.ToString();
             if (p2ScoreText != null) p2ScoreText.text = "P2: " + player2Count.ToString();
             if (textoAciertos != null) textoAciertos.text = ""; 
        }
    }
    
    // Método para mostrar la pantalla final (CORRECCIÓN DEL ERROR CRÍTICO)
    public void SendEndScreen()
    {
        // VERIFICACIÓN CRÍTICA: Si el panel final no está asignado, salimos para evitar el crasheo.
        if (endScreenUI == null)
        {
            Debug.LogError("ERROR CRÍTICO: endScreenUI no está asignado en ZonaDeEntrega. ¡Asígnalo en el Inspector!");
            return; 
        }

        if (!coopGame)
        {
            // Modo VERSUS
            // Verificamos si los Textos están asignados antes de usarlos
            if (player1TextoFinal != null) player1TextoFinal.text = player1Count.ToString();
            if (player2TextoFinal != null) player2TextoFinal.text = player2Count.ToString();
            
            if (player1Count > player2Count)
            {
                if (whoWins != null) whoWins.text = "Jugador 1 Gana!";
            }
            else if (player2Count > player1Count)
            {
                if (whoWins != null) whoWins.text = "Jugador 2 Gana!";
            }
            else
            {
                if (whoWins != null) whoWins.text = "Empate!";
            }

            endScreenUI.SetActive(true); // Mostrar la pantalla final (La línea que antes causaba el error 186)
        } else {
            // Modo COOP
            if (player1TextoFinal != null) player1TextoFinal.text = aciertosFinales.ToString() + " / " + cantidadObjetivos;
            
            if (player2TextoFinal != null) player2TextoFinal.gameObject.SetActive(false); 
            
            if (aciertosFinales >= cantidadObjetivos)
            {
                if (whoWins != null) whoWins.text = "¡Misión Cumplida!";
            } else {
                if (whoWins != null) whoWins.text = "¡Tiempo Agotado!"; 
            }
            
            endScreenUI.SetActive(true);
        }
    }
}