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
public class OnlineFindDiff : MonoBehaviour   
{
    public GameObject[] stimulis;
    public GameObject stimuli_reference;
    public Dictionary<String, int> freq_stimuliidx = new Dictionary<String, int>();

    public BackendController backendController; 
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
        filePaths = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "netflix_posters_genai"), "*.jpg",
                                         SearchOption.TopDirectoryOnly);

        filePaths = filePaths.OrderBy(a => rng.Next()).ToArray(); 

        stimuliIdx = Enumerable.Range(0, stimulis.Length).ToList();
        Debug.Log(Application.persistentDataPath);
        //Debug.Log(stimulis[0].GetComponent<PogressBar>().Frequency);
        //Debug.Log(stimulis[0].GetComponent<PogressBar>().buttonState);

        for (var i = 0; i < stimulis.Length; i++)
        {
            freq_stimuliidx.Add($"{stimulis[i].GetComponent<PogressBar>().Frequency:0.0}", i);
           
        }

        stimuli_reference.SetActive(false);
        UIButtons.active = false;
        buttonsFlag = false; 
        getRandomImagesGenAI(0,0);
        activateStimuli(false);

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

    void getRandomImagesGenAI(int idx = 0, int sample = 0)
    {
        string filepath = filePaths[idx + sample];
        string directoryName = Path.GetFileNameWithoutExtension(filepath);

        var filePathsGenAI = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "netflix_posters_genai", directoryName), "*.jpg",
                                         SearchOption.TopDirectoryOnly);

        var shuffledIdx = filePathsGenAI.OrderBy(a => rng.Next()).ToList();

        for (var i = 0; i < stimulis.Length; i++)
        {
            stimulis[i].GetComponent<RawImage>().texture = LoadImage(shuffledIdx[i]);
        }
        stimulis[idx].GetComponent<RawImage>().texture = LoadImage(filepath);
    }

    void getSpotDifference(int stimuliId )
    {
        int flipIdx;
        do
        {
            flipIdx = UnityEngine.Random.Range(0, stimulis.Length);
        } while (flipIdx == stimuliId);

        int colorRemoveIdx;
        do
        {
            colorRemoveIdx = UnityEngine.Random.Range(0, stimulis.Length);
        } while (colorRemoveIdx == stimuliId || colorRemoveIdx == flipIdx);

        stimulis[flipIdx].GetComponent<RawImage>().texture = FlipTextureHorizontally(stimulis[stimuliId].GetComponent<RawImage>().texture as Texture2D);
        stimulis[colorRemoveIdx].GetComponent<RawImage>().texture = RemoveRandomColorChannel(stimulis[stimuliId].GetComponent<RawImage>().texture as Texture2D);
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
        }
        if (trained && (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown("a")))
        {
            
            myText.text = "Press button (A) to restart the trainig!";
            trained = false;
        }

        if (Input.GetKeyDown("c"))
        {
            filePaths = filePaths.OrderBy(a => rng.Next()).ToArray();
            getRandomImagesGenAI();
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
        getRandomImagesGenAI(idx, sample);
        logger.writeLine("relax");
        myText.text = string.Format("Relax\n{0} / {1} Trial", sample + 1, numberSamples);
        yield return new WaitForSeconds(relax_t);
        
        BeepSound.Play();
        // # Instruction 
        myText.text = "Find and focus on the image";//+ stimulis[idx].GetComponentInChildren<Text>().text;

        stimuli_reference.GetComponent<RawImage>().texture = stimulis[idx].GetComponent<RawImage>().texture;
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
        while (stimuliFlag)  // Wait until trigger 
        {
            switch (backendController.buttonState)
            {
                case ButtonState.Inactive:
                    yield return new WaitForSeconds(2.0f);
                    backendController.buttonState = ButtonState.Idle;
                    break;
                case ButtonState.Idle:
                    backendController.isStimuliActive = true;
                    activateStimuli(true);
                    break;
                case ButtonState.Hover:
                    stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Hover; 
                    break;
                case ButtonState.Cancel:
                    stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Cancel;
                    backendController.buttonState = ButtonState.Idle;
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
