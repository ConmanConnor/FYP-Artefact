using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
   Rigidbody rb;

   [SerializeField]float moveSpeed;
   [SerializeField] float jumpForce;
   [SerializeField] bool isGrounded;
   [SerializeField] LayerMask layerMask;

   [SerializeField]Transform playerFeet;
   [SerializeField]Transform playerHead;

    PlayerInput playerInput;

    Coroutine movePlayer;

    public bool isMoving;

    float Input;
    Vector2 InputMove;

    //------------------Camera--------------------//
    Camera playerCamera;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerCamera = Camera.main;

        
    }
    private void Start()
    {
        playerInput.actions.FindAction("Jump").performed += JumpPerformed;
        

        playerInput.actions.FindAction("Move").performed += MovePerformed;
        playerInput.actions.FindAction("Move").canceled += MoveCancelled;


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
    }

   private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce * 10f, ForceMode.Impulse);
    }


    private void MovePerformed(InputAction.CallbackContext context)
    {
        InputMove = context.ReadValue<Vector2>();
        if(movePlayer == null && !isMoving)
        {
            isMoving = true;
            movePlayer = StartCoroutine(Move());
        }
    }

    private void MoveCancelled(InputAction.CallbackContext context)
    {
        InputMove = context.ReadValue<Vector2>();
        if (movePlayer != null)
        {
            isMoving = false;
            StopCoroutine(Move());
            movePlayer = null;
        }
    }
    IEnumerator  Move()
    {
        Vector3 movedirection = playerHead.forward * InputMove.y + playerHead.right * InputMove.x;
        rb.AddForce(movedirection.normalized * moveSpeed * 10f, ForceMode.Force);
        yield return new WaitForFixedUpdate();
    }

    private void groundCheck()
    {

    }
}
