using UnityEngine;

/// Detecta cuando un jugador entra en la zona de entrega
/// y notifica al gestor central para procesar el objeto entregado.
public class CajaDrop : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ==============================
        // 1. VERIFICAR QUE SEA UN JUGADOR
        // ==============================

        // Intenta obtener el PlayerController del objeto que colisiona
        PlayerController player = other.GetComponent<PlayerController>();

        // Si no es un jugador, no hace nada
        if (player == null) return;

        // ==================================
        // 2. VERIFICAR QUE LLEVE UN OBJETO
        // ==================================

        // Si el jugador no está sosteniendo ningún objeto, no se procesa la entrega
        if (player.GetHeldObject() == null) return;

        // ==================================
        // 3. PROCESAR ENTREGA CON EL MANAGER
        // ==================================

        // Llama al ZonaDeEntregaManager para validar y procesar la entrega
        if (ZonaDeEntregaManager.Instance == null) return;

        ZonaDeEntregaManager.Instance.CheckDelivery(player);
    }
}
