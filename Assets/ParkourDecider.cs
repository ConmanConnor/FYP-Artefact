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
    public Coroutine wallRunRoutine;

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
    public bool otherjumpPressed;
    [SerializeField]public bool wallRight;
    [SerializeField] public bool wallLeft;

    //------------Raycast Hits----------------------//
    public RaycastHit Hit;
    public RaycastHit rightWallHit;
    public RaycastHit leftWallHit;
    
    //------------Layers Shrek!----------------------//
    [Header("Layermask")]
    [SerializeField]LayerMask layerMaskWall;

    //------------Floats----------------------//
    [Header("Float Values")]
    public float distanceToWall;
    public float fallingThreshold;
    public float InputJump;
    public float InputWallJump;

    private float stateChangeCoolTime = 0.2f;
    private float lastStateChangeTime = 0f;
    private float movementThreshold = 0.15f;
    private float timeOnGround = 0f;
    private float requiredGroundTime = 0.1f;
    private float wallrunSpeed = 25f;
    private float wallrunTimeLimit = 3f;

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
    
    public Coroutine playerWallJump;

    //-------------Inputs--------------------------//
    [Header("Input Components")]
    public Vector2 InputMove;

    
    public PlayerInput playerInput;

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
        
        playerInput.actions.FindAction("WallJump").performed += WallJumpPerformed;
        playerInput.actions.FindAction("WallJump").canceled += WallJumpCancelled;


        playerInput.actions.FindAction("Move").performed += MovePerformed;
        playerInput.actions.FindAction("Move").canceled += MoveCancelled;
    }
    private void OnDisable()
    {
        playerInput.actions.FindAction("Jump").performed -= JumpPerformed;
        playerInput.actions.FindAction("Jump").canceled -= JumpCancelled;

        playerInput.actions.FindAction("WallJump").performed -= WallJumpPerformed;
        playerInput.actions.FindAction("WallJump").canceled -= WallJumpCancelled;

        playerInput.actions.FindAction("Move").performed -= MovePerformed;
        playerInput.actions.FindAction("Move").canceled -= MoveCancelled;
    }


    private void FixedUpdate()
    {
        NewCheckWall();

        CheckEdge();

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

    private void NewCheckWall()
    {
        wallRight = Physics.Raycast(controller.transform.position, controller.transform.right, out rightWallHit, 10f);
        Debug.DrawRay(controller.transform.position, controller.transform.right * 10f, wallRight ? Color.green : Color.red);
        
        wallLeft = Physics.Raycast(controller.transform.position, -controller.transform.right, out leftWallHit, 10f);
        Debug.DrawRay(controller.transform.position, -controller.transform.right * 10f, wallLeft ? Color.green : Color.red);

        if (wallLeft || wallRight)
        {
            //wall was detected
            wallDetected = true;
        }
        else
        {
            //wall was not detected
            wallDetected = false;
        }
        //store distance to obstacle
        distanceToWall = Mathf.Infinity;

        //Checking each collider around the player
        foreach (Collider col in Physics.OverlapSphere(controller.transform.position, 3f, layerMaskWall))
        {
            //Find the closest possible wall using the distance from the player to the hit collider
            if (Vector3.Distance(controller.transform.position, col.ClosestPoint(controller.transform.position)) < distanceToWall)
            {
                //Figures out distance to wall
                distanceToWall = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
                //Works out distance player is from wall (can be used to determine player intention)
                Vector3 toWall = col.ClosestPoint(transform.position) - transform.position;
                //Dot to calculate if player is going to a wall
                float towardsWall = Vector3.Dot(parkourMover.movedirection, toWall.normalized);
                //Threshold amount before wallrun can happen
                float wallDetectionThreshold = 0.95f;
                
                //if the player is heading into a wall and jump is pressed
                if (towardsWall < wallDetectionThreshold && InputJump > 0)
                {
                    isWallrun = true;
                }
            }
            else
            {
                isWallrun = false;
                playerInput.actions.FindAction("Move").Enable();
            }

        }
        //Debug.Log("hit distance is: " + distanceToObstacke;
    }

    private void CheckEdge()
    {
        //Put edge checker here

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

    private void stateChecker()
    {
        if (CanSwitchToWallRun())
        {
            currentState = PlayerState.WallRun;

            if (climbRunRoutine != null)
            {
                StopCoroutine(climbRunRoutine);
                isClimbing = false;
                climbRunRoutine = null;
                controller.rb.useGravity = true;
            }
        }
        
        else if (CanSwitchToWallJump()) { currentState = PlayerState.WallJump;}
        
        else if (CanSwitchToClimb())
        { 
            currentState = PlayerState.Climb;

            if (wallRunRoutine != null)
            {
                StopCoroutine(wallRunRoutine);
                isWallrun = false;
                wallRunRoutine = null;
                controller.rb.useGravity = true;
            }
        }
        
        else if (CanSwitchToJump()) { currentState = PlayerState.Jumping;}
        
        else if (CanSwitchToFall()) { currentState = PlayerState.Falling; }
        
        //Checks if the player can switch to moving
        else if (CanSwitchToMove()) { currentState = PlayerState.Moving; } //Debug.Log("Player is Moving"); }
        
        //Checks if the player can switch to idle
        else if (CanSwitchToIdle()) { currentState = PlayerState.Idle; } //Debug.Log("Player is Idle"); }
      
    }
    private void CheckPlayerState(PlayerState newState)
    {
        //Checks previous state of player
        if(currentState != newState)
        {
            previousState = currentState;
            currentState = newState;

            //Debug.Log(previousState);
        }
        //Switches between states
        switch (newState)
        {
            case PlayerState.WallRun:
                if (wallRunRoutine == null)
                {
                    lastStateChangeTime = Time.time;
                    parkourMover.moveSpeed = wallrunSpeed;
                    wallRunRoutine = StartCoroutine(parkourMover.WallRun());
                }
                break;
            
            case PlayerState.WallJump:
                if(playerWallJump == null)
                {
                    lastStateChangeTime = Time.time;
                    playerWallJump = StartCoroutine(parkourMover.WallJump());
                }
                break;

            case PlayerState.Climb:
                if(climbRunRoutine == null)
                {
                    lastStateChangeTime = Time.time;
                    isClimbing = true;
                    climbRunRoutine = StartCoroutine(parkourMover.Climb());
                }
                break;

            case PlayerState.Vault:
                break;
            
            case PlayerState.Jumping:
                if(playerJump == null)
                {
                    lastStateChangeTime = Time.time;
                    playerJump = StartCoroutine(parkourMover.Jump());
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
            
            case PlayerState.Moving:
                if(movePlayer == null)
                {
                    lastStateChangeTime = Time.time;
                    movePlayer = StartCoroutine(parkourMover.Move());
                }
                break;
            
            case PlayerState.Idle:
                lastStateChangeTime = Time.time;
                if (movePlayer != null)
                {
                    StopCoroutine(movePlayer);
                    movePlayer = null;
                    isMoving = false;
                }
                break;
            
            default:
                break;
        }
    }

    //------------------Input Handlers----------------------//

    private void JumpPerformed(InputAction.CallbackContext context)
    {

        InputJump = context.ReadValue<float>();

        //Debug.Log("Jump Pressed: " + InputJump);

        currentState = PlayerState.Jumping;
        
        Debug.Log(InputJump);
        //Set state to player moving


    }
    private void JumpCancelled(InputAction.CallbackContext context)
    {

        InputJump = context.ReadValue<float>();

        //Debug.Log("Jump Cancelled: " + InputJump);

        playerJump = null;
        

    }
    
    private void WallJumpPerformed(InputAction.CallbackContext context)
    {
        if (currentState == PlayerState.WallRun)
        {
            InputWallJump = context.ReadValue<float>();

            //Debug.Log("Jump Pressed: " + InputJump);

            otherjumpPressed = true;
            currentState = PlayerState.WallJump;
            
            StopCoroutine(wallRunRoutine);

            Debug.Log(InputWallJump);
            //Set state to player moving
        }

    }
    private void WallJumpCancelled(InputAction.CallbackContext context)
    {

        InputWallJump = context.ReadValue<float>();

        //Debug.Log("Jump Cancelled: " + InputJump);

        if(isFalling)
        {
            
            otherjumpPressed = false;
            
        }

        playerWallJump = null;
    }
    private void MovePerformed(InputAction.CallbackContext context)
    {
        //Reads player input as vector 2
        InputMove = context.ReadValue<Vector2>();
        //if coroutine is null and player is not moving 
        if (movePlayer == null && !isMoving)
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

    //--------------------State switcher booleans-----------------//
    public bool CanSwitchToWallRun()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime /*&& parkourMover.fDot >= 0.5f*/ && currentState != previousState
            && distanceToWall < 1f  && (wallLeft||wallRight) && isWallrun; 
    }
    public bool CanSwitchToClimb()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && parkourMover.fDot <= -0.5f && currentState != previousState
            && distanceToWall < 1f  && wallDetected; 
    }

    public bool CanSwitchToMove()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState
               && InputMove.magnitude >= movementThreshold && controller.isGrounded;
    }
    
    public bool CanSwitchToJump()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState &&
               controller.isGrounded && InputJump == 1;
    }
    
    public bool CanSwitchToWallJump()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState && 
               !controller.isGrounded && otherjumpPressed && currentState == PlayerState.WallRun;
    }

    public bool CanSwitchToFall()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState &&
           controller.rb.linearVelocity.y <= fallingThreshold && !controller.isGrounded;
    }

    public bool CanSwitchToIdle()
    {
        return Time.time > lastStateChangeTime + stateChangeCoolTime && currentState != previousState &&
            controller.rb.linearVelocity.magnitude <= (movementThreshold - 0.05f) && controller.isGrounded;
    }


}
