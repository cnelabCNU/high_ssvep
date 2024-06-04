using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public enum ButtonState { Inactive, Idle, Hover, Selection, SelectionIdle };

public class PogressBar : MonoBehaviour
{
    public ButtonState buttonState;
    public Shader shader;
    public float Frequency; 
    public int minimum; 
    public int maximum;
    public float current;
    private float timer_hover = 0.0f;
    public Image mask;
    public float _chargeSpeed = 0.25f;
    public RawImage imageraw;
    public float upper_limit = 1.0f; 

    // Start is called before the first frame update
    void Start()
    {
        //buttonState = ButtonState.Inactive;
        imageraw.material = new Material(shader);
        imageraw.material.SetFloat("_frequency", Frequency);
    }

    // Update is called once per frame
    void Update()
    {
        switch (buttonState)
        {
            case ButtonState.Inactive:
                current = 0;
                imageraw.material.SetInt("_Active", 0);
                break;
            case ButtonState.Idle:
                current = 0;
                imageraw.material.SetInt("_Active", 1);
                break;
            case ButtonState.Hover:
                StartPogress();
                break;
            case ButtonState.Selection:
                current = 1;
                imageraw.material.SetInt("_Active", 0);
                break;
            case ButtonState.SelectionIdle:
                current = 1;
                imageraw.material.SetInt("_Active", 1);
                break;
        }
        GetCurrentFill();
    }

    void GetCurrentFill()
    {
        float currentOffset = current - minimum;
        float maximumOffset = maximum - minimum;
        float fillAmount = currentOffset / maximumOffset;
        mask.fillAmount = fillAmount; 
    }

    void StartPogress()
    {
        if (current > 1) // Limit upper value to 1
        {
            current = 1;
            //buttonState = ButtonState.Selection;
        }else if(current < upper_limit)
        {
            current += _chargeSpeed * Time.deltaTime;
        }
        else if(current > upper_limit && upper_limit != 1 )
        {
            timer_hover += Time.deltaTime;
            if (timer_hover > 2.0f)
            {
                buttonState = ButtonState.Idle;
            }
        }
    }

    public void SetButtonState(ButtonState buttonState)
    {
        buttonState = buttonState;
    }
}
