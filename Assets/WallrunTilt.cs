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
            if (decider.wallRight)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(currentEuler.x,currentEuler.y,camTilt);
            }
            else if (decider.wallLeft)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(currentEuler.x,currentEuler.y,-camTilt);
            }
        }
    }
    
    void ResetCam()
    {
        if (!decider.isWallrun)
        {
            Vector3 currentEuler = playerCamera.transform.localEulerAngles;
            playerCamera.transform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, 0);
        }
    }
}
