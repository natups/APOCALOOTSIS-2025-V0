using UnityEngine;

/// <summary>
/// Detecta la colisión de un jugador para iniciar el proceso de entrega de objetos.
/// </summary>
public class CajaDrop : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verificamos que sea un jugador
        PlayerController player = other.GetComponent<PlayerController>();
        
        if (player == null) return;
        
        // 2. Verificamos que el jugador lleve un objeto
        if (player.GetHeldObject() == null) return;

        // 3. CRÍTICO: Llamamos al Manager Singleton para procesar la entrega
        if (ZonaDeEntregaManager.Instance == null)
        {
            Debug.LogError("CajaDrop: La instancia de ZonaDeEntregaManager no está disponible (Singleton).");
            return;
        }

        // 4. Le pasamos el PlayerController del jugador que está entregando
        ZonaDeEntregaManager.Instance.CheckDelivery(player);
    }
}