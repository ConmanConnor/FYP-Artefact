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
   [SerializeField] LayerMask layerMaskGround;

   [SerializeField]Transform playerFeet;
   [SerializeField]Transform playerHead;

    RaycastHit hit;

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
        groundCheck();
    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
    }

   private void Jump()
    {
        if(isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce * 10f, ForceMode.Impulse);
        }
        
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
        if(Physics.Raycast(playerFeet.transform.position, transform.TransformDirection(Vector3.down),out hit, 0.5f,layerMaskGround))
        {
            isGrounded = true;
            Debug.Log("Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
        }
        else
        {
            isGrounded = false;
            Debug.Log("No Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.red);
        }
    }
}
