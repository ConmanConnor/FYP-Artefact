using System.Collections;
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
    [SerializeField]private bool bufferedJump;
    private bool jumpPressed;

    //------------Raycast Hits----------------------//
    public RaycastHit Hit;

    //------------Layers Shrek!----------------------//
    [Header("Layermask")]
    [SerializeField]LayerMask layerMaskWall;

    //------------Floats----------------------//
    [Header("Float Values")]
    public float distanceToWall;
    public float fallingThreshold = -5f;
    public float InputJump;

    private float stateChangeCoolTime = 0.2f;
    private float lastStateChangeTime = 0f;
    private float movementThreshold = 0.15f;
    private float timeOnGround = 0f;
    private float requiredGroundTime = 0.2f;
    private float jumpBufferTime = 0.2f;
    private float lastJumpPressedTime = -1f;
    private float jumpPressedFloat;

    //-----------------------Vectors------------------//
    [Header("Player Vectors")]
    public Vector3 playerDire;
    Vector3 hitAngleCross;
    Vector3 directionOfPlayer;
    public Vector3 playerForward;
    private PlayerState previousState;

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

    InputCustom customInput;

    InputAction jumpAction;




    //-----------Player Controller-----------//
    PlayerController controller;
    private void Awake()
    {
        controller = GetComponent<PlayerController>();

        parkourMover = GetComponent<ParkourMover>();

        playerInput = GetComponent<PlayerInput>();

        customInput = new InputCustom();

        jumpAction = customInput.Player.Jump;

    }

    private void OnEnable()
    {
        playerInput.actions.FindAction("Move").performed += MovePerformed;
        playerInput.actions.FindAction("Move").canceled += MoveCancelled;

        customInput.Player.Enable();
    }
    private void OnDisable()
    {
        playerInput.actions.FindAction("Move").performed -= MovePerformed;
        playerInput.actions.FindAction("Move").canceled -= MoveCancelled;

        customInput.Player.Disable();
    }


    private void FixedUpdate()
    {
        CheckWall();

        stateChecker();

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
        CheckPlayerState(currentState);

        //Check for state if true set can jump to true
        if (currentState == PlayerState.WallRun) { canJump = true; }

    }

    private void stateChecker()
    {
        //Checks if the player can switch to idle
        if (CanSwitchToIdle()) { currentState = PlayerState.Idle; } //Debug.Log("Player is Idle"); }
        //Checks if the player can switch to moving
        else if (CanSwitchToMove()) { currentState = PlayerState.Moving; } //Debug.Log("Player is Moving"); }

        else if (CanSwitchToWallRun()){ currentState = PlayerState.WallRun; }

        else if (CanSwitchToClimb()){ currentState = PlayerState.Climb; }

        else if (CanSwitchToFall()) { currentState = PlayerState.Falling; }

        else if (CanSwitchToJump()) {currentState = PlayerState.Jumping ; }

        jumpPressedFloat = jumpAction.ReadValue<float>();

        bufferedJump = (Time.time < lastJumpPressedTime + jumpBufferTime);
    }
    private void CheckPlayerState(PlayerState newState)
    {
        //Checks previous state of player
        if(currentState != newState)
        {
            previousState = currentState;
            currentState = newState;

            //Debug.Log($"The state has switched to: {currentState}");
        }
        //Switches between states
        switch (newState)
        {
            case PlayerState.Idle:
                lastStateChangeTime = Time.time;
                if (movePlayer != null)
                {
                    StopCoroutine(movePlayer);
                    movePlayer = null;
                    isMoving = false;
                }
                break;

            case PlayerState.Moving:
                if(movePlayer == null)
                {
                    lastStateChangeTime = Time.time;
                    movePlayer = StartCoroutine(parkourMover.Move());
                }
                break;

            case PlayerState.Jumping:
                if(playerJump == null)
                {
                    lastJumpPressedTime = Time.time;
                    lastStateChangeTime = Time.time;
                    lastJumpPressedTime = -1f;
                    jumpPressed = true;
                    playerJump = StartCoroutine(parkourMover.Jump());
                }
                else if (Time.time > lastJumpPressedTime + jumpBufferTime)
                {
                    jumpPressed = false;
                }
                break;

            case PlayerState.Falling:
                if (controller.rb.linearVelocity.y <= fallingThreshold && !controller.isGrounded)
                {
                    lastStateChangeTime = Time.time;
                    isFalling = true;
                }
                else if(controller.isGrounded)
                {
                    timeOnGround += Time.deltaTime;

                    if(timeOnGround >= requiredGroundTime)
                    {
                        isFalling = false;
                        timeOnGround = 0f;
                    }
                }
                else
                {
                    timeOnGround = 0f;
                }
               
                break;

            case PlayerState.WallRun:
                 if (wallRunRoutine == null)
                {
                    lastStateChangeTime = Time.time;
                    isWallrun = true;
                    controller.rb.useGravity = false;
                    wallRunRoutine = StartCoroutine(parkourMover.WallRun());
                }
                else if (wallRunRoutine != null && !wallDetected)
                {
                    Debug.Log("Wall run is now disabled");
                    StopCoroutine(wallRunRoutine);
                    isWallrun = false;
                    wallRunRoutine = null;
                    controller.rb.useGravity = true;
                }
                break;

            case PlayerState.Climb:
                if(climbRunRoutine == null)
                {
                    lastStateChangeTime = Time.time;
                    isClimbing = true;
                    controller.rb.useGravity = false;
                    climbRunRoutine = StartCoroutine(parkourMover.Climb());
                }
                else if (climbRunRoutine != null && !wallDetected)
                {
                    Debug.Log("Climb is now disabled");
                    StopCoroutine(climbRunRoutine);
                    isClimbing = false;
                    climbRunRoutine = null;
                    controller.rb.useGravity = true;
                }
                break;

            case PlayerState.Vault:
                break;

            default:
                break;

        }
        //Debug.Log($"After switching - Previous State: {previousState}, Current State: {currentState}");
    }

    //--------------------State switcher booleans-----------------//
    public bool CanSwitchToWallRun()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState
            && distanceToWall < 1f && (bufferedJump) && wallDetected;
    }
    public bool CanSwitchToClimb()
    {
        //Debug.Log($"Player Forward Dot: {parkourMover.fDot}");
        return Time.time > lastStateChangeTime + stateChangeCoolTime && parkourMover.fDot <= -0.5f && currentState != previousState
            && distanceToWall < 1f && (bufferedJump) && wallDetected;
    }

    public bool CanSwitchToMove()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState && (currentState == PlayerState.Idle)
            && controller.rb.linearVelocity.magnitude >= movementThreshold && controller.isGrounded;
    }

    public bool CanSwitchToFall()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState &&
           controller.rb.linearVelocity.y <= fallingThreshold && !controller.isGrounded;
    }

    public bool CanSwitchToJump()
    {
        Debug.Log(jumpPressedFloat);
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState &&
            bufferedJump && canJump && jumpPressedFloat > 0;
    }

    public bool CanSwitchToIdle()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState &&
            controller.rb.linearVelocity.magnitude <= (movementThreshold - 0.05f) && controller.isGrounded;
    }

    //------------------Wall Checker------------------------//
    private void CheckWall()
    {
        //Detects if a wall was hit
        wallDetected = false;

        //check if player is approaching the wall
        parkourMover.fDot = Vector3.Dot(playerForward, Hit.normal);

        //move direction while running (finds the cross vector of the wall)
        parkourMover. wallRunDire = Vector3.Cross(Hit.normal, Vector3.up);

        //Checks wall relevant dot
        parkourMover.wallRelevantDot = Vector3.Dot(parkourMover.wallRunDire, playerForward);

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
        foreach (Collider col in Physics.OverlapSphere(controller.transform.position, 10f, layerMaskWall))
        {
            //Find the closest possible wall using the distance from the player to the hit collider
            if (Vector3.Distance(controller.transform.position, col.ClosestPoint(controller.transform.position)) < distanceToWall)
            {
                //Checks distance and closest contact point
                distanceToWall = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
            }
        }
    }

    //------------------Input Handlers----------------------//
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

            controller.rb.linearVelocity = Vector3.zero;
        }
    }
}
