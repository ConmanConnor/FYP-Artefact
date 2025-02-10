using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class ParkourDecider : MonoBehaviour
{
    //------------Other Scripts-------------------//
    ParkourMover parkourMover;

    //------------Coroutines----------------------//
    Coroutine wallRunRoutine;

    Coroutine climbRunRoutine;


    //------------Booleans----------------------//
    [Header("Boolean Checks")]
    public bool wallDetected;
    public bool isFalling = false;
    public bool isWallrun = false;
    public bool isClimbing = false;
    public bool isMoving;
    public bool canJump;
    public bool jumpPressed;

    //------------Raycast Hits----------------------//
    public RaycastHit Hit;

    //------------Layers Shrek!----------------------//
    [Header("Layermask")]
    [SerializeField] LayerMask layerMaskWall;

    //------------Floats----------------------//
    [Header("Float Values")]
    public float distanceToWall;
    public float fallingThreshold = -5f;
    public float InputJump;

    //-----------------------Vectors------------------//
    [Header("Player Vectors")]
    public Vector3 playerDire;
    Vector3 hitAngleCross;
    Vector3 directionOfPlayer;
    public Vector3 playerForward;

    //------------------Player Stae---------------------//
    [Header("Finite State Machine")]
    [SerializeField] public PlayerState currentState;

    //--------------------Coroutine---------------//
    [Header("Coroutines")]
    public Coroutine movePlayer;

    public Coroutine playerJump;

    //-------------Inputs--------------------------//
    [Header("Input Components")]
    public Vector2 InputMove;

    
    PlayerInput playerInput;

    //-----------Player Controller-----------//
    PlayerController controller;
    private void Awake()
    {
        controller = GetComponent<PlayerController>();

        parkourMover = GetComponent<ParkourMover>();

        playerInput = GetComponent<PlayerInput>();
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
        CheckWall();

        //Checks if the player is not moving
        if (currentState != PlayerState.Jumping && controller.rb.linearVelocity.magnitude <= 0.1f && controller.isGrounded) { currentState = PlayerState.Idle; Debug.Log("Player is Idle"); }
        else if (currentState != PlayerState.Jumping && controller.rb.linearVelocity.magnitude >= 0.1f && controller.isGrounded ) { currentState = PlayerState.Moving; Debug.Log("Player is Moving"); }

        if (distanceToWall < 1f && jumpPressed)
        {
            if (wallRunRoutine == null && WallisBeside())
            {
                currentState = PlayerState.WallRun;
            }
            else if (climbRunRoutine == null && WallisInfront())
            {
                
                currentState = PlayerState.Climb;
            }
        }

        if (currentState != PlayerState.WallRun || currentState != PlayerState.Climb)
        {
            //rotates the player to match the camera without following the z and x axis 
            var playerRotation = controller.playerCamera.transform.rotation;
            playerRotation.x = 0;
            playerRotation.z = 0;
            transform.rotation = playerRotation;
        }
        directionOfPlayer = controller.rb.linearVelocity.normalized;

        playerForward = controller.transform.forward.normalized;

    }

    private void Update()
    {
        FallCheck();

        CheckPlayerState();

        WallisBeside();

        WallisInfront();
    }

    private void CheckWall()
    {
        //Detects if a wall was hit
        wallDetected = false;

        //Raycast position = player position
        Vector3 wallRayPos = controller.transform.position;

        if (Physics.Raycast(wallRayPos, controller.transform.forward, out Hit, 10f, layerMaskWall))
        {

            //wall was detected
            wallDetected = true;

            Debug.DrawRay(wallRayPos, controller.transform.forward * 10f, Color.green);

        }
        else
        {
            //draw different color ray
            Debug.DrawRay(wallRayPos, controller.transform.forward * 10f, Color.red);
        }

        //store distance to obstacle
        distanceToWall = Mathf.Infinity;

        //Checking each collider around the player
        foreach (Collider col in Physics.OverlapSphere(controller.transform.position, 3f, layerMaskWall))
        {
            //Find the closest possible wall using the distance from the player to the hit collider
            if (Vector3.Distance(controller.transform.position, col.ClosestPoint(controller.transform.position)) < distanceToWall)
            {
                distanceToWall = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
            }

        }
        //Debug.Log("hit distance is: " + distanceToObstacke;
    }

    private void FallCheck()
    {
        if (controller.rb.linearVelocity.y < fallingThreshold)
        {
            currentState = PlayerState.Falling;
        }
        else { isFalling = false; }
    }

    public bool WallisBeside()
    {
        return Physics.Raycast(controller.transform.position, transform.right, out _, 1f) || Physics.Raycast(controller.transform.position, -transform.right, out _, 1f);
    }
    public bool WallisInfront()
    {
        return Physics.Raycast(controller.transform.position, transform.forward, out _, 1f) ;
    }
    private void CheckPlayerState()
    {

        switch (currentState)
        {
            case PlayerState.Idle:
                StopCoroutine(parkourMover.Move());
                break;

            case PlayerState.Moving:
                if(movePlayer == null)
                {
                    movePlayer = StartCoroutine(parkourMover.Move());
                }
                break;

            case PlayerState.Jumping:
                if(playerJump == null)
                {
                    playerJump = StartCoroutine(parkourMover.Jump());
                }
                break;

            case PlayerState.Falling:
                isFalling = true;
                break;

            case PlayerState.WallRun:
                 if (wallRunRoutine == null)
                {
                    isWallrun = true;
                    wallRunRoutine = StartCoroutine(parkourMover.WallRun());
                }
                 else
                {
                    StopCoroutine(wallRunRoutine);
                    isWallrun = false;
                    wallRunRoutine = null;
                }
                break;

            case PlayerState.Climb:
                if(climbRunRoutine == null)
                {
                    climbRunRoutine = StartCoroutine(parkourMover.Climb());
                    isClimbing = true;
                }
                else
                {
                    StopCoroutine(climbRunRoutine);
                    isClimbing = false;
                    climbRunRoutine= null;
                }
                break;

            case PlayerState.Vault:
                break;

            default:
                break;
        }
    }

    //------------------Input Handlers----------------------//

    private void JumpPerformed(InputAction.CallbackContext context)
    {

        InputJump = context.ReadValue<float>();

        Debug.Log("Jump Pressed: " + InputJump);

        jumpPressed = true;

        if(canJump && currentState != PlayerState.Jumping)
        {
            currentState = PlayerState.Jumping;
        }


        Debug.Log(InputJump);
        //Set state to player moving


    }
    private void JumpCancelled(InputAction.CallbackContext context)
    {

        InputJump = context.ReadValue<float>();

        Debug.Log("Jump Cancelled: " + InputJump);

        if (isFalling)
        {
           
        }
        playerJump = null;
        jumpPressed = false;


    }
    private void MovePerformed(InputAction.CallbackContext context)
    {
        //Reads player input as vector 2
        InputMove = context.ReadValue<Vector2>();
        //if coroutine is null and player is not moving 
        if (movePlayer == null && !isMoving && currentState != PlayerState.Moving)
        {
            //Player is now moving
            isMoving = true;

            //Set state to player moving
            currentState = PlayerState.Moving;   
            
            while (isMoving)
            {
                //Animator.Play("Running");
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

            //Stop in here aswell incase player is holding button in other states
            StopCoroutine(parkourMover.Move());

            //Couroutine is now null 
            movePlayer = null;

            controller.rb.linearVelocity = Vector3.zero;
        }
    }


}
