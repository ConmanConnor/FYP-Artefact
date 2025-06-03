using Unity.Mathematics;
using UnityEngine;

public class WallrunTilt : MonoBehaviour
{
    private ParkourDecider decider;
    
    private GameObject playerCamera;

    private float camTilt = 18;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCamera = this.gameObject;
        decider = GetComponentInParent<ParkourDecider>();
    }

    // Update is called once per frame
    void Update()
    {
        WallTilt();
        Strafe();
        ResetCam();
    }

    void WallTilt()
    {
        if (decider.isWallrun)
        {
            //Finds the current rotation
            Vector3 currentEuler = playerCamera.transform.localEulerAngles;
            Quaternion TargetRot;
            Quaternion SlerpedRot;
            
            //If wallrun right
            if (decider.wallRight)
            {
                //Set target rotation
                TargetRot = Quaternion.Euler(currentEuler.x,currentEuler.y,camTilt);
                //Lerp through current rotation and target rotation for smooth rotation
                SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
                //Set new rotation
                playerCamera.transform.localRotation = SlerpedRot;
            }
            else if (decider.wallLeft)
            {
                TargetRot = Quaternion.Euler(currentEuler.x,currentEuler.y,-camTilt);
                SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
                playerCamera.transform.localRotation = SlerpedRot;
            }
        }
    }
    
    void ResetCam()
    {
        if (!decider.isWallrun)
        {
            //Reverse of top method
            Vector3 currentEuler = playerCamera.transform.localEulerAngles;
            Quaternion TargetRot;
            Quaternion SlerpedRot;
            TargetRot = Quaternion.Euler(currentEuler.x,currentEuler.y,0);
            SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
            playerCamera.transform.localRotation = SlerpedRot;
        }
        if(!decider.isMoving)
        {
            //Reverse of top method
            Vector3 currentEuler = playerCamera.transform.localEulerAngles;
            Quaternion TargetRot;
            Quaternion SlerpedRot;
            TargetRot = Quaternion.Euler(currentEuler.x, currentEuler.y, 0);
            SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
            playerCamera.transform.localRotation = SlerpedRot;
        }
    }

    void Strafe()
    {
        if (decider.isMoving)
        {
            //Finds the current rotation
            Vector3 currentEuler = playerCamera.transform.localEulerAngles;
            Quaternion TargetRot;
            Quaternion SlerpedRot;

            //If wallrun right
            if (decider.InputMove.x == 1)
            {
                //Set target rotation
                TargetRot = Quaternion.Euler(currentEuler.x, currentEuler.y, -3);
                //Lerp through current rotation and target rotation for smooth rotation
                SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
                //Set new rotation
                playerCamera.transform.localRotation = SlerpedRot;
            }
            else if (decider.InputMove.x == -1)
            {
                TargetRot = Quaternion.Euler(currentEuler.x, currentEuler.y, 3);
                SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
                playerCamera.transform.localRotation = SlerpedRot;
            }
            else if (decider.InputMove.x == 1 && decider.InputMove.y == 1)
            {
                TargetRot = Quaternion.Euler(currentEuler.x, currentEuler.y, -3);
                SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
                playerCamera.transform.localRotation = SlerpedRot;
            }
            else if (decider.InputMove.x == -1 && decider.InputMove.y == -1)
            {
                TargetRot = Quaternion.Euler(currentEuler.x, currentEuler.y, 3);
                SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
                playerCamera.transform.localRotation = SlerpedRot;
            }
        }
    }
}
