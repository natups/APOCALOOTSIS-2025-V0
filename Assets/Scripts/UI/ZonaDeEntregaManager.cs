using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

// Definición de los modos de juego
public enum GameMode { COOP, VERSUS }

/// <summary>
/// Script principal que maneja la lógica de objetivos, puntuación, penalizaciones y el fin del juego.
/// </summary>
public class ZonaDeEntregaManager : MonoBehaviour
{
    // ==========================================================
    // SECCIÓN 1: SINGLETON INSTANCE
    // ==========================================================
    public static ZonaDeEntregaManager Instance { get; private set; }

    // ==========================================================
    // SECCIÓN 2: REFERENCIAS DE SCRIPTS 
    // ==========================================================
    
    [Header("Referencias de Scripts")]
    public ObjectSpawner objectSpawner;
    public GameTimer gameTimer; 
    public PlayerController player1Controller; // Asignar al Jugador 1
    public PlayerController player2Controller; // Asignar al Jugador 2
    public DarknessController darknessController; 
    public ObjectiveListUI objectiveListUI; 

    // ==========================================================
    // SECCIÓN 3: REFERENCIAS DE UI
    // ==========================================================
    
    [Header("Referencias de UI")]
    [Tooltip("Contenedor principal del HUD de juego (muestra contador, X/Y entregados, etc.).")]
    public GameObject inGameHUDContainer; 
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
    public int totalObjectsToWin = 5; 
    public float coopTimePenaltyAmount = 3f; // Segundos a restar al fallar en COOP
    
    private int objectsDeliveredCount = 0; 
    private int player1Score = 0; 
    private int player2Score = 0; 
    private bool gameOver = true; 
    
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 1. Inicializa el estado base de la UI (Todo oculto)
        if (endScreenUI != null) endScreenUI.SetActive(false);
        // CRÍTICO: Ocultar el HUD al inicio antes de la fase de memorización
        if (inGameHUDContainer != null) inGameHUDContainer.SetActive(false); 

        // 2. Inicializa el Spawner y selecciona objetivos
        if (objectSpawner != null)
        {
            objectSpawner.totalObjectsRequired = totalObjectsToWin;
            objectSpawner.InitializeSpawner();
            
            // 3. Inicializa el GameTimer
            if (gameTimer != null)
            {
                gameTimer.SetManager(this);
            }

            // 4. Inicia la fase de memorización
            if (objectiveListUI != null)
            {
                if (!objectiveListUI.gameObject.activeSelf)
                {
                    objectiveListUI.gameObject.SetActive(true);
                }
                
                // NOTA: Asumimos que ObjectSpawner.requiredObjects es List<Object>
                objectiveListUI.SetInitialObjectives(objectSpawner.requiredObjects);
                
                // CRÍTICO: Asignar el HUD al script de la lista para que pueda ocultarlo
                objectiveListUI.gameHUDContainer = inGameHUDContainer;

                objectiveListUI.ShowList();
            }
            else
            {
                Debug.LogError("Referencia a ObjectiveListUI NO asignada. Iniciando juego inmediatamente.");
                StartGamePhase(); 
            }
        }
        else
        {
            Debug.LogError("Referencia a ObjectSpawner NO asignada en el Manager.");
        }

        UpdateObjectiveUI();
        UpdateScoreUI();
    }
    
    /// <summary>
    /// CRÍTICO: Inicia la fase de juego (Llamado por ObjectiveListUI al terminar la memorización).
    /// </summary>
    public void StartGamePhase()
    {
        gameOver = false; 

        // 1. Reanudar el tiempo de juego
        Time.timeScale = 1f;
        
        // 2. Activa el HUD de juego (el objeto 'inGameHUDContainer')
        if (inGameHUDContainer != null) inGameHUDContainer.SetActive(true);
        
        // 3. Inicia los procesos de juego
        if (gameTimer != null) gameTimer.StartGame(); 
        if (objectSpawner != null) objectSpawner.StartSpawning();
        
        // 4. MODIFICACIÓN CRÍTICA: INICIAR EL CONTROL DE OSCURIDAD
        if (darknessController != null) 
        {
            darknessController.StartDarknessIncrease(); 
            Debug.Log("[MANAGER DEBUG] Llamado a StartDarknessIncrease.");
        }
        else
        {
            Debug.LogError("[MANAGER DEBUG] ERROR: La referencia a DarknessController es NULL.");
        }
        
        Debug.Log("Fase de Juego Iniciada y componentes activados.");
    }
    
    
    /// <summary>
    /// Procesa la entrega de un objeto por parte de un jugador a la zona de entrega.
    /// </summary>
    public void CheckDelivery(PlayerController player)
    {
        if (gameOver) return;

        GameObject heldObject = player.GetHeldObject();
        if (heldObject == null) return;
        
        // CRÍTICO: Obtener los datos usando el componente ObjectData y su campo 'data' (que es de tipo Object)
        ObjectData objectDataComponent = heldObject.GetComponent<ObjectData>();
        Object carriedObjectData = objectDataComponent?.data; 

        if (carriedObjectData == null)
        {
            Debug.LogError("El objeto entregado no tiene datos de ObjectData asignada. Destruyendo objeto.");
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
            Destroy(heldObject); // Elimina el objeto del mundo
            
            UpdateObjectiveUI();
            UpdateScoreUI();
            
            // Rellenar el mapa después de una entrega
            if (objectSpawner != null) objectSpawner.RefillObjectsOnScreen(); 
            
            if (objectsDeliveredCount >= totalObjectsToWin)
            {
                FinalizeGame(false); // Finaliza por victoria
            }
        }
        else
        {
            // --- ENTREGA INCORRECTA (PENALIZACIÓN) ---
            Debug.Log($"Objeto INCORRECTO: {carriedObjectData.objectName}. Aplicando penalización.");

            if (currentMode == GameMode.COOP)
            {
                if (gameTimer != null) gameTimer.AplicarPenalizacion(coopTimePenaltyAmount);
            }
            
            // CRÍTICO: Aplica la penalización de lentitud (SLOW) al jugador que falló.
            player.ApplySlowPenalty(); 
            
            player.ClearHeldObject();
            Destroy(heldObject); // Elimina el objeto del mundo
            
            // Rellenar el mapa después de un error
            if (objectSpawner != null) objectSpawner.RefillObjectsOnScreen(); 
        }
    }

    // ==========================================================
    // LÓGICA DE MODO DE JUEGO
    // ==========================================================
    /// <summary>
    /// Devuelve TRUE si el modo de juego está configurado como VERSUS.
    /// </summary>
    public bool IsVersusMode()
    {
        return currentMode == GameMode.VERSUS;
    }


    // ==========================================================
    // LÓGICA DE FIN DE PARTIDA
    // ==========================================================

    public void FinalizeGame(bool isTimeOut)
    {
        if (gameOver) return;
        gameOver = true;
        
        // Detiene todos los procesos
        if (gameTimer != null) gameTimer.DetenerTiempo(); 
        if (objectSpawner != null) objectSpawner.StopSpawning();
        // Detiene el aumento de oscuridad
        if (darknessController != null) darknessController.StopDarknessIncrease(); 

        string finalMessage = isTimeOut 
            ? "¡TIEMPO AGOTADO! No lograron entregar todos los objetos a tiempo." 
            : "¡MISIÓN CUMPLIDA! ¡VICTORIA COOPERATIVA!";
        
        // Ocultar el HUD de juego y mostrar la pantalla final
        if (inGameHUDContainer != null) inGameHUDContainer.SetActive(false);
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