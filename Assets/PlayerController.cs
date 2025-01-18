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
   [SerializeField] LayerMask layerMaskWall;

   [SerializeField]Transform playerFeet;
   [SerializeField]Transform playerHead;

    RaycastHit hit;

    PlayerInput playerInput;

    Coroutine movePlayer;

    public bool isMoving;
    public bool isWallrun;
    public bool canJump;

    float maxWallTime = 500f;

    Vector2 InputMove;

    Animator Animator;


    //------------------Camera--------------------//
    Camera playerCamera;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerCamera = Camera.main;
        Animator = GetComponent<Animator>();
       

        
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

        //rotates the player to match the camera without following the z and x axis 
        var playerRotation = playerCamera.transform.rotation;
        playerRotation.x = 0;
        playerRotation.z = 0;

        transform.rotation = playerRotation;

        if(!isGrounded)
        {
            canJump = false;
        }
        else if(isWallrun)
        {
            canJump = true;
        }
        else
        {
            canJump = true;
        }
            

        WallRun();
    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
    }

   private void Jump()
    {
        //checks if player is grounded
        if(isGrounded && canJump)
        {
            //Adds upward force
            rb.AddForce(Vector3.up * jumpForce * 10f, ForceMode.Impulse);
            //Debug.Log("Jumpy");
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

            while(isMoving)
            {
                Animator.Play("Running");
                return;
            }
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

            rb.linearVelocity = Vector3.zero;
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

        if (!isMoving) 
        {
            Animator.Play("Idle");
        }
    }

    private void WallRun()
    {
        //Holds a list of directions
        Vector3[] directions = { Vector3.right, Vector3.left,Vector3.forward };

        //Detects if a wall was hit
        bool wallDetected = false;

        //for each direction in array
        foreach (Vector3 direction in directions)
        {
            //Raycast position = player position
            Vector3 wallRayPos = transform.position;

            //Casts ray if hit result is wall layer
            if (Physics.Raycast(wallRayPos, transform.TransformDirection(direction), out hit, 0.8f, layerMaskWall))
            {
                //wall was detected
                wallDetected = true;

                //draw ray for debugging
                Debug.DrawRay(wallRayPos, transform.TransformDirection(direction) * 0.8f, Color.green);
                Debug.Log("Wall detected");
                break;
            }
            else
            {
                //draw different color ray
                Debug.DrawRay(wallRayPos, transform.TransformDirection(direction) * 0.8f, Color.red);
            }
        }
        if (!isGrounded)
        {
            //Updates player state
            isWallrun = wallDetected;
            canJump = wallDetected;

            //Add move player here!
            

            //rb.linearVelocity = Vector3.zero;
            //rb.useGravity = !wallDetected;
            
            maxWallTime -= Time.deltaTime;
            if(maxWallTime < 0)
            {
                //rb.useGravity = true;
                isWallrun = false;
               
            }
        }

    }

    private void groundCheck()
    {
        //Draws ray 
        if(Physics.Raycast(playerFeet.transform.position, transform.TransformDirection(Vector3.down),out hit, 0.5f,layerMaskGround))
        {
            //player is grounded
            isGrounded = true;
           // Debug.Log("Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
        }
        else
        {
            //Player is not grounded
            isGrounded = false;
            //Debug.Log("No Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.red);
        }
    }
}
