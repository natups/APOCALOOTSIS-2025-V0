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
    
    [Header("Configuración de Partida")]
    [Tooltip("Tiempo en segundos que el jugador tiene para memorizar la lista.")]
    public float memorizationTime = 7f; 

    [Header("Textos Opcionales")]
    [Tooltip("Referencia al TextMeshPro que muestra el tiempo restante de memorización.")]
    public TextMeshProUGUI memorizationTimerText;

    private List<Image> objectiveSlots = new List<Image>();
    private List<Object> currentObjectives; 

    private void Awake()
    {
        if (listRootContainer != null)
        {
            listRootContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Crea los slots y asigna los objetivos seleccionados por el Spawner.
    /// </summary>
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
            // Asumo que tu clase 'Object' (ScriptableObject) se usa aquí
            Object objData = objectives[i]; 
            
            // Instancia el slot como hijo del objeto que contiene este script
            GameObject newSlot = Instantiate(objectiveSlotPrefab, transform);
            Image slotImage = newSlot.GetComponent<Image>();
            
            if (slotImage != null)
            {
                objectiveSlots.Add(slotImage);
                
                // Asumo que el ScriptableObject 'Object' tiene estas propiedades
                slotImage.sprite = objData.objectSprite; 
                slotImage.color = objData.displayColor; 

                // Si el slot tiene un componente de texto, asignamos el nombre del objeto
                TextMeshProUGUI textComponent = newSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = objData.objectName;
                }
            }
        }
    }
    
    /// <summary>
    /// Muestra el contenedor raíz de la lista y comienza la corrutina de memorización.
    /// </summary>
    public void ShowList()
    {
        if (listRootContainer != null)
        {
             listRootContainer.SetActive(true);
             Debug.Log("ObjectiveListUI: Contenedor 'Lista' activado. Iniciando Fase de Memorización.");
        }
        
        StartCoroutine(MemorizationPhase());
    }

    private IEnumerator MemorizationPhase()
    {
        float timer = memorizationTime;

        // 1. Pausar el juego
        Time.timeScale = 0f;
        
        // 2. Bucle para contar y actualizar el texto (usa tiempo real)
        while (timer > 0)
        {
            if (memorizationTimerText != null)
            {
                memorizationTimerText.text = "MEMORIZA EN: " + Mathf.CeilToInt(timer).ToString() + "s";
            }
            
            yield return new WaitForSecondsRealtime(1f); 
            timer -= 1f;
        }
        
        // 3. Ocultar la lista, limpiar texto y reanudar el juego
        HideList();
        if (memorizationTimerText != null)
        {
            memorizationTimerText.text = "";
        }
        
        // 4. Reanudar el tiempo
        Time.timeScale = 1f;
        
        // 5. CRÍTICO: LLAMADA PARA INICIAR EL JUEGO
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