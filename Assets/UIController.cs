using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[ExecuteInEditMode()]
public class UIController : MonoBehaviour
{
    public GameObject[] stimulis;
    public TextMeshProUGUI myText;
    public enum UIState { Inactive, Flickering, Hover, Selected };
    private UIState uiState; 
    private int countHover;
    private int  gazedStimuli_idx;

    // Start is called before the first frame update
    void Start()
    {
        myText.text = "Select a Movie to watch!";
        setButtonsState(ButtonState.Idle);
        uiState = UIState.Flickering;
    }

    public void StartHoverFeedback(Vector3Int data)
    {
        int gazed = data[0];
        int stimuli_idx = data[1];
        int z = data[2];

        Debug.Log("UI state " + uiState + " CountHover " + countHover);
        switch (uiState)
        {
            case UIState.Inactive:
                break;
            case UIState.Flickering:
                countHover = 0;
                setButtonsState(ButtonState.Idle);
                if (gazed == 1)
                {
                    stimulis[stimuli_idx].GetComponent<PogressBar>().buttonState = ButtonState.Hover;
                    stimulis[stimuli_idx].GetComponent<PogressBar>().upper_limit = 0.25f;
                    uiState = UIState.Hover;
                    gazedStimuli_idx = stimuli_idx;
                    countHover += 1;
                }
                break;
            case UIState.Hover:
                if (data[0] == 1 && stimuli_idx == gazedStimuli_idx && stimulis[stimuli_idx].GetComponent<PogressBar>().buttonState == ButtonState.Hover)
                {  
                    if(countHover < 4)
                    {
                        countHover += 1;
                        stimulis[stimuli_idx].GetComponent<PogressBar>().upper_limit = 0.25f* (countHover);
                    }
                    if (countHover >= 4)
                    {
                        uiState = UIState.Selected;
                    }
                }
                else
                {
                    uiState = UIState.Flickering;
                }
                break;
            case UIState.Selected:
                setButtonsState(ButtonState.Inactive);
                myText.text = "Movie Selected!";
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setButtonsState(ButtonState buttonState)
    {
        for (var i = 0; i < stimulis.Length; i++)
        {
            stimulis[i].GetComponent<PogressBar>().buttonState = buttonState;
        }
    }
}
