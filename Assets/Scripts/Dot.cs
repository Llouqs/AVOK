using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum DotKind
{
    Fire,
    Water,
    Leaf,
    Moon,
    Sun,
    Eclipse,
    WaterVerticalBonus
}
public class Dot : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Transform dotTransform;
    private static readonly Color SelectedColor = new Color(.5f, .5f, .5f, 1.0f);
    private static Dot previousSelectedDot;
    private static BoardManager boardManager;

    [SerializeField] public DotKind dotKind;
    [SerializeField] private DotKind[] equalDots;
    [SerializeField] private Vector2Int dotPosition;
    [SerializeField] private GameObject effect;
    [SerializeField] private GameObject boomLight;

    private static bool isBoardUpdating = false;
    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        boardManager.BoardUpdated += OnBoardUpdated;
        spriteRenderer = GetComponent<SpriteRenderer>();
        dotTransform = GetComponent<Transform>();
    }

    public void SetBoomLight(bool flag)
    {
        boomLight.SetActive(flag);
    }

    public Vector2Int DotPosition
    {
        get { return dotPosition; }
        set { dotPosition = value; }
    }

    public bool IsSelected { get; set; }
    public DotKind DotKind
    {
        get { return dotKind; }
    }
    private bool IsEqualDots()
    {
        if (!Input.GetMouseButton(0) || previousSelectedDot == null) return false;
        return equalDots.Contains(previousSelectedDot.dotKind);
    }

    private bool IsNeighbourToPrevious()
    {
        var current = dotTransform.position;
        var previous = previousSelectedDot.dotTransform.position;

        return Mathf.Abs(current.x - previous.x) <= 1 && Mathf.Abs(current.y - previous.y) <= 1;
    }

    private void OnMouseEnter()
    {
        if (!IsEqualDots() || !IsNeighbourToPrevious()) return;
        if (IsSelected)
        {
            if (boardManager.IsPreviousDot(this))
            {
                SetSelected();
                previousSelectedDot.SetSelected();
                boardManager.RemoveFromChainLast();
                boardManager.RemoveFromChainLast();
            }
            else
            {
                return;
            }
        }

        previousSelectedDot = this;

        SetSelected();
        boardManager.AddToChain(this);
    }
    private void OnMouseDown()
    {
        if (previousSelectedDot != null || isBoardUpdating) return;
        previousSelectedDot = this;
        SetSelected();
        boardManager.AddToChain(this);
    }

    private void OnBoardUpdated()
    {
        // Выполните действия после обновления доски
        boardManager.ChainClear();
        previousSelectedDot = null;
        isBoardUpdating = false;
    }

    private void OnMouseUp()
    {
        boardManager.DestroyChain();
        if (boardManager.GetChainCount() > 2)
        {
            isBoardUpdating = true;
            boardManager.UpdateBoard();
        }
        else
        {
            OnBoardUpdated();
        }
    }

    public void SetSelected()
    {
        if (IsSelected)
        {
            spriteRenderer.color = Color.white;
        }
        else
        {
            spriteRenderer.color = SelectedColor;
        }
        IsSelected = !IsSelected;
    }


    public void StartEffect()
    {
        var effectObj = Instantiate(effect, dotTransform.position, Quaternion.identity);
        effectObj.transform.parent = boardManager.transform;
        Destroy(effectObj, 3.0f);
    }
}
