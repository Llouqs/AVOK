using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BoardManager : MonoBehaviour
{
    public GameObject tilePrefab; //префаб (элемент в общем виде)
    public GameObject[] dotsPrefab;
    public int width, height; //размер поля в элементах
    public GameObject[,] allTiles; //массив префабов (считать с левого нижнего угла по столбцу вверх, потом вправо и т.д.)
    public GameObject[,] allDots;
    private List<Vector2Int> chain;

    private void Start()
    {
        chain = new List<Vector2Int>();
        allTiles = new GameObject[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }

    private void NewTile(int i, int j)
    {
        Vector2 tempPosition = new Vector2(i, j);
        GameObject tile = Instantiate(tilePrefab, tempPosition, Quaternion.identity);
        tile.transform.parent = transform;
        tile.name = "( Tile: " + i + ", " + j + " )";
        allTiles[i, j] = tile;
    }

    private void NewDot(int i, int j)
    {
        Vector2 tempPosition = new Vector2(i, j);
        int dotToUse = Random.Range(0, dotsPrefab.Length);
        GameObject dot = Instantiate(dotsPrefab[dotToUse], tempPosition, Quaternion.identity);
        dot.transform.parent = transform;
        dot.name = "( Dot: " + i + ", " + j + " )";
        allDots[i, j] = dot;
    }

    private void SetUp()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                NewTile(i, j);
                NewDot(i, j);
            }
        }
    }

    public void DestroyChain()
    {
        if (chain.Count>2)
        {
            foreach (Vector2Int element in chain)
            {
                int x = element.x;
                int y = element.y;
                if (allDots[x, y].GetComponent<Dot>().GetState())
                {
                    Destroy(allDots[x, y]);
                    Debug.Log("Desrtoyed: (" + x + ", " + y + ")");
                    allDots[x, y] = null;
                }
            }
        }
    }

    public void UpdateBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    List<GameObject> dotsForFall = new List<GameObject>();
                    for (int c = j; c < height; c++)
                    {
                        if ((allDots[i, c] != null))
                        {
                            dotsForFall.Add(allDots[i, c]);
                        }
                    }

                    int k = j;
                    foreach (GameObject obj in dotsForFall)
                    {
                        Vector3 tmpVector = new Vector3(i, k, 0);
                        Debug.Log("Vector2 (" + i + ", " + k + ")");
                        allDots[i, k] = obj;
                        allDots[i, k].transform.position = tmpVector;
                        k++;
                    }
                    for(int u = k; u < height; u++)
                    {
                        NewDot(i, u);
                    }
                }
            }
        }
    }

    public void AddToChain(Vector2Int obj)
    {
        if (!chain.Contains(obj))
        chain.Add(obj);
    }

    public void ChainClear()
    {
        foreach(Vector2Int element in chain)
        {
            int x = element.x;
            int y = element.y;
            if (allDots[x, y].GetComponent<Dot>().GetState())
                allDots[x, y].GetComponent<Dot>().SetSelected();
        }
            chain.Clear();
    }

    public int GetChainCount()
    {
        return chain.Count;
    }
}

