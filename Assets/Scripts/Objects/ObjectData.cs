using UnityEngine;

/// <summary>
/// Componente que se coloca en el objeto de juego recogible.
/// Carga la plantilla de datos (BaseDataObject) y aplica sus propiedades visuales.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ObjectData : MonoBehaviour
{
    // Hacemos el campo privado para forzar el uso de SetData() y evitamos la asignación directa.
    // Usamos 'Object' porque es lo que espera ObjectSpawner, pero lo ocultamos del Inspector.
    [HideInInspector]
    public Object data; 

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Si data ya está asignada (p. ej., en Awake), se aplica.
        if (data != null)
        {
            ApplyData();
        }
    }

    /// <summary>
    /// CRÍTICO: Asigna el ScriptableObject dinámicamente y llama a ApplyData.
    /// </summary>
    public void SetData(Object objectData)
    {
        // 1. Asigna los datos.
        data = objectData;
        
        // 2. LLAMADA CRÍTICA: Aplica los datos inmediatamente al SpriteRenderer.
        ApplyData(); 

        // 3. FIX DE ESCALA: Ajusta la escala si es excesivamente grande.
        if (transform.localScale.x > 5f || transform.localScale.y > 5f)
        {
             transform.localScale = Vector3.one * 1f; 
        }
    }

    private void ApplyData()
    {
        // Intenta castear el Object genérico a tu clase específica BaseDataObject.
        BaseDataObject baseData = data as BaseDataObject;
        
        if (spriteRenderer != null && baseData != null)
        {
            if (baseData.objectSprite != null) // Verifica que el sprite exista
            {
                spriteRenderer.sprite = baseData.objectSprite;
                spriteRenderer.color = baseData.displayColor;
            }
            else
            {
                Debug.LogError($"El objeto de datos '{baseData.objectName}' NO tiene un sprite asignado.");
                spriteRenderer.sprite = null; 
                spriteRenderer.color = Color.magenta; // Magenta para fácil visualización de error.
            }
            
            gameObject.name = "Objeto - " + baseData.objectName; 
        }
        else if (data != null)
        {
            Debug.LogError($"FALLÓ EL CASTEO: El objeto asignado '{data.name}' no es un BaseDataObject válido o SpriteRenderer no encontrado.");
        }
    }
}