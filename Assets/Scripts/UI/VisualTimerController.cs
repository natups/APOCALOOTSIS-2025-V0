using UnityEngine;
using UnityEngine.UI;

// Gestiona la visualización del temporizador cambiando sprites
// según el progreso del tiempo del GameTimer.
public class VisualTimerController : MonoBehaviour
{
    // Lista de sprites del temporizador.
    // El índice 0 representa el tiempo completo y el último el tiempo agotado.
    public Sprite[] timerSprites;

    // SpriteRenderer del propio objeto (se obtiene automáticamente)
    private SpriteRenderer localSpriteRenderer;

    // Imagen UI opcional para mostrar el temporizador en Canvas
    public Image targetUIImage;

    // Referencia al GameTimer que informa el progreso del tiempo
    public GameTimer gameTimer;

    // Índice máximo disponible según la cantidad de sprites
    private int maxIndex;

    // Último índice aplicado (se inicia en -1 para forzar la primera actualización)
    private int currentSpriteIndex = -1;

    void Start()
    {
        // Verifica que haya sprites configurados
        if (timerSprites == null || timerSprites.Length == 0)
        {
            enabled = false;
            return;
        }

        // Obtiene automáticamente el SpriteRenderer del objeto
        localSpriteRenderer = GetComponent<SpriteRenderer>();

        // Si no hay forma de mostrar el sprite, se desactiva el script
        if (localSpriteRenderer == null && targetUIImage == null)
        {
            enabled = false;
            return;
        }

        // Si no se asignó el GameTimer, se intenta buscar en la escena
        if (gameTimer == null)
        {
            gameTimer = FindAnyObjectByType<GameTimer>();
            if (gameTimer == null)
            {
                enabled = false;
                return;
            }
        }

        // Calcula el índice máximo válido
        maxIndex = timerSprites.Length - 1;

        // Inicializa el visual en el primer sprite (tiempo completo)
        UpdateVisual(0);
    }

    // Se usa LateUpdate para asegurarse de que el GameTimer ya se haya actualizado
    void LateUpdate()
    {
        if (gameTimer == null || !enabled) return;

        float progress = gameTimer.GetTimeProgress();
        UpdateVisualByProgress(progress);
    }

    // Calcula qué sprite corresponde según el progreso del tiempo
    private void UpdateVisualByProgress(float progress)
    {
        // Invertimos el progreso: 0 = inicio, 1 = final
        float inverseProgress = 1f - progress;

        // Calcula el índice objetivo
        int targetIndex = Mathf.RoundToInt(maxIndex * inverseProgress);
        targetIndex = Mathf.Clamp(targetIndex, 0, maxIndex);

        // Evita actualizar si el sprite no cambió
        if (targetIndex == currentSpriteIndex) return;

        currentSpriteIndex = targetIndex;
        UpdateVisual(targetIndex);
    }

    // Aplica el sprite al SpriteRenderer y/o Image
    private void UpdateVisual(int index)
    {
        if (index < 0 || index > maxIndex) return;

        Sprite newSprite = timerSprites[index];

        if (localSpriteRenderer != null)
        {
            localSpriteRenderer.sprite = newSprite;
        }

        if (targetUIImage != null)
        {
            targetUIImage.sprite = newSprite;
        }
    }

    // Fuerza el estado visual final y detiene futuras actualizaciones
    public void StopVisuals()
    {
        if (timerSprites == null || timerSprites.Length == 0) return;

        UpdateVisual(maxIndex);
        enabled = false;
    }
}
