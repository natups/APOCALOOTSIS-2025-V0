using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;
using UnityEngine.UI; 

public enum GameMode { COOP, VERSUS }

/// <summary>
/// Script principal que maneja la lógica de objetivos, puntuación, penalizaciones y el fin del juego.
/// </summary>
public class ZonaDeEntregaManager : MonoBehaviour
{
    // ==========================================================
    // SECCIÓN 1: REFERENCIAS DE SCRIPTS 
    // ==========================================================

    [Header("Referencias de Scripts")]
    public ObjectSpawner objectSpawner;
    public GameTimer gameTimer; 
    public PlayerController player1Controller;
    public PlayerController player2Controller;
    public DarknessController darknessController; 

    // ==========================================================
    // SECCIÓN 2: REFERENCIAS DE UI
    // ==========================================================
    
    [Header("Referencias de UI")]
    [Tooltip("Texto que muestra la cantidad de objetos entregados.")]
    public TextMeshProUGUI listaObjetivoText; 
    [Tooltip("Panel de la pantalla de fin de juego.")]
    public GameObject endScreenUI; 
    [Tooltip("Mensaje principal de la pantalla de fin de juego.")]
    public TextMeshProUGUI endScreenMessageText; 

    [Header("Referencias de Puntuación")]
    public TextMeshProUGUI p1ScoreText; 
    public TextMeshProUGUI p2ScoreText; 

    // ==========================================================
    // SECCIÓN 3: CONFIGURACIÓN DE JUEGO Y ESTADO
    // ==========================================================

    [Header("Configuración de Juego")]
    public GameMode currentMode = GameMode.COOP;
    [Tooltip("Cantidad total de objetos correctos que deben entregarse para ganar.")]
    public int totalObjectsToWin = 5; 
    [Tooltip("Cantidad de segundos a restar del tiempo al entregar un objeto incorrecto en modo COOP.")]
    public float coopTimePenaltyAmount = 3f; 
    
    private int objectsDeliveredCount = 0; 
    private int player1Score = 0; 
    private int player2Score = 0; 
    private bool gameOver = false;
    
    
    private void Start()
    {
        if (endScreenUI != null) endScreenUI.SetActive(false);

        if (objectSpawner != null)
        {
            objectSpawner.totalObjectsRequired = totalObjectsToWin;
            objectSpawner.InitializeSpawner();
        }
        else
        {
            Debug.LogError("Referencia a ObjectSpawner NO asignada. Asigna el Spawner en el Inspector.");
        }
        
        // CRÍTICO: Asigna la referencia de este manager al timer para que pueda finalizar el juego.
        if (gameTimer != null)
        {
            gameTimer.SetManager(this);
        }

        UpdateObjectiveUI();
        UpdateScoreUI();
        Time.timeScale = 1f; 
        
        if (darknessController != null)
        {
            darknessController.StartDarknessIncrease();
        }
    }
    
    /// <summary>
    /// Procesa la entrega de un objeto por parte de un jugador (llamado por CajaDrop.cs).
    /// </summary>
    /// <param name="player">El PlayerController que está entregando el objeto.</param>
    public void CheckDelivery(PlayerController player)
    {
        if (gameOver) return;

        GameObject heldObject = player.GetHeldObject();
        if (heldObject == null) return;
        
        ObjectData objectComponent = heldObject.GetComponent<ObjectData>();
        
        Object carriedObjectData = objectComponent?.data;

        if (carriedObjectData == null)
        {
            Debug.LogError("El objeto entregado no tiene ObjectData o su data es nula.");
            player.ClearHeldObject();
            Destroy(heldObject); 
            return;
        }

        // Comprueba si el ScriptableObject entregado está en la lista de objetivos restantes del Spawner.
        if (objectSpawner.requiredObjects.Contains(carriedObjectData))
        {
            // --- ENTREGA CORRECTA ---
            
            Debug.Log($"Entrega Correcta: {carriedObjectData.objectName}");

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
                FinalizeGame(false); // isTimeOut = false (Victoria por objetivo)
            }
            // CRÍTICO: Se eliminó la llamada a objectSpawner.SpawnInitialObjects()
        }
        else
        {
            // --- ENTREGA INCORRECTA (PENALIZACIÓN) ---
            
            Debug.Log($"Objeto INCORRECTO: {carriedObjectData.objectName}. Aplicando penalización.");

            if (currentMode == GameMode.COOP)
            {
                if (gameTimer != null)
                {
                    gameTimer.AplicarPenalizacion(coopTimePenaltyAmount);
                }
                
                if (darknessController != null)
                {
                    darknessController.FlashPenalty(); 
                }
            }
            
            player.ApplySlowPenalty(); 
            
            player.ClearHeldObject();
            objectSpawner.RemoveObjectFromList(heldObject);
            
            // CRÍTICO: Se eliminó la llamada a objectSpawner.SpawnInitialObjects()
        }
        
        // CRÍTICO: Se eliminó la verificación de tiempo aquí. El GameTimer ahora maneja el timeout.
    }

    // ==========================================================
    // LÓGICA DE FIN DE PARTIDA
    // ==========================================================

    /// <summary>
    /// Finaliza el juego, congela el tiempo y muestra la pantalla de resultados.
    /// </summary>
    /// <param name="isTimeOut">True si se terminó por tiempo, False si fue por objetivo.</param>
    public void FinalizeGame(bool isTimeOut)
    {
        if (gameOver) return;
        gameOver = true;
        
        Time.timeScale = 0f; 
        
        if (gameTimer != null) gameTimer.DetenerTiempo(); 
        if (objectSpawner != null) objectSpawner.StopSpawning();
        if (darknessController != null) darknessController.StopDarknessIncrease(); 

        string finalMessage = isTimeOut 
            ? "¡TIEMPO AGOTADO! No lograron entregar todos los objetos a tiempo." 
            : "¡MISIÓN CUMPLIDA! ¡VICTORIA COOPERATIVA!";
        
        if (endScreenUI != null) endScreenUI.SetActive(true);
        if (endScreenMessageText != null) endScreenMessageText.text = finalMessage;
        
        Debug.Log("PARTIDA TERMINADA: " + finalMessage);
    }

    // ==========================================================
    // LÓGICA DE UI ADICIONAL
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