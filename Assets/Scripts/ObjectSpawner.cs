using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// NOTA: Asumimos que la clase 'Object' (ScriptableObject) y 'ObjectData' (MonoBehaviour) existen.
public class ObjectSpawner : MonoBehaviour
{
    [Header("Configuración de Objetos")]
    [Tooltip("El prefab con el componente ObjectData.")]
    public ObjectData baseObjectPrefab; 
    [Tooltip("Todos los ScriptableObjects de objetos posibles (Correctos + Incorrectos).")]
    public List<Object> allPossibleObjectData; 
    public List<Transform> spawnPoints;
    
    [Tooltip("Número máximo de objetos que puede haber en la escena a la vez (Ej: 10).")]
    public int maxObjectsOnScreen = 10; 
    [Tooltip("Número de objetos correctos requeridos para ganar (Ej: 5).")]
    public int totalObjectsRequired = 5; 

    [HideInInspector] public List<Object> requiredObjects = new List<Object>(); // Objetivos (SO)
    [HideInInspector] public List<ObjectData> spawnedObjects = new List<ObjectData>(); // Instancias en escena (MB)
    
    private bool isSpawning = false; 

    /// <summary>
    /// PRE-JUEGO: Selecciona los objetivos de la partida (5 correctos) y limpia el escenario.
    /// </summary>
    public void InitializeSpawner()
    {
        ClearSpawnedObjects();
        requiredObjects.Clear();
        isSpawning = false;
        
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
            availableObjects.RemoveAt(randomIndex); // Asegura que sean únicos
        }
        
        Debug.Log($"Objetivos seleccionados: {requiredObjects.Count}");
    }
    
    /// <summary>
    /// Llamado por el Manager para empezar a generar objetos en el escenario.
    /// </summary>
    public void StartSpawning()
    {
        isSpawning = true;
        RefillObjectsOnScreen(); // Primera llamada para llenar el mapa
    }
    
    /// <summary>
    /// Genera objetos (correctos e incorrectos) para rellenar el mapa hasta maxObjectsOnScreen.
    /// </summary>
    public void RefillObjectsOnScreen()
    {
        if (!isSpawning) return;
        
        spawnedObjects.RemoveAll(item => item == null); // Limpia referencias nulas
        
        List<Object> objectsToSpawn = new List<Object>();
        List<Object> allObjectsTyped = allPossibleObjectData.Cast<Object>().ToList();
        
        // 1. Identifica los objetos CORRECTOS que AÚN NO han sido entregados y AÚN NO están en escena.
        // Estos tienen la mayor prioridad para ser generados.
        var requiredNotInScene = requiredObjects
            .Where(req => !spawnedObjects.Any(s => s.data == req))
            .ToList();

        // 2. Identifica los objetos INCORRECTOS (distractores)
        // Son todos los objetos posibles que NO son requeridos y que NO están en escena.
        var incorrectNotInScene = allObjectsTyped
            .Except(requiredObjects)
            .Where(data => !spawnedObjects.Any(s => s.data == data))
            .ToList();
        
        int objectsToCreate = maxObjectsOnScreen - spawnedObjects.Count;
        
        // A. PRIORIDAD AL REQUERIDO: Generar uno de los requeridos que falta (si aplica)
        if (requiredNotInScene.Count > 0 && objectsToCreate > 0)
        {
            // Tomamos el primero de la lista de requeridos que falta en escena
            objectsToSpawn.Add(requiredNotInScene[0]);
            objectsToCreate--;
            requiredNotInScene.RemoveAt(0);
        }
        
        // B. RELLENO: Rellenar el resto con una mezcla
        while (objectsToCreate > 0)
        {
            // Decisión: ¿Generar otro Requerido o un Incorrecto?
            // Haremos que haya un 50% de probabilidad de generar un requerido (si quedan)
            
            bool canSpawnRequired = requiredNotInScene.Count > 0;
            bool canSpawnIncorrect = incorrectNotInScene.Count > 0;
            
            if (!canSpawnRequired && !canSpawnIncorrect) break; // No quedan objetos para generar

            Object objectToUse = null;

            if (canSpawnRequired && (canSpawnIncorrect == false || Random.Range(0f, 1f) < 0.5f))
            {
                // Generar Requerido
                int index = Random.Range(0, requiredNotInScene.Count);
                objectToUse = requiredNotInScene[index];
                requiredNotInScene.RemoveAt(index);
            }
            else if (canSpawnIncorrect)
            {
                // Generar Incorrecto (Distractor)
                int index = Random.Range(0, incorrectNotInScene.Count);
                objectToUse = incorrectNotInScene[index];
                incorrectNotInScene.RemoveAt(index);
            }
            
            if (objectToUse != null)
            {
                objectsToSpawn.Add(objectToUse);
                objectsToCreate--;
            }
        }
        
        // 3. GENERACIÓN: Mezclar la lista y generar en puntos aleatorios no ocupados
        ShuffleList(objectsToSpawn); 
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        
        foreach (var objData in objectsToSpawn)
        {
            if (availableSpawnPoints.Count == 0) break;

            int randomSpawnIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomSpawnIndex];
            
            ObjectData newObject = Instantiate(baseObjectPrefab, spawnPoint.position, Quaternion.identity, transform);
            
            // ************ CORRECCIÓN CRÍTICA DE ASIGNACIÓN ************
            // Antes: newObject.data = objData; <-- NO LLAMABA A ApplyData()
            // Ahora:
            newObject.SetData(objData); // <-- LLAMA A SetData, que llama a ApplyData() y fija la escala.
            // **********************************************************
            
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
    /// Remueve el ScriptableObject de la lista de objetivos restantes (entrega correcta).
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