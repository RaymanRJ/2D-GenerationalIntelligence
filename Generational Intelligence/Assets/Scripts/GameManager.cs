using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Publics:
    //[Range(0.01f, 1)]
     float mutationRate = 0.1f;
    public GameObject hunterPrefab;
    public static List<GameObject> hunters;
    public static List<GameObject> winningHunters;
    public static List<HunterMovement> winningHunterMovements;

    //[Range(1, 200)]
    int initialHunters = 100;
    float averageFitness;
    public Transform goal;
    public Transform startPosition;

    // Privates:
    int generation;
    bool completedGeneration;
    private static List<HunterMovement> hunterMovements;


    // Start is called before the first frame update
    void Start()
    {
        hunters = new List<GameObject>();
        hunterMovements = new List<HunterMovement>();
        winningHunters = new List<GameObject>();
        winningHunterMovements = new List<HunterMovement>();

        for(int i = 0; i < initialHunters; i++)
        {
            hunters.Add(Instantiate(hunterPrefab));
            hunterMovements.Add(hunters[i].GetComponent<HunterMovement>());
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
        hunterMovements[0] = GetBestParent();
        hunterMovements[0].Reset();
        hunterMovements[0].GetComponent<SpriteRenderer>().color = Color.blue;

        List<HunterMovement> potentialParents = new List<HunterMovement>();
        potentialParents.AddRange(winningHunterMovements);

        foreach (HunterMovement hm in hunterMovements)
            if (hm.GetFitness() > averageFitness && !hm.IsTouchingGoal())
                potentialParents.Add(hm);

        // First 25% will be only partially different from best parent.
        for(int i = 1; i < hunterMovements.Count / 4; i++)
        {
            hunterMovements[i].Reset();
            hunterMovements[i].Mutate(mutationRate, hunterMovements[0]);
        }

        // Remaining 75% will be heavily influenced by Best, but will also mix with the remaining potentials.
        for (int i = hunterMovements.Count / 4; i < hunterMovements.Count; i++)
        {
            hunterMovements[i].Reset();
            hunterMovements[i].Mutate(mutationRate, hunterMovements[0], potentialParents);
        }

        
        generation++;
        completedGeneration = false;
        averageFitness = 0;
        potentialParents.Clear();
        winningHunters.Clear();
        winningHunterMovements.Clear();
        Debug.Log("Generation = " + generation);
    }

    HunterMovement GetBestParent()
    {
        HunterMovement toReturn = null;
        float fitness;

        // If someone won, their fitness = moves made. The less, the better;
        if (winningHunters.Count != 0)
        {
            fitness = HunterMovement.lifeSpan;
            foreach (HunterMovement hm in winningHunterMovements)
            {
                averageFitness += hm.GetGenesUsed();
                if (hm.GetGenesUsed() < fitness) { 
                    fitness = hm.GetGenesUsed();
                    toReturn = hm;
                }
            }
            averageFitness /= winningHunters.Count;
            return toReturn;
        }

        // Otherwise, use their distance from the goal:

        fitness = 100;
        foreach(HunterMovement hm in hunterMovements)
        {
            averageFitness += hm.GetFitness();
            if (hm.GetFitness() < fitness)
            {
                fitness = hm.GetFitness();
                toReturn = hm;
            }
        }

        averageFitness /= hunterMovements.Count;
        return toReturn;
    }
}
