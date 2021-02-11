using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    public float TimeStep = 1.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = TimeStep;
        Time.fixedDeltaTime /= TimeStep;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
