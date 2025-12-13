using UnityEngine;
using System.Collections;

// -----------------------------------------------------------------------------
// WATER PUDDLE
// Representa un charco que ralentiza a los jugadores al entrar en contacto
// -----------------------------------------------------------------------------

public class WaterPuddle : MonoBehaviour
{
    [Header("Efecto de Charco")]
    [Tooltip("Factor de multiplicación de la velocidad (0.5 = 50% de velocidad).")]
    public float ralentizacionFactor = 0.5f;

    // Tiempo que el charco permanece visible en la escena
    public float duracionCharco = 6f;

    private void Start()
    {
        // Destruye el charco luego de un tiempo determinado
        // para evitar que permanezca indefinidamente en la escena
        Destroy(gameObject, duracionCharco);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica si el objeto que entra en el charco es un jugador
        PlayerController hitPlayer = other.GetComponent<PlayerController>();

        if (hitPlayer != null)
        {
            // Aplica el efecto de ralentización al jugador
            // La duración del efecto se gestiona internamente en PlayerController
            hitPlayer.ApplySlow(ralentizacionFactor);
        }
    }
}
