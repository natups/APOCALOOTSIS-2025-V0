using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections; 
using System; 
using TMPro; // Asegúrate de incluir esta referencia si usas TextMeshPro

/// <summary>
/// Controla la visualización de la lista de objetivos.
/// </summary>
public class ObjectiveListUI : MonoBehaviour
{
    [Header("Configuración de la UI")]
    [Tooltip("El objeto padre de TODA la lista (Fondo, Textos, Iconos) que se activará/desactivará (Debería ser 'Lista').")]
    public GameObject listRootContainer; 
    
    [Tooltip("Prefab del elemento Image que se usará para mostrar cada objetivo.")]
    public GameObject objectiveSlotPrefab;
    
    [Header("Configuración de Partida")]
    [Tooltip("Tiempo en segundos que el jugador tiene para memorizar la lista.")]
    public float memorizationTime = 7f; // Usamos 7s como en el flujo original

    [Header("Sprites y Slots")]
    [Tooltip("Número de slots a crear (se ajusta a totalObjectsToWin del Manager).")]
    public int totalSlots = 5;

    [Tooltip("Sprite de placeholder o default para los slots.")]
    public Sprite defaultObjectiveSprite; 

    // Lista de las IMAGENES instanciadas y los objetos requeridos
    private List<Image> objectiveSlots = new List<Image>();
    private List<Object> currentObjectives; 

    private void Awake()
    {
        // Aseguramos que el panel visual principal esté oculto al inicio.
        if (listRootContainer != null)
        {
            listRootContainer.SetActive(false);
        }
        // Nota: Este GameObject ('ObjectiveList_Dynamic') DEBE estar ACTIVO cuando se llama a ShowList()
        // o el Manager debe activarlo (como se hace ahora).
    }

    /// <summary>
    /// Crea los slots y asigna los objetivos aleatorios.
    /// </summary>
    public void SetInitialObjectives(List<Object> objectives)
    {
        ClearSlots();
        currentObjectives = objectives;

        // Ajustar el número de slots al número de objetivos reales.
        totalSlots = objectives.Count;

        // Generar los slots dinámicamente
        for (int i = 0; i < totalSlots; i++)
        {
            // Instancia el slot como hijo de este GameObject (ObjectiveList_Dynamic)
            GameObject newSlot = Instantiate(objectiveSlotPrefab, transform);
            Image slotImage = newSlot.GetComponent<Image>();
            if (slotImage != null)
            {
                objectiveSlots.Add(slotImage);
                
                // NOTA: Asignamos el sprite real del objetivo aquí para la memorización.
                // Revisa que tu clase 'Object' tenga un campo de tipo Sprite (ej. 'ObjectSprite')
                // slotImage.sprite = objectives[i].ObjectSprite; // EJEMPLO
                
                // Mientras tanto, usamos el sprite por defecto y color blanco para asegurar visibilidad
                slotImage.sprite = defaultObjectiveSprite; 
                slotImage.color = Color.white; 
            }
        }
    }
    
    /// <summary>
    /// Muestra el contenedor raíz de la lista y comienza la corrutina de memorización.
    /// Es llamado por el ZonaDeEntregaManager en Start().
    /// </summary>
    public void ShowList()
    {
        // Activamos el objeto contenedor visual 'Lista'
        if (listRootContainer != null)
        {
             listRootContainer.SetActive(true);
             Debug.Log("ObjectiveListUI: Contenedor 'Lista' activado. Iniciando Fase de Memorización.");
        }
        
        // Iniciar la Coroutine de memorización
        StartCoroutine(MemorizationPhase());
    }

    private IEnumerator MemorizationPhase()
    {
        // 1. Pausar el juego
        Time.timeScale = 0f;
        
        // 2. Mostrar la lista con los sprites
        UpdateUI(true); // Mostrar los objetivos para memorizar
        
        // 3. Esperar el tiempo de memorización (en tiempo real)
        yield return new WaitForSecondsRealtime(memorizationTime); 
        
        // 4. Ocultar la lista y reanudar el juego
        HideList();
        Time.timeScale = 1f;
        
        // 5. *** INICIO CRÍTICO DEL JUEGO ***
        // Aquí es donde empieza el tiempo y la oscuridad.
        if (ZonaDeEntregaManager.Instance != null && ZonaDeEntregaManager.Instance.gameTimer != null)
        {
            ZonaDeEntregaManager.Instance.gameTimer.StartGame();
        }
        if (ZonaDeEntregaManager.Instance != null && ZonaDeEntregaManager.Instance.darknessController != null)
        {
            // La oscuridad ya la inicia el Manager, pero si necesitas un inicio específico, descomenta:
            // ZonaDeEntregaManager.Instance.darknessController.StartDarknessIncrease(); 
        }
        
        Debug.Log("Fase de memorización terminada. ¡A jugar!");
    }

    
    /// <summary>
    /// Oculta el contenedor raíz de la lista de objetivos.
    /// </summary>
    public void HideList()
    {
        if (listRootContainer != null)
        {
            listRootContainer.SetActive(false);
        }
    }

    private void ClearSlots()
    {
        // Limpia cualquier slot previamente generado
        foreach (Image img in objectiveSlots)
        {
            if (img != null) Destroy(img.gameObject);
        }
        objectiveSlots.Clear();
    }

    /// <summary>
    /// Asigna los sprites de los objetivos a los slots de la UI.
    /// </summary>
    /// <param name="showSprites">Si es verdadero, muestra los sprites del objetivo. Si es falso, los oculta.</param>
    public void UpdateUI(bool showSprites = false)
    {
        if (currentObjectives == null || objectiveSlots.Count == 0) return;

        int maxSlots = Mathf.Min(objectiveSlots.Count, currentObjectives.Count);

        for (int i = 0; i < maxSlots; i++)
        {
            Image currentSlot = objectiveSlots[i];
            
            if (showSprites)
            {
                // Muestra los sprites reales (si tienes el campo 'sprite' en tu clase 'Object')
                currentSlot.sprite = defaultObjectiveSprite; // Manteniendo el defaultObjectiveSprite por seguridad.
                currentSlot.color = Color.white;
            }
            else
            {
                // Oculta el slot (se usa después de la memorización)
                currentSlot.sprite = defaultObjectiveSprite;
                currentSlot.color = new Color(1, 1, 1, 0); // Totalmente transparente
            }
        }
    }
}