using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using TMPro;

using Random = System.Random;
using System.Linq;
using System.IO;
using String = System.String;

using CsvLog = csvlog.CsvLog;

[ExecuteInEditMode()]
public class OnlineTest : MonoBehaviour   
{
    public GameObject[] stimulis;
    public GameObject stimuli_reference;
    public Dictionary<String, int> freq_stimuliidx = new Dictionary<String, int>();

    public BackendController backendController; 
    public TextMeshProUGUI myText;
    public AudioSource BeepSound;

    public int numberSamples = 5;
    public string description = ""; 
    private bool training = false;
    private bool trained = false;
    public bool isStimuliActive = false;

    private static Random rng = new Random();
    private List<int> stimuliIdx;
    string[] filePaths;

    private static int relax_t = 4;
    private static int inst_t = 3;
    private static int stimuli_t = 2;

    CsvLog logger;

    // Start is called before the first frame update
    void Start()
    {
        filePaths = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "netflix_posters"), "*.jpg",
                                         SearchOption.TopDirectoryOnly);
        stimuliIdx = Enumerable.Range(0, stimulis.Length).ToList();
        Debug.Log(Application.persistentDataPath);
        //Debug.Log(stimulis[0].GetComponent<PogressBar>().Frequency);
        //Debug.Log(stimulis[0].GetComponent<PogressBar>().buttonState);

        for (var i = 0; i < stimulis.Length; i++)
        {
            freq_stimuliidx.Add($"{stimulis[i].GetComponent<PogressBar>().Frequency:0.0}", i);
           
        }

        stimuli_reference.SetActive(false);
        getRandomImages();
        activateStimuli(true);

        myText.text = $"Press button (A) to Start the trainig!\n{description}";
        //OVRManager.display.displayFrequency = 120.0f;
        //OVRPlugin.systemDisplayFrequency = 120.0f;

    }

    void getRandomImages()
    {
        var shuffledIdx = filePaths.OrderBy(a => rng.Next()).ToList();
        for (var i = 0; i < stimulis.Length; i++)
        {
            stimulis[i].GetComponent<RawImage>().texture = LoadImage(shuffledIdx[i]);
        }
    }

    void activateStimuli(bool flag)
    {
        isStimuliActive = flag;
        setButtonsState(ButtonState.Idle);
        for (var i = 0; i < stimulis.Length; i++)
        {
            stimulis[i].SetActive(flag);
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (backendController.buttonState)
        {
            case ButtonState.Inactive:
                backendController.buttonState = ButtonState.Idle;
                backendController.isStimuliActive = true;
                break;
            case ButtonState.Hover:
                stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Hover;
                break;
            case ButtonState.Cancel:
                stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Idle;
                backendController.buttonState = ButtonState.Idle;
                break;
            case ButtonState.Selection:
                //stimuliFlag = false;
                backendController.isStimuliActive = false;
                backendController.buttonState = ButtonState.Inactive;
                stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Selection;
                //setButtonsState(ButtonState.Idle);
                break;
        }
        
    }

    public void setButtonsState(ButtonState buttonState)
    {
        for (var i = 0; i < stimulis.Length; i++)
        {
            stimulis[i].GetComponent<PogressBar>().buttonState = buttonState;
        }
    }


    private Texture2D LoadImage(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
}
