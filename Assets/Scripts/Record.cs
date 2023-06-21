using System;
using UnityEngine;
using UnityEngine.UI;

public class Record : MonoBehaviour
{
    [SerializeField] private GameObject scoresText;
    [SerializeField] private GameObject bestScoresText;
    private int _bestScores;

    private void Start()
    {
        if (PlayerPrefs.HasKey("bestScoresKey"))
        {
            _bestScores = PlayerPrefs.GetInt("bestScoresKey");
            bestScoresText.GetComponent<Text>().text = _bestScores.ToString();
        }
        else
        {
            _bestScores = 0;
            PlayerPrefs.SetInt("bestScoresKey", 0);
            PlayerPrefs.Save();
        }
    }

    public void ChangeScores(int scores)
    {
        scoresText.GetComponent<Text>().text = scores.ToString();
        if (scores <= _bestScores) return;
        bestScoresText.GetComponent<Text>().text = scores.ToString();
        PlayerPrefs.SetInt("bestScoresKey", Convert.ToInt32(scores.ToString()));
        PlayerPrefs.Save();
    }
}
