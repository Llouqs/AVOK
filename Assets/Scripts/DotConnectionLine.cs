using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    public GameObject GameObject { get; private set; }

    public Line(Vector2Int from, Vector2Int to, GameObject linePrefab)
    {
        //NewLine(from, to, linePrefab);
    }

    private GameObject NewLine(Vector2Int from, Vector2Int to)
    {
        //Debug.Log(from + ", " + to);
        float z = 0f;
        Vector2 tempPosition;
        Vector2Int direction = new Vector2Int(from.x - to.x, from.y - to.y);
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
            else if (direction.y == -1)
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
        GameObject line = Instantiate(linePrefab, tempPosition - direction, Quaternion.identity);
        line.transform.parent = transform;
        line.transform.rotation *= Quaternion.Euler(0f, 0f, z);
        line.name = "( Line: " + from.x + ", " + from.y + " )";
        return line;
    }
}