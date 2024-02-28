using System.Collections;
using System.Linq;
using UnityEngine;

public class Dot : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Transform dotTransform;
    private static readonly Color SelectedColor = new Color(.5f, .5f, .5f, 1.0f);
    private static Dot previousSelectedDot;
    private bool isSelected;
    private static BoardManager boardManager;

    [SerializeField] private GameObject effect;
    [SerializeField] private Sprite[] equalDots;

    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dotTransform = GetComponent<Transform>();
    }

    public void SetState(bool state)
    {
        isSelected = state;
    }

    public bool GetState()
    {
        return isSelected;
    }

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
        if (!Input.GetMouseButton(0) && previousSelectedDot == null) return false;
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
        if (this.GetState())
        {
            if (boardManager.IsPreviousDot(new Vector2Int((int)dotTransform.position.x,
                (int)dotTransform.position.y)))
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
        AddDotChain(new Vector2Int((int)dotTransform.position.x,
            (int)dotTransform.position.y));
    }

    private void OnMouseExit()
    {
        if (this.GetState())
            StartCoroutine(DownSlowScale());
    }

    private void OnMouseDown()
    {
        if (previousSelectedDot != null) return;
        previousSelectedDot = this;
        SetSelected();
        AddDotChain(new Vector2Int((int)dotTransform.position.x, (int)dotTransform.position.y));
    }

    private void OnMouseUp()
    {
        boardManager.DestroyChain();
        if (boardManager.GetChainCount() > 2)
        {
            boardManager.UpdateBoard();
        }
        boardManager.ChainClear();
        previousSelectedDot = null;
    }

    public void SetSelected()
    {
        if (isSelected)
        {
            spriteRenderer.color = Color.white;
            StartCoroutine(DownSlowScale());
        }
        else
        {
            spriteRenderer.color = SelectedColor;
            StartCoroutine(UpSlowScale());
        }
        isSelected = !isSelected;
    }

    private void AddDotChain(Vector2Int tmp)
    {
        boardManager.AddToChain(tmp);
    }

    public void StartEffect()
    {
        var effectObj = Instantiate(effect, dotTransform.position, Quaternion.identity);
        Destroy(effectObj, 3.0f);
    }
}
