using System.Text;
using UnityEngine;

public class ParkourDecider : MonoBehaviour
{
    //------------Booleans----------------------//
    [Header("Boolean Checks")]
    public bool isWallrun;
    public bool wallDetected;
    public bool isClimbing;

    //------------Raycast Hits----------------------//
    public RaycastHit Hit;

    //------------Layers Shrek!----------------------//
    [Header("Layermask")]
    [SerializeField] LayerMask layerMaskWall;

    //------------Floats----------------------//
    [Header("Float Values")]
    public float distanceToWall;

    //-----------Player Controller-----------//
    PlayerController controller;
    private void Start()
    {
        controller = GetComponent<PlayerController>();
    }


    private void FixedUpdate()
    {
        CheckWall();
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
}
