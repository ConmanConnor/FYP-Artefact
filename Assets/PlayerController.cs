using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

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
    Jumping,
    WallJump,
    Sliding
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

    //-----------------------------Floats---------//
    float lastGrounded = 0f;
    float groundCheckDelay = 0.2f;


    //-----------------------Transform(noAutobots)---------//
    [SerializeField] public Transform playerFeet;
    [SerializeField] public Transform playerHead;

    //-----------RayCast-------------------->
    RaycastHit hit;

    //-----------------Bools-----------------------//
    [Header("Boolean Checks")]
    [SerializeField] public bool isGrounded;

    //------------------Camera--------------------//
    [Header("Player Camera")]
    public Camera playerCamera;

void Awake()
    {
        rb = GetComponent<Rigidbody>();
      
        playerCamera = Camera.main;
      
        playerCTR = GetComponent<PlayerController>();
        parkourMover = GetComponent<ParkourMover>();
        decider = GetComponent<ParkourDecider>();

    }

    void Update()
    {
        groundCheck();
    }

    //---------------------------Ground Check------------------//
    //Check for grounded using a delay
    private void groundCheck()
    {
        bool isGroundedNow = Physics.Raycast(playerFeet.position, Vector3.down, 0.6f);

        if ((isGroundedNow))
        {
            lastGrounded = Time.time;
            isGrounded = true;
            decider.canJump = true;
        }
        else
        {
            if (Time.time - lastGrounded > groundCheckDelay)
            {
                isGrounded = false;
                decider.canJump = false;
            }
        }
    }
}
   

