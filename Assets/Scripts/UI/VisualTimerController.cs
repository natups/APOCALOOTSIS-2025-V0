using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Muestra el progreso del tiempo usando una secuencia de sprites (Ej: una ventana que se cierra).
/// Este script es actualizado por GameTimer.cs.
/// </summary>
public class VisualTimerController : MonoBehaviour
{
    [Header("Sprites del Temporizador")]
    [Tooltip("Lista de sprites para mostrar el progreso del tiempo, de lleno (índice 0) a vacío (índice final).")]
    public Sprite[] timerSprites; 

    [Header("Componente de Visualización")]
    [Tooltip("El componente SpriteRenderer (para objetos 2D) o Image (para UI) que mostrará los sprites.")]
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
        
        // Muestra el primer sprite (lleno) al inicio.
        if (timerSprites.Length > 0)
        {
             if (targetSpriteRenderer != null)
             {
                 targetSpriteRenderer.sprite = timerSprites[0];
             }
             else if (targetUIImage != null)
             {
                 targetUIImage.sprite = timerSprites[0];
             }
             currentSpriteIndex = 0;
        }
    }
    
    /// <summary>
    /// Llamado por GameTimer.cs para actualizar el sprite basado en el tiempo.
    /// </summary>
    /// <param name="timeProgress">El progreso del tiempo, de 1.0 (lleno) a 0.0 (vacío).</param>
    public void UpdateVisualTimer(float timeProgress)
    {
        if (timerSprites == null || timerSprites.Length == 0) return;

        // 1. Aseguramos que el progreso esté entre 0 y 1
        timeProgress = Mathf.Clamp01(timeProgress);

        int maxIndex = timerSprites.Length - 1;
        
        // 2. Calcula el índice (0 para lleno, maxIndex para vacío)
        // La lógica invierte el progreso: (1 - timeProgress) * maxIndex
        int indexSpent = Mathf.FloorToInt((1f - timeProgress) * maxIndex);
        
        // 3. Asegura límites
        indexSpent = Mathf.Clamp(indexSpent, 0, maxIndex);

        if (indexSpent != currentSpriteIndex)
        {
            currentSpriteIndex = indexSpent;
            // 4. Actualiza el sprite
            if (targetSpriteRenderer != null)
            {
                targetSpriteRenderer.sprite = timerSprites[currentSpriteIndex];
            }
            else if (targetUIImage != null)
            {
                targetUIImage.sprite = timerSprites[currentSpriteIndex];
            }
        }
    }
    
    /// <summary>
    /// Detiene la ventana visualmente al final o resetea al inicio (muestra el último sprite).
    /// </summary>
    public void StopVisuals()
    {
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
             currentSpriteIndex = lastIndex;
        }
    }
}