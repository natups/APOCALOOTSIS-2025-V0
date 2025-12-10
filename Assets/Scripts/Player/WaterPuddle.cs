using UnityEngine;
using System.Collections; 

public class WaterPuddle : MonoBehaviour
{
    [Header("Efecto de Charco")]
    [Tooltip("Factor de multiplicación de la velocidad (0.5 = 50% de velocidad).")]
    public float ralentizacionFactor = 0.5f; 
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
            // Cuando un jugador entra, llamamos a ApplySlow. 
            // La duración del efecto (5 segundos) es gestionada internamente por PlayerController.
            hitPlayer.ApplySlow(ralentizacionFactor);
        }
    }
}