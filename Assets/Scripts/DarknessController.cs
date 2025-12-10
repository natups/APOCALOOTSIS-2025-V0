using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Script auxiliar para manejar los efectos de oscuridad y penalización visual de pantalla completa.
/// Este script asume que el mismo 'fullScreenOverlay' se usa para la oscuridad inicial y para el flash de penalización.
/// </summary>
public class DarknessController : MonoBehaviour
{
    [Header("Referencia UI de Oscuridad")]
    [Tooltip("El GameObject (Panel Canvas) que cubre toda la pantalla. Es usado para el bloqueo inicial y el flash.")]
    public GameObject fullScreenOverlay;

    // Nota: Necesitamos el componente Image del overlay para cambiar su color y opacidad.
    private Image overlayImage; 
    
    [Header("Configuración de Flash")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.7f); // Rojo Semi-Transparente
    public float flashDuration = 0.2f; // Duración total del parpadeo

    private void Start()
    {
        if (fullScreenOverlay != null)
        {
            // OBTENEMOS la referencia al componente Image en el Awake/Start
            overlayImage = fullScreenOverlay.GetComponent<Image>();
            if (overlayImage == null)
            {
                Debug.LogError("DarknessController requiere un componente Image en el Full Screen Overlay para funcionar.");
            }
        }
    }

    // --- MÉTODOS DE OSCURIDAD DE JUEGO ---
    
    /// <summary>
    /// Desactiva la capa de oscuridad total para revelar el juego.
    /// Llamada por ZonaDeEntregaManager al inicio de la fase de juego.
    /// </summary>
    public void RemoveDarknessOverlay()
    {
        // Aseguramos que la opacidad del panel sea CERO antes de desactivarlo.
        if (overlayImage != null)
        {
            overlayImage.color = new Color(overlayImage.color.r, overlayImage.color.g, overlayImage.color.b, 0f);
        }

        if (fullScreenOverlay != null)
        {
            fullScreenOverlay.SetActive(false);
            Debug.Log("DarknessController: ¡Capa de oscuridad removida!");
        }
    }

    public void StartDarknessIncrease()
    {
        Debug.Log("DarknessController: Empezando aumento gradual de oscuridad (Lógica pendiente).");
    }
    
    public void StopDarknessIncrease()
    {
        Debug.Log("DarknessController: Deteniendo aumento.");
    }
    
    // --- LÓGICA DE PENALIZACIÓN VISUAL (Flash Rojo) ---

    /// <summary>
    /// Ejecuta un parpadeo de color (rojo) usando el mismo PanelOscuridad.
    /// Llamado por ZonaDeEntregaManager.
    /// </summary>
    public void FlashPenalty()
    {
        if (overlayImage == null)
        {
            Debug.LogError("DarknessController: Componente Image no encontrado. No se puede mostrar el flash.");
            return;
        }
        
        // Detiene cualquier flash anterior en curso y comienza uno nuevo.
        StopCoroutine("DoFlash");
        StartCoroutine("DoFlash");
    }
    
    private IEnumerator DoFlash()
    {
        // 1. Activamos el panel si no lo está (ya que se desactivó al inicio del juego)
        if (fullScreenOverlay != null && !fullScreenOverlay.activeSelf)
        {
            fullScreenOverlay.SetActive(true);
        }
        
        // 2. Establecer color inicial (Rojo con opacidad)
        overlayImage.color = flashColor;
        
        float timer = 0f;
        
        // 3. Desvanecimiento: de color inicial a transparente en la duración especificada.
        while (timer < flashDuration)
        {
            timer += Time.unscaledDeltaTime;
            
            // Interpolación de Alpha (opacidad)
            Color currentColor = flashColor;
            // Va del alfa inicial (0.7) a 0.0
            currentColor.a = Mathf.Lerp(flashColor.a, 0f, timer / flashDuration); 
            
            overlayImage.color = currentColor;
            yield return null;
        }

        // 4. Limpieza final: asegurar transparencia total y desactivar el panel.
        overlayImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        if (fullScreenOverlay != null)
        {
            fullScreenOverlay.SetActive(false);
        }
    }
}