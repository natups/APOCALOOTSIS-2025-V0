using UnityEngine;

/// <summary>
/// Este script es un componente de la escena que se adjunta a los objetos generados.
/// Contiene una referencia a su Scriptable Object (tipo 'Object').
/// </summary>
public class ObjectData : MonoBehaviour
{
    [Tooltip("La data principal (Scriptable Object) que define este objeto.")]
    // Usamos el tipo 'Object', que es la clase que definiste
    public Object data; 

    // Referencia al renderizador para actualizar la apariencia
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        // El objeto en la escena debe tener un SpriteRenderer para la parte visual
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("ObjectData requiere un SpriteRenderer en el mismo GameObject.");
        }
    }

    /// <summary>
    /// Asigna el Scriptable Object 'Object' a esta instancia y actualiza su apariencia.
    /// Este método es llamado por ObjectSpawner al crear el objeto.
    /// </summary>
    /// <param name="objectData">El Scriptable Object de tipo 'Object' a usar.</param>
    public void SetData(Object objectData)
    {
        if (objectData == null)
        {
            Debug.LogError("Se intentó asignar data nula a ObjectData.");
            return;
        }

        this.data = objectData;

        // Actualiza la apariencia visual
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = data.objectSprite;
            spriteRenderer.color = data.displayColor;
        }

        // Renombrar el objeto en la jerarquía (ayuda al debug)
        gameObject.name = "Objeto: " + data.objectName;
    }
}