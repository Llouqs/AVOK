using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public Sprite layer_one, layer_two;
    void OnMouseDown()
    {
        GetComponent<SpriteRenderer>().sprite = layer_two;
    }
    void OnMouseUp()
    {
        GetComponent<SpriteRenderer>().sprite = layer_one;
    }
    private void OnMouseUpAsButton()
    {
        switch (gameObject.name)
        {
            case "Play":
                SceneManager.LoadScene("InGame");
                break;
        }
    }
}
