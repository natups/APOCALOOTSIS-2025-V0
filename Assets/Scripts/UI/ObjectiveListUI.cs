using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// Maneja la fase de memorización de objetivos.
// Muestra la lista, pausa el juego y luego inicia la partida.
public class ObjectiveListUI : MonoBehaviour
{
    // Contenedor principal de la lista de objetivos
    public GameObject listRootContainer;

    // Prefab visual de cada objetivo
    public GameObject objectiveSlotPrefab;

    // HUD principal del juego (contador y cronómetro)
    public GameObject gameHUDContainer;

    // Tiempo disponible para memorizar los objetivos
    public float memorizationTime = 7f;

    // Texto que muestra el tiempo restante
    public TextMeshProUGUI memorizationTimerText;

    // Slots visuales creados dinámicamente
    private List<Image> objectiveSlots = new List<Image>();

    // Lista de objetivos actuales (ScriptableObjects)
    private List<Object> currentObjectives;

    private void Awake()
    {
        // La lista debe comenzar oculta
        if (listRootContainer != null)
        {
            listRootContainer.SetActive(false);
        }
    }

    // Recibe los objetivos del spawner y crea los slots visuales
    public void SetInitialObjectives(List<Object> objectives)
    {
        ClearSlots();
        currentObjectives = objectives;

        for (int i = 0; i < objectives.Count; i++)
        {
            Object objData = objectives[i];

            GameObject newSlot = Instantiate(objectiveSlotPrefab, transform);
            Image slotImage = newSlot.GetComponent<Image>();

            if (slotImage != null)
            {
                objectiveSlots.Add(slotImage);

                if (objData.objectSprite != null)
                {
                    slotImage.sprite = objData.objectSprite;
                }

                slotImage.color = objData.displayColor;

                TextMeshProUGUI textComponent = newSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = objData.objectName;
                }
            }
        }
    }

    // Muestra la lista, oculta el HUD y pausa el juego
    public void ShowList()
    {
        if (gameHUDContainer != null)
        {
            gameHUDContainer.SetActive(false);
        }

        if (listRootContainer != null)
        {
            listRootContainer.SetActive(true);
        }

        Time.timeScale = 0f;
        StartCoroutine(MemorizationPhase());
    }

    // Controla el conteo de la fase de memorización
    private IEnumerator MemorizationPhase()
    {
        float timer = memorizationTime;

        while (timer > 0)
        {
            if (memorizationTimerText != null)
            {
                memorizationTimerText.text = Mathf.CeilToInt(timer) + "s";
            }

            yield return new WaitForSecondsRealtime(1f);
            timer -= 1f;
        }

        HideList();

        if (memorizationTimerText != null)
        {
            memorizationTimerText.text = "";
        }

        if (ZonaDeEntregaManager.Instance != null)
        {
            ZonaDeEntregaManager.Instance.StartGamePhase();
        }
    }

    // Oculta la lista y desactiva el UI
    public void HideList()
    {
        if (listRootContainer != null)
        {
            listRootContainer.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    // Elimina los slots generados previamente
    private void ClearSlots()
    {
        foreach (Image img in objectiveSlots)
        {
            if (img != null)
            {
                Destroy(img.gameObject);
            }
        }

        objectiveSlots.Clear();
    }
}
