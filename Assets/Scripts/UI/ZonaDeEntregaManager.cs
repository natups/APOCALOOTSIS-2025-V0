using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;
using UnityEngine.UI; 

public enum GameMode { COOP, VERSUS }

public class ZonaDeEntregaManager : MonoBehaviour
{
    // ==========================================================
    // SECCIÓN 1: REFERENCIAS DE SCRIPTS 
    // ==========================================================

    [Header("Referencias de Scripts")]
    public ObjectSpawner objectSpawner;
    public GameTimer gameTimer; 
    
    // Referencias externas (Jugadores)
    public PlayerController player1Controller;
    public PlayerController player2Controller;

    // ==========================================================
    // SECCIÓN 2: REFERENCIAS DE UI (¡TODAS AQUÍ!)
    // ==========================================================
    
    [Header("Referencias de UI")]
    [Tooltip("Texto principal para mostrar el objetivo actual.")]
    public TextMeshProUGUI listaObjetivoText; 
    [Tooltip("Panel de Imagen (Canvas) para efectos de penalización (Flash Oscuro).")]
    public Image panelOscuridad; 
    [Tooltip("El GameObject del panel de resultados final (Debe estar inactivo al inicio).")]
    public GameObject endScreenUI; 
    [Tooltip("Texto para el mensaje final ('¡Tiempo Agotado!', '¡Victoria!').")]
    public TextMeshProUGUI endScreenMessageText; 

    [Header("Referencias de Puntuación")]
    public TextMeshProUGUI p1ScoreText; 
    public TextMeshProUGUI p2ScoreText; 

    // ==========================================================
    // SECCIÓN 3: CONFIGURACIÓN DE JUEGO Y ESTADO
    // ==========================================================

    [Header("Configuración de Juego")]
    public GameMode currentMode = GameMode.COOP;
    public int totalObjectsToWin = 5; 
    public float coopTimePenaltyAmount = 3f; 
    
    private int player1Score = 0;
    private int player2Score = 0;
    private bool gameOver = false;

    private void Start()
    {
        if (endScreenUI != null)
        {
            endScreenUI.SetActive(false);
        }
        if (panelOscuridad != null)
        {
            panelOscuridad.gameObject.SetActive(false); 
        }
        
        UpdateObjectiveList();
        UpdateScoreUI();
        Time.timeScale = 1f; 
    }

    // ==========================================================
    // LÓGICA DE ENTREGA (Llamada desde CajaDrop.cs)
    // ==========================================================
    
    public void CheckDelivery(PlayerController player)
    {
        if (gameOver) return;

        GameObject heldObject = player.GetHeldObject();
        if (heldObject == null) return;
        
        ObjectData objectComponent = heldObject.GetComponent<ObjectData>();
        if (objectComponent == null || objectComponent.data == null)
        {
            Debug.LogError("El objeto entregado no tiene ObjectData o su data es nula.");
            return;
        }

        Object carriedObjectData = objectComponent.data;

        if (objectSpawner != null && objectSpawner.IsCurrentObjective(carriedObjectData))
        {
            // --- ENTREGA CORRECTA ---
            objectSpawner.ObjectDelivered(carriedObjectData);
            player.ClearHeldObject(); 
            Destroy(heldObject); 

            if (player == player1Controller) { player1Score++; }
            else if (player == player2Controller) { player2Score++; }

            UpdateScoreUI();
            
            if (player1Score + player2Score >= totalObjectsToWin)
            {
                FinalizeGame(false); 
                return;
            }

            UpdateObjectiveList();
        }
        else
        {
            // --- ENTREGA INCORRECTA ---
            
            if (gameTimer != null)
            {
                gameTimer.AplicarPenalizacion(coopTimePenaltyAmount);
                StartCoroutine(FlashOscuridad()); 
            }

            player.ApplySlowPenalty(); 
            
            player.ClearHeldObject();
            Destroy(heldObject);
        }
    }
    
    IEnumerator FlashOscuridad()
    {
        if (panelOscuridad != null)
        {
            panelOscuridad.gameObject.SetActive(true);
            Color baseColor = panelOscuridad.color;
            panelOscuridad.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.5f); 
            yield return new WaitForSeconds(0.15f); 
            
            panelOscuridad.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            yield return new WaitForSeconds(0.15f); 
            panelOscuridad.gameObject.SetActive(false); 
        }
    }


    // ==========================================================
    // LÓGICA DE FIN DE PARTIDA
    // ==========================================================

    public void FinalizeGame(bool isTimeOut)
    {
        if (gameOver) return;
        gameOver = true;
        
        Time.timeScale = 0f; 
        if (gameTimer != null) gameTimer.DetenerTiempo();

        string finalMessage = "";
        if (!isTimeOut)
        {
            finalMessage = "¡MISIÓN CUMPLIDA! ¡VICTORIA COOPERATIVA!";
        }
        else
        {
            finalMessage = "¡TIEMPO AGOTADO! No lograron entregar todos los objetos a tiempo.";
             
            if (panelOscuridad != null)
            {
                panelOscuridad.gameObject.SetActive(true);
                panelOscuridad.color = new Color(0f, 0f, 0f, 0.8f); 
            }
        }
        
        if (endScreenUI != null)
        {
            endScreenUI.SetActive(true);
        }

        if (endScreenMessageText != null)
        {
            endScreenMessageText.text = finalMessage;
        }
    }

    // ==========================================================
    // LÓGICA DE UI ADICIONAL
    // ==========================================================

    private void UpdateObjectiveList()
    {
        if (listaObjetivoText != null && objectSpawner != null && objectSpawner.GetCurrentObjectives().Count > 0)
        {
            // ASUMIMOS que el objeto objetivo tiene una propiedad objectName
            listaObjetivoText.text = "Objetivo: " + objectSpawner.GetCurrentObjectives()[0].objectName;
        }
    }

    private void UpdateScoreUI()
    {
        if (p1ScoreText != null) p1ScoreText.text = "P1: " + player1Score;
        if (p2ScoreText != null) p2ScoreText.text = "P2: " + player2Score;
    }
}