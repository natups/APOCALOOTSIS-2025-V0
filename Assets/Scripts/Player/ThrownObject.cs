using UnityEngine;

// -----------------------------------------------------------------------------
// THROWN OBJECT
// Controla el comportamiento de un objeto que es lanzado y colisiona con algo
// -----------------------------------------------------------------------------

public class ThrownObject : MonoBehaviour
{
    [Header("Sabotaje")]
    // Prefab que genera un charco al impactar
    public GameObject charcoPrefab;

    // Prefab del efecto visual de la botella rota
    public GameObject botellaRotaPrefab;

    // Duración del efecto visual antes de destruirse
    public float roturaDuration = 0.5f;

    // Referencia al jugador que lanzó el objeto
    public GameObject owner;

    // Rigidbody del objeto lanzado
    private Rigidbody2D rb;

    void Awake()
    {
        // Obtiene el Rigidbody2D del objeto
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ---------------------------------------------------------------------
        // 1. EVITAR COLISIÓN CON EL PROPIO JUGADOR
        // ---------------------------------------------------------------------
        if (collision.gameObject == owner)
        {
            return;
        }

        // ---------------------------------------------------------------------
        // 2. LÓGICA DE IMPACTO
        // Al colisionar con cualquier objeto válido, se genera el sabotaje
        // ---------------------------------------------------------------------

        // Intenta obtener un PlayerController (si el objeto golpeado es un jugador)
        PlayerController hitPlayer = collision.gameObject.GetComponent<PlayerController>();

        // Genera el charco en el punto exacto de impacto
        if (charcoPrefab != null)
        {
            // Obtiene el punto de contacto de la colisión
            Vector2 impactPoint = collision.GetContact(0).point;

            // Instancia el charco en la escena
            Instantiate(charcoPrefab, impactPoint, Quaternion.identity);

            // La penalización de movimiento se aplica únicamente por el charco
        }

        // ---------------------------------------------------------------------
        // 3. EFECTO VISUAL DE BOTELLA ROTA
        // ---------------------------------------------------------------------
        if (botellaRotaPrefab != null)
        {
            // Instancia el efecto visual en la posición actual del objeto
            GameObject brokenBottleFX = Instantiate(
                botellaRotaPrefab,
                transform.position,
                Quaternion.identity
            );

            // Destruye el efecto visual luego de un tiempo
            Destroy(brokenBottleFX, roturaDuration);
        }

        // ---------------------------------------------------------------------
        // 4. DESTRUCCIÓN DEL OBJETO LANZADO
        // ---------------------------------------------------------------------
        Destroy(gameObject);
    }
}
