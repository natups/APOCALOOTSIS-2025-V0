using UnityEngine;
using UnityEngine.UI; // Necesario para Image

/// Controla el efecto de oscuridad progresiva en pantalla.
/// La opacidad aumenta de forma proporcional a medida que el tiempo del juego se agota.
public class DarknessController : MonoBehaviour
{
    // ---------------------------------------------------------------------
    // REFERENCIAS
    // ---------------------------------------------------------------------
    [Header("Referencias de Oscuridad")]
    [Tooltip("Image del panel de oscuridad que cubre la pantalla.")]
    public Image darknessPanel;

    [Header("Configuración de Oscuridad")]
    [Tooltip("Opacidad máxima del panel (0 = transparente, 1 = negro total).")]
    [Range(0f, 1f)]
    public float maxDarknessAlpha = 0.8f;

    [Tooltip("Referencia al temporizador del juego.")]
    public GameTimer gameTimer;

    // Indica si la oscuridad debe aumentar
    private bool isIncreasing = false;

    // ---------------------------------------------------------------------
    // INICIALIZACIÓN
    // ---------------------------------------------------------------------
    void Awake()
    {
        // Verificación de referencia al panel de oscuridad
        if (darknessPanel == null)
        {
            enabled = false;
            return;
        }

        // Verificación / búsqueda del GameTimer
        if (gameTimer == null)
        {
            gameTimer = FindAnyObjectByType<GameTimer>();
            if (gameTimer == null)
            {
                enabled = false;
                return;
            }
        }

        // Asegurar que el GameObject del panel esté activo
        if (!darknessPanel.gameObject.activeSelf)
        {
            darknessPanel.gameObject.SetActive(true);
        }

        // Inicializar el panel completamente transparente
        Color initialColor = darknessPanel.color;
        initialColor.a = 0f;
        darknessPanel.color = initialColor;

        // Asegurar que el componente Image esté habilitado
        darknessPanel.enabled = true;
    }

    // ---------------------------------------------------------------------
    // ACTUALIZACIÓN DE OSCURIDAD
    // ---------------------------------------------------------------------
    void Update()
    {
        // No ejecutar si la oscuridad no está activa o falta el temporizador
        if (!isIncreasing || gameTimer == null)
            return;

        // Progreso del tiempo (1 = inicio, 0 = fin)
        float progress = gameTimer.GetTimeProgress();

        // Convertir progreso a proporción de oscuridad (0 → 1)
        float darknessRatio = 1f - progress;

        // Calcular alpha objetivo según el máximo configurado
        float targetAlpha = darknessRatio * maxDarknessAlpha;

        // Aplicar el alpha al panel
        Color currentColor = darknessPanel.color;
        currentColor.a = targetAlpha;
        darknessPanel.color = currentColor;
    }

    // ---------------------------------------------------------------------
    // CONTROL DEL EFECTO
    // ---------------------------------------------------------------------

    /// Inicia el aumento progresivo de la oscuridad.
     public void StartDarknessIncrease()
    {
        if (!enabled || gameTimer == null || darknessPanel == null)
            return;

        isIncreasing = true;

        // Asegurar que el panel esté activo antes de modificar su opacidad
        if (!darknessPanel.gameObject.activeSelf)
        {
            darknessPanel.gameObject.SetActive(true);
        }

        darknessPanel.enabled = true;
    }

    /// Detiene el aumento de la oscuridad.
    public void StopDarknessIncrease()
    {
        if (!enabled || darknessPanel == null)
            return;

        isIncreasing = false;

        // Si el juego termina antes de que el tiempo se agote, se resetea la oscuridad
        if (gameTimer != null && gameTimer.GetTimeProgress() > 0.01f)
        {
            Color finalColor = darknessPanel.color;
            finalColor.a = 0f;
            darknessPanel.color = finalColor;
        }
    }
}
