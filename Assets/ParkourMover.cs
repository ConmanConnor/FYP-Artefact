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

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        decider = GetComponent<ParkourDecider>();
    }

    //----------------------------Movement Mechanics----------------------------//
    public IEnumerator WallRun()
    {
        Debug.Log("Wallrunning");
     
        
            decider.canJump = true;
            //Debug.Log("Wall Detected?: " + objectDetected + " Can Jump?: " + canJump);

            //move direction while running (finds the cross vector of the wall)
            Vector3 wallRunDire = Vector3.Cross(decider.Hit.normal, Vector3.up);
            //Debug.Log("Wall Run Direction is: "+wallRunDire);

            float wallRelevantDot = Vector3.Dot(wallRunDire, decider.playerForward);

            Debug.Log(wallRelevantDot);
                //moves player based on direction of approach (left,right,forward)
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

    public IEnumerator Climb()
    {
       
       
            decider.canJump = false;
            //Debug.Log("Wall Detected?: " + objectDetected + " Can Jump?: " + canJump);

            //move direction while running (finds the cross vector of the wall)
            Vector3 wallRunDire = Vector3.Cross(decider.Hit.normal, Vector3.up);
            //Debug.Log("Wall Run Direction is: "+wallRunDire);

            //check if player is approaching the wall
            float fDot = Vector3.Dot(decider.playerForward, decider.Hit.normal);

            float wallRelDot = Vector3.Dot(wallRunDire, decider.playerForward);

            Debug.Log(wallRelDot);
            if (fDot < -0.5f)
            {
                controller.rb.AddForce(Vector3.up * moveSpeed * 10f, ForceMode.Force);
                controller.rb.useGravity = false;
                Debug.Log("Climbing");
            }
        

        yield return new WaitForFixedUpdate();
    }

    public IEnumerator Jump()
    {
        //checks if player is grounded
        if (decider.canJump)
        {
            Debug.Log("Jump Routine Started");
            decider.jumpPressed = true;
            //Adds upward force
            controller.rb.AddForce(Vector3.up * jumpForce * 10f, ForceMode.Impulse);
            //Debug.Log("Jumpy");
            yield return null;
        }
        decider.jumpPressed = false;
        Debug.Log("Jump Routine Ended");

    }

    public IEnumerator Move()
    {
        while (decider.isMoving)
        {
            Debug.Log("Move Routine Started");
            //Calculates movement directions using vector 3 and input values
            Vector3 movedirection = controller.playerHead.forward * decider.InputMove.y + controller.playerHead.right * decider.InputMove.x;
            //Adds force to player
            controller.rb.AddForce(movedirection.normalized * moveSpeed * 10f, ForceMode.Force);

            yield return new WaitForFixedUpdate();
        }
        Debug.Log("Move Routine Ended");

    }
}
