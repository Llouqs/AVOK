using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject[] dotsPrefab;
    [SerializeField] private GameObject[] bonusesPrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject boomEffect;
    [SerializeField] private GameObject doubleBoomEffect;
    
    [SerializeField] private int width, height; //размер поля в элементах

    private int _scores = 0;
    [SerializeField] private Record recordUI;
    
    public GameObject[,] allTiles; //массив префабов (считать с левого нижнего угла по столбцу вверх, потом вправо и т.д.)
    public GameObject[,] allDots;
    public List<GameObject> allLines;
    private List<Vector2Int> _chain;
    
    private void Start()
    {
        height = width + width - 2;
        var mainCamera = FindObjectOfType<Camera>();
        mainCamera.transform.position = new Vector3((float)width / 2 - 0.5f, y: width-1,z: -10);
        mainCamera.orthographicSize = width;
        _chain = new List<Vector2Int>();
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

    private GameObject NewLine(Vector2Int from, Vector2Int to)
    {
        float z=0f;
        Vector2 tempPosition;
        Vector2Int direction = new Vector2Int(from.x-to.x, from.y-to.y);
        if (direction.x != 0)
        {
            tempPosition = direction.x == 1
                ? new Vector2(@from.x + 0.5f, @from.y)
                : new Vector2(@from.x - 0.5f, @from.y);
            if (direction.y == 1)
            {
                if (direction.x == 1)
                {
                    z = 45f;
                    tempPosition = new Vector2(@from.x + 0.5f, @from.y + 0.5f);
                }
                else
                {
                    z = -45f;
                    tempPosition = new Vector2(@from.x - 0.5f, @from.y + 0.5f);
                }
            }
            else if(direction.y == -1)
            {
                if (direction.x == 1)
                {
                    z = -45f;
                    tempPosition = new Vector2(@from.x + 0.5f, @from.y - 0.5f);
                }
                else
                {
                    z = 45f;
                    tempPosition = new Vector2(@from.x - 0.5f, @from.y - 0.5f);
                }
            }
        }
        else
        {
            tempPosition = direction.y == 1
                ? new Vector2(@from.x, @from.y + 0.5f)
                : new Vector2(@from.x, @from.y - 0.5f);
            z = 90f;
        }
        GameObject line = Instantiate(linePrefab, tempPosition-direction, Quaternion.identity);
        line.transform.parent = transform;
        line.transform.rotation*= Quaternion.Euler(0f, 0f, z);
        line.name = "( Line: " + from.x + ", " + from.y + " )";
        return line; 
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
        if (_chain.Count == 1)
        {
            return;
        }
        if (_chain.Count == 2)
        {
            Destroy(allLines[0]);
            allLines.RemoveAt(0);
            return;
        }
        _scores += _chain.Count;
        
        if (_chain.Count >= 7)
        {
            if (_chain.Count >= 10)
            {
                _scores += 30;
                var effect = Instantiate(doubleBoomEffect, new Vector2(_chain[_chain.Count - 1].x, _chain[_chain.Count - 1].y), Quaternion.identity);
                DoubleBoom(_chain[_chain.Count - 1]);
                Destroy(effect, 3000.0f);
            }
            else
            {
                _scores += 10;
                var effect = Instantiate(boomEffect, new Vector2(_chain[_chain.Count - 1].x, _chain[_chain.Count - 1].y), Quaternion.identity);
                Boom(_chain[_chain.Count - 1]);
                Destroy(effect, 3000.0f);
            }
        }

        recordUI.ChangeScores(_scores);
        foreach (Vector2Int element in _chain) {
            int x = element.x;
            int y = element.y;
            if (allDots[x, y].GetComponent<Dot>().GetState())
            {
                allDots[x, y].GetComponent<Dot>().StartEffect();
                Debug.Log("Desrtoyed: (" + x + ", " + y + ")");
                Destroy(allDots[x, y]);
                allDots[x, y] = null;
            }
        }
        foreach (var obj in allLines)
        {
            Destroy(obj);
        }
        allLines.Clear();
    }

    private void Boom(Vector2Int boomElement)
    {
        int x = boomElement.x;
        int y = boomElement.y;
        if (x > 0)
        {
            AddToChain(new Vector2Int(x - 1, y));
        }
        if (x < width - 1)
        {
            AddToChain(new Vector2Int(x+1, y));
        }
        if (y > 0)
        {
            AddToChain(new Vector2Int(x, y-1));
        }
        if (y < height - 1)
        {
            AddToChain(new Vector2Int(x, y+1));
        }
    }

    private void DoubleBoom(Vector2Int boomElement)
    {
        int x = boomElement.x;
        int y = boomElement.y;
        if (x > 0)
        {
            AddToChain(new Vector2Int(x - 1, y));
            Boom(new Vector2Int(x - 1, y));
        }
        if (x < width - 1)
        {
            AddToChain(new Vector2Int(x+1, y));
            Boom(new Vector2Int(x + 1, y));
        }
        if (y > 0)
        {
            AddToChain(new Vector2Int(x, y-1));
            Boom(new Vector2Int(x, y-1));
        }
        if (y < height - 1)
        {
            AddToChain(new Vector2Int(x, y+1));
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

                    for (int u = k; u < height; u++)
                    {
                        //NewDot(i, u);
                        Vector3 tempPosition = new Vector3(i, u + 5, 0); //тут падают всем блоком недостающих
                        GameObject dot;
                        if (Random.Range(0, 15) != 0)
                        {
                            int dotToUse = Random.Range(0, dotsPrefab.Length);
                            dot = Instantiate(dotsPrefab[dotToUse], tempPosition, Quaternion.identity);
                            dot.transform.parent = transform;
                            dot.name = "( Dot: " + i + ", " + u + " )";
                            allDots[i, u] = dot;
                        }
                        else
                        {
                            int dotToUse = Random.Range(0, bonusesPrefab.Length);
                            dot = Instantiate(bonusesPrefab[dotToUse], tempPosition, Quaternion.identity);
                            dot.transform.parent = transform;
                            dot.name = "( Bonus " + bonusesPrefab[dotToUse].name + ": " + i + ", " + u + " )";
                            allDots[i, u] = dot;
                        }
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
        if (!_chain.Contains(obj))
        {
            allDots[obj.x, obj.y].GetComponent<Dot>().SetState(true);
            _chain.Add(obj);
            if (_chain.Count > 1)
            {
                Vector2Int from = new Vector2Int(_chain[_chain.Count-2].x, _chain[_chain.Count-2].y);
                Vector2Int to = new Vector2Int(_chain[_chain.Count-1].x, _chain[_chain.Count-1].y);
                allLines.Add(NewLine(from, to));
            }
        }
    }

    public void RemoveFromChainLast()
    {
        _chain.RemoveAt(_chain.Count - 1);
        if (allLines.Count > 0)
        {
            Destroy(allLines[allLines.Count - 1]);
            allLines.RemoveAt(allLines.Count - 1);
        }
    }

    public bool IsPreviosDot(Vector2Int dotPos)
    {
        return _chain.IndexOf(dotPos) == _chain.Count - 2 ? true : false;
    }

    public void ChainClear()
    {
        foreach(Vector2Int element in _chain)
        {
            int x = element.x;
            int y = element.y;
            if (allDots[x, y].GetComponent<Dot>().GetState())
                allDots[x, y].GetComponent<Dot>().SetSelected();
        }
        _chain.Clear();
    }

    
    public int GetChainCount()
    {
        return _chain.Count;
    }
}

