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
        ResetCam();
    }

    void WallTilt()
    {
        if (decider.isWallrun)
        {
            Vector3 currentEuler = playerCamera.transform.localEulerAngles;
            Quaternion TargetRot;
            Quaternion SlerpedRot;
            if (decider.wallRight)
            {
                TargetRot = Quaternion.Euler(currentEuler.x,currentEuler.y,camTilt);
                SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
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
            Vector3 currentEuler = playerCamera.transform.localEulerAngles;
            Quaternion TargetRot;
            Quaternion SlerpedRot;
            TargetRot = Quaternion.Euler(currentEuler.x,currentEuler.y,0);
            SlerpedRot = Quaternion.Slerp(playerCamera.transform.localRotation, TargetRot, 5f * Time.deltaTime);
            playerCamera.transform.localRotation = SlerpedRot;
        }
    }
}
