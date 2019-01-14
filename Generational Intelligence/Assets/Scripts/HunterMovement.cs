using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterMovement : MonoBehaviour
{
    // Publics:
    [Range(200, 1000)]
    int lifeSpan = 500;
    [Range(2f, 20f)]
    public float hunterSpeed = 12f;

    // Privates:
    GameObject startPosition;
    GameObject goal;
    CircleCollider2D cc2d;
    Vector2[] directions;
    bool isDead;
    int numMovesMade;
    int moveDied;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = GameObject.Find("StartPosition");
        goal = GameObject.Find("Goal");
        cc2d = GetComponent<CircleCollider2D>();

        isDead = false;
        numMovesMade = 0;
        moveDied = 0;

        directions = new Vector2[lifeSpan];
        for (int i = 0; i < lifeSpan; i++)
        {
            float x = Random.value > 0.5 ? -Random.value : Random.value;
            float y = Random.value > 0.5 ? -Random.value : Random.value;
            directions[i] = new Vector2(x, y);
        }

        transform.position = startPosition.transform.position;
    }

    void inheritGenes(Vector2[] parent)
    {
        // Inherit first 33% of genes:
        for (int i = 0; i < parent.Length / 3; i++)
        {
            directions[i] = parent[i];
        }

        // Then, 50% chance of inheriting next 33%:
        for (int i = parent.Length / 3; i < parent.Length * 2 / 3; i++)
        {
            directions[i] = Random.value > 0.5 ? parent[i] : directions[i];
        }

        // Then, 25% change of inheriting last 33%:
        for (int i = parent.Length * 2 / 3; i < parent.Length; i++)
        {
            directions[i] = Random.value > 0.67 ? parent[i] : directions[i];
        }

        // Therefore, ~67% of the parent is copied over. This feels high ...
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        transform.Translate(directions[numMovesMade] * hunterSpeed * Time.deltaTime);
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
        Debug.Log("colided");
    }
}
