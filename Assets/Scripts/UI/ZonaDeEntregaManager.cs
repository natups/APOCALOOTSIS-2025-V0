using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;
using UnityEngine.UI; 

// Definición de los modos de juego (si se usan)
public enum GameMode { COOP, VERSUS }

/// <summary>
/// Script principal que maneja la lógica de objetivos, puntuación, penalizaciones y el fin del juego.
/// </summary>
public class ZonaDeEntregaManager : MonoBehaviour
{
    // ==========================================================
    // SECCIÓN 1: SINGLETON INSTANCE (Permite acceso global)
    // ==========================================================
    public static ZonaDeEntregaManager Instance { get; private set; }

    // ==========================================================
    // SECCIÓN 2: REFERENCIAS DE SCRIPTS 
    // ==========================================================
    
    [Header("Referencias de Scripts")]
    public ObjectSpawner objectSpawner;
    public GameTimer gameTimer; 
    public PlayerController player1Controller;
    public PlayerController player2Controller;
    public DarknessController darknessController; 
    
    [Tooltip("El script que controla la tabla de objetivos visibles al jugador.")]
    public ObjectiveListUI objectiveListUI; 

    // ==========================================================
    // SECCIÓN 3: REFERENCIAS DE UI
    // ==========================================================
    
    [Header("Referencias de UI")]
    [Tooltip("Texto que muestra la cantidad de objetos entregados (ej: 0/5).")]
    public TextMeshProUGUI listaObjetivoText; 
    [Tooltip("Panel de la pantalla de fin de juego.")]
    public GameObject endScreenUI; 
    [Tooltip("Mensaje principal de la pantalla de fin de juego.")]
    public TextMeshProUGUI endScreenMessageText; 

    [Header("Referencias de Puntuación")]
    public TextMeshProUGUI p1ScoreText; 
    public TextMeshProUGUI p2ScoreText; 

    // ==========================================================
    // SECCIÓN 4: CONFIGURACIÓN DE JUEGO Y ESTADO
    // ==========================================================

    [Header("Configuración de Juego")]
    public GameMode currentMode = GameMode.COOP;
    public int totalObjectsToWin = 5; // Total de objetos que deben ser entregados para ganar
    public float coopTimePenaltyAmount = 3f; // Segundos de penalización por error
    
    private int objectsDeliveredCount = 0; 
    private int player1Score = 0; 
    private int player2Score = 0; 
    private bool gameOver = false;
    
    
    private void Awake()
    {
        // Lógica Singleton para inicializar la instancia
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Destruye la instancia si ya existe otra
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Desactiva la pantalla de fin de juego al iniciar
        if (endScreenUI != null) endScreenUI.SetActive(false);

        // 1. Inicializa el Spawner y los objetivos aleatorios
        if (objectSpawner != null)
        {
            objectSpawner.totalObjectsRequired = totalObjectsToWin;
            objectSpawner.InitializeSpawner();
            
            // 2. Inicializa la UI de la lista de memorización
            if (objectiveListUI != null)
            {
                // *** CORRECCIÓN CRÍTICA DE ACTIVACIÓN ***
                // Forzamos la activación del GameObject que contiene el script ObjectiveListUI
                if (!objectiveListUI.gameObject.activeSelf)
                {
                    objectiveListUI.gameObject.SetActive(true);
                    Debug.Log("Manager: Activando GameObject 'ObjectiveList_Dynamic' para coroutine.");
                }
                
                DebugObjectiveList();
                // Pasa la lista de objetivos seleccionados a la UI
                objectiveListUI.SetInitialObjectives(objectSpawner.requiredObjects);
                
                // *** LLAMADA CRÍTICA FALTANTE ***
                // Inicia la fase de memorización (esto activa 'Lista', pausa el juego y usa la coroutine)
                objectiveListUI.ShowList();
            }
        }
        else
        {
            Debug.LogError("Referencia a ObjectSpawner NO asignada en el Manager.");
        }
        
        // Inicializa el resto de componentes de juego
        if (gameTimer != null)
        {
            gameTimer.SetManager(this);
        }

        UpdateObjectiveUI();
        UpdateScoreUI();
        
        // NOTA: El GameTimer y DarknessController NO deben iniciarse aquí.
        // Se inician DESPUÉS de que la lista se oculta.
    }
    
