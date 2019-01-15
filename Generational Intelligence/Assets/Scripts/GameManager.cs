using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Publics:
    public GameObject hunterPrefab;
    [Range(1, 200)]
    public int initialHunters = 100;
    public Transform goal;
    public Transform startPosition;

    // Privates:
    int generation;
    GameObject[] hunters;
    HunterMovement[] hunterMovements;
    bool completedGeneration;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Vector2.Distance(goal.position, startPosition.position));
        hunters = new GameObject[initialHunters];
        hunterMovements = new HunterMovement[initialHunters];
        for(int i = 0; i < initialHunters; i++)
        {
            hunters[i] = Instantiate(hunterPrefab);
            hunterMovements[i] = hunters[i].GetComponent<HunterMovement>();
        }

        generation = 0;
        completedGeneration = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (completedGeneration)
            return;

        foreach(HunterMovement hm in hunterMovements)
        {
            // If one hunter lives, keep going.
            if (!hm.IsDead())
                return;
        }

        completedGeneration = true;

        ResetLevel();
    }

    void ResetLevel()
    {
        float fitness = 0;
        int index = 0;

        foreach(HunterMovement hm in hunterMovements)
        {
            if (hm.Fitness() > fitness)
                fitness = hm.Fitness();

            index++;
        }

        Debug.Log(hunterMovements[index - 1].Fitness());

        //hunterMovements[0] = hunterMovements[index - 1];
        hunters[0].GetComponent<SpriteRenderer>().color = Color.blue;
        hunterMovements[0].Reset();
        hunterMovements[0].CopyGenes(hunterMovements[index - 1].GetGenes());
        
        for (int i = 1; i < initialHunters; i++)
        {
            // Create new ones
            hunterMovements[i].Reset();

            // Inherit from parent
            hunterMovements[i].inheritGenes(hunterMovements[0].GetGenes());
        }

        generation++;
        completedGeneration = false;
        Debug.Log("Generation = " + generation);
    }
}
