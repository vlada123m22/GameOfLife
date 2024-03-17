using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;
    [SerializeField] private Tile aliveTile;
    [SerializeField] private Tile deadTile;
    [SerializeField] private Pattern pattern;
    [SerializeField] private float updateInterval = 0.05f;

    private HashSet<Vector3Int> aliveCells;
    private HashSet<Vector3Int> cellsToCheck;

    public int population { get; private set; }
    public int iterations { get; private set; }
    public float time { get; private set; }

    private void Awake()
    {
        aliveCells = new HashSet<Vector3Int>();
        cellsToCheck = new HashSet<Vector3Int>();
    }

    private void Start()
    {
        SetPattern(pattern);
    }

    private void SetPattern(Pattern pattern)
    {
        Clear();

        Vector2Int center = pattern.GetCenter();

        for (int i = 0; i < pattern.cells.Length; i++)
        {
            Vector3Int cell = (Vector3Int)(pattern.cells[i] - center);
            currentState.SetTile(cell, aliveTile);
            aliveCells.Add(cell);
        }

        population = aliveCells.Count;
    }

    private void Clear()
    {
        aliveCells.Clear();
        cellsToCheck.Clear();
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        population = 0;
        iterations = 0;
        time = 0f;
    }

    private void OnEnable()
    {
        StartCoroutine(Simulate());
    }

    private IEnumerator Simulate()
    {
        var interval = new WaitForSeconds(updateInterval);
        yield return interval;

        while (enabled)
        {
            UpdateState();

            population = aliveCells.Count;
            iterations++;
            time += updateInterval;

            yield return interval;
        }
    }

public Vector2Int gridSize; // Size of the grid
public Vector2Int zoneSize; // Size of each zone
private int generationCount = 0;
public int reverseInterval = 5; // Change this to the desired interval for reversing time


private void UpdateState()
{
    generationCount++;

    cellsToCheck.Clear();

    // Gather cells to check
    foreach (Vector3Int cell in aliveCells)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                cellsToCheck.Add(cell + new Vector3Int(x, y));
            }
        }
    }

    // Transition cells to the next state
    foreach (Vector3Int cell in cellsToCheck)
    {
        int neighbors = CountNeighbors(cell);
        bool alive = IsAlive(cell);

        // Determine the zone of the cell
        int zoneX = cell.x < gridSize.x / 2 ? 0 : 1;
        int zoneY = cell.y < gridSize.y / 2 ? 0 : 1;

        // Apply different rules based on the zone
        if (zoneX == 1 && zoneY == 1) // Upper-right zone
        {
            if (generationCount % reverseInterval == 0)
            {
                if (alive)
                {
                    nextState.SetTile(cell, deadTile);
                    aliveCells.Remove(cell);
                }
                else
                {
                    nextState.SetTile(cell, aliveTile);
                    aliveCells.Add(cell);
                }
            }
            else
            {
                nextState.SetTile(cell, currentState.GetTile(cell));
            }
        }
        else // Standard rules for other zones
        {
            if (!alive && neighbors == 3)
            {
                nextState.SetTile(cell, aliveTile);
                aliveCells.Add(cell);
            }
            else if (alive && (neighbors < 2 || neighbors > 3))
            {
                nextState.SetTile(cell, deadTile);
                aliveCells.Remove(cell);
            }
            else
            {
                nextState.SetTile(cell, currentState.GetTile(cell));
            }
        }
    }

    // Swap current state with next state
    Tilemap temp = currentState;
    currentState = nextState;
    nextState = temp;
    nextState.ClearAllTiles();
}



    private int CountNeighbors(Vector3Int cell)
    {
        int count = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighbor = cell + new Vector3Int(x, y);

                if (x == 0 && y == 0) {
                    continue;
                } else if (IsAlive(neighbor)) {
                    count++;
                }
            }
        }

        return count;
    }

    private bool IsAlive(Vector3Int cell)
    {
        return currentState.GetTile(cell) == aliveTile;
    }

}