using UnityEngine;

public class updateCamera : MonoBehaviour
{
   [SerializeField] Transform playerHead;

    Vector3 offset;

    float m_lookSpeed = 200;

    float camrotX;
    float camrotY;

    // Update is called once per frame
    void Update()
    {
        transform.position = playerHead.position;

        Look();
    }

    private void Look()
    {
        float mouseInputX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * m_lookSpeed;
        float mouseInputY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * m_lookSpeed;

        camrotX -= mouseInputY;
        camrotY += mouseInputX;

        camrotX = Mathf.Clamp(camrotX, -90f, 90f);

        transform.rotation = Quaternion.Euler(camrotX, camrotY, 0);
        playerHead.transform.rotation = Quaternion.Euler(0, camrotY, 0);

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
