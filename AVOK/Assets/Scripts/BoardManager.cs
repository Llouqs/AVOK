using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public List<Sprite> characters = new List<Sprite>(); //список спрайтов элементов
    public GameObject tile; //префаб (элемент в общем виде)
    public int xSize, ySize; //размер поля в элементах
    public GameObject[,] tiles; //массив префабов (считать с левого нижнего угла по столбцу вверх, потом вправо и т.д.)
    void Start()
    {
        instance = GetComponent<BoardManager>();
        CreateBoard();
    }
    private void CreateBoard()
    {
        tiles = new GameObject[xSize, ySize];
        float startX = transform.position.x;
        float startY = transform.position.y;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                GameObject newTile = Instantiate(tile, //размещаем префаб на доске
                                                 new Vector3(startX + x, startY + y, 0), //в указанной координате
                                                 tile.transform.rotation); //с таким поворотом
                tiles[x, y] = newTile; //записываем в массив префабов
                newTile.transform.parent = transform; 
                Sprite newSprite = characters[Random.Range(0, characters.Count)]; //берем случайный спрайт
                newTile.GetComponent<SpriteRenderer>().sprite = newSprite; //присваеваем спрайт текущему префабу
            }
        }
    }
    private void Reffiling()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tiles[x, y] == null)
                { 
                //GameObject newTile = Instantiate(tile, //размещаем префаб на доске
                //                                 //new Vector3(startX + x, startY + y, 0), //в указанной координате
                //                                 tile.transform.rotation); //с таким поворотом
                //tiles[x, y] = newTile; //записываем в массив префабов
                //newTile.transform.parent = transform;
                //Sprite newSprite = characters[Random.Range(0, characters.Count)]; //берем случайный спрайт
                //newTile.GetComponent<SpriteRenderer>().sprite = newSprite; //присваеваем спрайт текущему префабу
                }
            }
        }
    }
}

