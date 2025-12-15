using UnityEngine;
using System.Collections.Generic;
using TMPro;

// Modos de juego disponibles
// COOP: jugadores colaboran
// VERSUS: jugadores compiten entre sí
public enum GameMode { COOP, VERSUS }

public class ZonaDeEntregaManager : MonoBehaviour
{
    // Singleton para poder acceder a este manager desde otros scripts
    public static ZonaDeEntregaManager Instance { get; private set; }

    // ===============================
    // REFERENCIAS PRINCIPALES
    // ===============================
    public ObjectSpawner objectSpawner; 		   // Maneja el spawn y los objetivos
    public GameTimer gameTimer; 			   // Controla el tiempo de la partida
    public PlayerController player1Controller;  // Referencia al jugador 1
    public PlayerController player2Controller;  // Referencia al jugador 2
    public DarknessController darknessController; // Controla el aumento de oscuridad
    public ObjectiveListUI objectiveListUI; 	   // UI de memorización de objetivos

    // ===============================
    // UI DE FIN DE PARTIDA
    // ===============================
    public EndGameScreenUI endGameScreenUI; 	 // Pantalla final modo COOP
    public EndGameScreenVsUI endGameVsScreenUI; // Pantalla final modo VERSUS

    // ===============================
    // UI EN PARTIDA
    // ===============================
    public GameObject inGameHUDContainer; // HUD principal
    public TextMeshProUGUI listaObjetivoText; // Texto X / Total
    public TextMeshProUGUI p1ScoreText; 	 // Puntaje jugador 1
    public TextMeshProUGUI p2ScoreText; 	 // Puntaje jugador 2

    // ===============================
    // CONFIGURACIÓN DE JUEGO
    // ===============================
    public GameMode currentMode = GameMode.COOP; // Modo actual
    public int totalObjectsToWin = 5; 		   // Objetos necesarios para ganar

    // ===============================
    // ESTADO INTERNO
    // ===============================
    private int objectsDeliveredCount = 0; // Cantidad de objetos entregados correctamente
    private int player1Score = 0; 		   // Puntaje jugador 1
    private int player2Score = 0; 		   // Puntaje jugador 2
    private bool gameOver = true; 		   // Bloquea acciones cuando termina el juego

    // ===============================
    // SINGLETON
    // ===============================
    private void Awake()
    {
        // Si no existe una instancia, esta se convierte en la principal
        if (Instance == null)
            Instance = this;
        // Si ya existe, destruimos el duplicado
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Oculta el HUD y las pantallas de fin al iniciar la escena
        inGameHUDContainer?.SetActive(false);
        endGameScreenUI?.HideEndScreen();
        endGameVsScreenUI?.HideEndScreen();

        // Verificamos que exista el spawner antes de usarlo
        if (objectSpawner != null)
        {
            // Configura cuántos objetos se necesitan para ganar
            objectSpawner.totalObjectsRequired = totalObjectsToWin;
            objectSpawner.InitializeSpawner();

            // Le informamos al timer quién es su manager
            gameTimer?.SetManager(this);

            // Si existe la UI de objetivos, arrancamos con la fase de memorización
            if (objectiveListUI != null)
            {
                objectiveListUI.gameObject.SetActive(true);
                objectiveListUI.SetInitialObjectives(objectSpawner.requiredObjects);
                objectiveListUI.gameHUDContainer = inGameHUDContainer;
                objectiveListUI.ShowList();
            }
            // Si no hay UI de memorización, se inicia el juego directamente
            else
            {
                StartGamePhase();
            }
        }

        // Actualiza textos iniciales
        UpdateObjectiveUI();
        UpdateScoreUI();
    }

    // ===============================
    // INICIO DE PARTIDA
    // ===============================
    public void StartGamePhase()
    {
        // El juego ya está activo
        gameOver = false;

        // Reanuda el tiempo del juego
        Time.timeScale = 1f;

        // Oculta la UI de memorización
        objectiveListUI?.gameObject.SetActive(false);

        // Muestra el HUD de juego
        inGameHUDContainer?.SetActive(true);

        // Inicia los sistemas principales
        objectSpawner?.StartSpawning();
        gameTimer?.StartGame();
        darknessController?.StartDarknessIncrease();
    }

