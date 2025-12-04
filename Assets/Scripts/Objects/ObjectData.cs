using UnityEngine;

/// <summary>
/// Componente que se coloca en el objeto de juego recogible.
/// Carga la plantilla de datos (Object ScriptableObject) y aplica sus propiedades visuales.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ObjectData : MonoBehaviour
{
    // CRÍTICO: Propiedad pública requerida por ZonaDeEntregaManager.
    // Usamos el tipo 'Object' (que hereda de BaseDataObject).
    [Tooltip("La plantilla de datos (ScriptableObject) que define este ítem.")]
    public Object data; 

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (data != null)
        {
            ApplyData();
        }
    }

    // Método usado por ObjectSpawner para asignar el ScriptableObject dinámicamente al generar.
    public void SetData(Object objectData)
    {
        data = objectData;
        ApplyData();
    }

    private void ApplyData()
    {
        if (spriteRenderer != null && data != null)
        {
            // Usamos las propiedades definidas en la clase base BaseDataObject
            spriteRenderer.sprite = data.objectSprite;
            spriteRenderer.color = data.displayColor;
        }
    }
}