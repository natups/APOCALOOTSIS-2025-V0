using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la visualización del temporizador (cambio de sprites de la ventana/pared).
/// Es autónomo y usa LateUpdate() para actualizarse, evitando conflictos con otros scripts.
/// </summary>
public class VisualTimerController : MonoBehaviour
{
    [Header("Sprites del Temporizador")]
    [Tooltip("Lista de sprites a usar, donde el índice 0 es el tiempo completo (ventana vacía) y el último es el tiempo agotado (lleno de zombies).")]
    public Sprite[] timerSprites;

    // ELIMINAMOS [Tooltip("El Sprite Renderer...")] y lo hacemos privado. 
    // Ahora OBTENDRÁ ESTE COMPONENTE AUTOMÁTICAMENTE en Start.
    private SpriteRenderer localSpriteRenderer;
    
    [Tooltip("El UI Image que mostrará los cambios (para Canvas/UI).")]
    public Image targetUIImage;
    
    [Header("Referencias de Juego")]
    [Tooltip("Referencia al GameTimer que proporciona el progreso del tiempo. ¡ASÍGNALO EN EL INSPECTOR!")]
    public GameTimer gameTimer; // Hacemos pública para facilitar la asignación en el Inspector

    private int maxIndex;
    private int currentSpriteIndex = -1; // -1 para forzar la primera actualización

    void Start()
    {
        // 1. Verificación de seguridad de Sprites
        if (timerSprites == null || timerSprites.Length == 0)
        {
            Debug.LogError("VisualTimerController: La lista 'Timer Sprites' está vacía. ¡Asigna los sprites en el Inspector!");
            enabled = false;
            return;
        }

        // 2. OBTENER EL SPRITE RENDERER EN ESTE OBJETO (Ventana)
        // ESTO REEMPLAZA LA ASIGNACIÓN MANUAL DEL INSPECTOR
        localSpriteRenderer = GetComponent<SpriteRenderer>();

        // 3. Verificación final de Componentes
        if (localSpriteRenderer == null && targetUIImage == null)
        {
            Debug.LogError("VisualTimerController: No se encontró Sprite Renderer ni se asignó UI Image. Desactivando script.");
            enabled = false;
            return;
        }
        
        // 4. Verificación y búsqueda de GameTimer 
        if (gameTimer == null)
        {
             gameTimer = FindAnyObjectByType<GameTimer>(); 
             if (gameTimer == null)
             {
                 Debug.LogError("VisualTimerController: No se encontró el GameTimer. Desactivando script.");
                 enabled = false;
                 return;
             }
        }

        maxIndex = timerSprites.Length - 1;
        
        // Inicializa el visual en el primer sprite (tiempo completo / ventana vacía)
        UpdateVisual(0);
        Debug.Log("VisualTimerController: Inicializado. Sprite Renderer encontrado en este objeto: " + (localSpriteRenderer != null));
    }
    
    // USAMOS LATEUPDATE: Se ejecuta DESPUÉS de todos los Update() (incluyendo el de GameTimer).
    void LateUpdate()
    {
        if (gameTimer == null || !enabled) return;

        float progress = gameTimer.GetTimeProgress();
        
        UpdateVisualByProgress(progress);
    }

    /// <summary>
    /// Calcula el índice del sprite y aplica la visualización.
    /// </summary>
    private void UpdateVisualByProgress(float progress)
    {
        float inverseProgress = 1f - progress;
        int targetIndex = Mathf.RoundToInt(maxIndex * inverseProgress);
        
        targetIndex = Mathf.Clamp(targetIndex, 0, maxIndex);

        if (targetIndex == currentSpriteIndex) return;
        
        currentSpriteIndex = targetIndex;

        // LOG DE DEPURACIÓN CRÍTICO (Solo se dispara al cambiar el sprite)
        Debug.Log($"VisualTimer: ¡Sprite cambiado! Progreso={progress:F2}, Nuevo Índice={targetIndex}");

        UpdateVisual(targetIndex);
    }
    
    /// <summary>
    /// Función interna que aplica el sprite al componente de visualización.
    /// </summary>
    private void UpdateVisual(int index)
    {
        if (index < 0 || index > maxIndex) return;

        Sprite newSprite = timerSprites[index];

        // Usamos el Sprite Renderer local forzado
        if (localSpriteRenderer != null)
        {
            localSpriteRenderer.sprite = newSprite;
            Debug.Log($"VisualTimer [APLICADO]: SpriteRenderer local establecido en: {newSprite.name} (Índice {index}).");
        }
        
        if (targetUIImage != null)
        {
            targetUIImage.sprite = newSprite;
        }
    }
    
    /// <summary>
    /// Detiene los visuales (llamado por GameTimer al finalizar).
    /// </summary>
    public void StopVisuals()
    {
        if (timerSprites == null || timerSprites.Length == 0) return;
        
        UpdateVisual(maxIndex); 
        enabled = false; 
        Debug.Log("VisualTimer: Visuales detenidos en el estado final (Índice " + maxIndex + ").");
    }
}