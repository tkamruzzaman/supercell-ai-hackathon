using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] HeroController heroController;
    private InputActionAsset _inputActionAsset;
    private InputAction moveAction;
    private InputAction InteractAction;
    
    private Vector2 moveInput;
    
    void Awake()
    {
        //Identify player
        var playerInput = GetComponent<PlayerInput>();
        Debug.Log("Player index: " + playerInput.playerIndex);
        rb = GetComponent<Rigidbody2D>();
        heroController = GetComponent<HeroController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (context.performed)
        {
            //Debug.Log($"{gameObject.name} MOVED!");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log($"{gameObject.name} INTERACTED!!");
            heroController.TryDeposit(heroController.GetCurrentZone(),heroController.HeroControllerPlayerId);
            
        }
    }

    void FixedUpdate()
    {
        Vector2 move = moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}
