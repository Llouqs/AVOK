using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    //public int DotTypeName;
    [SerializeField]
    private SpriteRenderer render; //переменная состояния тайла (цвет, размер и т.п.)
    private Transform transformDot;
    private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f); //цвет подсветки тайла
    private static Dot previosSelectedDot = null;
    private bool isEnable = false;
    private BoardManager boardManager;
    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        render = GetComponent<SpriteRenderer>();
        transformDot = GetComponent<Transform>();
    }

    public void SetState(bool state)
    {
        isEnable = state;
    }
    public bool GetState()
    {
        return isEnable;
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
        if (Input.GetMouseButton(0))
        {
            if (previosSelectedDot != null)
            {
                if (previosSelectedDot.GetComponent<SpriteRenderer>().sprite == render.sprite)
                {
                    if (transformDot.position.x >= previosSelectedDot.transformDot.position.x - 1 &&
                       transformDot.position.x <= previosSelectedDot.transformDot.position.x + 1 &&
                       transformDot.position.y >= previosSelectedDot.transformDot.position.y - 1 &&
                       transformDot.position.y <= previosSelectedDot.transformDot.position.y + 1)
                    {
                        if(this.GetState())
                        {
                            if (boardManager.IsPreviosDot(new Vector2Int((int) transform.position.x,
                                (int) transform.position.y)))
                            {
                                SetSelected();
                                previosSelectedDot.SetSelected();
                                boardManager.RemoveFromChainLast();
                                boardManager.RemoveFromChainLast();
                            }
                            else
                            {
                                StartCoroutine(UpSlowScale());
                                return;
                            }
                        }
                        previosSelectedDot = this;
                        
                        SetSelected();
                        AddDotChain(new Vector2Int((int) transformDot.position.x, 
                            (int) transformDot.transform.position.y));
                    }
                }
            }
        }
        StartCoroutine(UpSlowScale());
    }
    private void OnMouseExit()
    {
        StartCoroutine(DownSlowScale());
    }

    private void OnMouseDown()
    {
        if (previosSelectedDot == null)
        {
            previosSelectedDot = this;
            SetSelected();
            AddDotChain(new Vector2Int((int)transformDot.position.x, (int)transformDot.transform.position.y));
        }
    }

    private void OnMouseUp()
    {
        if (boardManager.GetChainCount() > 2)
        {
            boardManager.DestroyChain();
            boardManager.UpdateBoard();
        }
        boardManager.ChainClear();
        previosSelectedDot = null;
    }
    public void SetSelected()
    {

        if (isEnable == true)
        {
            render.color = Color.white;
            SetState(false);
        }
        else
        {
            render.color = selectedColor;
            SetState(true);
        }
    }
    private void AddDotChain(Vector2Int tmp)
    {
        boardManager.AddToChain(tmp);
    }
}
