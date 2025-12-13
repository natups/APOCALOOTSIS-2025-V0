using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Users;
using System.Collections.Generic;

// Obliga a que el GameObject tenga un Rigidbody2D
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    // Referencias al sistema de Input System
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction sprintAction;

    // Componentes del jugador
    private Rigidbody2D rb;
    private Animator playerAnimator;

    // Datos de movimiento
    private Vector2 moveInput;
    [SerializeField] public float moveSpeed = 5f;

    // Lista de objetos interactuables cercanos
    private List<GameObject> interactablesInRange = new List<GameObject>();

    // Variables del sistema de interacción / attach
    public GameObject attachedObject;   // Objeto actualmente interactuado
    private Vector3 attachOffset;        // Distancia relativa entre jugador y objeto
    private bool isInteracting = false;  // Indica si el jugador está interactuando

    private void Awake()
    {
        // Obtiene el Rigidbody2D del jugador
        rb = GetComponent<Rigidbody2D>();

        // Obtiene el componente PlayerInput
        playerInput = GetComponent<PlayerInput>();

        // Verifica que el PlayerInput y sus acciones existan
        if (playerInput != null && playerInput.actions != null)
        {
            // Acción de movimiento
            if (playerInput.actions.FindAction("Move") != null)
            {
                moveAction = playerInput.actions["Move"];
                moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
                moveAction.canceled += _ => moveInput = Vector2.zero;
            }

            // Acción de interacción
            if (playerInput.actions.FindAction("Interact") != null)
            {
                interactAction = playerInput.actions["Interact"];
                interactAction.started += _ => StartInteract();
            }

            // Acción de sprint
            if (playerInput.actions.FindAction("Sprint") != null)
            {
                sprintAction = playerInput.actions["Sprint"];
                sprintAction.started += _ => moveSpeed = 8f;
                sprintAction.canceled += _ => moveSpeed = 5f;
            }
        }
    }

    private void OnEnable()
    {
        // Habilita las acciones de input
        moveAction?.Enable();
        interactAction?.Enable();
        sprintAction?.Enable();
    }

    private void OnDisable()
    {
        // Deshabilita las acciones de input
        moveAction?.Disable();
        interactAction?.Disable();
        sprintAction?.Disable();
    }

    private void FixedUpdate()
    {
        // Aplica el movimiento al Rigidbody en físicas
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void Update()
    {
        // Si está interactuando, mantiene el objeto unido al jugador
        if (isInteracting && attachedObject != null)
        {
            attachedObject.transform.position = transform.position + attachOffset;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detecta objetos interactuables al entrar en su rango
        if (other.CompareTag("Interactable"))
        {
            interactablesInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Elimina objetos interactuables al salir del rango
        if (other.CompareTag("Interactable"))
        {
            interactablesInRange.Remove(other.gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // INTERACCIÓN
    // Maneja el inicio de la interacción con un objeto cercano
    // -------------------------------------------------------------------------
    private void StartInteract()
    {
        // Si hay objetos cerca y no se está interactuando
        if (interactablesInRange.Count > 0 && !isInteracting)
        {
            // Selecciona el primer objeto de la lista
            GameObject targetObject = interactablesInRange[0];

            // Calcula la distancia relativa entre jugador y objeto
            attachOffset = targetObject.transform.position - transform.position;

            // Marca el estado de interacción
            attachedObject = targetObject;
            isInteracting = true;

            // Cambia el Rigidbody del objeto a Kinematic
            // para que no sea afectado por físicas
            Rigidbody2D objectRb = attachedObject.GetComponent<Rigidbody2D>();
            if (objectRb != null)
            {
                objectRb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    // -------------------------------------------------------------------------
    // UTILIDAD
    // Devuelve un texto indicando la dirección relativa hacia un objeto
    // -------------------------------------------------------------------------
    private string GetDirectionText(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x > 0 ? "Derecha" : "Izquierda";
        }
        else
        {
            return direction.y > 0 ? "Arriba" : "Abajo";
        }
    }
}
