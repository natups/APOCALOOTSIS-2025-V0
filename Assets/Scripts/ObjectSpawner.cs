using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Configuración de Objetos")]
    public ObjectData baseObjectPrefab;  // Prefab con el componente ObjectData.
    public List<Object> allPossibleObjectData; // Lista de todos los objetos posibles (correctos e incorrectos).
    public List<Transform> spawnPoints;  // Puntos de aparición de los objetos en el mapa.
    
    public int maxObjectsOnScreen = 10;  // Número máximo de objetos en pantalla a la vez.
    public int totalObjectsRequired = 5; // Número de objetos correctos requeridos para ganar.

    [HideInInspector] public List<Object> requiredObjects = new List<Object>();  // Objetivos correctos.
    [HideInInspector] public List<ObjectData> spawnedObjects = new List<ObjectData>();  // Objetos instanciados en la escena.
    
    private bool isSpawning = false;  // Indica si la generación de objetos está activa.

    /// Inicializa el generador seleccionando los objetivos y limpiando la escena.
    public void InitializeSpawner()
    {
        ClearSpawnedObjects();  // Limpia los objetos actualmente en la escena.
        requiredObjects.Clear();  // Limpia los objetos requeridos.
        isSpawning = false;  // Detiene el proceso de generación.

        List<Object> availableObjects = new List<Object>(allPossibleObjectData);

        if (availableObjects.Count < totalObjectsRequired)
        {
            return;  // Si no hay suficientes objetos para cumplir los objetivos, no hacer nada.
        }

        // Selecciona los objetos correctos que serán los objetivos.
        for (int i = 0; i < totalObjectsRequired; i++)
        {
            int randomIndex = Random.Range(0, availableObjects.Count);
            requiredObjects.Add(availableObjects[randomIndex]);
            availableObjects.RemoveAt(randomIndex);  // Asegura que los objetos sean únicos.
        }
    }

    /// Comienza la generación de objetos en la escena.
    public void StartSpawning()
    {
        isSpawning = true;
        RefillObjectsOnScreen();  // Llama a la generación de objetos en pantalla.
    }

    /// Rellena la escena con objetos hasta el máximo permitido.
    public void RefillObjectsOnScreen()
    {
        if (!isSpawning) return;

        spawnedObjects.RemoveAll(item => item == null);  // Limpia las referencias nulas.

        List<Object> objectsToSpawn = new List<Object>();
        List<Object> allObjectsTyped = allPossibleObjectData.Cast<Object>().ToList();

        // Identifica los objetos correctos que no están en escena.
        var requiredNotInScene = requiredObjects
            .Where(req => !spawnedObjects.Any(s => s.data == req))
            .ToList();

        // Identifica los objetos incorrectos que no están en escena.
        var incorrectNotInScene = allObjectsTyped
            .Except(requiredObjects)
            .Where(data => !spawnedObjects.Any(s => s.data == data))
            .ToList();

        int objectsToCreate = maxObjectsOnScreen - spawnedObjects.Count;

        // Asegura que haya una mezcla de objetos correctos e incorrectos en la escena.
        while (objectsToCreate > 0)
        {
            bool canSpawnRequired = requiredNotInScene.Count > 0;
            bool canSpawnIncorrect = incorrectNotInScene.Count > 0;
            
            if (!canSpawnRequired && !canSpawnIncorrect) break;

            Object objectToUse = null;

            // Prioriza los objetos correctos si están disponibles.
            if (canSpawnRequired && (canSpawnIncorrect == false || Random.Range(0f, 1f) < 0.5f))
            {
                int index = Random.Range(0, requiredNotInScene.Count);
                objectToUse = requiredNotInScene[index];
                requiredNotInScene.RemoveAt(index);
            }
            else if (canSpawnIncorrect)
            {
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

        // Aleatoriza el orden de los objetos a generar.
        ShuffleList(objectsToSpawn); 
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        // Instancia los objetos en los puntos de aparición.
        foreach (var objData in objectsToSpawn)
        {
            if (availableSpawnPoints.Count == 0) break;

            int randomSpawnIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomSpawnIndex];
            
            ObjectData newObject = Instantiate(baseObjectPrefab, spawnPoint.position, Quaternion.identity, transform);
            newObject.SetData(objData);  // Asigna los datos al objeto.

            // Ajuste de escala dependiendo del modo de juego (versus o cooperativo).
            if (ZonaDeEntregaManager.Instance != null && ZonaDeEntregaManager.Instance.IsVersusMode())
            {
                newObject.transform.localScale = Vector3.one * 0.2f;  // Escala reducida en modo versus.
            }
            else
            {
                newObject.transform.localScale = Vector3.one * 0.1f;  // Escala normal en modo cooperativo.
            }

            spawnedObjects.Add(newObject);
            availableSpawnPoints.RemoveAt(randomSpawnIndex);
        }
    }

    /// Elimina un objeto de la lista y lo destruye en la escena.
    public void RemoveObjectFromList(GameObject heldObject)
    {
        ObjectData objData = heldObject.GetComponent<ObjectData>();
        if (objData != null)
        {
            spawnedObjects.Remove(objData);  // Elimina la referencia del objeto de la lista.
        }
        Destroy(heldObject);  // Destruye el objeto de la escena.
    }

    /// Remueve un objeto del conjunto de objetivos correctos.
    public void RemoveFromObjective(Object obj)
    {
        requiredObjects.Remove(obj);  // Elimina el objeto de los objetivos restantes.
    }

    /// Detiene el proceso de generación de objetos.
    public void StopSpawning()
    {
        isSpawning = false;  // Detiene la generación de nuevos objetos.
    }

    // Limpiar los objetos ya generados de la escena.
    private void ClearSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);  // Elimina el objeto de la escena.
            }
        }
        spawnedObjects.Clear();  // Limpia la lista de objetos generados.
    }

    // Aleatoriza el orden de los elementos de una lista.
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
