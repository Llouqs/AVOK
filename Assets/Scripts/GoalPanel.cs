using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalPanel : MonoBehaviour
{
    public Image thisImage;
    public TextMeshProUGUI thisText;
    public Sprite completedSprite;

    private Goal goal;
    public void Setup(Goal goal)
    {
        this.goal = goal;
        thisImage.sprite = goal.goalSprite;
        UpdateText();
    }

    public void UpdateText()
    {
        if (goal.IsCompleted())
        {
            thisText.text = "ok!";
        }
        else
        {
            thisText.text = $"{goal.neededNumber - goal.collectedNumber}";
        }
    }

    public Goal GetGoal()
    {
        return goal;
    }
}
