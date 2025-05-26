using UnityEngine;
using System.Collections;
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

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        decider = GetComponent<ParkourDecider>();
    }

    //----------------------------Movement Mechanics----------------------------//
    public IEnumerator WallRun()
    {
        Debug.Log("Wallrunning");

            //Debug.Log("Wall Detected?: " + objectDetected + " Can Jump?: " + canJump);

            //move direction while running (finds the cross vector of the wall)
            Vector3 wallRunDire = Vector3.Cross(decider.Hit.normal, Vector3.up);
            //Debug.Log("Wall Run Direction is: "+wallRunDire);

            float wallRelevantDot = Vector3.Dot(wallRunDire, decider.playerForward);

            Debug.Log(wallRelevantDot);
        //moves player based on direction of approach (left,right,forward)
        while (decider.isWallrun)
        {
            if (wallRelevantDot < 0f)
            {
                //Debug.Log("Banana");
                controller.rb.AddForce(-wallRunDire * moveSpeed * 10f, ForceMode.Force);
                controller.rb.useGravity = false;
            }

            else if (wallRelevantDot > 0f)
            {
                //Debug.Log("Apple");
                controller.rb.AddForce(wallRunDire * moveSpeed * 10f, ForceMode.Force);
                controller.rb.useGravity = false;
            }
            yield return new WaitForFixedUpdate();
        }
        controller.rb.useGravity = true;




        Debug.Log("Wallrun Routine Cancelled");

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
