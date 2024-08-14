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
public class offlineNetflix : MonoBehaviour   
{
    public GameObject[] stimulis;
    public GameObject stimuli_reference;

    public TextMeshProUGUI myText;
    public AudioSource BeepSound;

    public int numberSamples = 5;
    public string description = ""; 
    private bool training = false;
    private bool trained = false;

    private static Random rng = new Random();
    private List<int> stimuliIdx;
    string[] filePaths;

    private static int relax_t = 4;
    private static int inst_t = 3;
    private static int stimuli_t = 5;

    CsvLog logger;

    // Start is called before the first frame update
    void Start()
    {
        filePaths = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "netflix_posters"), "*.jpg",
                                         SearchOption.TopDirectoryOnly);
        stimuliIdx = Enumerable.Range(0, stimulis.Length).ToList();
        Debug.Log(stimulis[0].GetComponent<PogressBar>().Frequency);
        Debug.Log(stimulis[0].GetComponent<PogressBar>().buttonState);

        stimuli_reference.SetActive(false);
        getRandomImages();
        activateStimuli(false);

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
        myText.text = "****Train Finnished!****\nPress (A) to save session";
        training = false;
        trained = true;
    }

    IEnumerator StimuliSequence(int idx, int sample)
    {
        getRandomImages();
        logger.writeLine("relax");
        myText.text = string.Format("Relax\n{0} / {1} Epoch", sample + 1, numberSamples);
        yield return new WaitForSeconds(relax_t);
        
        BeepSound.Play();
        // # Instruction 
        myText.text = "Find and focus on the image\n Press button";//+ stimulis[idx].GetComponentInChildren<Text>().text;

        stimuli_reference.GetComponent<RawImage>().texture = stimulis[idx].GetComponent<RawImage>().texture;
        stimuli_reference.SetActive(true);
        yield return new WaitForSeconds(inst_t);

        //BeepSound.Play();
        //Start Flickering
        myText.text = "";
        float f_label = stimulis[idx].GetComponent<PogressBar>().Frequency; 
        stimuli_reference.SetActive(false);
        activateStimuli(true);
        logger.writeLine(string.Format("start_{0}", idx));

        while (!Input.GetKeyDown("a") && !OVRInput.GetDown(OVRInput.Button.One))
        {
            yield return new WaitForSeconds(0.001f);
        }
        BeepSound.Play();
        logger.writeLine(idx);

        yield return new WaitForSeconds(stimuli_t);

        //Stop Flickering
        activateStimuli(false);
        BeepSound.Play();
    }

    private static Texture2D LoadImage(string filePath)
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
