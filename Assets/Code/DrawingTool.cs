using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingController : MonoBehaviour
{
    public static int UNKOWN = 0, HORIZ = 1, VERT = 2, DOWN_V = 3, UPWARD_V = 4;
    public static Color[] colors =
    {
        Color.white,
        Color.red,
        Color.blue,
        Color.yellow,
        Color.green
    };

    //private bool running = false;
    private bool drawing = false;

    public GameObject circlePrefab;
    private float circleDist = 0.1f;

    private List<GameObject> circles = new List<GameObject>();
    private List<Vector3> points = new List<Vector3>();
    private int lastColoredCircle = -1;

    private Vector3 lastMousePosition;

    private float minX = float.PositiveInfinity;
    private float maxX = float.NegativeInfinity;
    private float minY = float.PositiveInfinity;
    private float maxY = float.NegativeInfinity;

    private float currentShape = -1;

    private ScoreKeeper scoreKeeper;
    private AudioPlayer audioPlayer;

    public AudioClip attackSoundEffect;

    void Start()
    {
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        audioPlayer = FindObjectOfType<AudioPlayer>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Draw();
            if (circles.Count > 30)
            {
                Calculate();
            }
        }

        
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            int shape = Calculate();
            AttackEnemies(shape);
            Clear();
        }
    }

    private void Draw()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        if (mousePosition == lastMousePosition)
        {
            return;
        }

        GameObject circle = Instantiate(circlePrefab, mousePosition, Quaternion.identity);

        circles.Add(circle);
        points.Add(mousePosition);

        minX = Mathf.Min(mousePosition.x, minX);
        maxX = Mathf.Max(mousePosition.x, maxX);
        minY = Mathf.Min(mousePosition.y, minY);
        maxY = Mathf.Max(mousePosition.y, maxY);

        if (drawing && Vector3.Distance(mousePosition, lastMousePosition) > circleDist)
        {
            AddCirclesBetween(mousePosition, lastMousePosition);
        }

        lastMousePosition = mousePosition;
        drawing = true;
    }

    private void AddCirclesBetween(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        int steps = Mathf.CeilToInt(distance / circleDist);

        for (int i = 0; i < steps; i++)
        {
            Vector3 interpolatedPosition = Vector3.Lerp(start, end, (float) i / steps);
            interpolatedPosition.z = 0;

            GameObject circle = Instantiate(circlePrefab, interpolatedPosition, Quaternion.identity);
            circles.Add(circle);
            points.Add(interpolatedPosition);

            minX = Mathf.Min(interpolatedPosition.x, minX);
            maxX = Mathf.Max(interpolatedPosition.x, maxX);
            minY = Mathf.Min(interpolatedPosition.y, minY);
            maxY = Mathf.Max(interpolatedPosition.y, maxY);
        }
    }

    private int Calculate()
    {
        if (points.Count == 0)
        {
            return UNKOWN;
        }

        int shape = RecognizeShape(points[0], lastMousePosition);
        if (shape == currentShape)
        {
            Recolor(shape, lastColoredCircle + 1);

        }
        else
        {
            Recolor(shape, 0);
        }

        return shape;
    }

    private int RecognizeShape(Vector3 startPos, Vector3 endPos)
    {
        float lineTolerance = 0.2f;
        float dx = maxX - minX, dy = maxY - minY;
        float xLineTolerance = dy * lineTolerance, yLineTolerance = dx * lineTolerance;

        // horizontal line
        if (dy < yLineTolerance)
        {
            return HORIZ;
        }

        //vertival line
        if (dx < xLineTolerance)
        {
            return VERT;
        }

        float vTolerance = 0.5f;
        float yVTolerance = dx * vTolerance;

        float startEnd_dy = Mathf.Abs(endPos.y - startPos.y);
        float largeYTolerance = dx * 1.5f;

        float endpointTolerance = dy * 0.1f;

        // down v
        bool startEndMax = Mathf.Abs(startPos.y - maxY) < endpointTolerance || Mathf.Abs(endPos.y - maxY) < endpointTolerance;
        bool downV = false;
        if (startEndMax && startEnd_dy < largeYTolerance && maxY - minY > yVTolerance)
        {
            downV = true;
        }

        // upward v
        bool startEndMin = Mathf.Abs(startPos.y - minY) < endpointTolerance || Mathf.Abs(endPos.y - minY) < endpointTolerance;
        bool upwardV = false;
        if (startEndMin && startEnd_dy < largeYTolerance && maxY - minY > yVTolerance)
        {
            upwardV = true;
        }

        if (downV == upwardV)
        {
            return UNKOWN;
        }
        else
        {
            return downV ? DOWN_V : UPWARD_V;
        }
    }

    private void Recolor(int shape, int startFrom)
    {
        for (int i = startFrom; i < circles.Count; i++)
        {
            SpriteRenderer sr = circles[i].GetComponent<SpriteRenderer>();
            sr.color = colors[shape];
        }

        lastColoredCircle = circles.Count - 1;
    }

    private void AttackEnemies(int shape)
    {
        if (shape == UNKOWN)
        {
            return;
        }

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        int enemiesDamaged = 0;
        foreach (var enemy in enemies)
        {
            bool damaged = enemy.Damage(shape);
            if (damaged)
            {
                enemiesDamaged++;
            }
        }

        if (enemiesDamaged > 0)
        {
            audioPlayer.playAudio(attackSoundEffect);
            scoreKeeper.addScore(enemiesDamaged);
        }
    }

    private void Clear()
    {
        drawing = false;

        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        minY = float.PositiveInfinity;
        maxY = float.NegativeInfinity;

        foreach (var circle in circles)
        {
            Destroy(circle);
        }
        circles.Clear();
        points.Clear();
    }
}
