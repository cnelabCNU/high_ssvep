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
public class OnlineAlphabet : MonoBehaviour   
{
    public GameObject[] stimulis;
    public GameObject stimuli_reference;
    public Dictionary<String, int> freq_stimuliidx = new Dictionary<String, int>();

    private string referencePair;

    private int[] stimuliCount;

    private  BackendController backendController; 
    public TextMeshProUGUI myText;
    public AudioSource BeepSound;

    public GameObject UIButtons;
    private bool buttonsFlag;
    private int buttonInput = -1; 

    public int numberSamples = 5;
    public string description = ""; 
    private bool training = false;
    private bool trained = false;
    public bool isStimuliActive = false;
    public bool activateFeedback = true;

    private static Random rng = new Random();
    private List<int> stimuliIdx;
    string[] filePaths;

    private static int relax_t = 4;
    private static int inst_t = 3;
    private static int stimuli_t = 2;


    private List<string> similarCharacterPairs = new List<string>
    {
        "1I", "1l", "1L", "I1", "lI",
        "0O", "O0", "Q0", "OQ", "0Q",
        "5S", "5Z", "S5", "S8", "Z2",
        "2Z", "ZS", "S2", "B8", "8B",
        "8S", "B5", "6G", "G9", "G6",
        "6B", "B6", "9G", "G9", "9S",
        "C0", "0C", "D0", "0D", "D6",
        "P1", "P7", "P0", "P4", "P3",
        "H4", "H1", "N1", "M1", "N0",
        "V1", "V7", "W1", "W7", "X1",
        "I1", "L1", "0O", "O8", "O9",
        "C6", "C9", "O9", "U1", "V0",
        "M1", "N1", "A4", "B6", "8B",
        "Z9", "Z0", "3E", "E3", "E8",
        "E9", "Z8", "C9", "R1", "R7",
        "T7", "T1", "V9", "V3", "VV",
        "V8", "W8", "W3"
    };


    CsvLog logger;

    // Start is called before the first frame update
    void Start()
    {

        backendController = GetComponentInParent<BackendController>();
        stimuliIdx = Enumerable.Range(0, stimulis.Length).ToList();
        Debug.Log(Application.persistentDataPath);
        //Debug.Log(stimulis[0].GetComponent<PogressBar>().Frequency);
        //Debug.Log(stimulis[0].GetComponent<PogressBar>().buttonState);

        for (var i = 0; i < stimulis.Length; i++)
        {
            freq_stimuliidx.Add($"{stimulis[i].GetComponent<PogressBar>().Frequency:0.0}", i);
           
        }
        stimuliCount = new int[stimulis.Length];

        stimuli_reference.SetActive(false);
        UIButtons.active = false;
        buttonsFlag = false; 
        
        activateStimuli(false);
        getRandomCharacterPairs();
        myText.text = $"Press button (A) to Start experiment!\n{description}";

    }

    void getRandomImages(bool spotDifference = false)
    {
        var shuffledIdx = filePaths.OrderBy(a => rng.Next()).ToList();
        for (var i = 0; i < stimulis.Length; i++)
        {
            stimulis[i].GetComponent<RawImage>().texture = LoadImage(shuffledIdx[i]);
        }
    }

