using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    //------------------------RigidBody/Player components--------------------//
   Rigidbody rb;
    PlayerController playerCTR;
  
   
    //-----------------------------Ogres have Layers---------//
   [SerializeField] LayerMask layerMaskGround;
   [SerializeField] LayerMask layerMaskWall;

    //-----------------------Transform(noAutobots)---------//
   [SerializeField]Transform playerFeet;
   [SerializeField]Transform playerHead;

    //-----------RayCast-------------------->
    RaycastHit hit;
    RaycastHit sphereHit;

   
    //--------------------Coroutine---------------//
    Coroutine movePlayer;
    Coroutine wallrunActive;

    //-----------------Bools-----------------------//
    public bool isMoving;
    public bool isWallrun;
    public bool canJump;
    [SerializeField] public bool isGrounded;
    bool wallDetected;
   

    //-----------------floats----------------------//
    float maxWallTime = 10f;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    float distanceToObstacke;
    float dot;
    float wallNormal;
    float wallANgle;
    
   

    //-------------Inputs--------------------------//
    Vector2 InputMove;

    PlayerInput playerInput;

    //-----------------Aninimation----------------//
    Animator Animator;

    //------------------Camera--------------------//
    Camera playerCamera;

    //-----------------------Vectors------------------//
    Vector3 playerDire;
    Vector3 hitAngleCross;
    Vector3 directionOfPlayer;
    Vector3 playerForward;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerCamera = Camera.main;
        Animator = GetComponent<Animator>();
        playerCTR = GetComponent<PlayerController>();

    }
    private void OnEnable()
    {
        playerInput.actions.FindAction("Jump").performed += JumpPerformed;
        playerInput.actions.FindAction("Jump").canceled += JumpCancelled;


        playerInput.actions.FindAction("Move").performed += MovePerformed;
        playerInput.actions.FindAction("Move").canceled += MoveCancelled;
    }
    private void OnDisable()
    {
        playerInput.actions.FindAction("Jump").performed -= JumpPerformed;
        playerInput.actions.FindAction("Jump").canceled -= JumpCancelled;


        playerInput.actions.FindAction("Move").performed -= MovePerformed;
        playerInput.actions.FindAction("Move").canceled -= MoveCancelled;
    }

    private void FixedUpdate()
    {
        //rotates the player to match the camera without following the z and x axis 
        var playerRotation = playerCamera.transform.rotation;
        playerRotation.x = 0;
        playerRotation.z = 0;
        
        if(wallDetected && wallNormal == 0)
        {
            wallrunActive = StartCoroutine(WallRun());

        }
        

        transform.rotation = playerRotation;

        directionOfPlayer = rb.linearVelocity.normalized;

        playerForward = transform.forward.normalized;

        if (!isGrounded && !isWallrun)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }
    }

    void Update()
    {
        groundCheck();


        CheckWall(); 

    }

    //----------------------------Movement Mechanics----------------------------//

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
        
    }
    private void JumpCancelled(InputAction.CallbackContext context)
    {
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
        //Plays run animation
        if (!isMoving) 
        {
            Animator.Play("Idle");
        }
        
    }

    IEnumerator WallRun()
    {

        //if the player is close to the wall and there is a detected wall
        if (distanceToObstacke < 1)
        {
            //Updates player state
            isWallrun = true;
            canJump = true;
            //Debug.Log("Wall Detected?: " + objectDetected + " Can Jump?: " + canJump);

            //move direction while running (finds the cross vector of the wall)
            Vector3 wallRunDire = Vector3.Cross(sphereHit.normal, Vector3.up);
            //Debug.Log("Wall Run Direction is: "+wallRunDire);

            //moves player based on direction of approach (left,right,forward)
            if (wallANgle < 45f) // Running along the wall
            {
                //rb.AddForce(wallRunDire * moveSpeed * 10f, ForceMode.Force);
            }
            else if (wallANgle < 135f) // Climbing
            {
                //rb.AddForce(Vector3.up * moveSpeed * 10f, ForceMode.Force);
            }
            else // Opposite wall-run direction
            {
                //rb.AddForce(-wallRunDire * moveSpeed * 10f, ForceMode.Force);
            }
        }
        else
        {
            isWallrun = false;
        }

        yield return new WaitForFixedUpdate();

    }

    private void CheckWall()
    {
        //Detects if a wall was hit
        wallDetected = false;

        //Raycast position = player position
        Vector3 wallRayPos = transform.position;

        Vector3[] playerFeelerDire = { transform.right.normalized, -transform.right.normalized,playerForward.normalized, (playerForward + -transform.right).normalized, (transform.forward + transform.right).normalized};

        
        foreach (Vector3 playerFeeler in  playerFeelerDire)
        {
            
            //Casts ray if hit result is wall layer
            if (Physics.Raycast(wallRayPos, playerFeeler, out sphereHit, 1f, layerMaskWall))
            {

                //wall was detected
                wallDetected = true;
               
                
                //check for the hit object
                //Debug.Log("Hit result is " + sphereHit);

                //Get the entry point and calculate the angle of the player
                wallANgle = Vector3.Angle( sphereHit.normal, playerForward);
                Debug.Log("Angle entry: " + wallANgle);

                //Get the orthaganol axis
                dot = Vector3.Dot(sphereHit.normal, Vector3.up);

                //Turns orthagonal axis into angle
                wallNormal = Mathf.Acos(dot) * Mathf.Rad2Deg;

                //Debug.Log("Wall detected");

                //draw ray for debugging
                Debug.DrawRay(wallRayPos, playerFeeler * 1f, Color.green);

            }
            else
            {
                //draw different color ray
                Debug.DrawRay(wallRayPos, playerFeeler * 1f, Color.red);
            }
        }

        //store distance to obstacle
        distanceToObstacke = sphereHit.distance;
        //Debug.Log("hit distance is: " + distanceToObstacke;
    }


    //---------------------------Ground Check------------------//
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
