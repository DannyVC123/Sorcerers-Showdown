using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

public class Enemy : MonoBehaviour
{
    public static int RANDOM_SHAPE = -1;

    private Transform player;

    public GameObject shapePrefab;
    public Sprite[] shapes;

    private float shapeWidth = -1;
    private float shapeYOffset = 0.85f;

    private Queue<GameObject> currentShapes = new Queue<GameObject>();
    private Queue<int> currentShapeNums = new Queue<int>();

    private Rigidbody2D rb;
    private float velocity = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>().transform;
        rb = GetComponent<Rigidbody2D>();

        RenderShapes();
    }

    public void Initialize(int numShapes, int shapeNum, float velocity, Sprite goblinSprite)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = goblinSprite;

        generateShapes(numShapes, shapeNum);
        RenderShapes();

        this.velocity = velocity;
    }

    private void generateShapes(int numShapes, int shapeNum)
    {
        for (int i = 0; i < numShapes; i++)
        {
            GameObject shape = Instantiate(shapePrefab);
            shape.transform.parent = transform;

            SpriteRenderer shapeSR = shape.GetComponent<SpriteRenderer>();

            int shapeNumToGenerate = shapeNum == RANDOM_SHAPE ? Random.Range(1, shapes.Length) : shapeNum;
            shapeSR.sprite = shapes[shapeNumToGenerate];
            shapeWidth = shapeSR.bounds.size.x;

            currentShapes.Enqueue(shape);
            currentShapeNums.Enqueue(shapeNumToGenerate);
        }
    }

    private void RenderShapes()
    {
        float spacing = 0f;
        float totalWidth = (currentShapes.Count - 1) * (shapeWidth + spacing);

        Vector3 leftMostCenter = transform.position + new Vector3(-totalWidth / 2f, shapeYOffset, 0);

        for (int i = 0; i < currentShapes.Count; i++)
        {
            GameObject shape = currentShapes.Dequeue();
            Vector3 shapePosition = leftMostCenter + new Vector3(i * (shapeWidth + spacing), 0, 0);
            shape.transform.position = shapePosition;
            currentShapes.Enqueue(shape);
        }
    }


    // Update is called once per frame
    void Update()
    {
        //
    }

    void FixedUpdate()
    {
        moveTowardPlayer();
    }

    private void moveTowardPlayer()
    {
        float dx = velocity * Time.fixedDeltaTime;

        if (Vector2.Distance(this.transform.position, player.position) < dx)
        {
            return;
        }

        Vector2 targetPosition = Vector2.MoveTowards(rb.position, player.position, dx);
        rb.MovePosition(targetPosition);
    }

    // true if damaged;
    public bool Damage(int shapeNum)
    {
        if (currentShapeNums.Peek() != shapeNum)
        {
            return false;
        }

        GameObject firstShape = currentShapes.Dequeue();
        currentShapeNums.Dequeue();

        Destroy(firstShape);
        if (currentShapes.Count == 0)
        {
            Destroy(gameObject);
        }

        RenderShapes();

        return true;
    }
}
