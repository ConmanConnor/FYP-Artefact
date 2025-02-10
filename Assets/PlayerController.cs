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
    [SerializeField] public Transform playerFeet;
    [SerializeField] public Transform playerHead;

    //-----------RayCast-------------------->
    RaycastHit hit;

    //-----------------Bools-----------------------//
    [Header("Boolean Checks")]
    [SerializeField] public bool isGrounded;

    //-----------------Aninimation----------------//
    [Header("Animation")]
    Animator Animator;

    //------------------Camera--------------------//
    [Header("Player Camera")]
    public Camera playerCamera;

 

 

void Awake()
    {
        rb = GetComponent<Rigidbody>();
      
        playerCamera = Camera.main;
        Animator = GetComponent<Animator>();
        playerCTR = GetComponent<PlayerController>();
        parkourMover = GetComponent<ParkourMover>();
        decider = GetComponent<ParkourDecider>();

    }

    void Update()
    {
        groundCheck();
    }

    //---------------------------Ground Check------------------//
    private void groundCheck()
    {
        //Draws ray 
        if(Physics.Raycast(playerFeet.transform.position, transform.TransformDirection(Vector3.down),out hit, 0.5f))
        {
            //player is grounded
            isGrounded = true;

            decider.canJump = true;
           // Debug.Log("Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
        }
        else
        {
            //Player is not grounded
            isGrounded = false;

            decider.canJump = false;

            //Debug.Log("No Ground");
            Debug.DrawRay(playerFeet.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.red);
        }
    }
}
   

