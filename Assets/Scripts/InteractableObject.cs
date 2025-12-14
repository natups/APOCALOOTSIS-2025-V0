using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private bool isHeld = false; // Indica si el objeto está siendo sostenido
    private Transform holder; // Referencia al jugador que sostiene el objeto
    private Rigidbody2D rb; // Rigidbody2D del objeto para controlar la física
    private Vector3 holdLocalPos; // Posición local del objeto respecto al jugador
    private Vector3 lastHolderPosition; // Última posición conocida del jugador
    private Quaternion lastHolderRotation; // Última rotación conocida del jugador
    private Collider2D col; // Collider2D del objeto para controlar las colisiones
    private bool prevIsTrigger = false; // Guardamos el estado previo del trigger del collider

    // Estado público de solo lectura: indica si el objeto está siendo sostenido
    public bool IsHeld { get { return isHeld; } }

    // Inicialización de componentes
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Obtener Rigidbody2D
        col = GetComponent<Collider2D>(); // Obtener Collider2D

        // Si el objeto tiene un Rigidbody2D, lo configuramos como kinematic
        // Esto evita que la física lo mueva hasta que el jugador lo agarre
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // Desactivamos la física para que no se mueva
            rb.linearVelocity = Vector2.zero; // Aseguramos que no haya velocidad inicial
        }
    }

    // Actualización de la posición del objeto en cada frame
    void Update()
    {
        if (isHeld && holder != null)
        {
            // Solo actualizamos la posición si el jugador se movió
            // Evita que el objeto se mueva por redondeos de física
            if (holder.position != lastHolderPosition || holder.rotation != lastHolderRotation)
            {
                // Calculamos la nueva posición del objeto en función del jugador
                transform.position = holder.TransformPoint(holdLocalPos);
                lastHolderPosition = holder.position; // Actualizamos la posición del jugador
                lastHolderRotation = holder.rotation; // Actualizamos la rotación del jugador
            }
        }
    }

    // Método para agarrar o soltar el objeto
    public void Interact(Transform player)
    {
        if (!isHeld)
        {
            // Agarrar el objeto
            isHeld = true;
            holder = player;

            // Si tiene Rigidbody2D, lo mantenemos kinematic para evitar la física
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Detenemos cualquier movimiento previo
                rb.bodyType = RigidbodyType2D.Kinematic; // Desactivamos la física mientras se sostiene
            }

            // Desactivamos las colisiones físicas mientras se sostiene el objeto
            if (col != null)
            {
                prevIsTrigger = col.isTrigger; // Guardamos el estado de trigger anterior
                col.isTrigger = true; // Activamos el modo trigger para que no colisione con el jugador
            }

            // Posición local del objeto con respecto al jugador
            holdLocalPos = new Vector3(0, 1f, 0); // Ajuste para que el objeto se sostenga arriba del jugador
            transform.position = holder.TransformPoint(holdLocalPos); // Aplicamos la posición global

            // Inicializamos la última posición y rotación del jugador
            lastHolderPosition = holder.position;
            lastHolderRotation = holder.rotation;
        }
        else
        {
            // Soltar el objeto
            isHeld = false;
            holder = null;

            // Desparentamos el objeto y dejamos que la física lo controle
            transform.SetParent(null);

            // Si tiene Rigidbody2D, lo dejamos kinematic y lo detenemos
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic; // Aseguramos que siga siendo kinematic
                rb.linearVelocity = Vector2.zero; // Detenemos cualquier movimiento previo
            }

            // Restauramos el estado de trigger original del collider
            if (col != null)
            {
                col.isTrigger = prevIsTrigger;
            }
        }
    }

    // Sobrecarga: Permite especificar una posición en el mundo donde se debe colocar el objeto al agarrarlo
    // Esto es útil para situar el objeto en la dirección hacia donde mira el jugador
    public void Interact(Transform player, Vector3 holdWorldPosition)
    {
        if (!isHeld)
        {
            isHeld = true;
            holder = player;

            // Si tiene Rigidbody2D, lo mantenemos kinematic
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Detenemos cualquier movimiento previo
                rb.bodyType = RigidbodyType2D.Kinematic; // Desactivamos la física mientras se sostiene
            }

            // Desactivamos las colisiones físicas mientras se sostiene el objeto
            if (col != null)
            {
                prevIsTrigger = col.isTrigger;
                col.isTrigger = true; // Activamos el modo trigger
            }

            // Convertimos la posición mundial deseada a la local respecto al jugador y la aplicamos
            transform.position = holdWorldPosition;
            holdLocalPos = holder.InverseTransformPoint(holdWorldPosition); // Guardamos la posición local del objeto

            // Inicializamos la última posición y rotación del jugador
            lastHolderPosition = holder.position;
            lastHolderRotation = holder.rotation;
        }
        else
        {
            // Soltar: Dejamos el objeto en la posición actual y seguimos siendo kinematic
            isHeld = false;
            holder = null;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic; // Aseguramos que siga siendo kinematic
                rb.linearVelocity = Vector2.zero; // Detenemos cualquier movimiento previo
            }

            // Restauramos el estado de trigger original del collider
            if (col != null)
            {
                col.isTrigger = prevIsTrigger;
            }
        }
    }
}
