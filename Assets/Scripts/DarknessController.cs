using UnityEngine;

/// <summary>
/// Script auxiliar para manejar los efectos de oscuridad y penalización visual.
/// </summary>
public class DarknessController : MonoBehaviour
{
    // Lógica para iniciar el aumento de oscuridad.
    public void StartDarknessIncrease()
    {
        Debug.Log("DarknessController: Empezando aumento gradual de oscuridad.");
    }
    
    // Lógica para detener el aumento.
    public void StopDarknessIncrease()
    {
        Debug.Log("DarknessController: Deteniendo aumento.");
    }
}