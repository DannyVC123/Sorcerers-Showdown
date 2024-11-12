using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    public static int LEFT = -1, RIGHT = 1;

    private SpriteRenderer sr;

    float velocity = 8f;
    float scale = 0.5f;

    private HealthTracker healthTracker;
    private EnemySpawner enemySpawner;

    private AudioPlayer audioPlayer;

    public AudioClip damageSoundEffect;
    public AudioClip lossSoundEffect;

    public Sprite[] idleCycle;
    public Sprite[] runCycle;

    private float timeBetweenFrames = 0.1f;
    private int currFrame = 0;
    private float prevFrameTime = 0f;

    private bool running = false;
    private int direction = LEFT;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        healthTracker = FindObjectOfType<HealthTracker>();
        enemySpawner = FindObjectOfType<EnemySpawner>();

        audioPlayer = FindObjectOfType<AudioPlayer>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Check for key press
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            direction = LEFT;
            transform.localScale = new Vector3(-1 * scale, scale, scale);
            running = true;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            direction = RIGHT;
            transform.localScale = new Vector3(scale, scale, scale);
            running = true;
        }

        // Check for key release
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A) ||
            Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
        {
            sr.sprite = runCycle[0]; // Set to initial frame
            running = false;
        }
    }

    private void FixedUpdate()
    {
        if (running)
        {
            Run();
        } else
        {
            Idle();
        }
    }

    private void Idle()
    {
        float currentTime = Time.time;
        if (currentTime - prevFrameTime >= timeBetweenFrames)
        {
            currFrame = (currFrame + 1) % idleCycle.Length;
            sr.sprite = idleCycle[currFrame];
            prevFrameTime = currentTime;
        }
    }

    private void Run()
    {
        transform.position += new Vector3(direction * velocity * Time.fixedDeltaTime, 0, 0);

        float currentTime = Time.time;
        if (currentTime - prevFrameTime >= timeBetweenFrames)
        {
            currFrame = (currFrame + 1) % runCycle.Length;
            sr.sprite = runCycle[currFrame];
            prevFrameTime = currentTime;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Enemy>() == null)
        {
            return;
        }
        Destroy(collision.gameObject);

        int hpLeft = healthTracker.LoseLife();
        audioPlayer.playAudio(damageSoundEffect);
        if (hpLeft == 0)
        {
            enemySpawner.loseGame();

        }
    }
}