    // ===============================
    // ENTREGA DE OBJETOS
    // ===============================
    public void CheckDelivery(PlayerController player)
    {
        // Si el juego terminó, no se procesa ninguna entrega
        if (gameOver) return;

        // Obtenemos el objeto que el jugador está llevando
        GameObject heldObject = player.GetHeldObject();

        // Si no lleva nada, no hacemos nada
        if (heldObject == null) return;

        // Intentamos obtener los datos del objeto
        ObjectData data = heldObject.GetComponent<ObjectData>();
        Object carriedObject = data?.data;

        // Si el objeto no tiene datos válidos, se elimina
        if (carriedObject == null)
        {
            player.ClearHeldObject();
            Destroy(heldObject);
            return;
        }

        // ===============================
        // ENTREGA CORRECTA
        // ===============================
        // Verificamos si el objeto entregado es uno de los requeridos
        if (objectSpawner.requiredObjects.Contains(carriedObject))
        {
            // Quitamos el objeto de la lista de objetivos
            objectSpawner.RemoveFromObjective(carriedObject);

            // Incrementamos el contador global (COOP)
            objectsDeliveredCount++;

            // Sumamos puntos al jugador correspondiente (VS)
            if (player == player1Controller)
                player1Score++;
            else if (player == player2Controller)
                player2Score++;

            // Limpiamos y destruimos el objeto entregado
            player.ClearHeldObject();
            Destroy(heldObject);

            // Actualizamos UI
            UpdateObjectiveUI();
            UpdateScoreUI();

            // Reponemos objetos en escena
            objectSpawner.RefillObjectsOnScreen();

            // Si se alcanzó el objetivo total, termina la partida
            if (objectsDeliveredCount >= totalObjectsToWin)
            {
                FinalizeGame(false);
            }
        }
        // ===============================
        // ENTREGA INCORRECTA
        // ===============================
        else
        {
            // Aplicamos penalización al jugador
            player.ApplySlowPenalty();

            // Eliminamos el objeto incorrecto
            player.ClearHeldObject();
            Destroy(heldObject);

            // Reponemos objetos en escena
            objectSpawner.RefillObjectsOnScreen();
        }
    }

    // ===============================
    // UTILIDADES
    // ===============================
    public bool IsVersusMode()
    {
        // Devuelve true si el modo actual es VERSUS
        return currentMode == GameMode.VERSUS;
    }

    private void UpdateObjectiveUI()
    {
        // Actualiza el texto X / Total (coop)
        if (listaObjetivoText != null)
            listaObjetivoText.text = $"{objectsDeliveredCount}/{totalObjectsToWin}";
    }

    private void UpdateScoreUI()
    {
        // Actualiza puntajes de jugadores (vs)
        if (p1ScoreText != null) p1ScoreText.text = "P1: " + player1Score;
        if (p2ScoreText != null) p2ScoreText.text = "P2: " + player2Score;
    }

    // ===============================
    // FIN DE PARTIDA
    // ===============================
    public void FinalizeGame(bool isTimeOut)
    {
        // Evita que se ejecute más de una vez
        if (gameOver) return;
        gameOver = true;

        // Detiene todos los sistemas activos
        gameTimer?.DetenerTiempo();
        objectSpawner?.StopSpawning();
        darknessController?.StopDarknessIncrease();

        // Oculta el HUD
        inGameHUDContainer?.SetActive(false);

        // ===============================
        // MODO COOP
        // ===============================
        if (currentMode == GameMode.COOP)
        {
            // Se gana si no fue por tiempo y se alcanzó el objetivo
            bool won = !isTimeOut && objectsDeliveredCount >= totalObjectsToWin;
            endGameScreenUI?.ShowEndScreen(won, objectsDeliveredCount, totalObjectsToWin);
        }
        // ===============================
        // MODO VERSUS
        // ===============================
        else
        {
            EndGameScreenVsUI.GameResult result;

            // Compara puntajes para determinar ganador
            if (player1Score > player2Score)
                result = EndGameScreenVsUI.GameResult.Player1Wins;
            else if (player2Score > player1Score)
                result = EndGameScreenVsUI.GameResult.Player2Wins;
            else
                result = EndGameScreenVsUI.GameResult.Draw;

            // Llama al nuevo método que solo necesita el enum de resultado
            endGameVsScreenUI?.ShowEndScreenVs(result);
        }
    }

    // ===============================
    // REINICIO DE PARTIDA
    // ===============================
    public void RestartGame()
    {
        // Reinicia contadores y estado
        objectsDeliveredCount = 0;
        player1Score = 0;
        player2Score = 0;
        gameOver = false;

        // Oculta pantallas finales y muestra HUD
        endGameScreenUI?.HideEndScreen();
        endGameVsScreenUI?.HideEndScreen();
        inGameHUDContainer?.SetActive(true);

        // Reinicia spawner
        objectSpawner?.InitializeSpawner();

        // Vuelve a mostrar la fase de memorización
        if (objectiveListUI != null)
        {
            objectiveListUI.SetInitialObjectives(objectSpawner.requiredObjects);
            objectiveListUI.ShowList();
        }

        // Reinicia sistemas
        gameTimer?.StartGame();
        darknessController?.StartDarknessIncrease();
    }
}