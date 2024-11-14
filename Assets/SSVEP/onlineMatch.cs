using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using TMPro;

using System.Linq;
using System.IO;
using String = System.String;

using CsvLog = csvlog.CsvLog;

[ExecuteInEditMode()]
public class OnlineMatch : MonoBehaviour   
{
    public GameObject[] stimulis;
    public GameObject stimuli_reference;
    public Dictionary<String, int> freq_stimuliidx = new Dictionary<String, int>();

    // GAME variables
    List<string> shuffledPairs = new List<String>();
    private Queue<int> sequence;
    private int pairsFound = 0;

    public BackendController backendController; 
    public TextMeshProUGUI myText;
    public AudioSource BeepSound;

    public bool isStimuliActive = false;
    private bool show_start = true; 

    //private static Random rng = new Random();
    private List<int> stimuliIdx;
    string[] filePaths;

    private static int stimuli_t = 2;

    private bool beingHandled = false;

    CsvLog logger;

    // Start is called before the first frame update
    void Start()
    {
        filePaths = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "netflix_posters"), "*.jpg",
                                         SearchOption.TopDirectoryOnly);
        stimuliIdx = Enumerable.Range(0, stimulis.Length).ToList();
        Debug.Log(Application.persistentDataPath);

        for (var i = 0; i < stimulis.Length; i++)
        {
            freq_stimuliidx.Add($"{stimulis[i].GetComponent<PogressBar>().Frequency:0.0}", i);
           
        }

        stimuli_reference.SetActive(false);
        GetRandomImagePairs();
        activateStimuli(true);
        pairsFound = 0;
        sequence = new Queue<int>();
        show_start = true; 

        myText.text = $"Press button (A) to Start the trainig!";
        //OVRManager.display.displayFrequency = 120.0f;
        //OVRPlugin.systemDisplayFrequency = 120.0f;

    }

    void GetRandomImagePairs()
    {
        // Shuffle the file paths to ensure a random order without repeats
        List<string> shuffledFilePaths = filePaths.OrderBy(a => UnityEngine.Random.value).ToList();

        List<string> imagePool = new List<string>();
        int pairCount = stimulis.Length / 2;

        // Ensure we have enough unique images to make pairs
        if (shuffledFilePaths.Count < pairCount)
        {
            Debug.LogError("Not enough unique images to create the required number of pairs.");
            return;
        }

        // Populate the pool with pairs of unique images
        for (int i = 0; i < pairCount; i++)
        {
            string imagePath = shuffledFilePaths[i]; // Take a unique image
            imagePool.Add(imagePath); // Add the first instance
            imagePool.Add(imagePath); // Add the second instance to form a pair
        }

        // Shuffle the list of pairs to randomize their placement
        shuffledPairs = imagePool.OrderBy(a => UnityEngine.Random.value).ToList();

        // Assign the shuffled images to the stimuli
        for (var i = 0; i < stimulis.Length; i++)
        {
            stimulis[i].GetComponent<RawImage>().texture = LoadImage(shuffledPairs[i]);
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

    void rotateStimulis()
    {
        for (var i = 0; i < stimulis.Length; i++)
        {
            StartCoroutine(RotateObject(stimulis[i], 1));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown("a")))
        {
            show_start = true;
            StartCoroutine(showstimuliStart());
        }
        if ( /*some case  */ !beingHandled )
        {
            StartCoroutine(HandleIt());
        }
    }

    private IEnumerator showstimuliStart()
    {
        Debug.Log("rotate");
        rotateStimulis();
        yield return new WaitForSeconds(3);
        rotateStimulis();
        show_start = false; 
    }

    private IEnumerator HandleIt()
    {
        beingHandled = true;
        switch (backendController.buttonState)
        {
            case ButtonState.Inactive:
                yield return new WaitForSeconds(2.0f);
                backendController.buttonState = ButtonState.Idle;

                if (sequence.Count >= 2)
                {
                    int firstelement = sequence.Dequeue();
                    int secondelement = sequence.Dequeue();
                    if (shuffledPairs[firstelement] == shuffledPairs[secondelement])
                    {
                        pairsFound += 1;
                        Debug.Log($"Paris found: {pairsFound}");
                    }
                    else
                    {
                        StartCoroutine(RotateObject(stimulis[firstelement], 1));
                        StartCoroutine(RotateObject(stimulis[secondelement], 1));
                        stimulis[firstelement].GetComponent<PogressBar>().buttonState = ButtonState.Idle;
                        stimulis[secondelement].GetComponent<PogressBar>().buttonState = ButtonState.Idle;
                    }
                }
                break;
            case ButtonState.Idle:
                backendController.isStimuliActive = true;
                //activateStimuli(true);
                break;
            case ButtonState.Hover:
                stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Hover;
                break;
            case ButtonState.Cancel:
                stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Idle;
                backendController.buttonState = ButtonState.Inactive;
                break;
            case ButtonState.Selection:
                //stimuliFlag = false;
                backendController.isStimuliActive = false;
                stimulis[freq_stimuliidx[backendController.stimuliFrequency]].GetComponent<PogressBar>().buttonState = ButtonState.Selection;
                //setButtonsState(ButtonState.Idle);
                backendController.buttonState = ButtonState.Inactive;
                
                
                sequence.Enqueue(freq_stimuliidx[backendController.stimuliFrequency]);
                StartCoroutine(RotateObject(stimulis[freq_stimuliidx[backendController.stimuliFrequency]], 1));
              

                break;
        }
        beingHandled = false;
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


    public IEnumerator RotateObject(GameObject target, float duration)
    {
        Quaternion startRotation = target.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0); // Rotate 180 degrees on Y-axis

        float elapsed = 0f;
        while (elapsed < duration)
        {
            target.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null; // Wait until the next frame
        }
        target.transform.rotation = endRotation; // Ensure it ends exactly at the target rotation
    }


}
