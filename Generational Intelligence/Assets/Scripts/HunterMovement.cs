using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterMovement : MonoBehaviour
{
    // Publics:
    //[Range(200, 3000)]
    int lifeSpan = 100;
    [Range(2f, 20f)]
    public float hunterSpeed = 10f;

    // Privates:
    GameObject startPosition;
    GameObject goal;
    CircleCollider2D cc2d;
    Vector2[] genes;
    Vector2 startPos;
    bool isDead;
    int numMovesMade;
    int moveDied;

    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        startPosition = GameObject.Find("StartPosition");
        goal = GameObject.Find("Goal");
        cc2d = GetComponent<CircleCollider2D>();

        isDead = false;
        numMovesMade = 0;
        moveDied = 0;

        genes = new Vector2[lifeSpan];
        for (int i = 0; i < lifeSpan; i++)
        {
            float x = Random.value > 0.5 ? -Random.value : Random.value;
            float y = Random.value > 0.5 ? -Random.value : Random.value;
            genes[i] = new Vector2(x, y);
        }

        float widthRange = Random.value * startPosition.GetComponent<SpriteRenderer>().bounds.size.x/2;
        float widthHeight = Random.value * startPosition.GetComponent<SpriteRenderer>().bounds.size.y/2;

        widthRange = Random.value > 0.5 ? -widthRange : widthRange;
        widthHeight = Random.value > 0.5 ? -widthHeight : widthHeight;

        startPos = new Vector2(startPosition.transform.position.x + widthRange, startPosition.transform.position.y + widthHeight);
        transform.position = startPos;
    }

    public void inheritGenes(Vector2[] parent)
    {
        
        // Inherit 50% of the first 50% of genes:
        for (int i = 0; i < parent.Length; i++)
        {
            genes[i] = Random.value > 0.5 ? parent[i] : genes[i];
        }
        /*
        // Then, 10% chance of inheriting next 67%:
        for (int i = parent.Length / 2; i < parent.Length; i++)
        {
            genes[i] = Random.value > 0.9 ? parent[i] : genes[i];
        }
        /*
        // Then, 25% change of inheriting last 33%:
        for (int i = parent.Length * 2 / 3; i < parent.Length; i++)
        {
            genes[i] = Random.value > 0.67 ? parent[i] : genes[i];
        }
        */
        // Therefore, ~67% of the parent is copied over. This feels high ...
        /*
        // Testing --
        for(int i = 0; i < parent.Length; i++)
        {
            genes[i] = parent[i];
        }

        for(int i = 0; i < parent.Length; i++)
        {
            genes[i] = Random.value > 0.5 ? genes[i] : parent[i];
        }*/

        // 50% of all genes:
    }

    public Vector2 GetStartPos() { return this.startPos; }
    public bool IsDead() { return this.isDead; }
    public Vector2[] GetGenes() { return this.genes; }
    public void CopyGenes(Vector2[] genes) { this.genes = genes; }

    public void Reset()
    {
        isDead = false;
        numMovesMade = 0;
        moveDied = 0;

        transform.position = startPos;
    }

    public Vector2[] GetUsedGenes()
    {
        Vector2[] toReturn = new Vector2[numMovesMade + 1];
        for (int i = 0; i < numMovesMade; i++)
            toReturn[i] = genes[i];
        return toReturn;
    }

    public float Fitness()
    {
        // Soooo .. the closer you get, in the fewest moves, yields a higher fitness.
        float distance = Mathf.Abs(Vector2.Distance(transform.position, goal.transform.position));
        //float fitness = distance / numMovesMade;
        return distance;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        transform.Translate(genes[numMovesMade] * hunterSpeed * Time.deltaTime);
        numMovesMade++;

        CheckDeath();
    }

    void CheckDeath()
    {
        if (numMovesMade == lifeSpan)
            isDead = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Hunter")
            return;

        isDead = true;
    }
}
