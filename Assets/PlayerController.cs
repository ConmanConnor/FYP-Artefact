using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
   Rigidbody rb;

   [SerializeField]float moveSpeed;
   [SerializeField] float jumpForce;
   [SerializeField] bool isGrounded;
   [SerializeField] LayerMask layerMask;

   [SerializeField]Transform playerFeet;

    PlayerInput playerInput;

    Coroutine movePlayer;

    Camera playerCamera;

    float m_axis;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerCamera = Camera.main;
    }
    private void Start()
    {
        playerInput.actions.FindAction("Jump").performed += JumpPerformed;
        

        playerInput.actions.FindAction("Move").performed += MovePerformed;
        playerInput.actions.FindAction("Move").canceled += MoveCancelled;

        playerInput.actions.FindAction("Look").performed += LookPerformed;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
    }

   private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce * 10f, ForceMode.Impulse);
    }

    private void LookPerformed(InputAction.CallbackContext context)
    {
        m_axis = context.ReadValue<float>();
        Look();
    }

    private void Look()
    {
        Vector3 moveDer = new Vector3(m_axis, m_axis, 0);
        playerCamera.transform.position = moveDer;
    }


    private void MovePerformed(InputAction.CallbackContext context)
    {
        m_axis = context.ReadValue<float>();
        if(movePlayer == null)
        {
            movePlayer = StartCoroutine(Move());
        }
    }

    private void MoveCancelled(InputAction.CallbackContext context)
    {
        m_axis = context.ReadValue<float>();
        if (movePlayer != null)
        {
            
            StopCoroutine(movePlayer);
            movePlayer = null;
        }
    }
    IEnumerator  Move()
    {
        Vector3 moveDer = new Vector3(m_axis,m_axis, 0);
        rb.AddForce(moveDer * moveSpeed * 10f, ForceMode.Force);
        yield return new WaitForFixedUpdate();
    }

    private void groundCheck()
    {

    }
}
