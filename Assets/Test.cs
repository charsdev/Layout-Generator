using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private GameObject[,] _cells;
    private int gridSizeX = 16;
    private int gridSizeY = 8;
    public Vector3 offset;
    public Vector3 pivot;
    private void Start()
    {
        _cells = new GameObject[gridSizeX, gridSizeY];

        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = i.ToString() + " x " + j.ToString();
                cube.transform.position = pivot + new Vector3(i * offset.x, j * offset.y, 0);
                _cells[i,j] = cube;
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition) - pivot;

            int cellX = Mathf.RoundToInt(worldPosition.x / offset.x); // Coordenada x de la celda
            int cellY = Mathf.RoundToInt(worldPosition.y / offset.y); // Coordenada y de la celda

            // Asegurarse de que las coordenadas estén dentro de los límites de la grilla
            if (cellX >= 0 && cellX < gridSizeX && cellY >= 0 && cellY < gridSizeY)
            {
                GameObject cell = _cells[cellX, cellY];
                print(cell.name);
                cell.GetComponent<Renderer>().material.color = Color.yellow;
                // Accede a la celda y haz lo que necesites hacer con ella
            }
        }
    }
}
