using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class highRR : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        OVRManager.display.displayFrequency = 90.0f;
        OVRPlugin.systemDisplayFrequency = 90.0f;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
