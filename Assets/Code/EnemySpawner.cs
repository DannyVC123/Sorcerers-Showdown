using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public static int RANDOM_SHAPE = -1;
    public static int UNKOWN = 0, HORIZ = 1, VERT = 2, DOWN_V = 3, UPWARD_V = 4;

    public GameObject EnemyPrefab;
    public TMP_Text title;

    public Sprite[] goblinSprites;

    private AudioPlayer audioPlayer;
    public GameObject backgroundMusicObj;
    private AudioSource backgroundMusic;

    public AudioClip winSoundEffect;
    public AudioClip lossSoundEffect;

    private float prevTime = 0;
    private float timeBetweenSubWaves = 2f;
    private float timeBetweenWaves = 3f;
    private float timeBetweenLevels = 4f;

    private int currLevel = 0;
    private int currWave = 1;
    private int currSubWave = 0;

    public const int START = 0, TUTORIAL = 1, WAVE = 2, BETWEEN_WAVE = 3, BETWEEN_LEVEL = 4, WIN = 5, LOSE = 6;
    private int state;

    //
    // LEVELS
    //
    int[][][,] levels;

    //
    // VELOCITIES
    //
    float[] velocities;

    //
    // TUTORIAL
    //
    float tutorialVelocity = 0.4f;
    int[] tutorial = { HORIZ, VERT, DOWN_V, UPWARD_V };

    //
    // LEVEL 1
    //
    int[][,] level1;
    float level1Velocity = 0.5f;

    // numGhosts, numShapes
    int[,] wave11 = {
        {1, 1}, {1, 1}, {1, 1}, {1, 1}
    };
    int[,] wave12 = {
        {1, 3}, {1, 3}
    };
    int[,] wave13 = {
        {2, 1}, {2, 1}, {2, 1}, {2, 1}
    };
    int[,] boss1 = {
        {1, 5}, {1, 5}, {1, 5}, {1, 5}
    };

    //
    // LEVEL 2
    //
    int[][,] level2;
    float level2Velocity = 0.6f;

    int[,] wave21 = {
        {4, 2}, {4, 2}
    };
    int[,] wave22 = {
        {5, 1},
        {5, 3}
    };
    int[,] wave23 = {
        {1, 2}, {1, 2}, {1, 2}, {1, 2}, {1, 2}, {1, 2}, {1, 2}, {1, 2}
    };
    int[,] boss2 = {
        {2, 3}, {2, 3}, {1, 5},
        {2, 3}, {2, 3}, {1, 5},
        {2, 3}, {2, 3}, {1, 5}
    };

    //
    // LEVEL 3
    //
    float level3Velocity = 0.7f;
    int[][,] level3;

    int[,] wave31 = {
        { 2, 3 }
    };
    int[,] wave32 = {
        { 1, 6 }
    };
    int[,] wave33 = {
        { 2, 4 }
    };
    int[,] wave34 = {
        { 2, 3 }, { 1, 1 }, { 2, 4 }, { 1 , 1 }
    };
    int[,] boss3 = {
        { 2, 4 }, { 2, 4 }, { 2, 4 }, { 2, 4 }
    };

    //
    // LEVEL 4
    //
    float level4Velocity = 0.8f;
    int[][,] level4;

    int[,] boss41 = {
        { 1, 2 }, { 1, 2 }, { 1, 4 }, { 1, 2 }, { 1, 2 }, { 1, 4 }, { 1, 2 }, { 1, 2 }, { 1, 8 },
    };
    int[,] boss42 = {
        { 2, 2 }, { 2, 2 }, { 2, 2 }, { 1, 8 },
    };
    int[,] boss43 = {
        { 1, 2 }, { 1, 2 }, { 1, 2 }, { 1, 2 }, { 2, 2 }, { 2, 2 }, { 2, 2 }, { 2, 2 }, { 1, 10 },
    };
    int[,] boss44 = {
        { 2, 1 }, { 2, 1 }, { 2, 2 }, { 2, 2 }, { 2, 3 }, { 2, 3 }, { 1, 15 }
    };

    // Start is called before the first frame update
    void Start()
    {
        audioPlayer = FindObjectOfType<AudioPlayer>();
        backgroundMusic = backgroundMusicObj.GetComponent<AudioSource>();

        // levels
        level1 = new int[][,] { wave11, wave12, wave13, boss1 };
        level2 = new int[][,] { wave21, wave22, wave23, boss2 };
        level3 = new int[][,] { wave31, wave32, wave33, wave34, boss3 };
        level4 = new int[][,] { boss41, boss42, boss43, boss44 };

        levels = new int[][][,] { null, level1, level2, level3, level4 };

        velocities = new float[] { tutorialVelocity, level1Velocity, level2Velocity, level3Velocity, level4Velocity };

        state = START;
    }

    // Update is called once per frame
    void Update()
    {
        float currentTime = Time.time;
        switch (state)
        {
            case START:
                title.enabled = true;
                title.text = "Wave " + currLevel;

                if (currentTime - prevTime >= timeBetweenLevels)
                {
                    title.enabled = false;

                    state = TUTORIAL;
                    spawnEnemy(1, tutorial[currSubWave], tutorialVelocity);
                    currSubWave++;
                    prevTime = currentTime;
                }
                break;
            case TUTORIAL:
                if (currSubWave == 4)
                {
                    Enemy[] enemies = FindObjectsOfType<Enemy>();
                    if (enemies.Length == 0)
                    {
                        state = BETWEEN_LEVEL;
                        currLevel++;

                        title.enabled = true;
                        title.text = "Wave " + currLevel;

                        prevTime = currentTime;
                    }
                    return;
                }

                if (currentTime - prevTime >= timeBetweenSubWaves)
                {
                    spawnEnemy(1, tutorial[currSubWave], tutorialVelocity);
                    currSubWave++;
                    prevTime = currentTime;
                }
                break;
            case WAVE:
                bool lastWave = currWave == levels[currLevel].Length; // currWave = length (out of bounds)
                bool lastSubWave = currSubWave == levels[currLevel][currWave - 1].GetLength(0);

                if (lastWave && lastSubWave)
                {
                    Enemy[] enemies = FindObjectsOfType<Enemy>();
                    if (enemies.Length == 0)
                    {
                        // next level
                        state = BETWEEN_LEVEL;
                        currLevel++;
                        if (currLevel == 4)
                        {
                            timeBetweenWaves = 5f;
                        } else
                        {
                            timeBetweenWaves = 3f;
                        }

                        if (currLevel == levels.Length)
                        {
                            state = WIN;

                            title.enabled = true;
                            title.text = "You Win!";

                            backgroundMusic.Stop();
                            audioPlayer.playAudio(winSoundEffect);

                            return;
                        }

                        title.enabled = true;
                        title.text = "Wave " + currLevel;

                        prevTime = currentTime;
                    }
                    return;
                }

                //bool lastSubWave = currSubWave == levels[currLevel][currWave - 1].GetLength(0);
                if (lastSubWave)
                {
                    prevTime = currentTime;
                    state = BETWEEN_WAVE;
                    return;
                }

                if (currentTime - prevTime >= timeBetweenSubWaves)
                {
                    spawnSubWave(currLevel, currWave, currSubWave);
                    currSubWave++;
                    prevTime = currentTime;
                }
                break;
            case BETWEEN_WAVE:
                if (currentTime - prevTime >= timeBetweenWaves)
                {
                    currWave++;
                    state = WAVE;

                    currSubWave = 0;
                    spawnSubWave(currLevel, currWave, currSubWave);
                    currSubWave++;

                    prevTime = currentTime;
                }
                break;
            case BETWEEN_LEVEL:
                if (currentTime - prevTime >= timeBetweenLevels)
                {
                    state = WAVE;

                    title.enabled = false;

                    currWave = 1;
                    currSubWave = 0;
                    spawnSubWave(currLevel, currWave, currSubWave);
                    currSubWave++;
                    prevTime = currentTime;
                }
                break;
            case WIN:
                return;
            case LOSE:
                return;
        }

        //title.text = currLevel + " " + currWave + " " + currSubWave + " State: " + state;
        //title.enabled = true;
    }

    /*
    private void spawnWave(int levelNum, int waveNum)
    {
        int[,] wave = levels[levelNum][waveNum - 1];
        float levelVelocity = velocities[levelNum];

        for (int i = 0; i < wave.GetLength(0); i++) // Iterate over rows
        {
            int numEnemies = wave[i, 0];
            int numShapes = wave[i, 1];

            for (int j = 0; j < numEnemies; j++)
            {
                GameObject enemy = spawnEnemy(numShapes, levelVelocity);
            }
        }
    }
    */

    private void spawnSubWave(int levelNum, int waveNum, int subWaveNum)
    {
        float levelVelocity = velocities[levelNum];

        int[,] wave = levels[levelNum][waveNum - 1];
        int numEnemies = wave[subWaveNum, 0];
        int numShapes = wave[subWaveNum, 1];

        for (int j = 0; j < numEnemies; j++)
        {
            GameObject enemy = spawnEnemy(numShapes, RANDOM_SHAPE, levelVelocity);
        }
    }

    private GameObject spawnEnemy(int numShapes, int shapeNum, float levelVelocity)
    {
        float[] xBounds = { -7, 7 };
        //float[] centerSpawnBounds = { -9.5f, 9.5f };
        float[] yBounds = { 2.5f, 3.5f };

        float randX = Random.Range(xBounds[0], xBounds[1]);
        //if (centerSpawnBounds[0] < randX && randX < centerSpawnBounds[1])
        //{
        //yBounds = new float[] { 5.7f, 6.1f };
        //}
        float randY = Random.Range(yBounds[0], yBounds[1]);

        Vector3 spawnPosition = new Vector3(randX, randY, 0);
        GameObject enemy = Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);

        Sprite goblinSprite = goblinSprites[Random.Range(0, goblinSprites.Length)];

        Enemy enemyScript = enemy.GetComponent<Enemy>();
        enemyScript.Initialize(numShapes, shapeNum, levelVelocity, goblinSprite);

        return enemy;
    }

    public void loseGame()
    {
        state = LOSE;

        GameObject[] allObjects = FindObjectsOfType<GameObject>(); ;
        foreach (GameObject obj in allObjects)
        {
            if (obj.GetComponent<Enemy>() != null)
            {
                Destroy(obj);
            }
        }

        title.enabled = true;
        title.text = "You Lose!";

        backgroundMusic.Stop();
        audioPlayer.playAudio(lossSoundEffect);
    }
}