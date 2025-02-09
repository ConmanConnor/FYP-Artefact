using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public enum PlayerState
{
    Idle,
    Moving,
    WallRun,
    Climb,
    Vault,
    Falling,
    Jumping
}

public class PlayerController : MonoBehaviour
{
    //-----------------------Scipt References-----------------------------//
    ParkourDecider decider;

    ParkourMover parkourMover;

    //------------------------RigidBody/Player components--------------------//
    [Header("Rigidbody Components")]
    public Rigidbody rb;
    PlayerController playerCTR;


    //-----------------------------Ogres have Layers---------//
    [SerializeField] LayerMask layerMaskGround;
 

    //-----------------------Transform(noAutobots)---------//
    [SerializeField] Transform playerFeet;
    [SerializeField] Transform playerHead;

    //-----------RayCast-------------------->
    RaycastHit hit;



    //--------------------Coroutine---------------//
    Coroutine movePlayer;

    Coroutine playerJump;

    Coroutine wallRunRoutine;

    Coroutine climbRunRoutine;



    //-----------------Bools-----------------------//
    [Header("Boolean Checks")]
    public bool isMoving;
    public bool canJump;
    public bool jumpPressed;
    [SerializeField] public bool isGrounded;
    public bool isFalling = false;



    //-----------------floats----------------------//
    [Header("Float Values")]
    [SerializeField] public float moveSpeed;
    [SerializeField] float jumpForce;
    public float fallingThreshold = -5f;

    //-------------Inputs--------------------------//
    [Header("Input Components")]
    Vector2 InputMove;

    float InputJump;

    PlayerInput playerInput;

    //-----------------Aninimation----------------//
    [Header("Animation")]
    Animator Animator;

    //------------------Camera--------------------//
    [Header("Player Camera")]
    Camera playerCamera;

    //------------------Player Stae---------------------//
    [Header("Finite State Machine")]
    [SerializeField] public PlayerState currentState;

    //-----------------------Vectors------------------//
    [Header("Player Vectors")]
    public Vector3 playerDire;
    Vector3 hitAngleCross;
    Vector3 directionOfPlayer;
    public Vector3 playerForward;

void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerCamera = Camera.main;
        Animator = GetComponent<Animator>();
        playerCTR = GetComponent<PlayerController>();

        decider = GetComponent<ParkourDecider>();
        parkourMover = GetComponent<ParkourMover>();

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
        if (!decider.isWallrun || !decider.isClimbing)
        {
            //rotates the player to match the camera without following the z and x axis 
            var playerRotation = playerCamera.transform.rotation;
            playerRotation.x = 0;
            playerRotation.z = 0;
            transform.rotation = playerRotation;
        }

        rb.useGravity = true;
        
        //Checks if the player is not moving
        if(currentState != PlayerState.Jumping && rb.linearVelocity.magnitude <= 0.1f  && isGrounded) { currentState = PlayerState.Idle; }
        else if(currentState != PlayerState.Jumping && rb.linearVelocity.magnitude >= 0.1f && isGrounded) { currentState = PlayerState.Moving; }

        directionOfPlayer = rb.linearVelocity.normalized;

        playerForward = transform.forward.normalized;
    }

    void Update()
    {
        groundCheck();

        FallCheck();

        CheckPlayerState();

    }

    private void CheckPlayerState()
    {

        switch(currentState)
        {
            case PlayerState.Idle:
                StopCoroutine(Move());
                break;

            case PlayerState.Moving:
                if (movePlayer == null)
                {
                    movePlayer = StartCoroutine(Move());
                }
                else
                {
                    StopCoroutine(movePlayer);
                    movePlayer = null;
                    movePlayer = StartCoroutine(Move());
                }
                break;

            case PlayerState.Jumping:
                if (playerJump != null)
                {
                    StopCoroutine(Jump());
                    playerJump = null;
                }
                break;

            case PlayerState.Falling:
                isFalling = true;
                break;

            case PlayerState.WallRun:
                if (wallRunRoutine == null)
                {
                   wallRunRoutine = StartCoroutine(parkourMover.WallRun());
                }
                break;

            case PlayerState.Climb:
                if (climbRunRoutine == null)
                {
                    climbRunRoutine = StartCoroutine(parkourMover.Climb());
                }
                break;

            case PlayerState.Vault: 
                break;

            default: 
                break;
        }
    }

    //----------------------------Movement Mechanics----------------------------//

    private void JumpPerformed(InputAction.CallbackContext context)
    {
      
        InputJump = context.ReadValue<float>();
        //Set state to player moving
        jumpPressed = true;

        if (playerJump == null)
        {
            playerJump = StartCoroutine(Jump());
        }


    }
    private void JumpCancelled(InputAction.CallbackContext context)
    {
        
        InputJump = context.ReadValue<float>();
        jumpPressed = false;
    }

    private IEnumerator Jump()
    {
        //checks if player is grounded
        if(canJump )
        {
             currentState = PlayerState.Jumping;
            //Adds upward force
            rb.AddForce(Vector3.up * jumpForce * 10f, ForceMode.Impulse);
            Debug.Log("Jumpy");
            yield return new WaitForFixedUpdate();
        }
        
    }

    private void FallCheck()
    {
        if(rb.linearVelocity.y < fallingThreshold)
        {
            currentState = PlayerState.Falling;
        }
        else { isFalling = false;}
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

            //Set state to player moving
            currentState = PlayerState.Moving;

            while (isMoving)
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

            //Stop in here aswell incase player is holding button in other states
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

    //---------------------------Ground Check------------------//
    private void groundCheck()
    {
        //Draws ray 
        if(Physics.Raycast(playerFeet.transform.position, transform.TransformDirection(Vector3.down),out hit, 0.5f))
        {
            //player is grounded
            isGrounded = true;

            canJump = true;
           // Debug.Log("Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
        }
        else
        {
            //Player is not grounded
            isGrounded = false;

            canJump = false;

            //Debug.Log("No Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.red);
        }
    }
}
   

