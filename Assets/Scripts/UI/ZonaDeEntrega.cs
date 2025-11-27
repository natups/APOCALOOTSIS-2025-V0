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
    public int cantidadObjetivos = 5; // Cantidad de objetivos para ganar (en VERSUS) o completar la misión (en COOP)

    [Header("Visualización UI")]
    public TextMeshProUGUI textoAciertos; // Usado en COOP
    public TextMeshProUGUI textoTotal; 
    public TextMeshProUGUI p1ScoreText; // Usado en VERSUS (P1: X / Y)
    public TextMeshProUGUI p2ScoreText; // Usado en VERSUS (P2: X / Y)

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
    public bool coopGame = true; // Define si es modo COOP (true) o VERSUS (false)
    
    // Referencias para la pantalla final (End Screen)
    [SerializeField] private GameObject endScreenUI; 
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
                gameTimer.AplicarPenalizacion(penalizacionTiempo); 
            }
            
            objetoSostenido.SetActive(false);
            
            ActualizarUI(); 
            return; 
        }

        objetoSostenido.SetActive(false);
        ActualizarUI();

        // 1. Lógica de victoria COOP (Total de aciertos)
        if (coopGame)
        {
            totalCount = player1Count + player2Count;
            if (aciertosFinales >= cantidadObjetivos && gameTimer != null)
            {
                Debug.Log("¡Victoria COOP! Misión Cumplida.");
                gameTimer.TerminarPartida(); 
                SendEndScreen();
            }
        } 
        // 2. Lógica de victoria VERSUS (Gana el primero que alcanza la meta)
        else 
        {
            if (player1Count >= cantidadObjetivos || player2Count >= cantidadObjetivos)
            {
                Debug.Log("¡Victoria VERSUS! Un jugador alcanzó la meta.");
                
                // Detenemos el timer, aunque la victoria no haya sido por tiempo.
                if (gameTimer != null)
                {
                    gameTimer.TerminarPartida(); 
                }
                SendEndScreen();
            }
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
             // UI COOP: Muestra el total de aciertos de ambos jugadores
             if (textoAciertos != null)
             {
                 textoAciertos.text = "Objetivos: " + aciertosFinales + " / " + cantidadObjetivos;
             }
             if (textoTotal != null) textoTotal.text = ""; 
             
             // Ocultar o limpiar UI de Versus
             if (p1ScoreText != null) p1ScoreText.text = ""; 
             if (p2ScoreText != null) p2ScoreText.text = "";
        }
        else // Modo VERSUS
        {
             // UI VERSUS: Muestra el progreso individual (X / Y)
             if (p1ScoreText != null) p1ScoreText.text = $"P1: {player1Count} / {cantidadObjetivos}";
             if (p2ScoreText != null) p2ScoreText.text = $"P2: {player2Count} / {cantidadObjetivos}";
             
             // Ocultar o limpiar UI de COOP
             if (textoAciertos != null) textoAciertos.text = ""; 
             if (textoTotal != null) textoTotal.text = ""; 
        }
    }
    
    // Método para mostrar la pantalla final
    public void SendEndScreen()
    {
        if (endScreenUI == null)
        {
            Debug.LogError("ERROR: endScreenUI no está asignado en ZonaDeEntrega.");
            return; 
        }

        string ganadorTexto = "Empate!";
        if (!coopGame)
        {
            // Modo VERSUS: Determinar el ganador
            if (player1Count >= cantidadObjetivos)
            {
                ganadorTexto = "¡Jugador 1 Gana!";
            }
            else if (player2Count >= cantidadObjetivos)
            {
                ganadorTexto = "¡Jugador 2 Gana!";
            }
            // Si el juego terminó por tiempo (aunque no debería), usamos el puntaje más alto
            else if (player1Count > player2Count)
            {
                 ganadorTexto = "¡Jugador 1 Gana!";
            }
            else if (player2Count > player1Count)
            {
                 ganadorTexto = "¡Jugador 2 Gana!";
            }
            
            if (player1TextoFinal != null) player1TextoFinal.text = player1Count.ToString();
            if (player2TextoFinal != null) player2TextoFinal.text = player2Count.ToString();
        } 
        else 
        {
            // Modo COOP: Mostrar aciertos y estado de la misión
            if (player1TextoFinal != null) player1TextoFinal.text = aciertosFinales.ToString() + " / " + cantidadObjetivos;
            if (player2TextoFinal != null) player2TextoFinal.gameObject.SetActive(false); 
            
            if (aciertosFinales >= cantidadObjetivos)
            {
                ganadorTexto = "¡Misión Cumplida!";
            } else {
                ganadorTexto = "¡Tiempo Agotado!"; 
            }
        }
        
        if (whoWins != null) whoWins.text = ganadorTexto;
        endScreenUI.SetActive(true);
    }
}