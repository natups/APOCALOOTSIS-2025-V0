using UnityEngine;
using System.Collections.Generic;
using TMPro;

// ===============================
// ENUM DE MODOS DE JUEGO
// ===============================
public enum GameMode { COOP, VERSUS }

public class ZonaDeEntregaManager : MonoBehaviour
{
    public static ZonaDeEntregaManager Instance { get; private set; }

    [Header("Referencias de Scripts")]
    public ObjectSpawner objectSpawner;
    public GameTimer gameTimer;
    public PlayerController player1Controller;
    public PlayerController player2Controller;
    public DarknessController darknessController;
    public ObjectiveListUI objectiveListUI;

    [Header("End Game UI")]
    public EndGameScreenUI endGameScreenUI;
    public EndGameScreenVsUI endGameVsScreenUI;

    [Header("UI")]
    public GameObject inGameHUDContainer;
    public TextMeshProUGUI listaObjetivoText;
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    [Header("Configuración de juego")]
    public GameMode currentMode = GameMode.COOP;
    public int totalObjectsToWin = 5;

    private int objectsDeliveredCount = 0;
    private int player1Score = 0;
    private int player2Score = 0;
    private bool gameOver = true;

    // ===============================
    // SINGLETON & INICIALIZACIÓN
    // ===============================
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ocultar HUD y panel final al inicio
        inGameHUDContainer?.SetActive(false);
        endGameScreenUI?.HideEndScreen();
        endGameVsScreenUI?.HideEndScreen();

        // Inicializar spawner y objetivos
        if (objectSpawner != null)
        {
            objectSpawner.totalObjectsRequired = totalObjectsToWin;
            objectSpawner.InitializeSpawner();

            // Inicializar timer
            gameTimer?.SetManager(this);

            // Mostrar fase de memorización con ObjectiveListUI
            if (objectiveListUI != null)
            {
                if (!objectiveListUI.gameObject.activeSelf)
                    objectiveListUI.gameObject.SetActive(true);

                objectiveListUI.SetInitialObjectives(objectSpawner.requiredObjects);
                objectiveListUI.gameHUDContainer = inGameHUDContainer;
                objectiveListUI.ShowList();
            }
            else
            {
                Debug.LogWarning("ObjectiveListUI no asignada. Iniciando juego directamente.");
                StartGamePhase();
            }
        }
        else
        {
            Debug.LogError("ObjectSpawner no asignado en el manager.");
        }

        UpdateObjectiveUI();
        UpdateScoreUI();
    }

    // ===============================
    // INICIO DE FASE DE JUEGO
    // ===============================
    public void StartGamePhase()
    {
        gameOver = false;
        Time.timeScale = 1f;

        // Ocultar lista de objetivos
        objectiveListUI?.gameObject.SetActive(false);

        // Mostrar HUD
        inGameHUDContainer?.SetActive(true);

        // Iniciar spawner y timer
        objectSpawner?.StartSpawning();
        gameTimer?.StartGame();

        // Iniciar incremento de oscuridad
        darknessController?.StartDarknessIncrease();

        Debug.Log("Fase de juego iniciada.");
    }

    // ===============================
    // ENTREGA DE OBJETOS
    // ===============================
    public void CheckDelivery(PlayerController player)
    {
        if (gameOver) return;

        GameObject heldObject = player.GetHeldObject();
        if (heldObject == null) return;

        ObjectData data = heldObject.GetComponent<ObjectData>();
        Object carriedObject = data?.data;

        if (carriedObject == null)
        {
            Debug.LogError("Objeto sin ObjectData. Eliminando.");
            player.ClearHeldObject();
            Destroy(heldObject);
            return;
        }

        if (objectSpawner.requiredObjects.Contains(carriedObject))
        {
            // --- ENTREGA CORRECTA ---
            objectSpawner.RemoveFromObjective(carriedObject);
            objectsDeliveredCount++;

            if (player == player1Controller) player1Score++;
            else if (player == player2Controller) player2Score++;

            player.ClearHeldObject();
            Destroy(heldObject);

            UpdateObjectiveUI();
            UpdateScoreUI();

            objectSpawner?.RefillObjectsOnScreen();

            // Condición de victoria: alguien llega al máximo de objetos
            if (objectsDeliveredCount >= totalObjectsToWin)
            {
                FinalizeGame(false);
            }
        }
        else
        {
            // --- ENTREGA INCORRECTA (penalización: lentitud) ---
            Debug.Log($"Objeto incorrecto: {carriedObject.objectName}. Aplicando penalización.");
            player.ApplySlowPenalty();

            player.ClearHeldObject();
            Destroy(heldObject);

            objectSpawner?.RefillObjectsOnScreen();
        }
    }

    // ===============================
    // MÉTODOS AUXILIARES
    // ===============================
    public bool IsVersusMode() => currentMode == GameMode.VERSUS;

    private void UpdateObjectiveUI()
    {
        if (listaObjetivoText != null)
            listaObjetivoText.text = $"{objectsDeliveredCount}/{totalObjectsToWin}";
    }

    private void UpdateScoreUI()
    {
        if (p1ScoreText != null) p1ScoreText.text = "P1: " + player1Score;
        if (p2ScoreText != null) p2ScoreText.text = "P2: " + player2Score;
    }

    // ===============================
    // FIN DE PARTIDA
    // ===============================
    public void FinalizeGame(bool isTimeOut)
    {
        if (gameOver) return;
        gameOver = true;

        // Detener procesos
        gameTimer?.DetenerTiempo();
        objectSpawner?.StopSpawning();
        darknessController?.StopDarknessIncrease();

        // Ocultar HUD
        inGameHUDContainer?.SetActive(false);

        // -------------------------------
        // LOGICA DE FIN DE JUEGO SEGÚN MODO
        // -------------------------------
        if (currentMode == GameMode.COOP)
        {
            bool won = !isTimeOut && (objectsDeliveredCount >= totalObjectsToWin);
            endGameScreenUI?.ShowEndScreen(won, objectsDeliveredCount, totalObjectsToWin);
        }
        else if (currentMode == GameMode.VERSUS)
        {
            // Determinar quién ganó
            string winnerText = "";
            if (player1Score > player2Score) winnerText = "Jugador 1 gana!";
            else if (player2Score > player1Score) winnerText = "Jugador 2 gana!";
            else winnerText = "¡Empate!";

            // Mostrar panel de fin de juego VERSUS
            endGameVsScreenUI?.ShowEndScreenVs(winnerText, player1Score, player2Score);
        }
    }

    // ===============================
    // REINICIAR JUEGO
    // ===============================
    public void RestartGame()
    {
        objectsDeliveredCount = 0;
        player1Score = 0;
        player2Score = 0;
        gameOver = false;

        endGameScreenUI?.HideEndScreen();
        endGameVsScreenUI?.HideEndScreen();
        inGameHUDContainer?.SetActive(true);

        if (objectSpawner != null)
        {
            objectSpawner.InitializeSpawner();
            if (objectiveListUI != null)
            {
                objectiveListUI.SetInitialObjectives(objectSpawner.requiredObjects);
                objectiveListUI.ShowList();
            }
        }

        gameTimer?.StartGame();
        darknessController?.StartDarknessIncrease();
    }
}
