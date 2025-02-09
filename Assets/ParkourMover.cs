using UnityEngine;
using System.Collections;

public class ParkourMover : MonoBehaviour
{
    //-----------Player Controller & Parkour Decider-----------//
    PlayerController controller;

    ParkourDecider decider;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        decider = GetComponent<ParkourDecider>();
    }
    public IEnumerator WallRun()
    {

        //if the player is close to the wall and there is a detected wall
        if (decider.distanceToWall < 1f && controller.jumpPressed)
        {
            //Updates player state
            decider.isWallrun = true;
            controller.canJump = true;

            controller.currentState = PlayerState.WallRun;
            //Debug.Log("Wall Detected?: " + objectDetected + " Can Jump?: " + canJump);

            //move direction while running (finds the cross vector of the wall)
            Vector3 wallRunDire = Vector3.Cross(decider.Hit.normal, Vector3.up);
            //Debug.Log("Wall Run Direction is: "+wallRunDire);

            //check if player is approaching the wall
            float forwardDot = Vector3.Dot(controller.playerForward, decider.Hit.normal);

            float wallRelevantDot = Vector3.Dot(wallRunDire, controller.playerForward);

            Debug.Log(wallRelevantDot);
                //moves player based on direction of approach (left,right,forward)
                if (wallRelevantDot < 0f)
                {
                    //Debug.Log("Banana");
                    controller.rb.AddForce(-wallRunDire * controller.moveSpeed * 5f, ForceMode.Force);
                    controller.rb.useGravity = false;
                }
                else if (wallRelevantDot > 0f)
                {
                    //Debug.Log("Apple");
                    controller.rb.AddForce(wallRunDire * controller.moveSpeed * 5f, ForceMode.Force);
                    controller.rb.useGravity = false;
            }
         }

        yield return new WaitForFixedUpdate();

    }

    public IEnumerator Climb()
    {
        //if the player is close to the wall and there is a detected wall
        if (decider.distanceToWall < 1f && controller.jumpPressed)
        {
            //Updates player state
            decider.isClimbing = true;
            controller.canJump = false;
            //Debug.Log("Wall Detected?: " + objectDetected + " Can Jump?: " + canJump);

            //move direction while running (finds the cross vector of the wall)
            Vector3 wallRunDire = Vector3.Cross(decider.Hit.normal, Vector3.up);
            //Debug.Log("Wall Run Direction is: "+wallRunDire);

            //check if player is approaching the wall
            float forwardDot = Vector3.Dot(controller.playerForward, decider.Hit.normal);

            float wallRelevantDot = Vector3.Dot(wallRunDire, controller.playerForward);

            Debug.Log(wallRelevantDot);
            if (forwardDot < -0.5f)
            {
                controller.rb.AddForce(Vector3.up * controller.moveSpeed * 5f, ForceMode.Force);
                controller.rb.useGravity = false;
                controller.currentState = PlayerState.Climb;
            }
        }

        yield return new WaitForFixedUpdate();
    }
}
