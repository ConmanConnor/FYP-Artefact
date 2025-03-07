using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    float entry;
    
   

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
        wallrunActive = StartCoroutine(WallRun());

        transform.rotation = playerRotation;

        directionOfPlayer = rb.linearVelocity.normalized;

        if (!isGrounded)
        {
            canJump = false;
        }
        else if (wallDetected)
        {
            canJump = true;
        }
        else
        {
            canJump = true;
        }
        /*
        if (isWallrun)
        {
            
            //Tiks down the wall time to make sure you cant infinitely wall run
            
            maxWallTime -= Time.deltaTime;
            Debug.Log(maxWallTime);
            if(maxWallTime <= 0)
            {
                rb.linearVelocity = Vector3.zero;
                rb.useGravity = true;
                isWallrun = false;
                return;
            }
            else
            {
                maxWallTime = 0.1f;
            }
        }*/


        //WallRun();
    }
    void Update()
    {
        groundCheck();

        
    }

    //----------------------------Movement Mechanics----------------------------//

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
        wallrunActive = StartCoroutine(WallRun());
        
    }
    private void JumpCancelled(InputAction.CallbackContext context)
    {
       StopCoroutine(WallRun());
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
        //Holds a list of directions
        Vector3[] directions = { Vector3.right, Vector3.left,Vector3.forward, Vector3.forward+Vector3.left,Vector3.forward+Vector3.right };

        //Detects if a wall was hit
        wallDetected = false;

        //for each direction in array
        foreach (Vector3 direction in directions)
        {
            //Raycast position = player position
            Vector3 wallRayPos = transform.position;

            //Casts ray if hit result is wall layer
            if (Physics.SphereCast(wallRayPos, 0.5f, direction, out sphereHit, 1f, layerMaskWall,QueryTriggerInteraction.Collide))
            {
                //wall was detected
                wallDetected = true;

                //check for the hit object
                //Debug.Log("Hit result is " + sphereHit);

                //store distance to obstacle
                distanceToObstacke = sphereHit.distance;
                //Debug.Log("hit distance is: " + distanceToObstacke;

                //Get the entry point and calculate the angle of the player
                dot = Vector3.Dot(directionOfPlayer,sphereHit.normal);
                //Turns dot into degrees
                entry = Mathf.Acos(dot) * Mathf.Rad2Deg;
                Debug.Log("Angle entry: " + dot);
                //draw ray for debugging
                Debug.DrawRay(wallRayPos, transform.TransformDirection(direction) * 1f, Color.green);
                //Debug.Log("Wall detected");
            }
            else
            {
                //draw different color ray
                Debug.DrawRay(wallRayPos, transform.TransformDirection(direction) * 1f, Color.red);
            }
        }
        //if the player is close to the wall and there is a detected wall
        if (distanceToObstacke > 0 && wallDetected)
        {
            //Updates player state
            //isWallrun = true;
            canJump = true;
            Debug.Log("Wall Detected?: " + wallDetected + " Can Jump?: " + canJump);

            //move direction while running 
            Vector3 wallRunDire = Vector3.Cross(sphereHit.normal, Vector3.up);
            

            //moves player based on direction of approach (left,right,forward)
            switch (entry)
            {
                case 180:
                    rb.AddForce(wallRunDire * moveSpeed * 10f, ForceMode.Force);
                    //rb.useGravity = !wallDetected;
                    break;
                case 0:
                    rb.AddForce(-1 * wallRunDire * moveSpeed * 10f, ForceMode.Force);
                    //rb.useGravity = !wallDetected;
                    break;
                case 90:
                    rb.AddForce(Vector3.up * moveSpeed * 10f, ForceMode.Force);
                    //rb.useGravity = !wallDetected;
                    break;
            }
        }

        yield return new WaitForFixedUpdate();

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
