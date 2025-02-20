using Unity.VisualScripting;
using UnityEngine;

public class HeadBib : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField]//[Range(1f, 30f)]
    float frequency = 18.0f;
    [SerializeField]//[Range(0.01f, 1.3f)]
    float Amplitude = 1f;
    [SerializeField]//[Range(10f, 100f)]
    float INTERspeed = 20.0f;

    [SerializeField]Camera camHold;

    ParkourDecider decider;
    PlayerController controller;

    Vector3 originPos;

    private void Awake()
    {
        rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        originPos = transform.localPosition;
        decider = GameObject.FindGameObjectWithTag("Player").GetComponent<ParkourDecider>();
        camHold = this.GetComponentInChildren<Camera>();

        decider = GetComponentInParent<ParkourDecider>();
        controller = GetComponentInParent<PlayerController>();

        originPos = transform.localPosition;
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

        if (decider.isMoving && controller.isGrounded)
        {
            StartBob();
        }
        else
        {
            transform.localPosition = originPos;
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
