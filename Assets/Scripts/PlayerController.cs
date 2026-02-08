using System;
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
    bool canMove = false;
    
    private Vector2 moveInput;

    private void OnEnable()
    {
        GameManager.OnMatchStart+= EnableMovement;
    }
    private void OnDisable()
    {
        GameManager.OnMatchStart-= EnableMovement;
    }

    private void EnableMovement()
    {
        canMove = true;
    }

    void Awake()
    {
        canMove = false;
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
            
            // FIXED: Check if zone exists before trying to deposit
            CaptureZone currentZone = heroController.GetCurrentZone();
            
            if (currentZone == null)
            {
                Debug.Log("Not in a capture zone! Cannot deposit.");
                return;
            }
            
            // Check if hero has followers
            if (!heroController.HasFollowers())
            {
                Debug.Log("No followers to deposit!");
                return;
            }
            
            // Try to deposit
            bool success = heroController.TryDeposit(currentZone, heroController.HeroControllerPlayerId);
            
            if (!success)
            {
                Debug.Log("Deposit failed! (Zone might be locked)");
            }
        }
    }
    
    void FixedUpdate()
    {
        if (!canMove) return;
        
        Vector2 move = moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}