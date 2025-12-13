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
    private float baseRunSpeed; // Guardar la velocidad base de corrida para restauración
    
    [Header("Lógica de Ralentización")]
    public bool isSlowed = false; 
    private Color originalColor; 
    
    [Tooltip("Duración en segundos del efecto SLOW.")]
    public float slowDuration = 5f; 
    
    [Tooltip("Factor de multiplicación para la velocidad base durante la penalización.")]
    public float defaultSlowFactor = 0.5f; 
    
    [Header("Límite de Sabotaje")]
    public int maxBottles = 3; 
    private int bottlesRemaining; 
    public TextMeshProUGUI bottleCounterText; 
    
    // --- CONTROLES ---
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
        baseRunSpeed = runSpeed; 
        bottlesRemaining = maxBottles; 
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; 
        }
        UpdateBottleUI(); 

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
            // --- DETECCIÓN DE INPUT DE MOVIMIENTO ---
            movement = Vector2.zero;
            if (Input.GetKey(upKey)) movement.y += 1f;
            if (Input.GetKey(downKey)) movement.y -= 1f;
            if (Input.GetKey(leftKey)) movement.x -= 1f;
            if (Input.GetKey(rightKey)) movement.x += 1f;
            
            movement = movement.normalized; 
            
            isSprinting = Input.GetKey(sprintKey) && !isSlowed; 
        }

        // --- INTERACCIÓN (AGARRAR / SOLTAR) ---
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

        // --- ARROJAR BOTELLA ---
        if (canMove && Input.GetKeyDown(throwKey))
        {
            ThrowObject();
        }

        // --- ANIMATOR ---
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
            rb.linearVelocity = Vector2.zero; // CAMBIO: reemplazado velocity obsoleto
            return;
        }
        
        float currentSpeed = (isSprinting && !isHolding) ? runSpeed : walkSpeed;
        Vector2 newVelocity = movement * currentSpeed;

        // CAMBIO: usamos linearVelocity en lugar de velocity
        rb.linearVelocity = newVelocity;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            pickableObject = other.gameObject;
        }
        
        if (other.CompareTag("DeliveryZone") && isHolding)
        {
            if (ZonaDeEntregaManager.Instance != null)
            {
                ZonaDeEntregaManager.Instance.CheckDelivery(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == pickableObject)
        {
            pickableObject = null;
        }
    }

    // --- AGARRAR OBJETO ---
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
    
    public GameObject GetHeldObject()
    {
        return heldObject;
    }

    public void ClearHeldObject()
    {
        heldObject = null;
        heldObjectRB = null;
        isHolding = false;
    }

    // --- SOLTAR OBJETO ---
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

    // --- LANZAR BOTELLA ---
    void ThrowObject()
    {
        if (ZonaDeEntregaManager.Instance == null || !ZonaDeEntregaManager.Instance.IsVersusMode())
        {
            Debug.Log("Lanzamiento de botella deshabilitado.");
            return; 
        }
        
        if (bottlesRemaining <= 0)
        {
            Debug.Log("Límite de botellas alcanzado.");
            return; 
        }
        
        if (botellaPrefab == null)
        {
            Debug.LogWarning("Prefab de Botella no asignado.");
            return;
        }

        // --- CAMBIO CRÍTICO ---
        // NO SOLTAR el objeto que el jugador ya tiene en la mano
        // if (isHolding) { DropObject(); } --> eliminado

        bottlesRemaining--;
        UpdateBottleUI(); 

        GameObject bottleInstance = Instantiate(botellaPrefab, transform.position + (Vector3)lastMoveDirection * 0.5f, Quaternion.identity);

        Rigidbody2D bottleRB = bottleInstance.GetComponent<Rigidbody2D>();
        ThrownObject projectile = bottleInstance.GetComponent<ThrownObject>();
        
        if(projectile == null)
        {
            projectile = bottleInstance.AddComponent<ThrownObject>();
        }
        
        projectile.owner = this.gameObject; 

        if (bottleRB != null)
        {
            bottleRB.bodyType = RigidbodyType2D.Dynamic;
            bottleRB.linearVelocity = Vector2.zero; 
            bottleRB.AddForce(lastMoveDirection.normalized * throwForce, ForceMode2D.Impulse);
        }
    }

    // --- RALENTIZACIÓN ---
    public void ApplySlow(float factor)
    {
        StopCoroutine("SlowRoutine");
        StartCoroutine("SlowRoutine", factor);
    }
    
    public void ApplySlowPenalty()
    {
        ApplySlow(defaultSlowFactor); 
    }

    private IEnumerator SlowRoutine(float factor)
    {
        isSlowed = true; 
        
        walkSpeed = baseWalkSpeed * factor;
        float runWalkRatio = baseRunSpeed / baseWalkSpeed; 
        runSpeed = walkSpeed * runWalkRatio; 
        
        float blinkDuration = 0.15f; 
        float timeElapsed = 0f;

        while (timeElapsed < slowDuration) 
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red; 
                yield return new WaitForSeconds(blinkDuration); 

                spriteRenderer.color = originalColor; 
                yield return new WaitForSeconds(blinkDuration);
            }
            else
            {
                yield return null;
            }

            timeElapsed += (blinkDuration * 2); 
        }

        walkSpeed = baseWalkSpeed; 
        runSpeed = baseRunSpeed; 
        isSlowed = false; 
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor; 
        }

        Debug.Log($"Velocidad de {gameObject.name} restaurada.");
    }

    public void UpdateBottleUI()
    {
        if (bottleCounterText != null)
        {
            bottleCounterText.text = $"x{bottlesRemaining}"; 
        }
    }
}
