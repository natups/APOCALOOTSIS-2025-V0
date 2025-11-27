using UnityEngine;
using System.Collections; 

public class WaterPuddle : MonoBehaviour
{
    [Header("Efecto de Charco")]
    public float ralentizacionFactor = 0.5f; // Factor que se envía al PlayerController (ej: 0.5 = 50% de velocidad)
    public float duracionCharco = 6f; // Duración del charco visible antes de desaparecer

    private void Start()
    {
        // El charco desaparece del mundo después de 6 segundos
        Destroy(gameObject, duracionCharco);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController hitPlayer = other.GetComponent<PlayerController>();
        
        if (hitPlayer != null)
        {
            // Cuando un jugador entra, llamamos a la función del PlayerController.
            // El PlayerController es el que gestiona los 5 segundos de duración.
            hitPlayer.ApplySlow(ralentizacionFactor);
        }
    }
}