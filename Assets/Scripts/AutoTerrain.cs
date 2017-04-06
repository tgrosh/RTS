using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTerrain : MonoBehaviour {
    public AutoTerrainPrefab defaultPrefab;
    public AutoTerrainPrefab defaultPrefabCorner;
    public AutoTerrainPrefab[] autoTerrainPrefabs;
    public int maxRows;
    public int maxCols;
    public GameObject waterPrefab;
    public float waterHeight = .5f;
    public AutoTerrainPrefab borderPrefab;
    public int borderWidth;

    [HideInInspector]
    public float limitX;
    [HideInInspector]
    public float limitY;

    Vector3 landPrefabBounds;
    Vector3 defaultPrefabCornerBounds;
    AutoTerrainCell[,] grid;

    void Awake()
    {
        System.Array.Sort(autoTerrainPrefabs, delegate (AutoTerrainPrefab first, AutoTerrainPrefab second)
        {
            return first.probability.CompareTo(second.probability);
        });
        landPrefabBounds = defaultPrefab.prefab.transform.GetComponentInChildren<MeshRenderer>().bounds.size;
        defaultPrefabCornerBounds = defaultPrefabCorner.prefab.transform.GetComponentInChildren<MeshRenderer>().bounds.size;
        grid = new AutoTerrainCell[maxRows + (borderWidth * 2), maxCols + (borderWidth * 2)];
    }

    // Use this for initialization
    void Start() {
        
    }

    public void Generate()
    {
        CreateWater();
        CreateLand(defaultPrefab, borderWidth, borderWidth);
        CreateBorder();
        //CreateCorners();
        transform.GetComponentInChildren<AstarPath>().Scan();
        
        limitX = ((maxCols - (borderWidth * 2)) * landPrefabBounds.x) / 2;
        limitY = ((maxRows - (borderWidth * 2)) * landPrefabBounds.z) / 2;
    }

    public Vector3 TerrainCenter {
        get {
            return new Vector3(landPrefabBounds.x * (grid.GetLength(0) / 2f) - landPrefabBounds.x, landPrefabBounds.y, landPrefabBounds.z * (grid.GetLength(1) / 2f) - landPrefabBounds.z);
        }
    }

    void CreateCorners()
    {
        for (int row = 0; row < grid.GetLength(0); row++)
        {
            for (int col = 0; col < grid.GetLength(1); col++)
            {
                if (grid[row, col].template.name == defaultPrefab.name)
                {
                    //if we are on a default land tile, check if it is at a corner
                    //corner means, for example: low dirt left, default land upper left
                    AutoTerrainCell[] neighbors = GetNeighbors(row, col);
                    if (neighbors[1] != null && neighbors[1].template.name == "DefaultLand" && //upper right
                        neighbors[2] != null && neighbors[2].template.name == "LowDirt") //right
                    {
                        //grid[row, col].instance.transform.position += Vector3.up * 2;
                        //TODO cannot use position of actual neighbors because they may have been rotated. revert to calculating from grid index
                        Instantiate(defaultPrefabCorner.prefab, new Vector3(neighbors[2].instance.transform.position.x + defaultPrefabCornerBounds.x, landPrefabBounds.y - defaultPrefabCornerBounds.y, neighbors[2].instance.transform.position.z + landPrefabBounds.z), Quaternion.identity);                     
                    }
                }
            }
        }
    }

    void CreateBorder()
    {
        for (int row = 0; row < grid.GetLength(0); row++)
        {
            for (int col = 0; col < grid.GetLength(1); col++)
            {
                if (grid[row, col] == null)
                {
                    GameObject obj = Instantiate(borderPrefab.prefab, new Vector3(col * landPrefabBounds.x, 0, row * landPrefabBounds.z), Quaternion.identity);
                    grid[row, col] = new AutoTerrainCell(borderPrefab, obj);
                }
            }
        }
    }

    void CreateWater()
    {
        GameObject obj = Instantiate(waterPrefab, new Vector3(TerrainCenter.x, waterHeight, TerrainCenter.z), Quaternion.identity);
        obj.transform.localScale = new Vector3(grid.GetLength(0) / 2f * landPrefabBounds.x, 1, grid.GetLength(1) / 2f * landPrefabBounds.z);
    }

    void CreateLand(AutoTerrainPrefab autoTerrainPrefab, int row, int col)
    {
        if (row > maxRows + borderWidth - 1 || row < borderWidth || col > maxCols + borderWidth - 1 || col < borderWidth) return; //out of bounds
        if (grid[row, col] != null) return; //already created

        GameObject obj = Instantiate(autoTerrainPrefab.prefab, new Vector3(col * landPrefabBounds.x, 0, row * landPrefabBounds.z), Quaternion.identity);
        obj.transform.RotateAround(obj.transform.GetComponentInChildren<MeshRenderer>().bounds.center, Vector3.up, Random.Range(0, 3) * 90);
        grid[row, col] = new AutoTerrainCell(autoTerrainPrefab, obj);

        List<int> indexes = Shuffle<int>(new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 })); //shuffled list of indexes
        List<int> rows = new List<int>(new int[] { 0, 1, 0, -1, 1, -1, -1, 1 }); //row col pairs
        List<int> cols = new List<int>(new int[] { -1, 1, 1, -1, 0, 1, 0, -1 }); //row col pairs
        foreach (int i in indexes)
        {
            //loop through each shuffled index and create a new Land on that row/col pair
            CreateLand(GetAutoTerrainPrefab(autoTerrainPrefab), row + rows[i], col + cols[i]);
        }
    }

    AutoTerrainPrefab GetAutoTerrainPrefab(AutoTerrainPrefab sourceAutoTerrainPrefab)
    {
        if (sourceAutoTerrainPrefab.groupingProbability > 0)
        {
            //determine if we are grouping
            float groupingRoll = Random.value;
            if (groupingRoll <= sourceAutoTerrainPrefab.groupingProbability)
            {
                return sourceAutoTerrainPrefab;
            }
        }

        float roll = Random.value;
        List<AutoTerrainPrefab> matches = new List<AutoTerrainPrefab>();
        foreach (AutoTerrainPrefab autoTerrainPrefab in autoTerrainPrefabs)
        {
            if (roll <= autoTerrainPrefab.probability)
            {
                //find all winners, there could be a probability tie
                if (matches.Count == 0 || matches.Find((AutoTerrainPrefab item) => { return item.probability == autoTerrainPrefab.probability; }) != null)
                {
                    matches.Add(autoTerrainPrefab);
                }
            }
        }
        if (matches.Count > 0)
        {
            //pick a random winner
            int matchRoll = Random.Range(0, matches.Count);
            return matches[matchRoll];
        }

        return defaultPrefab;
    }

    AutoTerrainCell[] GetNeighbors(int row, int col)
    {
        AutoTerrainCell[] neighbors = new AutoTerrainCell[8];
        neighbors[0] = row < maxRows -1 ? grid[row + 1, col] : null;
        neighbors[1] = row < maxRows - 1 && col < maxCols - 1 ? grid[row + 1, col + 1] : null;
        neighbors[2] = col < maxCols - 1 ? grid[row, col + 1] : null;
        neighbors[3] = row > 0 && col < maxCols - 1 ? grid[row - 1, col + 1] : null;
        neighbors[4] = row > 0 ? grid[row - 1, col] : null;
        neighbors[5] = row > 0 && col > 0 ? grid[row - 1, col - 1] : null;
        neighbors[6] = col > 0 ? grid[row, col - 1] : null;
        neighbors[7] = row < maxRows - 1 && col > 0 ? grid[row + 1, col - 1] : null;

        return neighbors;
    }

    List<T> Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    bool IsEdgeTile(int row, int col)
    {
        if (row == 0 || row == maxRows - 1 ||
            col == 0 || col == maxCols - 1)
        {
            return true;
        }
        return false;
    }
}

public enum AutoTerrainType
{
    EMPTY = 0,
    LAND = 1,
    BORDER = 2
}
