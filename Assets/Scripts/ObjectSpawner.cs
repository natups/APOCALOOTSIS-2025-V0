using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Configuración de Objetos")]
    public ObjectData baseObjectPrefab; 
    public List<Object> allPossibleObjectData; 
    public List<Transform> spawnPoints;
    
    public int maxObjectsOnScreen = 10; 
    public int totalObjectsRequired = 5; 

    [HideInInspector] public List<Object> requiredObjects = new List<Object>(); // Objetivos restantes
    [HideInInspector] public List<ObjectData> spawnedObjects = new List<ObjectData>(); // Instancias en escena
    
    private bool isSpawning = true; // Control de StopSpawning

    /// <summary>
    /// Inicializa el spawner, selecciona objetivos y genera el set inicial.
    /// </summary>
    public void InitializeSpawner()
    {
        ClearSpawnedObjects();
        requiredObjects.Clear();
        isSpawning = true;
        
        List<Object> availableObjects = new List<Object>(allPossibleObjectData);
        
        if (availableObjects.Count < totalObjectsRequired)
        {
            Debug.LogError("ERROR: No hay suficientes Data Objects para el objetivo. Necesitas al menos " + totalObjectsRequired + " únicos.");
            return;
        }

        // 1. Seleccionar objetos CORRECTOS (Objetivo)
        for (int i = 0; i < totalObjectsRequired; i++)
        {
            if (availableObjects.Count == 0) break; 
            int randomIndex = Random.Range(0, availableObjects.Count);
            requiredObjects.Add(availableObjects[randomIndex]);
            availableObjects.RemoveAt(randomIndex); 
        }
        
        SpawnInitialObjects();
    }
    
    /// <summary>
    /// Genera los objetos correctos e incorrectos.
    /// </summary>
    public void SpawnInitialObjects()
    {
        if (!isSpawning) return;
        
        // Limpia referencias a objetos que ya fueron destruidos
        spawnedObjects.RemoveAll(item => item == null);
        
        // 1. Crear la lista de objetos a generar (Objetivos + Cebo)
        List<Object> objectsToSpawn = new List<Object>(requiredObjects);
        
        // 2. Seleccionar objetos INCORRECTOS
        List<Object> allObjectsTyped = allPossibleObjectData.Cast<Object>().ToList();
        List<Object> incorrectObjects = allObjectsTyped.Except(requiredObjects).ToList();
        
        int incorrectsNeeded = maxObjectsOnScreen - spawnedObjects.Count; // Rellenar hasta el máximo
        
        for (int i = 0; i < incorrectsNeeded && incorrectObjects.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, incorrectObjects.Count);
            objectsToSpawn.Add(incorrectObjects[randomIndex]);
            incorrectObjects.RemoveAt(randomIndex);
        }
        
        ShuffleList(objectsToSpawn); 

        // 3. Generar en puntos aleatorios no ocupados
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        
        foreach (var objData in objectsToSpawn)
        {
            if (availableSpawnPoints.Count == 0) break;

            int randomSpawnIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomSpawnIndex];
            
            ObjectData newObject = Instantiate(baseObjectPrefab, spawnPoint.position, Quaternion.identity);
            
            // Asigna la plantilla de datos al componente del objeto
            newObject.SetData(objData);
            
            spawnedObjects.Add(newObject);
            availableSpawnPoints.RemoveAt(randomSpawnIndex); 
        }
    }
    
    /// <summary>
    /// Elimina el GameObject y su referencia de la lista (Llamado desde Manager).
    /// </summary>
    public void RemoveObjectFromList(GameObject heldObject)
    {
        ObjectData objData = heldObject.GetComponent<ObjectData>();
        if (objData != null)
        {
            spawnedObjects.Remove(objData); 
        }
        Destroy(heldObject);
    }
    
    /// <summary>
    /// Remueve el ScriptableObject de la lista de objetivos restantes.
    /// </summary>
    public void RemoveFromObjective(Object obj)
    {
        requiredObjects.Remove(obj);
    }
    
    /// <summary>
    /// Detiene la generación de objetos.
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
    }

    private void ClearSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
        spawnedObjects.Clear();
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}