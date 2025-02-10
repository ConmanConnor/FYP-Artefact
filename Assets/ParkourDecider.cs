using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.XR;

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

    //------------Raycast Hits----------------------//
    public RaycastHit Hit;

    //------------Layers Shrek!----------------------//
    [Header("Layermask")]
    [SerializeField] LayerMask layerMaskWall;

    //------------Floats----------------------//
    [Header("Float Values")]
    public float distanceToWall;
    public float fallingThreshold = -5f;

    //-----------Player Controller-----------//
    PlayerController controller;
    private void Start()
    {
        controller = GetComponent<PlayerController>();

        parkourMover = GetComponent<ParkourMover>();
    }


    private void FixedUpdate()
    {
        CheckWall();

        //Checks if the player is not moving
        if (controller.currentState != PlayerState.Jumping && controller.rb.linearVelocity.magnitude <= 0.1f && controller.isGrounded) { controller.currentState = PlayerState.Idle; }
        else if (controller.currentState != PlayerState.Jumping && controller.rb.linearVelocity.magnitude >= 0.1f && controller.isGrounded ) { controller.currentState = PlayerState.Moving; }

        if (distanceToWall < 1f && controller.jumpPressed)
        {
            if (wallRunRoutine == null && WallisBeside())
            {
                controller.currentState = PlayerState.WallRun;
                wallRunRoutine = StartCoroutine(parkourMover.WallRun());
            }
            else if (climbRunRoutine == null && WallisInfront())
            {
                
                controller.currentState = PlayerState.Climb;
                climbRunRoutine = StartCoroutine(parkourMover.Climb());
            }
        }

        if (controller.currentState != PlayerState.WallRun || controller.currentState != PlayerState.Climb)
        {
            //rotates the player to match the camera without following the z and x axis 
            var playerRotation = controller.playerCamera.transform.rotation;
            playerRotation.x = 0;
            playerRotation.z = 0;
            transform.rotation = playerRotation;
        }
    }

    private void Update()
    {
        FallCheck();


        CheckPlayerState();
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
            controller.currentState = PlayerState.Falling;
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

        switch (controller.currentState)
        {
            case PlayerState.Idle:
                StopCoroutine(controller.Move());
                break;

            case PlayerState.Moving:
                if (controller.movePlayer == null)
                {
                    controller.movePlayer = StartCoroutine(controller.Move());
                }
                else
                {
                    StopCoroutine(controller.movePlayer);
                    controller.movePlayer = null;
                    controller.movePlayer = StartCoroutine(controller.Move());
                }
                break;

            case PlayerState.Jumping:
                controller.jumpPressed = true;

                if (controller.playerJump != null)
                {
                    StopCoroutine(controller.Jump());
                    controller.playerJump = null;
                    controller.jumpPressed = false;
                }
                break;

            case PlayerState.Falling:
                isFalling = true;
                break;

            case PlayerState.WallRun:
                isWallrun = true;
                if (wallRunRoutine != null)
                {
                    isWallrun = false;
                    StopCoroutine(parkourMover.WallRun());
                    wallRunRoutine = null;
                }
                break;

            case PlayerState.Climb:
                isClimbing = true;
                if (climbRunRoutine != null)
                {
                    isClimbing = false;
                    StopCoroutine(parkourMover.Climb());
                    climbRunRoutine = null;
                }
                break;

            case PlayerState.Vault:
                break;

            default:
                break;
        }
    }

}
