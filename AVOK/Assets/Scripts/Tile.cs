using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Coord
{
    public Coord(int dx, int dy)
    {
        x = dx;
        y = dy;
    }
    public int x { get; private set; }
    public int y { get; private set; }
}
public class Tile : MonoBehaviour
{
    private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f); //цвет подсветки тайла
    private static Color deathColor = new Color(.5f, .5f, .5f, .0f); //цвет подсветки тайла
    private static Tile previousSelected = null; //последний выбранный тайл
    private static List <Coord> TilesArra = new List <Coord>(); //список выбранных тайлов
    private static List<Tile> TilesArray = new List<Tile>(); //список выбранных тайлов
    private SpriteRenderer render; //переменная состояния тайла (цвет, размер и т.п.)
    private int row;
    private int col;
    void Awake()
    {
        render = GetComponent<SpriteRenderer>();
        row = (int)transform.position.y;
        col = (int)transform.position.x;
    }
    private void Select()
    {
        TilesArray.Add(this);
        render.color = selectedColor;
        previousSelected = gameObject.GetComponent<Tile>(); //запоминаем последний выбранный тайл 
    }
    private void Deselect()
    {
        render.color = Color.white;
        previousSelected = null;
        TilesArray.Remove(this);
    }
    IEnumerator UpSlowScale()
    {
       for (float q = 1f; q < 2f && transform.localScale.x <= 1.3f; q += .1f)
       {
                transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
                yield return new WaitForSeconds(.015f);
       }
    }
    IEnumerator DownSlowScale()
    {
        for (float q = 1f; q < 2f && transform.localScale.x >= 1f; q += .1f)
        {
            transform.localScale += new Vector3(-0.05f, -0.05f, -0.05f);
            yield return new WaitForSeconds(.015f);
        }
    }
    void OnMouseEnter()
    {
        if (Input.GetMouseButton(0))
        {
            if (!TilesArray.Contains(this))
            {
                Debug.Log("OnMouseEnter");
                Chain();
            }
            else
            {
                previousSelected.Deselect();
                previousSelected = gameObject.GetComponent<Tile>();
                Debug.Log("takoy uze est");
                Debug.Log(TilesArray.Count);
            }
        }
        StartCoroutine(UpSlowScale()); //Корутины работают параллельно, самое то для анимации
    }
    void OnMouseExit()
    {
        StartCoroutine(DownSlowScale());
    }
    void OnMouseUp()
    {
        DestroyMatches();
    }
    void DestroyMatches()
    {
        if (TilesArray.Count >= 3)
        {
            int Tx, Ty;
            for (int i = 0; i < TilesArray.Count; i++)
            {
                Tx = (int)TilesArray[i].transform.position.x;
                Ty = (int)TilesArray[i].transform.position.y;
                Debug.Log("xxxxxxxxxxx");
                Debug.Log(Tx);
                Debug.Log("yyyyyyyyyyy");
                Debug.Log(Ty);
                Debug.Log("яя");
                //Destroy(BoardManager.instance.tiles[Tx, Ty]);
                //TilesArray[i].render.color = deathColor;
                TilesArray.Remove(TilesArray[i]);
                Debug.Log(TilesArray.Count);
            }
            TilesArray.Clear();
        }
    }
    void OnMouseDown()
    {
        if (render.sprite == null) { return; }
        else if (TilesArray.Count <= 1)
            {
                Chain();
            }
    }
    private void Chain()
    {
        if (SelectSprites(previousSelected))
            {
                //previousSelected.Deselect();
            }
        else
            {
                for (int i = 0; i < TilesArray.Count; i++)
                {
                    TilesArray[i].Deselect();
                }
            }
        Select(); //выбрать новый тайл
    }
private bool SelectSprites(Tile previous)
{
        if (previousSelected != null) //если уже выбран какой-то тайл
        {
            if (render.sprite == previous.render.sprite) //если выбранный спрайт такой же как исходный
            {
                if ((transform.position.x == previous.transform.position.x - 1
                    || transform.position.x == previous.transform.position.x + 1
                    || transform.position.x == previous.transform.position.x)
                    && (transform.position.y == previous.transform.position.y - 1
                    || transform.position.y == previous.transform.position.y + 1
                    || transform.position.y == previous.transform.position.y))
                {
                    Debug.Log("yes");
                    return true;
                }
                Debug.Log("so far");
                return false;
            }
            else
            {
                Debug.Log("no");
                return false;
            }
        }
        else return true;
    }
}