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
    [SerializeField] private GameObject boomEffectPrefab;
    [SerializeField] private GameObject doubleBoomEffectPrefab;
    [SerializeField] private GameObject boomAreaPrefab;
    
    [SerializeField] private int width, height; 

    private int _scores = 0;
    [SerializeField] private Record recordUI;
    
    public GameObject[,] allTiles;
    public GameObject[,] allDots;
    public List<GameObject> allLines;
    private List<Vector2Int> _chain;
    private GameObject[,] _boomPool;
    
    private void Start()
    {
        height = width + width - 5; //временная заглушка для размера
        var mainCamera = FindObjectOfType<Camera>();
        mainCamera.transform.position = new Vector3((float)width / 2 - 0.5f, y: width-1,z: -10);
        mainCamera.orthographicSize = width + 1.5f;
        _chain = new List<Vector2Int>();
        allTiles = new GameObject[width, height];
        allDots = new GameObject[width, height];
        _boomPool = new GameObject[width, height];
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
                SetBoomArea(i, j);
            }
        }
    }

    private void SetBoomArea(int i, int j)
    {
        Vector2 tempPosition = new Vector2(i, j);
        GameObject boomLight = Instantiate(boomAreaPrefab, tempPosition, Quaternion.identity);
        boomLight.transform.parent = transform;
        boomLight.name = "( Boom Light: " + i + ", " + j + " )";
        _boomPool[i, j] = boomLight;
        _boomPool[i, j].SetActive(false);
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
                var effect = Instantiate(doubleBoomEffectPrefab, new Vector2(_chain[_chain.Count - 1].x, _chain[_chain.Count - 1].y), Quaternion.identity);
                DoubleBoom(_chain[_chain.Count - 1]);
                Destroy(effect, 2.0f);
            }
            else
            {
                _scores += 10;
                var effect = Instantiate(boomEffectPrefab, new Vector2(_chain[_chain.Count - 1].x, _chain[_chain.Count - 1].y), Quaternion.identity);
                Boom(_chain[_chain.Count - 1]);
                Destroy(effect, 2.0f);
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
                if (allTiles[x, y] != null)
                {
                    Destroy(allTiles[x, y]);
                    allTiles[x, y] = null;
                }
            }
        }
        foreach (var obj in allLines)
        {
            Destroy(obj);
        }
        allLines.Clear();
        ClearBoom();
    }

    private void Boom(Vector2Int boomElement)
    {
        int x = boomElement.x;
        int y = boomElement.y;

        AddToChainIfValid(new Vector2Int(x - 1, y));
        AddToChainIfValid(new Vector2Int(x + 1, y));
        AddToChainIfValid(new Vector2Int(x, y - 1));
        AddToChainIfValid(new Vector2Int(x, y + 1));
    }

    private void DoubleBoom(Vector2Int boomElement)
    {
        int x = boomElement.x;
        int y = boomElement.y;

        AddToChainAndBoom(new Vector2Int(x - 1, y));
        AddToChainAndBoom(new Vector2Int(x + 1, y));
        AddToChainAndBoom(new Vector2Int(x, y - 1));
        AddToChainAndBoom(new Vector2Int(x, y + 1));
    }

    private void AddToChainIfValid(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            AddToChain(position);
        }
    }

    private void AddToChainAndBoom(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            AddToChain(position);
            Boom(position);
        }
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return  position.x >= 0 && position.x < width &&
                position.y >= 0 && position.y < height;
    }

    public void UpdateBoard()
    {
        for (int i = 0; i < width; i++)
        {
            List<GameObject> dotsForFall = new List<GameObject>();

            // Сначала добавляем существующие элементы в список для падения
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    dotsForFall.Add(allDots[i, j]);
                }
            }

            // Затем добавляем новые элементы только в свободные места
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tmpVector = new Vector2(i, height+j);
                    GameObject dot;
                    int dotToUse;

                    if (Random.Range(0, 15) != 0)
                    {
                        dotToUse = Random.Range(0, dotsPrefab.Length);
                        dot = Instantiate(dotsPrefab[dotToUse], tmpVector, Quaternion.identity);
                        dot.name = "( Dot: " + i + ", " + j + " )";
                    }
                    else
                    {
                        dotToUse = Random.Range(0, bonusesPrefab.Length);
                        dot = Instantiate(bonusesPrefab[dotToUse], tmpVector, Quaternion.identity);
                        dot.name = "( Bonus " + bonusesPrefab[dotToUse].name + ": " + i + ", " + j + " )";
                    }

                    dot.transform.parent = transform;
                    dotsForFall.Add(dot);
                }
            }

            // Помещаем элементы из списка обратно в массив и запускаем падение
            for (int j = 0; j < height; j++)
            {
                allDots[i, j] = dotsForFall[j];
                StartCoroutine(DownSlowFall(allDots[i, j], new Vector2(i, j)));
            }
        }
    }


    IEnumerator DownSlowFall(GameObject dot, Vector2 targetPosition)
    {
        float duration = 0.4f;
        float elapsed = 0f;
        float bounceDuration = 0.2f; // Длительность подпрыгивания
        float bounceHeight = 0.1f;   // Высота подпрыгивания

        Vector2 startPosition = dot.transform.position;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            dot.transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            elapsed += Time.deltaTime;

            yield return null;
        }

        dot.transform.position = targetPosition;

        // Проверяем, что элемент действительно упал
        if (targetPosition.y < startPosition.y)
        {
            // Эффект подпрыгивания
            float bounceElapsed = 0f;
            Vector2 initialPosition = dot.transform.position;

            while (bounceElapsed < bounceDuration)
            {
                float bounceT = bounceElapsed / bounceDuration;
                float yOffset = Mathf.Sin(bounceT * Mathf.PI) * bounceHeight;
                dot.transform.position = new Vector2(targetPosition.x, initialPosition.y + yOffset);

                bounceElapsed += Time.deltaTime;
                yield return null;
            }

            dot.transform.position = targetPosition; // Устанавливаем точно на целевую позицию
        }
    }

    public void AddToChain(Vector2Int obj)
    {
        ClearBoom();
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

            if (_chain.Count > 6 && _chain.Count < 10)
            {
                _ShowBoom(obj);
            }

            if (_chain.Count >= 10)
            {
                _ShowDoubleBoom(obj);
            }
        }
    }

    private void ClearBoom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                _boomPool[i,j].SetActive(false);
            }
        }
    }

    private void DrawBoomArea(Vector2Int boomElement)
    {
        if (IsValidPosition(boomElement))
        {
            _boomPool[boomElement.x, boomElement.y].SetActive(true);
        }
    }
    private void _ShowBoom(Vector2Int boomElement)
    {
        int x = boomElement.x;
        int y = boomElement.y;
        DrawBoomArea(new Vector2Int(x, y));
        DrawBoomArea(new Vector2Int(x - 1, y));
        DrawBoomArea(new Vector2Int(x + 1, y));
        DrawBoomArea(new Vector2Int(x, y - 1));
        DrawBoomArea(new Vector2Int(x, y + 1));
    }
    private void  _ShowDoubleBoom(Vector2Int boomElement)
    {
        int x = boomElement.x;
        int y = boomElement.y;
        _ShowBoom(new Vector2Int(x - 1, y));
        _ShowBoom(new Vector2Int(x + 1, y));
        _ShowBoom(new Vector2Int(x, y - 1));
        _ShowBoom(new Vector2Int(x, y + 1));
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

    public bool IsPreviousDot(Vector2Int dotPos)
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

