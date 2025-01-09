using UnityEngine;

public class updateCamera : MonoBehaviour
{
   [SerializeField] Transform playerHead;

    // Update is called once per frame
    void Update()
    {
        transform.position = playerHead.position;
    }
}
