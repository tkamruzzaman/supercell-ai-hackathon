using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputActionAsset _inputActionAsset;
    private InputAction moveAction;
    private InputAction InteractAction;
    
    private Vector2 moveInput;
    
    void Awake()
    {
        //Identify player
        var playerInput = GetComponent<PlayerInput>();
        Debug.Log("Player index: " + playerInput.playerIndex);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (context.performed)
        {
            Debug.Log($"{gameObject.name} MOVED!");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log($"{gameObject.name} INTERACTED!!");
        }
    }

    void Update()
    {
        // Example movement
        transform.Translate(moveInput * (Time.deltaTime * 5f));
    }
}
