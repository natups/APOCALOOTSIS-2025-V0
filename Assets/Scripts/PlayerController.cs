using UnityEngine;
using System.Collections; 
using TMPro; 

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    // --- VARIABLES DE MOVIMIENTO EXISTENTES ---
    [Header("Ajustes de Movimiento")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    
    // --- VARIABLES DE RALENTIZACIÓN/SABOTAJE ---
    private float baseWalkSpeed; 
    
    [Header("Lógica de Ralentización")]
    public bool isSlowed = false; 
    private Color originalColor; 
    
    [Header("Límite de Sabotaje")]
    public int maxBottles = 3; 
    private int bottlesRemaining; 
    // CRÍTICO: Esta es la referencia del texto individual para P1 o P2
    public TextMeshProUGUI bottleCounterText; 
    
    // --- RESTO DE VARIABLES EXISTENTES ---
    [Header("Controles (Inspector)")]
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode sprintKey;
    public KeyCode interactKey; 
    public KeyCode throwKey; 
    
    [Header("Lógica de Agarre")]
    public Transform holdParent; 
    public float throwForce = 10f; 
    
    [Header("Lógica de Sabotaje (Versus)")]
    public GameObject botellaPrefab; 
    public float stunDuration = 0.5f; 
    
    [HideInInspector] public GameObject heldObject;
    private Rigidbody2D heldObjectRB;
    private GameObject pickableObject; 

    // Componentes
    private Rigidbody2D rb;
    private Animator playerAnimator;
    private SpriteRenderer spriteRenderer; 
    private Vector2 movement;
    
    // Estados
    public bool isHolding = false;
    private bool isSprinting = false;
    private bool canMove = true; 
    private Vector2 lastMoveDirection = new Vector2(0, -1); 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        
        // --- INICIALIZACIÓN ---
        baseWalkSpeed = walkSpeed; 
        bottlesRemaining = maxBottles; 
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; 
        }
        UpdateBottleUI(); // Muestra "x3" al inicio
        // --- FIN INICIALIZACIÓN ---

        if (holdParent == null)
        {
            Debug.LogError("Asigna el 'Hold Parent' (holdPoint) en el Inspector del jugador.");
        }
    }

    void Update()
    {
        if (!canMove)
        {
            movement = Vector2.zero;
        }
        else
        {
            // --- 1. DETECCIÓN DE INPUT DE MOVIMIENTO ---
            movement = Vector2.zero;
            if (Input.GetKey(upKey)) movement.y += 1f;
            if (Input.GetKey(downKey)) movement.y -= 1f;
            if (Input.GetKey(leftKey)) movement.x -= 1f;
            if (Input.GetKey(rightKey)) movement.x += 1f;
            
            movement = movement.normalized; 
            
            // Bloquea sprint si está ralentizado
            isSprinting = Input.GetKey(sprintKey) && !isSlowed; 
        }

        // --- 2. Input de Interacción (Agarrar/Soltar) ---
        if (canMove && Input.GetKeyDown(interactKey))
        {
            if (isHolding)
            {
                DropObject();
            }
            else 
            {
                playerAnimator.SetTrigger("Grab");
                if (pickableObject != null)
                {
                    PickUpObject(pickableObject);
                }
            }
        }

        // --- 3. Input de Arrojar (Sabotaje) ---
        if (canMove && Input.GetKeyDown(throwKey))
        {
            ThrowObject();
        }

        // --- 4. ACTUALIZAR ANIMATOR ---
        bool isMoving = movement.magnitude > 0.01f;
        
        playerAnimator.SetBool("IsMoving", isMoving);
        playerAnimator.SetBool("IsHolding", isHolding);
        playerAnimator.SetFloat("Speed", isSprinting ? 1f : 0f); 
        
        if (isMoving)
        {
            playerAnimator.SetFloat("MoveX", movement.x);
            playerAnimator.SetFloat("MoveY", movement.y);
            lastMoveDirection = movement; 
        }
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero; 
            return;
        }
        
        float currentSpeed = (isSprinting && !isHolding) ? runSpeed : walkSpeed;
        Vector2 newVelocity = movement * currentSpeed;
        rb.linearVelocity = newVelocity;
    }
    
    // --- LÓGICA DE DETECCIÓN (Triggers) ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            pickableObject = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == pickableObject)
        {
            pickableObject = null;
        }
    }

    // --- LÓGICA DE AGARRAR / SOLTAR / ARROJAR ---
    public void PickUpObject(GameObject obj)
    {
        heldObject = obj;
        heldObjectRB = heldObject.GetComponent<Rigidbody2D>();

        if (heldObjectRB != null)
        {
            heldObjectRB.bodyType = RigidbodyType2D.Kinematic;
            heldObjectRB.linearVelocity = Vector2.zero; 
        }

        Collider2D heldCollider = heldObject.GetComponent<Collider2D>();
        if (heldCollider != null)
        {
            heldCollider.enabled = false;
        }
        
        heldObject.transform.SetParent(holdParent);
        heldObject.transform.localPosition = Vector3.zero; 
        heldObject.transform.localRotation = Quaternion.identity;
        
        isHolding = true;
    }

    public void DropObject()
    {
        if (heldObject == null) return;

        Collider2D heldCollider = heldObject.GetComponent<Collider2D>();
        if (heldCollider != null)
        {
            heldCollider.enabled = true;
        }

        ThrownObject projectile = heldObject.GetComponent<ThrownObject>();
        if (projectile != null)
        {
            Destroy(projectile);
        }

        heldObject.transform.SetParent(null);
        heldObject.transform.position = transform.position + (Vector3)lastMoveDirection * 0.5f; 
        
        if (heldObjectRB != null)
        {
            heldObjectRB.bodyType = RigidbodyType2D.Kinematic; 
            heldObjectRB.linearVelocity = Vector2.zero;
        }

        heldObject = null;
        heldObjectRB = null;
        isHolding = false;
    }

    // --- FUNCIÓN DE ARROJAR BOTELLA (Sabotaje - Versus) ---
    void ThrowObject()
    {
        // 1. Verificar el límite
        if (bottlesRemaining <= 0)
        {
            Debug.Log("Límite de botellas alcanzado.");
            return; 
        }
        
        if (botellaPrefab == null)
        {
            Debug.LogWarning("Prefab de Botella no asignado. No se puede lanzar.");
            return;
        }

        if (isHolding)
        {
             DropObject(); 
        }
        
        // 2. Disminuir el contador y actualizar la UI
        bottlesRemaining--;
        UpdateBottleUI(); // Llama a la función que actualiza el texto de P1 o P2

        // 3. Instanciar y lanzar la botella
        GameObject bottleInstance = Instantiate(botellaPrefab, transform.position + (Vector3)lastMoveDirection * 0.5f, Quaternion.identity);

        // 4. Añadir/Obtener los componentes necesarios
        Rigidbody2D bottleRB = bottleInstance.GetComponent<Rigidbody2D>();
        ThrownObject projectile = bottleInstance.GetComponent<ThrownObject>();
        
        if(projectile == null)
        {
            projectile = bottleInstance.AddComponent<ThrownObject>();
        }
        
        // 5. Configurar el proyectil
        projectile.owner = this.gameObject; 

        // 6. Aplicar fuerza de lanzamiento
        if (bottleRB != null)
        {
            bottleRB.bodyType = RigidbodyType2D.Dynamic;
            bottleRB.linearVelocity = Vector2.zero; 
            bottleRB.AddForce(lastMoveDirection.normalized * throwForce, ForceMode2D.Impulse);
        }
    }

    // --- FUNCIÓN PÚBLICA PARA SER GOLPEADO (Ralentización por impacto) ---
    public void GetHit()
    {
        if (canMove)
        {
            StartCoroutine(StunRoutine());
        }
    }

    // --- CORUTINA DE ATURDIMIENTO ---
    private IEnumerator StunRoutine()
    {
        canMove = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        
        if (isHolding)
        {
             DropObject();
        }

        yield return new WaitForSeconds(stunDuration); 

        canMove = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    // --- LÓGICA DE RALENTIZACIÓN DEL CHARCO ---
    public void ApplySlow(float factor)
    {
        StopCoroutine("SlowRoutine");
        StartCoroutine("SlowRoutine", factor);
    }

    private IEnumerator SlowRoutine(float factor)
    {
        float duration = 5f; 
        isSlowed = true; 
        
        float originalWalkSpeed = baseWalkSpeed; 
        walkSpeed = originalWalkSpeed * factor;
        
        float blinkDuration = 0.15f; 
        float timeElapsed = 0f;

        while (timeElapsed < duration) 
        {
            spriteRenderer.color = Color.red; 
            yield return new WaitForSeconds(blinkDuration); 

            spriteRenderer.color = originalColor; 
            yield return new WaitForSeconds(blinkDuration);

            timeElapsed += (blinkDuration * 2); 
        }

        walkSpeed = originalWalkSpeed; 
        isSlowed = false; 
        spriteRenderer.color = originalColor; 

        Debug.Log($"Velocidad de {gameObject.name} restaurada.");
    }

    // --- FUNCIÓN PARA ACTUALIZAR LA UI DEL CONTADOR ---
    public void UpdateBottleUI()
    {
        if (bottleCounterText != null)
        {
            // Actualiza el texto con el formato "x3", "x2", etc.
            bottleCounterText.text = $"x{bottlesRemaining}"; 
        }
    }
}