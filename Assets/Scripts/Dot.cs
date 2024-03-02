using System.Collections;
using System.Linq;
using UnityEngine;

public class Dot : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Transform dotTransform;
    private static readonly Color SelectedColor = new Color(.5f, .5f, .5f, 1.0f);
    private static Dot previousSelectedDot;
    private static BoardManager boardManager;
    [SerializeField] private Vector2Int dotPosition;

    [SerializeField] private GameObject effect;
    [SerializeField] private Sprite[] equalDots;

    private static bool isBoardUpdating = false;
    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        boardManager.BoardUpdated += OnBoardUpdated;
        spriteRenderer = GetComponent<SpriteRenderer>();
        dotTransform = GetComponent<Transform>();
        //dotPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }

    public Vector2Int DotPosition
    {
        get { return dotPosition; }
        set { dotPosition = value; }
    }

    public bool IsSelected { get; set; }

    private IEnumerator UpSlowScale()
    {
        for (float q = 1f; q < 2f && transform.localScale.x <= 1.3f; q += .1f)
        {
            transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
            yield return new WaitForSeconds(.015f);
        }
    }

    private IEnumerator DownSlowScale()
    {
        for (float q = 1f; q < 2f && transform.localScale.x >= 1f; q += .1f)
        {
            transform.localScale += new Vector3(-0.05f, -0.05f, -0.05f);
            yield return new WaitForSeconds(.015f);
        }
    }

    private bool IsEqualDots()
    {
        if (!Input.GetMouseButton(0) || previousSelectedDot == null || previousSelectedDot.spriteRenderer == null) return false;
        return equalDots.Contains(previousSelectedDot.spriteRenderer.sprite);
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
                StartCoroutine(UpSlowScale());
                return;
            }
        }

        previousSelectedDot = this;

        SetSelected();
        boardManager.AddToChain(this);
    }

    private void OnMouseExit()
    {
        if (IsSelected)
            StartCoroutine(DownSlowScale());
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
            StartCoroutine(DownSlowScale());
        }
        else
        {
            spriteRenderer.color = SelectedColor;
            StartCoroutine(UpSlowScale());
        }
        IsSelected = !IsSelected;
    }


    public void StartEffect()
    {
        var effectObj = Instantiate(effect, dotTransform.position, Quaternion.identity);
        Destroy(effectObj, 3.0f);
    }
}
