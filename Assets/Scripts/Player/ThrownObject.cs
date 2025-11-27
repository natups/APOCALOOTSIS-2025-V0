using UnityEngine;

public class ThrownObject : MonoBehaviour
{
    // Variables públicas añadidas para las referencias en el Inspector
    [Header("Sabotaje")]
    public GameObject charcoPrefab;      
    public GameObject botellaRotaPrefab; 
    public float roturaDuration = 0.5f; // Duración del efecto visual

    public GameObject owner; // El jugador que lanzó el objeto
    
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Evitar golpearse a sí mismo
        if (collision.gameObject == owner)
        {
            return;
        }

        // --- 2. LÓGICA DE IMPACTO ---
        
        PlayerController hitPlayer = collision.gameObject.GetComponent<PlayerController>();
        
        // Si golpeamos a alguien o algo, creamos el Charco y el efecto de Botella Rota
        if (charcoPrefab != null)
        {
            // Instancia el charco en el punto de impacto
            Vector2 impactPoint = collision.GetContact(0).point; 
            Instantiate(charcoPrefab, impactPoint, Quaternion.identity);
            
            // Si golpeamos a un jugador, podemos aturdirlo ligeramente (opcional)
            if (hitPlayer != null)
            {
                 // hitPlayer.GetHit(); 
            }
        }
        
        // 3. EFECTO VISUAL DE ROMPERSE (Autodestrucción integrada)
        if (botellaRotaPrefab != null)
        {
             GameObject brokenBottleFX = Instantiate(botellaRotaPrefab, transform.position, Quaternion.identity);
             // Autodestrucción después de un tiempo corto
             Destroy(brokenBottleFX, roturaDuration); 
        }

        // 4. Destruir la botella que fue lanzada (este objeto)
        Destroy(gameObject);
    }
}