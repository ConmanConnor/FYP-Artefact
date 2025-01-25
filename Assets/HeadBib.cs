using UnityEngine;

public class HeadBib : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField][Range(1f, 30f)]
    float frequency = 10.0f;
    [SerializeField][Range(0.001f, 0.01f)]
    float Amount = 0.005f;
    [SerializeField][Range(10f, 100f)]
    float Tspeed = 10.0f;

    private void Awake()
    {
        rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckMotion();
    }

    private void CheckMotion()
    {
        float movementMagnitude = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y).magnitude;
        Debug.Log(movementMagnitude);

        if (movementMagnitude > 0) 
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
