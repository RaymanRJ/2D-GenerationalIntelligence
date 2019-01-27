using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Publics:
    [Range(0.0001f, 1)]
    public float mutationRate = 0.01f;
    public GameObject hunterPrefab;
    public Text generationText;
    public Text timeText;
    public static List<GameObject> hunters;
    public static List<GameObject> winningHunters;
    public static List<HunterMovement> winningHunterMovements;

    //[Range(1, 200)]
    int initialHunters = 100;
    int hours = 0;
    int minutes = 0;
    int seconds = 0;
    int milliseconds = 0;
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
        generationText.text = "Generation " + generation;
        timeText.text = "Time = " + hours + ":" + minutes + ":" + seconds + "." + milliseconds;
    }

    // Update is called once per frame
    void Update()
    {
        timeText.text = GetTime();

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

    string GetTime()
    {
        float time = Time.realtimeSinceStartup;
        int seconds = (int) time % 60;
        int minutes = (int) (time / 60) % 60;
        int hours = (int) (time / (60 * 60)) % 24;

        return (hours < 10 ? "0" + hours : hours.ToString()) + ":"
            + (minutes < 10 ? "0" + minutes : minutes.ToString()) + ":"
            + (seconds < 10 ? "0" + seconds : seconds.ToString());
    }

    void ResetLevel()
    {
        hunterMovements[0].CopyGenes(GetBestParent().GetGenes());
        hunterMovements[0].Reset();
        hunterMovements[0].GetComponent<SpriteRenderer>().color = Color.red;

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
            hunterMovements[i].GetComponent<SpriteRenderer>().color = Color.white;
        }

        // Remaining 75% will be heavily influenced by Best, but will also mix with the remaining potentials.
        for (int i = hunterMovements.Count / 4; i < hunterMovements.Count; i++)
        {
            hunterMovements[i].Reset();
            hunterMovements[i].Mutate(mutationRate, hunterMovements[0], potentialParents);
            hunterMovements[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
        
        generation++;
        generationText.text = "Generation " + generation;
        completedGeneration = false;
        averageFitness = 0;
        potentialParents.Clear();
        winningHunters.Clear();
        winningHunterMovements.Clear();
        Debug.Log("Generation " + generation);
    }

    HunterMovement GetBestParent()
    {
        HunterMovement toReturn = null;
        float fitness = HunterMovement.lifeSpan;

        // If someone won, their fitness = moves made. The less, the better.
        if (winningHunters.Count != 0)
        {
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

        foreach(HunterMovement hm in hunterMovements)
        {
            if (hm.IsTouchingBorder())
                continue;

            averageFitness += hm.GetFitness();
            if (hm.GetFitness() < fitness)
            {
                fitness = hm.GetFitness();
                toReturn = hm;
            }
        }

        averageFitness /= hunterMovements.Count;
        return toReturn == null ? hunterMovements[0] : toReturn;
    }
}
