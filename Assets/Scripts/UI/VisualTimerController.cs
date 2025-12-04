using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Este script debe adjuntarse al GameObject "Ventana" o a donde se muestre el reloj visual.
public class VisualTimerController : MonoBehaviour
{
    [Header("Sprites del Temporizador")]
    [Tooltip("Lista de sprites para mostrar el progreso del tiempo, de lleno (índice 0) a vacío (índice final).")]
    public Sprite[] timerSprites; 

    [Header("Componente de Visualización")]
    [Tooltip("El componente SpriteRenderer (para objetos 2D) o Image (para UI) que mostrará los sprites.")]
    // La Ventana es probablemente un SpriteRenderer.
    public SpriteRenderer targetSpriteRenderer;
    public Image targetUIImage; 

    private int currentSpriteIndex = -1;

    void Start()
    {
        if (timerSprites == null || timerSprites.Length == 0)
        {
            Debug.LogError("VisualTimerController: No hay sprites asignados. ¡Asigna tus sprites de tiempo!");
            return;
        }

        if (targetSpriteRenderer == null && targetUIImage == null)
        {
            Debug.LogError("VisualTimerController: No se ha asignado un Renderer o Image.");
            return;
        }
        
        // Muestra el primer sprite (lleno) al inicio
        // Usamos el que esté asignado, dando prioridad al SpriteRenderer
        if (targetSpriteRenderer != null)
        {
             targetSpriteRenderer.sprite = timerSprites[0];
        }
        else if (targetUIImage != null)
        {
             targetUIImage.sprite = timerSprites[0];
        }
    }
    
    /// <summary>
    /// Llamado por GameTimer.cs para actualizar el sprite basado en el tiempo.
    /// </summary>
    /// <param name="timeProgress">El progreso del tiempo, de 1.0 (lleno) a 0.0 (vacío).</param>
    public void UpdateVisualTimer(float timeProgress)
    {
        if (timerSprites == null || timerSprites.Length == 0) return;

        // Aseguramos que el progreso esté entre 0 y 1
        timeProgress = Mathf.Clamp01(timeProgress);

        int maxIndex = timerSprites.Length - 1;
        
        // La lógica invierte el progreso:
        // Si timeProgress es 1.0 (lleno) -> index final 0
        // Si timeProgress es 0.0 (vacío) -> index final maxIndex
        
        // 1. Calcula el índice proporcional (0 para lleno, maxIndex para vacío)
        int indexSpent = Mathf.FloorToInt((1f - timeProgress) * maxIndex);
        
        // 2. Asegura límites
        indexSpent = Mathf.Clamp(indexSpent, 0, maxIndex);

        if (indexSpent != currentSpriteIndex)
        {
            currentSpriteIndex = indexSpent;
            // Actualiza el sprite
            if (targetSpriteRenderer != null)
            {
                targetSpriteRenderer.sprite = timerSprites[currentSpriteIndex];
            }
            else if (targetUIImage != null)
            {
                targetUIImage.sprite = timerSprites[currentSpriteIndex];
            }
        }
        
        // --- AQUÍ IRÍA CUALQUIER OTRA MECÁNICA DE VENTANA ---
        // Ejemplo: Si la ventana debe "temblar" o "acercar la amenaza" 
        // a medida que timeProgress se acerca a cero, iría en esta sección.
    }
    
    // Método para detener la ventana visualmente al final
    public void StopVisuals()
    {
        // Esto asegura que la ventana muestre el sprite de tiempo agotado (el último de la lista)
        if (timerSprites != null && timerSprites.Length > 0)
        {
            int lastIndex = timerSprites.Length - 1;
             if (targetSpriteRenderer != null)
            {
                 targetSpriteRenderer.sprite = timerSprites[lastIndex];
            }
            else if (targetUIImage != null)
            {
                 targetUIImage.sprite = timerSprites[lastIndex];
            }
        }
    }
}