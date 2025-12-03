using UnityEngine;

public class CajaDrop : MonoBehaviour
{
    // CRÍTICO: Debe ser público para poder arrastrar el Manager en el Inspector
    [Header("Referencias")]
    public ZonaDeEntregaManager zonaDeEntregaManager; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verificamos que sea un jugador
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        
        // 2. Verificamos que el jugador lleve un objeto
        if (player.GetHeldObject() == null) return;

        // 3. Verificamos que el Manager esté asignado
        if (zonaDeEntregaManager == null)
        {
            Debug.LogError("CajaDrop: La referencia a ZonaDeEntregaManager está missing. Arrástrala en el Inspector.");
            return;
        }

        // 4. Llamamos a CheckDelivery con el PlayerController (¡Error corregido!)
        // Ahora solo pasamos 1 argumento: el jugador que está entregando.
        zonaDeEntregaManager.CheckDelivery(player);
    }
}