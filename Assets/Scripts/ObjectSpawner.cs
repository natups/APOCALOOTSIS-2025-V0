using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectSpawner : MonoBehaviour
{
    // ==========================================================
    // VARIABLES EXISTENTES 
    // ==========================================================
    [Header("Configuración de Objetos")]
    // La lista ahora debe contener referencias al ScriptableObject 'Object'
    // ya que este es el tipo que define la data.
    public List<Object> allPossibleObjectData; 
    public GameObject baseObjectPrefab; 

    [Header("Configuración de Spawn")]
    public List<Transform> spawnPoints;
    public int maxObjectsOnScreen = 10;
    public float spawnInterval = 3f;

    // ==========================================================
    // GESTIÓN DE ESTADO Y OBJETIVOS
    // ==========================================================
    // Los objetivos son ahora del tipo ScriptableObject 'Object'
    private List<Object> currentObjectives = new List<Object>();
    private List<GameObject> activeObjects = new List<GameObject>();
    private float spawnTimer = 0f;
    private bool isSpawning = true;

    // ==========================================================
    // MÉTODOS BASE DE UNITY
    // ==========================================================
    private void Start()
    {
        GenerateNewObjective(1); // Genera el objetivo inicial
        for (int i = 0; i < maxObjectsOnScreen; i++)
        {
            SpawnNewObject();
        }
    }

    private void Update()
    {
        if (isSpawning)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval && activeObjects.Count < maxObjectsOnScreen)
            {
                SpawnNewObject();
                spawnTimer = 0f;
            }
        }
    }

    // ==========================================================
    // MÉTODOS LLAMADOS POR ZONADEENTREGAMANAGER (¡Errores corregidos!)
    // ==========================================================
    
    // Ahora devuelve la lista de Scriptable Objects (Object)
    public List<Object> GetCurrentObjectives()
    {
        return currentObjectives;
    }

    // El deliveredObject es el Scriptable Object 'Object'
    public bool IsCurrentObjective(Object deliveredObject)
    {
        if (deliveredObject != null && currentObjectives.Count > 0)
        {
            // ** ERROR CS1061 RESUELTO: 'objectName' está en BaseDataObject, al que Object tiene acceso. **
            foreach(var objective in currentObjectives)
            {
                if (objective.objectName == deliveredObject.objectName)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    // El deliveredObject es el Scriptable Object 'Object'
    public void ObjectDelivered(Object deliveredObject)
    {
        GenerateNewObjective(1); 
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    // ==========================================================
    // LÓGICA INTERNA DE SPAWNING
    // ==========================================================
    
    private void GenerateNewObjective(int count)
    {
        currentObjectives.Clear();
        for (int i = 0; i < count; i++)
        {
            if (allPossibleObjectData.Count > 0)
            {
                int randomIndex = Random.Range(0, allPossibleObjectData.Count);
                currentObjectives.Add(allPossibleObjectData[randomIndex]);
            }
        }
    }

    private void SpawnNewObject()
    {
        if (spawnPoints.Count == 0 || allPossibleObjectData.Count == 0 || activeObjects.Count >= maxObjectsOnScreen) return;

        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject newObjectGO = Instantiate(baseObjectPrefab, randomSpawnPoint.position, Quaternion.identity);
        activeObjects.Add(newObjectGO);

        // Lógica CRÍTICA: Asignar la data al objeto recién creado
        ObjectData objectDataComponent = newObjectGO.GetComponent<ObjectData>();
        if (objectDataComponent != null)
        {
            // 1. Selecciona un Scriptable Object de la lista de posibles
            Object selectedData = allPossibleObjectData[Random.Range(0, allPossibleObjectData.Count)];
            
            // 2. Llama a tu función SetData para inicializar el objeto visual
            objectDataComponent.SetData(selectedData);
        }
    }
}