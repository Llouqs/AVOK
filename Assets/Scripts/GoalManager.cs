using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Goal
{
    public int neededNumber;
    public int collectedNumber;
    public Sprite goalSprite;
    public DotKind dotKind;

    public bool IsCompleted()
    {
        return collectedNumber >= neededNumber;
    }

    public void Complete()
    {
        collectedNumber = neededNumber;
    }
}

public class GoalManager : MonoBehaviour
{
    public Goal[] levelGoals;
    public GameObject goalPrefab;
    public GameObject goalGameParent;
    public List<GoalPanel> currentGoals = new List<GoalPanel>();

    public void Start()
    {
        LevelGoalsSetup();
    }

    private void LevelGoalsSetup()
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            GameObject goal = Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalGameParent.transform);
            GoalPanel goalPanel = goal.GetComponent<GoalPanel>();
            goalPanel.Setup(levelGoals[i]);
            currentGoals.Add(goalPanel);
        }
    }

    public void ChangeGoalsScores(List<Dot> dots)
    {
        foreach (Goal goal in levelGoals)
        {
            if (!goal.IsCompleted()) // Проверяем, не завершена ли уже цель
            {
                foreach (Dot dot in dots)
                {
                    if (dot.dotKind == goal.dotKind)
                    {
                        goal.collectedNumber++;
                    }
                }
                if (goal.IsCompleted())
                {
                    goal.Complete();
                }
            }
        }
        UpdateGoalUI();
    }

    private void UpdateGoalUI()
    {
        foreach (GoalPanel goalPanel in currentGoals)
        {
            goalPanel.UpdateText();
        }
    }
}
