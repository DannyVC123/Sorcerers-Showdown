using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthTracker : MonoBehaviour
{
    public Image[] heartSprites;
    private int healthPoints = 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int LoseLife()
    {
        healthPoints--;
        heartSprites[healthPoints].enabled = false;
        return healthPoints;
    }
}
