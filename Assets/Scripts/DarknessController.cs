using UnityEngine;
using UnityEngine.UI; // Necesario para Image

/// <summary>
/// Script auxiliar para manejar los efectos de oscuridad. La oscuridad simplemente aumenta
/// gradual y proporcionalmente a medida que disminuye el tiempo restante.
/// Solución CRÍTICA: Se añade la activación explícita del GameObject en Awake y StartDarknessIncrease.
/// </summary>
public class DarknessController : MonoBehaviour
{
    [Header("Referencias de Oscuridad")]
    [Tooltip("El componente Image del 'Panel Oscuridad' que cubrirá la pantalla. ¡DEBE ESTAR ASIGNADO!")]
    public Image darknessPanel; 
    
    [Header("Configuración de Oscuridad")]
    [Tooltip("La opacidad MÁXIMA que tendrá el panel (0.0 = transparente, 1.0 = negro total).")]
    [Range(0f, 1f)]
    public float maxDarknessAlpha = 0.8f; // Opacidad máxima de negro (80%)
    
    [Tooltip("Referencia al GameTimer para obtener el progreso del tiempo. ¡DEBE ESTAR ASIGNADO!")]
    public GameTimer gameTimer; 

    // Ya no necesitamos 'panelColor' globalmente. Leeremos/escribiremos el color directamente en Update
    // para mayor estabilidad y para evitar problemas de copia de struct Color.
    private bool isIncreasing = false;
    
    // --- SOLUCIÓN CRÍTICA: Inicializar en AWAKE para garantizar referencias en el build. ---
    void Awake()
    {
        // ===================================
        // VERIFICACIÓN CRÍTICA DE REFERENCIAS
        // ===================================
        if (darknessPanel == null)
        {
            Debug.LogError("DARKNESS CRÍTICO: ¡El Panel Oscuridad (Image) NO está asignado en el Inspector! El script se desactivará.");
            enabled = false;
            return;
        }

        // Si GameTimer no fue asignado, intentamos encontrarlo inmediatamente, 
        if (gameTimer == null)
        {
             gameTimer = FindAnyObjectByType<GameTimer>();
             if (gameTimer == null)
             {
                 Debug.LogError("DARKNESS CRÍTICO: No se encontró GameTimer. El script se desactivará.");
                 enabled = false;
                 return;
             }
        }
        
        // ** ¡FIX 1 CRÍTICO! **
        // Aseguramos que el GameObject del Panel esté activo desde el inicio.
        if (!darknessPanel.gameObject.activeSelf)
        {
            darknessPanel.gameObject.SetActive(true);
            Debug.Log("DARKNESS FIX: GameObject del panel activado en Awake.");
        }
        
        // Inicializar completamente transparente
        Color initialColor = darknessPanel.color;
        initialColor.a = 0f;
        darknessPanel.color = initialColor;

        // Asegurar que el componente Image esté activo (Enabled).
        darknessPanel.enabled = true; 
        
        Debug.Log($"DARKNESS DEBUG: Inicialización completada en Awake. Alpha inicial: {darknessPanel.color.a:F2}.");
    }
    
    void Update()
    {
        // Solo ejecutar lógica si la oscuridad debe aumentar y tenemos la referencia del temporizador
        if (!isIncreasing || gameTimer == null)
        {
            return; 
        }

        // 1. Obtener progreso del tiempo: 1.0 (inicio) a 0.0 (fin)
        float progress = gameTimer.GetTimeProgress(); 
        
        // 2. Invertir para obtener la proporción de oscuridad: 0.0 (inicio) a 1.0 (fin)
        float darknessRatio = 1f - progress; 
        
        // 3. Mapear la proporción al máximo de opacidad deseado (ej. 0.0 a 0.8)
        float targetAlpha = darknessRatio * maxDarknessAlpha;

        // 4. Aplicar opacidad: leemos el color actual, cambiamos el alpha y lo asignamos.
        Color currentColor = darknessPanel.color;
        currentColor.a = targetAlpha;
        darknessPanel.color = currentColor;
    }

    /// <summary>
    /// Lógica para iniciar el aumento de oscuridad.
    /// </summary>
    public void StartDarknessIncrease()
    {
        if (!enabled)
        {
             Debug.LogWarning("DARKNESS WARNING: Intentando iniciar la oscuridad pero el script está desactivado.");
             return;
        }

        if (gameTimer == null || darknessPanel == null)
        {
            Debug.LogError("DARKNESS DEBUG: Referencia nula. Imposible iniciar oscuridad.");
            return;
        }
        
        isIncreasing = true;
        
        // ** ¡FIX 2 CRÍTICO! **
        // Asegurar que el GameObject del Panel esté activo JUSTO antes de empezar a cambiar su opacidad.
        if (!darknessPanel.gameObject.activeSelf)
        {
            darknessPanel.gameObject.SetActive(true);
            Debug.Log("DARKNESS FIX: GameObject del panel activado en StartDarknessIncrease.");
        }
        
        // Asegurar que el componente Image esté activo.
        darknessPanel.enabled = true;
    }
    
    /// <summary>
    /// Lógica para detener el aumento.
    /// </summary>
    public void StopDarknessIncrease()
    {
        if (!enabled || darknessPanel == null) return;
        
        isIncreasing = false;
        
        // Si el juego termina antes de que el tiempo se agote, reiniciamos a transparente.
        if (gameTimer != null && gameTimer.GetTimeProgress() > 0.01f)
        {
            Color finalColor = darknessPanel.color;
            finalColor.a = 0f;
            darknessPanel.color = finalColor;
            
            // Opcional: desactivamos el GameObject si no se necesita visible
            // darknessPanel.gameObject.SetActive(false); 
            
            Debug.Log("DARKNESS DEBUG: Deteniendo aumento y reseteando Alpha a 0.0 (Fin por victoria).");
        }
        else
        {
             Debug.Log("DARKNESS DEBUG: Deteniendo aumento. El Alpha se mantendrá en su valor actual (Fin por tiempo agotado).");
        }
    }
}