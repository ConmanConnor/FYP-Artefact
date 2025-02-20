using UnityEngine;

public class AnimatorChecker : MonoBehaviour
{


    //-----------------Aninimation----------------//
    [Header("Animation")]
    Animator Animator;

    //----------------Float----------------------//
    float playerVel;
    bool isMove;

    //---------------Scripts---------------------//
    PlayerController pc;
    ParkourDecider decider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Animator = GetComponent<Animator>();
        pc = GetComponent<PlayerController>();
        decider = GetComponent<ParkourDecider>();
    }

    // Update is called once per frame
    void Update()
    {
        playerVel = pc.rb.linearVelocity.magnitude;
        animPlayer();
    }

    private void animPlayer()
    {
        Animator.SetFloat("rbVelocity", playerVel);

        if (isRunning())
        {
            while (decider.currentState == PlayerState.Moving)
            {
                Animator.Play("Running");
                return;
            }
        }
        else if(isIdle()) 
        {
            while (decider.currentState == PlayerState.Idle)
            {
                Animator.Play("Idle");
                return;
            }
        }
    }

    public bool isRunning()
    {
        return playerVel > 0 && decider.currentState == PlayerState.Moving;
    }

    public bool isIdle()
    {
        return playerVel == 0 && decider.currentState == PlayerState.Idle;
    }
}
