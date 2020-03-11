using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BoardManager : MonoBehaviour
{
    public GameObject tilePrefab; //префаб (элемент в общем виде)
    public GameObject[] dots;
    public int width, height; //размер поля в элементах
    public Tile[,] allTiles; //массив префабов (считать с левого нижнего угла по столбцу вверх, потом вправо и т.д.)
    public GameObject[,] allDots;
    void Start()
    {
        allTiles = new Tile[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }

    private void SetUp()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                Vector2 tempPosition = new Vector2(i, j);
                GameObject tile = Instantiate(tilePrefab, tempPosition, Quaternion.identity);
                tile.transform.parent = this.transform;
                tile.name = "( " + i + ", " + j + " )";
                int dotToUse = Random.Range(0, dots.Length);
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.transform.parent = this.transform;
                dot.name = "( " + i + ", " + j + " )";
                allDots[i, j] = dot;
            }
        }
    }
}

