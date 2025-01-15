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

        rb.rotation = playerCamera.transform.rotation;
    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
    }

   private void Jump()
    {
        //checks if player is grounded
        if(isGrounded)
        {
            //Adds upward force
            rb.AddForce(Vector3.up * jumpForce * 10f, ForceMode.Impulse);
        }
        
    }


    private void MovePerformed(InputAction.CallbackContext context)
    {
        //Reads player input as vector 2
        InputMove = context.ReadValue<Vector2>();
        //if coroutine is null and player is not moving 
        if(movePlayer == null && !isMoving)
        {
            //Player is now moving
            isMoving = true;
            //Start the coroutine to move player
            movePlayer = StartCoroutine(Move());
        }
    }

    private void MoveCancelled(InputAction.CallbackContext context)
    {
        //Reads player input as vector 2
        InputMove = context.ReadValue<Vector2>();
        //if coroutine does not = null or if player stops input
        if (movePlayer != null)
        {
            //player not moving
            isMoving = false;
            //stop coroutine
            StopCoroutine(Move());
            //Couroutine is now null 
            movePlayer = null;
        }
    }
    IEnumerator  Move()
    {
        while (isMoving)
        {
            //Calculates movement directions using vector 3 and input values
            Vector3 movedirection = playerHead.forward * InputMove.y + playerHead.right * InputMove.x;
            //Adds force to player
            rb.AddForce(movedirection.normalized * moveSpeed * 10f, ForceMode.Force);
            yield return new WaitForFixedUpdate();
        }
    }

    private void groundCheck()
    {
        //Draws ray 
        if(Physics.Raycast(playerFeet.transform.position, transform.TransformDirection(Vector3.down),out hit, 0.5f,layerMaskGround))
        {
            //player is grounded
            isGrounded = true;
            Debug.Log("Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
        }
        else
        {
            //Player is not grounded
            isGrounded = false;
            Debug.Log("No Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.red);
        }
    }
}