    void getRandomCharacterPairs()
    {
        var random = new Random();
        referencePair = similarCharacterPairs[random.Next(similarCharacterPairs.Count)];
        char referenceChar = referencePair[0];
        char fillerChar = referencePair[1];

        for (var i = 0; i < stimulis.Length; i++)
        {
            int referenceCount = random.Next(1, 13);
            var charArray = new char[95];

            for (int j = 0; j < referenceCount; j++)
            {
                charArray[j] = referenceChar;
            }
            for (int j = referenceCount; j < 95; j++)
            {
                charArray[j] = fillerChar;
            }
            for (int j = charArray.Length - 1; j > 0; j--)
            {
                int swapIndex = random.Next(j + 1);
                char temp = charArray[j];
                charArray[j] = charArray[swapIndex];
                charArray[swapIndex] = temp;
            }
            TextMeshProUGUI textComponent = stimulis[i].GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = new string(charArray);

            stimuliCount[i] = referenceCount;
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
        if (!training && !trained && (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown("a")))
        {
            StartCoroutine("MySequence");
            training = true;
        }
        if (training && (OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown("b")))
        {
            myText.text = "Stopped\nPress button (A) to restart the trainig!";
            activateStimuli(false);
            stimuli_reference.SetActive(false);
            StopCoroutine("MySequence");
            training = false;
            activateStimuli(false);
        }
        if (trained && (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown("a")))
        {
            
            myText.text = "Press button (A) to restart the trainig!";
            trained = false;
        }

        if (training && (Input.GetKeyDown("s")))
        {

            logger.saveFile();

        }

        if (Input.GetKeyDown("c"))
        {
            getRandomCharacterPairs();
        }

    }

    public void setButtonsState(ButtonState buttonState)
    {
        for (var i = 0; i < stimulis.Length; i++)
        {
            stimulis[i].GetComponent<PogressBar>().buttonState = buttonState;
        }
    }

    IEnumerator MySequence()
    {
        activateStimuli(false);
        stimuli_reference.SetActive(false);
        UIButtons.active = false;
        logger = new CsvLog(description);
        logger.writeLine("start");
        myText.text = $"Start training! {description}" ; // Test 
        yield return new WaitForSeconds(2);
        BeepSound.Play();

        logger.writeLine("restingCE");
        myText.text = "Close Eyes!";
        yield return new WaitForSeconds(5);
        BeepSound.Play();

        for (var j = 0; j < numberSamples; j++)
        {
            var shuffledIdx = stimuliIdx.OrderBy(a => rng.Next()).ToList();

            foreach (var x in shuffledIdx)
            {
                yield return StartCoroutine(StimuliSequence(x, j));
                //yield return new WaitForSeconds(relax_t + inst_t + stimuli_t);
            }
        }
        myText.text = "****Train Finnished!****\nPress (A) to start again";
        logger.saveFile();
        training = false;
        trained = true;
    }

    IEnumerator StimuliSequence(int idx, int sample)
    {
        getRandomCharacterPairs();
        logger.writeLine("relax");
        myText.text = string.Format("Relax\n{0} / {1} Trial", sample + 1, numberSamples);
        yield return new WaitForSeconds(relax_t);

        BeepSound.Play();
        // # Instruction 
        myText.text = $"Find {stimuliCount[idx]} occurrences of the letter {referencePair[0]}";//+ stimulis[idx].GetComponentInChildren<Text>().text;

        //stimuli_reference.GetComponent<RawImage>().texture = stimulis[idx].GetComponent<RawImage>().texture;

        TextMeshProUGUI textComponent = stimuli_reference.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = referencePair[0].ToString();
        stimuli_reference.SetActive(true);
        yield return new WaitForSeconds(inst_t);

        //BeepSound.Play();
        //Start Flickering
        myText.text = "";
        stimuli_reference.SetActive(false);

        activateStimuli(true);
        logger.writeColumn(string.Format("start_{0}", idx));

        //while (!Input.GetKeyDown("a") && !OVRInput.GetDown(OVRInput.Button.One))
        bool stimuliFlag = true;
        backendController.buttonState = ButtonState.Inactive;
        while (stimuliFlag)  // Wait until trigger 
        {
            switch (backendController.buttonState)
            {
                case ButtonState.Inactive:
                    yield return new WaitForSeconds(1.0f);
                    backendController.buttonState = ButtonState.Idle;
                    break;
                case ButtonState.Idle:
                    backendController.isStimuliActive = true;
                    activateStimuli(true);
                    break;
                case ButtonState.Hover:
                    if (activateFeedback)
                    {
                        stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Hover;
                    }
                    break;
                case ButtonState.Cancel:
                    if (activateFeedback)
                    {
                        stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Cancel;

                    }
                    backendController.buttonState = ButtonState.Inactive;
                    break;
                case ButtonState.Selection:
                    stimuliFlag = false;
                    backendController.isStimuliActive = false;

                    setButtonsState(ButtonState.Inactive);
                    stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Selection;
                    backendController.buttonState = ButtonState.Inactive;
                    break;
            }
            yield return new WaitForSeconds(0.001f);
        }

        BeepSound.Play();

        float target_label = stimulis[idx].GetComponent<PogressBar>().Frequency;
        float stimuli_label = stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().Frequency;

        yield return new WaitForSeconds(stimuli_t);

        BeepSound.Play();

        // Get feedback from user 
        myText.text = "Was this your selection?";
        buttonsFlag = true;
        UIButtons.active = true;

        stimulis[freq_stimuliidx[backendController.stimuliFrequency]].SetActive(true);

        while (buttonsFlag)
        {
            yield return new WaitForSeconds(0.001f);
        }

        logger.writeLine($"{target_label:0.0},{stimuli_label:0.0},{buttonInput}");
        UIButtons.active = false;
        stimuli_reference.SetActive(false);
        activateStimuli(false);

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

    public static Texture2D FlipTextureHorizontally(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);
        for (int y = 0; y < original.height; y++)
        {
            for (int x = 0; x < original.width; x++)
            {
                flipped.SetPixel(x, y, original.GetPixel(original.width - x - 1, y));
            }
        }
        flipped.Apply();
        return flipped;
    }

    public static Texture2D RemoveRandomColorChannel(Texture2D original)
    {
        // Clone the original texture to avoid modifying it directly
        Texture2D modified = new Texture2D(original.width, original.height);
        Color[] pixels = original.GetPixels();

        // Randomly choose which channel to remove: 0 = Red, 1 = Green, 2 = Blue
        int channelToRemove = UnityEngine.Random.Range(0, 3);

        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            switch (channelToRemove)
            {
                case 0: // Remove Red
                    pixel.r = 0;
                    break;
                case 1: // Remove Green
                    pixel.g = 0;
                    break;
                case 2: // Remove Blue
                    pixel.b = 0;
                    break;
            }
            pixels[i] = pixel;
        }

        // Apply the modified pixels back to the new texture
        modified.SetPixels(pixels);
        modified.Apply();
        return modified;
    }

    public void ButtonClick(string buttonLetter)
    {
        switch (buttonLetter)
        {
            case "YES":
                buttonInput = 1; 
                buttonsFlag = false; 
                break;
            case "NO":
                buttonInput = 0;
                buttonsFlag = false;
                break;
        }
    }

}
