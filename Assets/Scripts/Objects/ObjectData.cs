using UnityEngine;

/// Componente que se coloca en cada objeto recogible del juego.
/// Se encarga de cargar los datos desde un ScriptableObject
/// y aplicar sus propiedades visuales al SpriteRenderer.
[RequireComponent(typeof(SpriteRenderer))]
public class ObjectData : MonoBehaviour
{
    // ==============================
    // DATOS DEL OBJETO
    // ==============================

    // ScriptableObject con la información del objeto (sprite, color, nombre, etc.)
    // Se mantiene oculto para forzar el uso de SetData()
    [HideInInspector]
    public Object data;

    // Referencia al SpriteRenderer del objeto
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // Obtiene el SpriteRenderer obligatorio
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Si los datos ya estaban asignados, se aplican automáticamente
        if (data != null)
        {
            ApplyData();
        }
    }

    /// Asigna dinámicamente el ScriptableObject al objeto
    /// y aplica inmediatamente sus propiedades visuales.
    public void SetData(Object objectData)
    {
        // Asigna los datos del objeto
        data = objectData;

        // Aplica sprite, color y nombre
        ApplyData();

        // Ajuste de seguridad para evitar escalas excesivamente grandes
        if (transform.localScale.x > 5f || transform.localScale.y > 5f)
        {
            transform.localScale = Vector3.one;
        }
    }

    /// Aplica al SpriteRenderer la información visual contenida en el ScriptableObject.
    private void ApplyData()
    {
        // Convierte el Object genérico al tipo base esperado
        BaseDataObject baseData = data as BaseDataObject;

        // Verifica que existan los datos y el renderer
        if (spriteRenderer != null && baseData != null)
        {
            // Asigna el sprite y el color definidos en el ScriptableObject
            if (baseData.objectSprite != null)
            {
                spriteRenderer.sprite = baseData.objectSprite;
                spriteRenderer.color = baseData.displayColor;
            }
            else
            {
                // Si no hay sprite, se limpia la referencia visual
                spriteRenderer.sprite = null;
                spriteRenderer.color = Color.magenta;
            }

            // Renombra el GameObject para facilitar debugging en la jerarquía
            gameObject.name = "Objeto - " + baseData.objectName;
        }
    }
}
