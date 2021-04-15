using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    public GameObject tilePrefab; //префаб (элемент в общем виде)
    public GameObject[] dotsPrefab;
    public int width, height; //размер поля в элементах
    public GameObject[,] allTiles; //массив префабов (считать с левого нижнего угла по столбцу вверх, потом вправо и т.д.)
    public GameObject[,] allDots;
    private List<Vector2Int> chain;
    private int scores = 0;
    private int bestScores = 0;
    public GameObject scoresText;
    public GameObject bestScoresText;

    private void Start()
    {
        if (PlayerPrefs.HasKey("bestScoresKey"))
        {
            bestScores = PlayerPrefs.GetInt("bestScoresKey");
            bestScoresText.GetComponent<Text>().text = bestScores.ToString();
        }
        else{
            bestScores = 0;
            PlayerPrefs.SetInt("bestScoresKey", 0);
            PlayerPrefs.Save();
        }

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
        if (chain.Count <= 2) return;
        scores += chain.Count;
        
        if (chain.Count >= 7)
        {
            if (chain.Count >= 10)
            {
                scores += 30;
                DoubleBoom(chain[chain.Count - 1]);
            }
            else
            {
                scores += 10;
                Boom(chain[chain.Count - 1]);
            }
        }

        scoresText.GetComponent<Text>().text = scores.ToString();
        if (scores > bestScores) {
            bestScoresText.GetComponent<Text>().text = scores.ToString();
            PlayerPrefs.SetInt("bestScoresKey", Convert.ToInt32(scores.ToString()));
            PlayerPrefs.Save();
        }
        foreach (Vector2Int element in chain) {
            int x = element.x;
            int y = element.y;
            if (allDots[x, y].GetComponent<Dot>().GetState())
            {
                Debug.Log("Desrtoyed: (" + x + ", " + y + ")");
                Destroy(allDots[x, y]);
                allDots[x, y] = null;
            }
        }
    }

    private void Boom(Vector2Int boomElement)
    {
        int x = boomElement.x;
        int y = boomElement.y;
        if (x > 0)
        {
            AddToChain(new Vector2Int(x - 1, y));
            allDots[x - 1, y].GetComponent<Dot>().SetState(true);
        }
        if (x < width - 1)
        {
            AddToChain(new Vector2Int(x+1, y));
            allDots[x + 1, y].GetComponent<Dot>().SetState(true);
        }

        if (y > 0)
        {
            AddToChain(new Vector2Int(x, y-1));
            allDots[x, y-1].GetComponent<Dot>().SetState(true);
        }
        if (y < height - 1)
        {
            AddToChain(new Vector2Int(x, y+1));
            allDots[x, y+1].GetComponent<Dot>().SetState(true);
        }
    }
    
    private void DoubleBoom(Vector2Int boomElement)
    {
        int x = boomElement.x;
        int y = boomElement.y;
        if (x > 0)
        {
            AddToChain(new Vector2Int(x - 1, y));
            allDots[x - 1, y].GetComponent<Dot>().SetState(true);
            Boom(new Vector2Int(x - 1, y));
        }
        if (x < width - 1)
        {
            AddToChain(new Vector2Int(x+1, y));
            allDots[x + 1, y].GetComponent<Dot>().SetState(true);
            Boom(new Vector2Int(x + 1, y));
        }

        if (y > 0)
        {
            AddToChain(new Vector2Int(x, y-1));
            allDots[x, y-1].GetComponent<Dot>().SetState(true);
            Boom(new Vector2Int(x, y-1));
        }
        if (y < height - 1)
        {
            AddToChain(new Vector2Int(x, y+1));
            allDots[x, y+1].GetComponent<Dot>().SetState(true);
            Boom(new Vector2Int(x, y+1));
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
                        StartCoroutine(DownSlowFall(allDots[i, k], tmpVector));
                        //allDots[i, k].transform.position = tmpVector;
                        k++;
                    }
                    for(int u = k; u < height; u++)
                    {
                        //NewDot(i, u);
                        Vector3 tempPosition = new Vector3(i, u+5, 0); //тут падают всем блоком недостающих
                        int dotToUse = Random.Range(0, dotsPrefab.Length);
                        GameObject dot = Instantiate(dotsPrefab[dotToUse], tempPosition, Quaternion.identity);
                        dot.transform.parent = transform;
                        dot.name = "( Dot: " + i + ", " + u + " )";
                        allDots[i, u] = dot;
                        StartCoroutine(DownSlowFall(allDots[i, u], new Vector3(i, u, 0)));
                    }
                }
            }
        }
    }
    
    private IEnumerator DownSlowFall(GameObject obj, Vector3 tmpVector)
    {
        float yOffset = obj.transform.position.y - tmpVector.y;
        for (float q = 0; q <= yOffset; q += .1f)
        {
            obj.transform.position += new Vector3(0, -0.1f, 0);
            yield return new WaitForSeconds(.007f);
        }
        obj.transform.position = tmpVector;
    }

    public void AddToChain(Vector2Int obj)
    {
        if (!chain.Contains(obj))
            chain.Add(obj);
    }

    public void RemoveFromChainLast()
    {
        chain.RemoveAt(chain.Count-1);
    }

    public bool IsPreviosDot(Vector2Int dotPos)
    {
        return chain.IndexOf(dotPos) == chain.Count - 2 ? true : false;
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

