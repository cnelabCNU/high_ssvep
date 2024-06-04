using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using TMPro;

using Random = System.Random;
using System.Linq;
using String = System.String;

using CsvLog = csvlog.CsvLog;

[ExecuteInEditMode()]
public class Training : MonoBehaviour   
{
    public GameObject[] stimulis;
    public GameObject[] arrows;
    public TextMeshProUGUI myText;
    public AudioSource BeepSound;

    public int numberSamples = 2;
    private bool training = false;
    private bool trained = false;

    private static Random rng = new Random();
    static List<int> stimuliIdx = new List<int>() { 0, 1, 2, 3 };

    private static int relax_t = 2;
    private static int inst_t = 2;
    private static int stimuli_t = 5;

    CsvLog logger;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(stimulis[0].GetComponent<PogressBar>().Frequency);
        Debug.Log(stimulis[0].GetComponent<PogressBar>().buttonState);
        for (var i = 0; i < arrows.Length; i++)
        {
            arrows[i].SetActive(false);
        }

        myText.text = "Press button (A) to Start the trainig!";
        OVRManager.display.displayFrequency = 120.0f;
        OVRPlugin.systemDisplayFrequency = 120.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!training && !trained && (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown("a")))
        {
            StartCoroutine("MySequence");
            training = true;
        }
        if (training && (OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown("b")))
        {
            myText.text = "Stopped\nPress button (A) to restart the trainig!";
            StopCoroutine("MySequence");
            training = false;
        }
        if (trained && (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown("a")))
        {
            logger.saveFile();
            myText.text = "Saved!\nPress button (A) to restart the trainig!";
            trained = false;
        }
    }

    void setButtonsState(ButtonState buttonState)
    {
        for (var i = 0; i < stimulis.Length; i++)
        {
            stimulis[i].GetComponent<PogressBar>().buttonState = buttonState;
        }
    }

    IEnumerator MySequence()
    {
        logger = new CsvLog("NetflixOff");
        logger.writeLine("start");
        myText.text = "Start training!"; // Test 
        yield return new WaitForSeconds(5);
        BeepSound.Play();

        logger.writeLine("restingCE");
        myText.text = "Close Eyes!";
        yield return new WaitForSeconds(10);
        BeepSound.Play();

        for (var j = 0; j < numberSamples; j++)
        {
            var shuffledIdx = stimuliIdx.OrderBy(a => rng.Next()).ToList();

            foreach (var x in shuffledIdx)
            {
                StartCoroutine(StimuliSequence(x, j));
                yield return new WaitForSeconds(relax_t + inst_t + stimuli_t);
            }
        }
        myText.text = "****Train Finnished!****\nPress (A) to save session";
        training = false;
        trained = true;
    }

    IEnumerator StimuliSequence(int idx, int sample)
    {
        logger.writeLine("relax");
        myText.text = string.Format("{0} / {1} Epoch\n*Relax*", sample + 1, numberSamples);
        yield return new WaitForSeconds(relax_t);
        BeepSound.Play();
        myText.text = "Focus on Stimulus\n" + idx.ToString();//+ stimulis[idx].GetComponentInChildren<Text>().text;
        //Hover Button 
        arrows[idx].SetActive(true);
        stimulis[idx].GetComponent<PogressBar>().buttonState = ButtonState.SelectionIdle;

        yield return new WaitForSeconds(inst_t);
        //Start Flickering
        //setButtonsState(ButtonState.Idle);
        logger.writeLine(idx);

        yield return new WaitForSeconds(stimuli_t);
        //Stop Flickering
        stimulis[idx].GetComponent<PogressBar>().buttonState = ButtonState.Idle;
        arrows[idx].SetActive(false);
        //setButtonsState(ButtonState.Inactive);
    }
}
