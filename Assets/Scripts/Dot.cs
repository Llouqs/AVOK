using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    //public int DotTypeName;
    [SerializeField]
    private SpriteRenderer render; //переменная состояния тайла (цвет, размер и т.п.)
    private Transform _transformDot;
    private static Color _selectedColor = new Color(.5f, .5f, .5f, 1.0f); //цвет подсветки тайла
    private static Dot _previosSelectedDot = null;
    private bool _isEnable = false;
    private BoardManager _boardManager;
    public GameObject effect;
    public GameObject effectObj;
    private void Start()
    {
        _boardManager = FindObjectOfType<BoardManager>();
        render = GetComponent<SpriteRenderer>();
        _transformDot = GetComponent<Transform>();
    }

    public void SetState(bool state)
    {
        _isEnable = state;
    }
    public bool GetState()
    {
        return _isEnable;
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
    private void OnMouseEnter()
    {
        var flag = false;
        if (Input.GetMouseButton(0))
        {
            if (_previosSelectedDot != null)
            {
                if (_previosSelectedDot.GetComponent<SpriteRenderer>().sprite == render.sprite)
                {
                    flag = true;
                }
                else
                {
                    if (_previosSelectedDot.GetComponent<SpriteRenderer>().sprite.name == "sun" ||
                        _previosSelectedDot.GetComponent<SpriteRenderer>().sprite.name == "moon")
                    {
                        if (render.sprite.name == "eclipse")
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        if (_previosSelectedDot.GetComponent<SpriteRenderer>().sprite.name == "eclipse")
                        {
                            if (render.sprite.name == "sun" ||
                                render.sprite.name == "moon")
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
        }

        if (flag == true &&
            _transformDot.position.x >= _previosSelectedDot._transformDot.position.x - 1 &&
            _transformDot.position.x <= _previosSelectedDot._transformDot.position.x + 1 &&
            _transformDot.position.y >= _previosSelectedDot._transformDot.position.y - 1 &&
            _transformDot.position.y <= _previosSelectedDot._transformDot.position.y + 1)
        {
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
    }
    
    private void OnMouseExit()
    {
        if(this.GetState())
            StartCoroutine(DownSlowScale());
    }

    private void OnMouseDown()
    {
        if (_previosSelectedDot == null)
        {
            _previosSelectedDot = this;
            SetSelected();
            AddDotChain(new Vector2Int((int)_transformDot.position.x, (int)_transformDot.transform.position.y));
        }
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

        if (_isEnable == true)
        {
            render.color = Color.white;
            SetState(false);
            StartCoroutine(DownSlowScale());
        }
        else
        {
            render.color = _selectedColor;
            SetState(true);
            StartCoroutine(UpSlowScale());
        }
    }
    private void AddDotChain(Vector2Int tmp)
    {
        _boardManager.AddToChain(tmp);
    }

    public void StartEffect()
    {
        effectObj = Instantiate(effect, transform.position, Quaternion.identity);
        Destroy(effectObj, 2.0f);
    }
}
