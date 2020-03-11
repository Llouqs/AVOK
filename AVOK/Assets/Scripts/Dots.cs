using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dots : MonoBehaviour
{
    private BoardManager board;
    public int targetX;
    public int targetY;
    void Start()
    {
        board = FindObjectOfType<BoardManager>();
        targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
    }

    private void OnMouseDown()
    {
        
    }
    void Update()
    {
        
    }
}
