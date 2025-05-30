using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateShower : MonoBehaviour
{
    ParkourDecider Decider;
    GameObject Controller;
    [SerializeField]GameObject TextToChange;
    [SerializeField] TextMeshProUGUI ChangeText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Controller = GameObject.FindGameObjectWithTag("Player");
        Decider = Controller.GetComponent<ParkourDecider>();
        TextToChange = this.gameObject;
        ChangeText = TextToChange.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ChangeText != null && Decider != null)
        {
            ChangeText.text = Decider.currentState.ToString();
        }
    }
}
