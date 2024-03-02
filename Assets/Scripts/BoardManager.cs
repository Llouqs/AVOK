using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System;

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
    public Dot[,] allDots;
    public List<GameObject> allLines;
    private List<Dot> _chain;
    private GameObject[,] _boomPool;
    public event Action BoardUpdated;

    private void Start()
    {
        InitializeCamera();
        InitializeGameData();
        SetUp();
    }
    private void InitializeCamera()
    {
        var mainCamera = FindObjectOfType<Camera>();
        mainCamera.transform.position = new Vector3((float)width / 2 - 0.5f, width - 1, -10);
        mainCamera.orthographicSize = width + 1.5f;
    }
    private void InitializeGameData()
    {
        height = width + width - 5; // временная заглушка для размера
        _chain = new List<Dot>();
        allTiles = new GameObject[width, height];
        allDots = new Dot[width, height];
        _boomPool = new GameObject[width, height];
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
        allDots[i, j] = dot.GetComponent<Dot>();
        allDots[i, j].DotPosition = new Vector2Int(i, j);
    }

    private GameObject NewLine(Vector2Int from, Vector2Int to)
    {
        //Debug.Log(from + ", " + to);
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
        int chainCount = _chain.Count;

        if (chainCount <= 2)
        {
            if (chainCount == 2)
            {
                Destroy(allLines[0]);
                allLines.RemoveAt(0);
            }
            return;
        }

        _scores += chainCount;

        if (chainCount >= 7)
        {
            int bonusScore = (chainCount >= 10) ? 30 : 10;
            _scores += bonusScore;

            GameObject effectPrefab = (chainCount >= 10) ? doubleBoomEffectPrefab : boomEffectPrefab;
            var effect = Instantiate(effectPrefab, _chain[chainCount - 1].transform.position, Quaternion.identity);

            if (chainCount >= 10)
            {
                DoubleBoom(_chain[chainCount - 1].DotPosition);
            }
            else
            {
                Boom(_chain[chainCount - 1].DotPosition);
            }

            Destroy(effect, 2.0f);
        }

        recordUI.ChangeScores(_scores);

        foreach (Dot dot in _chain)
        {
            DestroyDot(dot);
        }

        foreach (var line in allLines)
        {
            Destroy(line);
        }

        allLines.Clear();
        ClearBoom();
    }

    private void DestroyDot(Dot dot)
    {
        int x = dot.DotPosition.x;
        int y = dot.DotPosition.y;

        if (allDots[x, y].IsSelected)
        {
            Dot currentDot = allDots[x, y];

            currentDot.StartEffect();
            Destroy(currentDot.gameObject);
            allDots[x, y] = null;

            if (allTiles[x, y] != null)
            {
                Destroy(allTiles[x, y]);
                allTiles[x, y] = null;
            }
        }
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

    private void AddToChainIfValid(Vector2Int dotPosition)
    {
        if (IsValidPosition(dotPosition))
        {
            AddToChain(allDots[dotPosition.x, dotPosition.y]);
        }
    }

    private void AddToChainAndBoom(Vector2Int dotPosition)
    {
        if (IsValidPosition(dotPosition))
        {
            AddToChain(allDots[dotPosition.x, dotPosition.y]);
            Boom(dotPosition);
        }
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return  position.x >= 0 && position.x < width &&
                position.y >= 0 && position.y < height;
    }
    public void UpdateBoard()
    {
        StartCoroutine(UpdateBoardWithDelay());
    }

    private IEnumerator UpdateBoardWithDelay()
    {
        yield return new WaitForSeconds(0.15f);
        Debug.Log("yes");
        for (int i = 0; i < width; i++)
        {
            List<Dot> dotsForFall = new List<Dot>();

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
                    Vector2 tmpVector = new Vector2(i, height + j);
                    GameObject dot = Instantiate(Random.Range(0, 15) != 0 ? dotsPrefab[Random.Range(0, dotsPrefab.Length)] : bonusesPrefab[Random.Range(0, bonusesPrefab.Length)], tmpVector, Quaternion.identity);
                    dot.transform.parent = transform;
                    dotsForFall.Add(dot.GetComponent<Dot>());
                }
            }

            // Помещаем элементы из списка обратно в массив и запускаем падение
            for (int j = 0; j < height; j++)
            {
                dotsForFall[j].DotPosition = new Vector2Int(i, j);
                allDots[i, j] = dotsForFall[j];
                StartCoroutine(DownSlowFall(allDots[i, j], new Vector2(i, j)));
                allDots[i, j].name = "( Dot: " + i + ", " + j + " )";
            }
        }
        BoardUpdated?.Invoke();
    }


    IEnumerator DownSlowFall(Dot dot, Vector2 targetPosition)
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

    public void AddToChain(Dot dot)
    {
        ClearBoom();
        if (!_chain.Contains(dot))
        {
            dot.IsSelected = true;
            _chain.Add(dot);
            if (_chain.Count > 1)
            {
                Vector2Int from = _chain[_chain.Count - 2].DotPosition;
                Vector2Int to = _chain[_chain.Count - 1].DotPosition;
                allLines.Add(NewLine(from, to));
            }

            if (_chain.Count > 6 && _chain.Count < 10)
            {
                _ShowBoom(dot.DotPosition);
            }

            if (_chain.Count >= 10)
            {
                _ShowDoubleBoom(dot.DotPosition);
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

    public bool IsPreviousDot(Dot dot)
    {
        return _chain.IndexOf(dot) == _chain.Count - 2 ? true : false;
    }

    public void ChainClear()
    {
        foreach(Dot element in _chain)
        {
            int x = element.DotPosition.x;
            int y = element.DotPosition.y;
            if (allDots[x, y].IsSelected)
                allDots[x, y].SetSelected();
        }
        _chain.Clear();
    }

    
    public int GetChainCount()
    {
        return _chain.Count;
    }
}

