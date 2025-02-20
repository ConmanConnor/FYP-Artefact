using UnityEngine;

public class HeadBobNew : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField][Range(1f, 30f)]
    float frequency = 18.0f;
    [SerializeField][Range(0.001f, 1f)]
    float Amount = 0.8f;
    [SerializeField][Range(10f, 100f)]
    float Tspeed = 12.0f;

    ParkourDecider decider;

    private void Awake()
    {
        rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        decider = GetComponentInParent<ParkourDecider>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckMotion();
    }

    private void CheckMotion()
    {
        float movementMagnitude = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y).magnitude;
        Debug.Log("Movement Magnitude is: "+ movementMagnitude);

        if (decider.isMoving) 
        {
            StartBob();
        }
    }

    private Vector3 StartBob()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Lerp(pos.y, Mathf.Sin(Time.time * frequency) * Amount * 1.4f, Tspeed * Time.deltaTime);
        pos.x = Mathf.Lerp(pos.x, Mathf.Cos(Time.time * frequency / 2) * Amount * 1.6f, Tspeed * Time.deltaTime);
        transform.localPosition += pos;

        return pos;
    }
}
