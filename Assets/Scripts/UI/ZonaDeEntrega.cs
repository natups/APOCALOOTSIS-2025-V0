using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ZonaDeEntrega : MonoBehaviour
{
    // ====================================================================
    // PROPIEDADES PÚBLICAS CONFIGURABLES EN EL INSPECTOR
    // ====================================================================

    [Header("Configuración de la Zona")]
    [SerializeField] private int capacidadMaxima = 5;
    [SerializeField] private int cantidadObjetivos = 5;

    [Header("Visualización UI (TextMeshProUGUI)")]
    [SerializeField] private TextMeshProUGUI textoAciertos;
    [SerializeField] private TextMeshProUGUI textoTotal;
    [SerializeField] private TextMeshProUGUI P1_ScoreText;
    [SerializeField] private TextMeshProUGUI P2_ScoreText;
    [SerializeField] private TextMeshProUGUI textoListaObjetivo;

    [Header("Objetos & Spawning")]
    [SerializeField] private List<BaseDataObject> allPossibleObjectData = new List<BaseDataObject>();
    [SerializeField] private GameObject baseObjectPrefab;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    // [NUEVOS CAMPOS A CONECTAR EN EL INSPECTOR]
    [Header("End Screen UI")]
    [SerializeField] private GameObject endScreenUI; // Panel/GameObject de la pantalla final
    [SerializeField] private TextMeshProUGUI player1TextoFinal; // Texto para la puntuación final de P1
    [SerializeField] private TextMeshProUGUI player2TextoFinal; // Texto para la puntuación final de P2
    [SerializeField] private TextMeshProUGUI whoWinsText; // Texto que muestra "P1 Gana" o "Empate"

    // ====================================================================
    // LÓGICA INTERNA Y ESTADO
    // ====================================================================

    private List<BaseDataObject> requiredObjects = new List<BaseDataObject>();
    private List<ObjectData> activeObjects = new List<ObjectData>();
    
    private int p1Aciertos = 0;
    private int p2Aciertos = 0;
    private int p1Puntuacion = 0;
    private int p2Puntuacion = 0;

    private PlayerMovement player1Movement;
    private PlayerMovement player2Movement;
    
    private bool gameOver = false; // Bandera para detener la entrega/spawning al final

    void Start()
    {
        // Al inicio, aseguramos que la pantalla final esté oculta
        if (endScreenUI != null) endScreenUI.SetActive(false);
        
        SetupPlayers();
        ResetObjective();
        SpawnInitialObjects(); 
        UpdateUI();
    }

    private void SetupPlayers()
    {
        // Intenta encontrar los jugadores por Tag
        GameObject player1Obj = GameObject.FindWithTag("Player");
        GameObject player2Obj = GameObject.FindWithTag("Player");

        if (player1Obj != null) player1Movement = player1Obj.GetComponent<PlayerMovement>();
        if (player2Obj != null) player2Movement = player2Obj.GetComponent<PlayerMovement>();
        
        if (player1Movement == null || player2Movement == null)
        {
            Debug.LogWarning("ADVERTENCIA: No se encontró uno o ambos scripts PlayerMovement. La entrega no funcionará.");
        }
    }

    private void ResetObjective()
    {
        requiredObjects.Clear();
        p1Aciertos = 0;
        p2Aciertos = 0;
        
        if (allPossibleObjectData.Count == 0) return;

        // Selecciona objetos al azar
        for (int i = 0; i < cantidadObjetivos; i++)
        {
            int randomIndex = Random.Range(0, allPossibleObjectData.Count);
            requiredObjects.Add(allPossibleObjectData[randomIndex]);
        }
        
        Debug.Log("NUEVO OBJETIVO: Se necesitan " + requiredObjects.Count + " ítems.");
        UpdateUI();
    }

    private void SpawnInitialObjects()
    {
        // LOG CRÍTICO para saber si la función se llama
        Debug.Log("DEBUG: ===> FUNCIÓN DE SPAWN LLAMADA <==="); 

        if (baseObjectPrefab == null) 
        {
            Debug.LogError("ERROR DE SPAWN: Base Object Prefab (molde) no asignado.");
            return;
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("ERROR DE SPAWN: No hay Spawn Points asignados.");
            return;
        }

        // Spawnea en los puntos disponibles
        for (int i = 0; i < Mathf.Min(capacidadMaxima, spawnPoints.Count); i++)
        {
            SpawnSingleObject(spawnPoints[i].position);
        }
    }

    private void SpawnSingleObject(Vector3 position)
    {
        if (allPossibleObjectData.Count == 0 || gameOver) return;

        // 1. Elegir datos y clonar
        int dataIndex = Random.Range(0, allPossibleObjectData.Count);
        BaseDataObject chosenData = allPossibleObjectData[dataIndex];

        GameObject newObject = Instantiate(baseObjectPrefab, position, Quaternion.identity);

        // 2. Asignar datos
        ObjectData objectDataScript = newObject.GetComponent<ObjectData>();
        if (objectDataScript != null)
        {
            objectDataScript.SetObjectData(chosenData);
            activeObjects.Add(objectDataScript);
            Debug.Log($"SPAWN EXITOSO: Generado objeto '{chosenData.objectName}' en {position}");
        }
        else
        {
            Debug.LogError($"ERROR: El Prefab '{baseObjectPrefab.name}' NO tiene el script 'ObjectData'. ¡FALLO DE ASIGNACIÓN!");
            Destroy(newObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameOver) return; // Si el juego terminó, ignora la entrega.
        
        // Lógica de entrega (solo se ejecuta si el jugador tiene un objeto)
        bool isP1 = other.CompareTag("Player");
        bool isP2 = other.CompareTag("Player");

        if (isP1 || isP2)
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            
            if (playerMovement != null && playerMovement.attachedObject != null)
            {
                // El jugador tiene un objeto, intentamos entregarlo
                GameObject deliveredObject = playerMovement.attachedObject;
                ObjectData deliveredData = deliveredObject.GetComponent<ObjectData>();
                
                if (deliveredData != null)
                {
                    bool success = CheckDelivery(deliveredData.GetObjectData());

                    if (success)
                    {
                        if (isP1) { p1Aciertos++; p1Puntuacion++; }
                        else { p2Aciertos++; p2Puntuacion++; }
                        Debug.Log($"ENTREGA CORRECTA de {deliveredData.GetObjectData().objectName} por J{(isP1 ? "1" : "2")}.");
                    }
                    else
                    {
                        if (isP1) p1Puntuacion--;
                        else p2Puntuacion--;
                        Debug.Log($"ENTREGA INCORRECTA de {deliveredData.GetObjectData().objectName} por J{(isP1 ? "1" : "2")}. ¡PENALIZACIÓN!");
                    }
                    
                    // Quitar el objeto
                    RemoveDeliveredObject(playerMovement, deliveredObject);
                    CheckObjectiveComplete();
                }
            }
        }
    }

    private bool CheckDelivery(BaseDataObject deliveredData)
    {
        // Busca el objeto requerido en la lista
        for (int i = 0; i < requiredObjects.Count; i++)
        {
            if (requiredObjects[i].objectName == deliveredData.objectName)
            {
                requiredObjects.RemoveAt(i); // Lo quitamos de la lista de requeridos
                return true;
            }
        }
        return false;
    }

    private void RemoveDeliveredObject(PlayerMovement playerMovement, GameObject deliveredObject)
    {
        // 1. Soltar el objeto en el script de movimiento
        playerMovement.attachedObject = null;
        
        // 2. Devolver la física
        Rigidbody2D objectRb = deliveredObject.GetComponent<Rigidbody2D>();
        if (objectRb != null) objectRb.bodyType = RigidbodyType2D.Dynamic;
        
        // 3. Destruir y reemplazar
        ObjectData deliveredData = deliveredObject.GetComponent<ObjectData>();
        if (deliveredData != null) activeObjects.Remove(deliveredData);
        
        Destroy(deliveredObject);
        
        // Generar un reemplazo (si la capacidad no está llena)
        if (activeObjects.Count < capacidadMaxima)
        {
            int randomSpawnIndex = Random.Range(0, spawnPoints.Count);
            SpawnSingleObject(spawnPoints[randomSpawnIndex].position);
        }
    }

    private void CheckObjectiveComplete()
    {
        if (requiredObjects.Count == 0)
        {
            Debug.Log("===> OBJETIVO DE RONDA COMPLETO <===");
            // Si el objetivo se completa antes de que termine el tiempo, se reinicia.
            ResetObjective();
        }

        UpdateUI();
    }
    
    private void UpdateUI()
    {
        // Actualiza el progreso de objetivos
        if (textoAciertos != null)
        {
             textoAciertos.text = (cantidadObjetivos - requiredObjects.Count).ToString();
        }
        if (textoTotal != null)
        {
            textoTotal.text = cantidadObjetivos.ToString();
        }

        // Actualiza la puntuación
        if (P1_ScoreText != null) P1_ScoreText.text = p1Puntuacion.ToString();
        if (P2_ScoreText != null) P2_ScoreText.text = p2Puntuacion.ToString();

        // Actualiza la lista de requeridos
        if (textoListaObjetivo != null)
        {
            string requiredList = "ITEMS REQUERIDOS:\n";
            foreach (BaseDataObject data in requiredObjects)
            {
                requiredList += $"- {data.objectName}\n";
            }
            textoListaObjetivo.text = requiredList;
        }
    }

    // ====================================================================
    // LÓGICA DE FIN DE PARTIDA (Función solicitada por VisualGameTimer.cs)
    // ====================================================================
    
    public void SendEndScreen()
    {
        if (gameOver) return;
        gameOver = true;
        
        Debug.Log("FIN DE PARTIDA: Activando pantalla final.");

        // 1. Detener el tiempo/interacciones
        Time.timeScale = 0; // Opcional: Pausar el juego
        // Destruir objetos activos para que no interfieran al reanudar
        foreach(ObjectData obj in activeObjects)
        {
            if (obj != null) Destroy(obj.gameObject);
        }
        activeObjects.Clear();

        // 2. Determinar el ganador
        string winnerText = "";
        if (p1Puntuacion > p2Puntuacion)
        {
            winnerText = "¡JUGADOR 1 GANA!";
        }
        else if (p2Puntuacion > p1Puntuacion)
        {
            winnerText = "¡JUGADOR 2 GANA!";
        }
        else
        {
            winnerText = "¡EMPATE!";
        }
        
        // 3. Actualizar la UI de la pantalla final
        if (endScreenUI != null) endScreenUI.SetActive(true);

        if (player1TextoFinal != null) player1TextoFinal.text = $"Puntuación J1: {p1Puntuacion}";
        if (player2TextoFinal != null) player2TextoFinal.text = $"Puntuación J2: {p2Puntuacion}";
        if (whoWinsText != null) whoWinsText.text = winnerText;
        
        // NOTA: Recuerda poner Time.timeScale = 1; al reiniciar la escena o volver al menú.
    }
}