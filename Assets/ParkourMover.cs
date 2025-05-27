using System;
using UnityEngine;
using System.Collections;
using UnityEngine.PlayerLoop;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class ParkourMover : MonoBehaviour
{
    //-----------Player Controller & Parkour Decider-----------//
    PlayerController controller;

    ParkourDecider decider;

    //-----------------floats----------------------//
    [Header("Float Values")]
    [SerializeField] public float moveSpeed;
    [SerializeField] public float jumpForce;
    public float fDot;

    public float wallRelDot;

    [SerializeField] private float wallrunTimeLimit = 3f;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        decider = GetComponent<ParkourDecider>();
    }

    private void Update()
    {
        if (decider.currentState == PlayerState.WallRun)
        {
            wallrunTimeLimit -= Time.deltaTime;
            if (wallrunTimeLimit <= 0f)
            {
                StopWallrun();
            }
            else if (!decider.wallDetected)
            {
                wallrunTimeLimit = 0f;
            }
        }
    }

    //----------------------------Movement Mechanics----------------------------//
    public IEnumerator WallRun()
    {
        Debug.Log("Wallrunning");

            //Debug.Log("Wall Detected?: " + objectDetected + " Can Jump?: " + canJump);

            controller.rb.useGravity = false;
            decider.playerInput.actions.FindAction("Move").Disable();

            //Sets rigidbody y velocity to 0
            controller.rb.linearVelocity =
                new Vector3(controller.rb.linearVelocity.x, 0f, controller.rb.linearVelocity.z);
            
            Vector3 wallNormal = decider.wallRight ? decider.rightWallHit.normal : decider.leftWallHit.normal;
            //move direction while running (finds the cross vector of the wall)
            Vector3 wallRunDire = Vector3.Cross(wallNormal, Vector3.up);
            //Debug.Log("Wall Run Direction is: "+wallRunDire);

            float wallRelevantDot = Vector3.Dot(wallRunDire, decider.playerForward);

            Debug.Log(wallRelevantDot);
        //moves player based on direction of approach (left,right,forward)
        while (decider.isWallrun)
        {
            if (wallRelevantDot < 0f)
            {
                //Debug.Log("Banana");
                controller.rb.AddForce(-wallRunDire * moveSpeed, ForceMode.Force);
            }

            else if (wallRelevantDot > 0f)
            {
                //Debug.Log("Apple");
                controller.rb.AddForce(wallRunDire * moveSpeed, ForceMode.Force);
            }
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("Wallrun Routine Cancelled");
    }

    public void StopWallrun()
    {
        if (decider.wallRunRoutine != null || (!decider.wallRight||!decider.wallLeft))
        {
            StopCoroutine(decider.wallRunRoutine);
            decider.wallRunRoutine = null;
        }

        decider.isWallrun = false;
        controller.rb.useGravity = true;
        moveSpeed = 30f;
        decider.playerInput.actions.FindAction("Move").Enable();

        decider.currentState = PlayerState.Falling;
        wallrunTimeLimit = 3f;
    }

    public IEnumerator Climb()
    {

        Debug.Log("Climb Routine Started");
        //Debug.Log("Wall Detected?: " + objectDetected + " Can Jump?: " + canJump);

        //move direction while running (finds the cross vector of the wall)
         Vector3 wallRunDire = Vector3.Cross(decider.Hit.normal, Vector3.up);
        //Debug.Log("Wall Run Direction is: "+wallRunDire);

        //check if player is approaching the wall
         fDot = Vector3.Dot(decider.playerForward, decider.Hit.normal);

         wallRelDot = Vector3.Dot(wallRunDire, decider.playerForward);

         Debug.Log(wallRelDot);

        while (decider.isClimbing)
        {
            
            controller.rb.AddForce(Vector3.up * moveSpeed * 10f, ForceMode.Force);
            controller.rb.useGravity = false;
            //Debug.Log("Climbing");

            
            yield return new WaitForFixedUpdate();
        }
        controller.rb.useGravity = true;
        Debug.Log("Climb Routine ended");



    }

    public IEnumerator Jump()
    {
        //checks if player is grounded
        if (decider.canJump)
        {
            //Jump buffer fix here creates a true jump buffer
            
            //Creates a variable taking in the change in position of the player
            Vector3 vel = controller.rb.linearVelocity;
            //Sets vel to 0 on y axis
            vel.y = 0;
            //Sets the player velocity to vel
            controller.rb.linearVelocity = vel;
            
            //Adds upward force
            controller.rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //Debug.Log("Jumpy");
            yield return null;
        }
        //Debug.Log("Jump Routine Ended");

    }
    
    public IEnumerator WallJump()
    {
        //checks if player is grounded
      
        if (decider.wallLeft)
        {
            //Adds upward force
            controller.rb.AddForce(Vector3.left + Vector3.up * jumpForce, ForceMode.Impulse);
        }
        else if (decider.wallRight)
        {
            //Adds upward force
            controller.rb.AddForce(Vector3.right + Vector3.up * jumpForce, ForceMode.Impulse);
        }

        //Debug.Log("Jumpy");
        yield return null;
        
        //Debug.Log("Jump Routine Ended");

    }

    public IEnumerator Move()
    {
        while (decider.isMoving)
        {
            //Debug.Log("Move Routine Started");
            //Calculates movement directions using vector 3 and input values
            Vector3 movedirection = controller.playerHead.forward * decider.InputMove.y + controller.playerHead.right * decider.InputMove.x;
            //Adds force to player
            controller.rb.AddForce(movedirection.normalized * moveSpeed, ForceMode.Force);

            yield return new WaitForFixedUpdate();
        }
        //Debug.Log("Move Routine Ended");

    }
}
