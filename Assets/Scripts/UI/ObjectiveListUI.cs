using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections; 
using TMPro; 

/// <summary>
/// Controla la visualización de la lista de objetivos durante la fase de memorización.
/// </summary>
public class ObjectiveListUI : MonoBehaviour
{
    [Header("Configuración de la UI")]
    [Tooltip("El objeto padre de TODA la lista (Fondo, Textos, Iconos).")]
    public GameObject listRootContainer; 
    
    [Tooltip("Prefab del elemento Image que se usará para mostrar cada objetivo.")]
    public GameObject objectiveSlotPrefab;
    
    [Tooltip("Referencia al contenedor del HUD del juego (Contador 0/5, Cronómetro).")]
    public GameObject gameHUDContainer; // CRÍTICO: Referencia al HUD principal del juego
    
    [Header("Configuración de Partida")]
    [Tooltip("Tiempo en segundos que el jugador tiene para memorizar la lista.")]
    public float memorizationTime = 7f; 

    [Header("Textos Opcionales")]
    [Tooltip("Referencia al TextMeshPro que muestra el tiempo restante de memorización.")]
    public TextMeshProUGUI memorizationTimerText;

    private List<Image> objectiveSlots = new List<Image>();
    // Usamos el tipo correcto: Object (que hereda de BaseDataObject)
    private List<Object> currentObjectives; 

    private void Awake()
    {
        // Aseguramos que la lista esté OCULTA al inicio
        if (listRootContainer != null)
        {
            listRootContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Crea los slots y asigna los objetivos seleccionados por el Spawner.
    /// </summary>
    // Ahora espera List<Object>
    public void SetInitialObjectives(List<Object> objectives)
    {
        ClearSlots();
        currentObjectives = objectives;
        
        if (objectiveSlotPrefab == null)
        {
            Debug.LogError("ObjectiveListUI: ¡El prefab del slot no está asignado!");
            return;
        }

        // Generar los slots dinámicamente
        for (int i = 0; i < objectives.Count; i++)
        {
            // Usamos la clase Object, que hereda de BaseDataObject para acceder a sus propiedades visuales
            Object objData = objectives[i]; 
            
            // Instancia el slot como hijo del objeto que contiene este script
            GameObject newSlot = Instantiate(objectiveSlotPrefab, transform);
            Image slotImage = newSlot.GetComponent<Image>();
            
            if (slotImage != null)
            {
                objectiveSlots.Add(slotImage);
                
                // Asignación de Sprite/Color
                if (objData.objectSprite != null)
                {
                    slotImage.sprite = objData.objectSprite;
                }
                slotImage.color = objData.displayColor; 

                // Asignamos el nombre del objeto
                TextMeshProUGUI textComponent = newSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = objData.objectName;
                }
            }
        }
    }
    
    /// <summary>
    /// Muestra el contenedor raíz de la lista, OCULTA el HUD de juego y comienza la corrutina de memorización.
    /// </summary>
    public void ShowList()
    {
        // 1. Oculta el HUD de juego (el 0/5 y el cronómetro)
        if (gameHUDContainer != null)
        {
            gameHUDContainer.SetActive(false);
        }
        
        // 2. Muestra la lista de memorización
        if (listRootContainer != null)
        {
            listRootContainer.SetActive(true);
            Debug.Log("ObjectiveListUI: Contenedor 'Lista' activado. Iniciando Fase de Memorización.");
        }

        // 3. Pausa el juego
        if (Time.timeScale != 0f) 
        {
            Time.timeScale = 0f;
        }
        
        StartCoroutine(MemorizationPhase());
    }

    private IEnumerator MemorizationPhase()
    {
        float timer = memorizationTime;
        
        // Bucle para contar y actualizar el texto
        while (timer > 0)
        {
            if (memorizationTimerText != null)
            {
                memorizationTimerText.text = Mathf.CeilToInt(timer).ToString() + "s";
            }
            
            yield return new WaitForSecondsRealtime(1f); 
            timer -= 1f;
        }
        
        // Ocultar la lista y limpiar texto
        HideList();
        if (memorizationTimerText != null)
        {
            memorizationTimerText.text = "";
        }
        
        // CRÍTICO: LLAMADA PARA INICIAR EL JUEGO (Esto reanuda el tiempo y activa el HUD)
        if (ZonaDeEntregaManager.Instance != null)
        {
            ZonaDeEntregaManager.Instance.StartGamePhase();
        }
        
        Debug.Log("Fase de memorización terminada. ¡A jugar!");
    }

    public void HideList()
    {
        if (listRootContainer != null)
        {
            listRootContainer.SetActive(false);
        }
    }

    private void ClearSlots()
    {
        foreach (Image img in objectiveSlots)
        {
            if (img != null) Destroy(img.gameObject);
        }
        objectiveSlots.Clear();
    }
}