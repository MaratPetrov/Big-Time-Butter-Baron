using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour //код для действий с игровым полем
{
    public static GridManager Instance;
    
    [Header("Grid Settings")]
    public int width = 9;  // По горизонтали (X)
    public int depth = 9;  // По вертикали (Z)
    public float cellSize = 1f;
	[HideInInspector] public Cell[] allCells;
    
    private Cell[,] grid; //для проверки строк
    
    void Awake()
    {
        Instance = this;
    }
	
	void FillCells(){
		allCells = FindObjectsOfType<Cell>();
	}

	public int CountEmpty(){ //количество пустых ячеек
		int result = 0;
		foreach (Cell oneCell in allCells){
			if (oneCell.type == CellType.Empty){
				result ++;
			}
		}
		return result;
	}

    void Start()
    {
		// Заполняем массив из существующих ячеек на сцене
		FillCells();
        BuildGridFromScene();
    }
    
    void BuildGridFromScene()
    {
        grid = new Cell[width, depth];
        
        foreach (Cell cell in allCells){
            if (cell.gridX >= 0 && cell.gridX < width && 
                cell.gridZ >= 0 && cell.gridZ < depth)
            {
                grid[cell.gridX, cell.gridZ] = cell;
            }
        }
    }
    
    public Cell GetCell(int x, int z)
    {
        if (x >= 0 && x < width && z >= 0 && z < depth)
            return grid[x, z];
        return null;
    }
    
    public List<Cell> GetCellsInRow(int z)
    {
        List<Cell> row = new List<Cell>();
        for (int x = 0; x < width; x++)
        {
            if (grid[x, z] != null)
                row.Add(grid[x, z]);
        }
        return row;
    }
    
    public List<Cell> GetCellsInColumn(int x)
    {
        List<Cell> column = new List<Cell>();
        for (int z = 0; z < depth; z++)
        {
            if (grid[x, z] != null)
                column.Add(grid[x, z]);
        }
        return column;
    }
    
    public List<List<Cell>> GetAllRows()
    {
        List<List<Cell>> rows = new List<List<Cell>>();
        for (int z = 0; z < depth; z++)
        {
            rows.Add(GetCellsInRow(z));
        }
        return rows;
    }
    
    public List<List<Cell>> GetAllColumns()
    {
        List<List<Cell>> columns = new List<List<Cell>>();
        for (int x = 0; x < width; x++)
        {
            columns.Add(GetCellsInColumn(x));
        }
        return columns;
    }

	public List<Cell> UpdateEmpty(){ //список пустых ячеек на всём поле
		List<Cell> emptyCells = new List<Cell>();
		foreach (Cell cell in GridManager.Instance.allCells){
            if (cell.isEmpty())
                emptyCells.Add(cell);
        }
		return emptyCells;
	}
}