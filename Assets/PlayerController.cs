using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
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

    ParkourMover parkourMover;

    ParkourDecider decider;

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
    [Header("Coroutines")]
    public Coroutine movePlayer;

    public Coroutine playerJump;



    //-----------------Bools-----------------------//
    [Header("Boolean Checks")]
    public bool isMoving;
    public bool canJump;
    public bool jumpPressed;
    [SerializeField] public bool isGrounded;
  



    //-----------------floats----------------------//
    [Header("Float Values")]
    [SerializeField] public float moveSpeed;
    [SerializeField] float jumpForce;

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
    public Camera playerCamera;

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
        parkourMover = GetComponent<ParkourMover>();
        decider = GetComponent<ParkourDecider>();

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
        rb.useGravity = true;

        directionOfPlayer = rb.linearVelocity.normalized;

        playerForward = transform.forward.normalized;
    }

    void Update()
    {
        groundCheck();
    }

   

    //----------------------------Movement Mechanics----------------------------//

    private void JumpPerformed(InputAction.CallbackContext context)
    {
      
        InputJump = context.ReadValue<float>();
        //Set state to player moving
       

        if (playerJump == null)
        {
            playerJump = StartCoroutine(Jump());
        }


    }
    private void JumpCancelled(InputAction.CallbackContext context)
    {
        
        InputJump = context.ReadValue<float>();
    }

    public IEnumerator Jump()
    {
        //checks if player is grounded
        if(canJump )
        {
             currentState = PlayerState.Jumping;
            //Adds upward force
            rb.AddForce(Vector3.up * jumpForce * 10f, ForceMode.Impulse);
            //Debug.Log("Jumpy");
            yield return new WaitForFixedUpdate();
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
    public IEnumerator  Move()
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
   

