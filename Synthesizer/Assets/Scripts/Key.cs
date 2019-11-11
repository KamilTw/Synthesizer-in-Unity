using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{ 
    public KeyCode keyCode;
    public double frequency;

    private Oscillator oscillator;
    private Image keyImage;

    void Start()
    {
        keyImage = GetComponent<Image>();
        oscillator = GetComponent<Oscillator>();
        oscillator.frequency = frequency;
    }

    void Update()
    {
        ControlKey();
    }

    void ControlKey()
    {
        if (Input.GetKeyDown(keyCode))
        {
            oscillator.NoteOn();
            keyImage.color = Color.grey;
        }

        if (Input.GetKeyUp(keyCode))
        {
            oscillator.NoteOff();
            keyImage.color = Color.white;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}