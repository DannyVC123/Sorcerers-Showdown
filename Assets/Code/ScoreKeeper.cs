using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static ScoreKeeper Singleton;

    public TMP_Text scoreText;

    private int ptsPerDamage = 10;
    private int multiplier = 5;

    public int score = 0;

    void Start()
    {
        scoreText.text = "Score: " + score;
    }

    public void addScore(int enemiesDamaged)
    {
        score += enemiesDamaged * ptsPerDamage;
        if (enemiesDamaged > 1)
        {
            score += enemiesDamaged * multiplier;
        }
        scoreText.text = "Score: " + score;
    }
}

