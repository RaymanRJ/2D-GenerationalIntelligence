using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class HunterMovement : MonoBehaviour
{
    // Publics:
    //[Range(200, 3000)]
    public static int lifeSpan = 1000;
    [Range(2f, 20f)]
    public float hunterSpeed = 10f;

    // Privates:
    GameObject startPosition;
    GameObject goal;
    CircleCollider2D cc2d;
    Vector2[] genes;
    Vector2 startPos;
    bool isDead;
    bool touchedGoal;
    bool touchedBorder;
    int numMovesMade;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = GameObject.Find("StartPosition");
        goal = GameObject.Find("Goal");
        cc2d = GetComponent<CircleCollider2D>();

        isDead = false;
        touchedGoal = false;
        touchedBorder = false;
        numMovesMade = 0;

        genes = new Vector2[lifeSpan];
        for (int i = 0; i < lifeSpan; i++)
        {
            genes[i] = NewGene();
        }

        SetPosition();
    }

    private void SetPosition()
    {
        float widthRange = Random.value * startPosition.GetComponent<SpriteRenderer>().bounds.size.x / 2;
        float widthHeight = Random.value * startPosition.GetComponent<SpriteRenderer>().bounds.size.y / 2;

        widthRange = Random.value > 0.5 ? -widthRange : widthRange;
        widthHeight = Random.value > 0.5 ? -widthHeight : widthHeight;

        startPos = new Vector2(startPosition.transform.position.x + widthRange, startPosition.transform.position.y + widthHeight);
        transform.position = startPos;

    }

    public void inheritGenes(Vector2[] parent)
    {
        
        // Inherit 50% of the parent of genes:
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
    public int GetGenesUsed() { return this.numMovesMade; }
    public bool IsTouchingGoal() { return this.touchedGoal; }
    public bool IsTouchingBorder() { return this.touchedBorder; }

    public void Reset()
    {
        isDead = false;
        touchedBorder = false;
        numMovesMade = 0;

        SetPosition();
    }

    public float GetFitness()
    {
        // Noone here touched the goal, else this wouldn't have been reached.
        return Mathf.Abs(Vector2.Distance(this.transform.position, goal.transform.position));
    }

    public void Mutate(float mutationRate, HunterMovement bestParent)
    {
        for(int i = 0; i < lifeSpan; i++)
        {
            genes[i] = Random.value > mutationRate ? bestParent.genes[i] : NewGene();
        }
    }

    public void Mutate(float mutationRate, HunterMovement bestParent, List<HunterMovement> potentialParents)
    {
        if(potentialParents.Count == 0)
        {
            // Should never really be here.
            for(int i = 0; i < lifeSpan; i++)
                genes[i] = Random.value > 1- mutationRate ? bestParent.genes[i] : NewGene();
            return;
        }

        HunterMovement parent1, parent2;

        // 50% chance parent1 = best
        parent1 = Random.value > .5f ? bestParent : potentialParents[(int)Mathf.Clamp(Random.value * potentialParents.Count,0, potentialParents.Count)];

        // Parent 2 is random from the potentials
        parent2 = potentialParents[(int)Mathf.Clamp(Random.value * potentialParents.Count, 0, potentialParents.Count)];

        // 50/50 from the two parents, with added potential mutation:

        for(int i = 0; i < lifeSpan; i++)
        {
            if (i % 2 == 0)
                genes[i] = parent1.genes[i];
            else
                genes[i] = parent2.genes[i];

            genes[i] = Random.value < 1 - mutationRate ? bestParent.genes[i] : NewGene();
        }
    }

    private Vector2 NewGene()
    {
        float x = Random.value > 0.5 ? -Random.value : Random.value;
        float y = Random.value > 0.5 ? -Random.value : Random.value;
        return new Vector2(x, y).normalized;
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
        float distance = 1/Mathf.Pow(Vector2.Distance(goal.transform.position, transform.position), 2);
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
        else if (collision.tag == "Border")
            touchedBorder = true;
        else if (collision.tag == "Finish")
        {
            touchedGoal = true;
            GameManager.winningHunters.Add(gameObject);
            GameManager.winningHunterMovements.Add(this);
        }

        isDead = true;
    }
}
