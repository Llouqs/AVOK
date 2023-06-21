using System.Collections;
using System.Linq;
using UnityEngine;
public class Dot : MonoBehaviour
{
    //public int DotTypeName;
    private SpriteRenderer _render; //переменная состояния тайла (цвет, размер и т.п.)
    private Transform _transformDot;
    private static readonly Color SelectedColor = new Color(.5f, .5f, .5f, 1.0f); //цвет подсветки тайла
    private static Dot _previosSelectedDot;
    private bool _selected;
    private static BoardManager _boardManager;
    [SerializeField] private GameObject effect;
    [SerializeField] private Sprite[] equalDots;
    private void Start()
    {
        _boardManager = FindObjectOfType<BoardManager>();
        _render = GetComponent<SpriteRenderer>();
        _transformDot = GetComponent<Transform>();
    }

    public void SetState(bool state)
    {
        _selected = state;
    }
    public bool GetState()
    {
        return _selected;
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
        if (!Input.GetMouseButton(0) && _previosSelectedDot == null) return false;
        return equalDots.Contains(_previosSelectedDot.GetComponent<SpriteRenderer>().sprite);
    }

    private bool IsNeighbourToPrevios()
    {
        var current = _transformDot.position;
        var previous = _previosSelectedDot._transformDot.position;

        return Mathf.Abs(current.x - previous.x) <= 1 && Mathf.Abs(current.y - previous.y) <= 1;
    }
    
    private void OnMouseEnter()
    {
        if (!IsEqualDots() || !IsNeighbourToPrevios()) return;
        if (this.GetState())
        {
            if (_boardManager.IsPreviosDot(new Vector2Int((int) transform.position.x,
                (int) transform.position.y)))
            {
                SetSelected();
                _previosSelectedDot.SetSelected();
                _boardManager.RemoveFromChainLast();
                _boardManager.RemoveFromChainLast();
            }
            else
            {
                StartCoroutine(UpSlowScale());
                return;
            }
        }

        _previosSelectedDot = this;

        SetSelected();
        AddDotChain(new Vector2Int((int) _transformDot.position.x,
            (int) _transformDot.transform.position.y));
    }

    private void OnMouseExit()
    {
        if(this.GetState())
            StartCoroutine(DownSlowScale());
    }

    private void OnMouseDown()
    {
        if (_previosSelectedDot != null) return;
        _previosSelectedDot = this;
        SetSelected();
        AddDotChain(new Vector2Int((int)_transformDot.position.x, (int)_transformDot.transform.position.y));
    }

    private void OnMouseUp()
    {
        _boardManager.DestroyChain();
        if (_boardManager.GetChainCount() > 2)
        {
            _boardManager.UpdateBoard();
        }
        _boardManager.ChainClear();
        _previosSelectedDot = null;
    }
    public void SetSelected()
    {
        if (_selected)
        {
            _render.color = Color.white;
            StartCoroutine(DownSlowScale());
        }
        else
        {
            _render.color = SelectedColor;
            StartCoroutine(UpSlowScale());
        }
        _selected = !_selected;
    }
    private void AddDotChain(Vector2Int tmp)
    {
        _boardManager.AddToChain(tmp);
    }

    public void StartEffect()
    {
        var effectObj = Instantiate(effect, transform.position, Quaternion.identity);
        Destroy(effectObj, 2.0f);
    }
}
