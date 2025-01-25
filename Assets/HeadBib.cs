using Unity.VisualScripting;
using UnityEngine;

public class HeadBib : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField][Range(1f, 30f)]
    float frequency = 10.0f;
    [SerializeField][Range(0.01f, 1.3f)]
    float Amplitude = 0.08f;
    [SerializeField][Range(10f, 100f)]
    float INTERspeed = 10.0f;

    [SerializeField]Camera cam;

    PlayerController playerController;

    Vector3 originPos;

    private void Awake()
    {
        rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        originPos = transform.localPosition;
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckMotion();
        StopBob();
    }

    private void CheckMotion()
    {
        float movementMagnitude = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y).magnitude;
        //Debug.Log(movementMagnitude);

        if (playerController.isMoving && playerController.isGrounded)
        {
            StartBob();
        }
    }

    private Vector3 StartBob()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Lerp(pos.y, Mathf.Sin(Time.time * frequency) * Amplitude, INTERspeed * Time.deltaTime);
        pos.x = Mathf.Lerp(pos.x, Mathf.Cos(Time.time * frequency / 2) * Amplitude, INTERspeed * Time.deltaTime);
        transform.localPosition += originPos + pos;

        return pos;
    }

    private void StopBob()
    {
        if (transform.localPosition == originPos) return;
        transform.localPosition = Vector3.Lerp(transform.localPosition, originPos, 1 * Time.deltaTime);
    }
}
