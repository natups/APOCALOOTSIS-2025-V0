using UnityEngine;

// Asegúrate de que este script esté adjunto al Prefab base de tus objetos.
public class ObjectData : MonoBehaviour
{
    // Cambiamos de 'Object' a 'BaseDataObject' para asegurar la compatibilidad de tipos
    private BaseDataObject objectData;

    public BaseDataObject GetObjectData() => objectData;

    // Usamos el nombre del método de tu compañero y el tipo correcto de dato
    public void SetObjectData(BaseDataObject data)
    {
        objectData = data;
        
        // Actualizar el sprite y el color usando las propiedades de BaseDataObject
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Usamos objectSprite (del BaseDataObject.cs) en lugar de 'Asset'
            spriteRenderer.sprite = data.objectSprite; 
            // Usamos displayColor (del BaseDataObject.cs)
            spriteRenderer.color = data.displayColor; 
        }

        // Opcional: Establecer el nombre del GameObject para facilitar la depuración
        gameObject.name = "Objeto: " + data.objectName;
    }
}