    /// <summary>
    /// Imprime la lista de objetivos seleccionados para depuración en la consola.
    /// </summary>
    private void DebugObjectiveList()
    {
        string debugList = "OBJETIVOS DE LA PARTIDA: ";
        // Aseguramos que objectSpawner y requiredObjects existan antes de acceder
        if (objectSpawner != null && objectSpawner.requiredObjects != null)
        {
            foreach (var obj in objectSpawner.requiredObjects)
            {
                // Aquí usamos 'objectName' asumiendo que tu clase 'Object' lo tiene.
                // Si 'Object' no tiene 'objectName', esto fallará.
                if (obj != null && obj.objectName != null) 
                {
                    debugList += obj.objectName + ", ";
                }
            }
        }
        Debug.Log(debugList.TrimEnd(',', ' '));
    }
    
    /// <summary>
    /// Procesa la entrega de un objeto por parte de un jugador a la zona de entrega.
    /// (Lógica de entrega omitida por ser igual a la anterior).
    /// </summary>
    public void CheckDelivery(PlayerController player)
    {
        if (gameOver) return;

        GameObject heldObject = player.GetHeldObject();
        if (heldObject == null) return;
        
        ObjectData objectComponent = heldObject.GetComponent<ObjectData>();
        Object carriedObjectData = objectComponent?.data; 

        if (carriedObjectData == null)
        {
            Debug.LogError("El objeto entregado no tiene ObjectData.");
            player.ClearHeldObject();
            Destroy(heldObject); 
            return;
        }

        // Verifica si el objeto entregado está en la lista de objetivos requeridos
        if (objectSpawner.requiredObjects.Contains(carriedObjectData))
        {
            // --- ENTREGA CORRECTA ---
            objectSpawner.RemoveFromObjective(carriedObjectData); 
            objectsDeliveredCount++; 
            if (player == player1Controller) { player1Score++; }
            else if (player == player2Controller) { player2Score++; }
            
            player.ClearHeldObject(); 
            objectSpawner.RemoveObjectFromList(heldObject);
            
            UpdateObjectiveUI();
            UpdateScoreUI();
            
            if (objectsDeliveredCount >= totalObjectsToWin)
            {
                FinalizeGame(false);
            }
        }
        else
        {
            // --- ENTREGA INCORRECTA (PENALIZACIÓN) ---
            Debug.Log($"Objeto INCORRECTO: {carriedObjectData.objectName}. Aplicando penalización.");

            if (currentMode == GameMode.COOP)
            {
                if (gameTimer != null) gameTimer.AplicarPenalizacion(coopTimePenaltyAmount);
                if (darknessController != null) darknessController.FlashPenalty(); 
            }
            
            player.ApplySlowPenalty(); 
            player.ClearHeldObject();
            objectSpawner.RemoveObjectFromList(heldObject);
        }
    }

    // ==========================================================
    // LÓGICA DE FIN DE PARTIDA
    // ==========================================================

    public void FinalizeGame(bool isTimeOut)
    {
        if (gameOver) return;
        gameOver = true;
        
        Time.timeScale = 0f; // Pausa el tiempo de juego
        
        // Detiene todos los procesos
        if (gameTimer != null) gameTimer.DetenerTiempo(); 
        if (objectSpawner != null) objectSpawner.StopSpawning();
        if (darknessController != null) darknessController.StopDarknessIncrease(); 

        string finalMessage = isTimeOut 
            ? "¡TIEMPO AGOTADO! No lograron entregar todos los objetos a tiempo." 
            : "¡MISIÓN CUMPLIDA! ¡VICTORIA COOPERATIVA!";
        
        // Muestra la pantalla final
        if (endScreenUI != null) endScreenUI.SetActive(true);
        if (endScreenMessageText != null) endScreenMessageText.text = finalMessage;
        
        Debug.Log("PARTIDA TERMINADA: " + finalMessage);
    }

    // ==========================================================
    // LÓGICA DE UI DE PUNTUACIÓN Y CONTADOR
    // ==========================================================

    private void UpdateObjectiveUI()
    {
        if (listaObjetivoText != null)
        {
            listaObjetivoText.text = $"{objectsDeliveredCount}/{totalObjectsToWin}";
        }
    }

    private void UpdateScoreUI()
    {
        if (p1ScoreText != null) p1ScoreText.text = "P1: " + player1Score;
        if (p2ScoreText != null) p2ScoreText.text = "P2: " + player2Score;
    }
}