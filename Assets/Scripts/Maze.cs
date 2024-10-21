
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Maze : MonoBehaviour
{
    public static Maze Instance { get; private set; }
    public MazeSetting[] settingArray;
    [HideInInspector]
    public MazeSetting setting = null;
    [HideInInspector]
    public int currentSettingIndex = 0;
    [HideInInspector]
    public Vector3[,] gridList;
    [HideInInspector]
    public Vector3 gridScale;
    [HideInInspector]
    public List<GameObject>[,] gridWallList;

    [HideInInspector]
    public GameObject mapCanvas;
    [HideInInspector]
    public GameObject mapRawImage;
    [HideInInspector]
    public Camera mapCamera;

    [HideInInspector]
    public GameObject mainCam;
    [HideInInspector]
    public GameObject target;
    [HideInInspector]
    public GameObject enemyAgent;

    [HideInInspector]
    public Vector3 startPosition;
    [HideInInspector]
    public Vector3 endPosition;
    [HideInInspector]
    public Vector3 randomPosition;
    [HideInInspector]
    public Vector3 destroyedWallPosition;

    void Start()
    {
        setting = settingArray[currentSettingIndex];
        mainCam = GameObject.FindWithTag("MainCamera");
        GenerateMaze();
    }

    private void Awake()
    {
        Instance = this;

    }

    public void GenerateMaze()
    {
        ResetMaze();
        Random.InitState(setting.seed);
        gridList = new Vector3[setting.row, setting.column];
        gridWallList = new List<GameObject>[setting.row, setting.column];
        CreateGrid();
        GeneratePrimMaze();
        SetupUpStartingPointAndEndingPoint();
        SetupEnemy();
        enemyAgent.SetActive(setting.enableEnemy);
        target = Instantiate(setting.aiPlayerPrefab, startPosition, Quaternion.identity, transform);
        SetupMap();
        mapCanvas.SetActive(setting.map);
    }

    public Vector3 GetRandomValidPosition()
    {
        int randomRow = Random.Range(0, setting.row);
        int randomCol = Random.Range(0, setting.column);
        Vector3 enemyPosition = gridList[randomRow, randomCol];
        enemyPosition.x += (gridScale.x / 2);
        enemyPosition.z -= (gridScale.z / 2);
        return enemyPosition;

    }

    void ResetMaze()
    {
        if (target != null)
        {
            Destroy(target);
        }
        if (gridWallList != null)
        {
            for (int i = 0; i < gridWallList.GetLength(0); i++)
            {
                for (int j = 0; j < gridWallList.GetLength(1); j++)
                {
                    List<GameObject> wallDestroy = gridWallList[i, j];
                    if (wallDestroy != null)
                    {
                        foreach (GameObject wall in wallDestroy)
                        {
                            if (wall != null)
                                Destroy(wall);
                        }
                        wallDestroy.Clear();
                    }
                }
            }
        }
        GameObject floor = GameObject.Find("MazeFloor");
        if (floor != null)
        {
            Destroy(floor);
        }
        if (mapCanvas != null)
        {
            Destroy(mapCanvas);
        }
        if (mapCamera != null)
        {
            Destroy(mapCamera.gameObject);
        }
        mapCanvas = null;
        mapCamera = null;
        if(enemyAgent != null)
        {
            Destroy(enemyAgent.gameObject);
        }

    }

    void CreateGrid()
    {
        // Generate the floor
        GameObject floor = Instantiate(setting.floorPrefab, transform);
        floor.name = "MazeFloor";
        floor.transform.localScale = new Vector3(setting.width, 0.1f, setting.height);

        // Generate the grid from the top left corner of the floor
        Vector3 tempGridPosition = floor.transform.position;
        tempGridPosition.x -= (setting.width / 2);
        tempGridPosition.y += floor.transform.localScale.y;
        tempGridPosition.z += (setting.height / 2);

        // The position of the grid is the top left corner coordinate
        Vector3 gridPosition = tempGridPosition;
        gridScale = new Vector3(setting.width / setting.column, 1f, setting.height / setting.row);
        for (int row = 0; row < setting.row; row++)
        {
            for (int col = 0; col < setting.column; col++)
            {
                gridList[row, col] = gridPosition;
                gridPosition.x += gridScale.x;
            }
            gridPosition.z -= gridScale.z;
            gridPosition.x = tempGridPosition.x;
        }

        // Initiate the wall for the four side of the grid
        for (int row = 0; row < setting.row; row++)
        {
            for (int col = 0; col < setting.column; col++)
            {
                Vector3 wallPos = gridList[row, col];
                List<GameObject> wallList = new();
                // clockwise to set up the wall
                // wall up
                GameObject wallUp = Instantiate(setting.wallPrefab, wallPos, Quaternion.identity, transform);
                wallUp.transform.localScale = new Vector3(gridScale.x, setting.depth, gridScale.z / 5);
                wallUp.transform.position = new Vector3(wallPos.x + (wallUp.transform.localScale.x / 2), wallPos.y + (setting.depth / 2), wallPos.z);
                wallUp.name = string.Format("({0},{1}) Wall Up", row, col);
                wallList.Add(wallUp);

                // wall right
                wallPos.x += gridScale.x;
                GameObject wallRight = Instantiate(setting.wallPrefab, wallPos, Quaternion.identity, transform);
                wallRight.transform.localScale = new Vector3(gridScale.x / 5, setting.depth, gridScale.z);
                wallRight.transform.position = new Vector3(wallPos.x, wallPos.y + (setting.depth / 2), wallPos.z - (wallRight.transform.localScale.z / 2));
                wallRight.name = string.Format("({0},{1}) Wall Right", row, col);
                wallList.Add(wallRight);

                // wall down
                wallPos.z -= gridScale.z;
                GameObject wallDown = Instantiate(setting.wallPrefab, wallPos, Quaternion.identity, transform);
                wallDown.transform.localScale = new Vector3(gridScale.x, setting.depth, gridScale.z / 5);
                wallDown.transform.position = new Vector3(wallPos.x - (wallDown.transform.localScale.x / 2), wallPos.y + (setting.depth / 2), wallPos.z);
                wallDown.name = string.Format("({0},{1}) Wall Down", row, col);
                wallList.Add(wallDown);

                // wall left
                wallPos.x -= gridScale.x;
                GameObject wallLeft = Instantiate(setting.wallPrefab, wallPos, Quaternion.identity, transform);
                wallLeft.transform.localScale = new Vector3(gridScale.x / 5, setting.depth, gridScale.z);
                wallLeft.transform.position = new Vector3(wallPos.x, wallPos.y + (setting.depth / 2), wallPos.z + (wallLeft.transform.localScale.z / 2));
                wallLeft.name = string.Format("({0},{1}) Wall Left", row, col);
                wallList.Add(wallLeft);
                gridWallList[row, col] = wallList;
            }
        }
    } // end of CreateGrid

    private void GeneratePrimMaze()
    {
        Vector2Int startingCell = new(Random.Range(0, setting.row - 1), Random.Range(0, setting.column - 1));
        List<Vector2Int> visitedCells = new();
        visitedCells.Add(startingCell);
        List<Vector2Int> frontierCells = new();
        AddUnvisitedNeighboringCellsToFrontier(startingCell, visitedCells, frontierCells);

        // Continue until the frontier list is empty
        while (frontierCells.Count > 0)
        {
            int randomIndex = Random.Range(0, frontierCells.Count - 1);
            Vector2Int currentCell = frontierCells[randomIndex];
            frontierCells.RemoveAt(randomIndex);
            Vector2Int neighbourCell = GetRandomVisitedNeighborCell(currentCell, visitedCells);
            CreatePassageway(currentCell, neighbourCell);
            visitedCells.Add(currentCell);
            AddUnvisitedNeighboringCellsToFrontier(currentCell, visitedCells, frontierCells);
        }
    }

    private void AddUnvisitedNeighboringCellsToFrontier(Vector2Int cell, List<Vector2Int> visitedCells, List<Vector2Int> frontierCells)
    {
        // cell above
        if (cell.x - 1 >= 0 && !visitedCells.Contains(new Vector2Int(cell.x - 1, cell.y)) && !frontierCells.Contains(new Vector2Int(cell.x - 1, cell.y)))
            frontierCells.Add(new Vector2Int(cell.x - 1, cell.y));

        // cell right
        if (cell.y + 1 < setting.column && !visitedCells.Contains(new Vector2Int(cell.x, cell.y + 1)) && !frontierCells.Contains(new Vector2Int(cell.x, cell.y + 1)))
            frontierCells.Add(new Vector2Int(cell.x, cell.y + 1));

        // cell below
        if (cell.x + 1 < setting.row && !visitedCells.Contains(new Vector2Int(cell.x + 1, cell.y)) && !frontierCells.Contains(new Vector2Int(cell.x + 1, cell.y)))
            frontierCells.Add(new Vector2Int(cell.x + 1, cell.y));

        // cell left
        if (cell.y - 1 >= 0 && !visitedCells.Contains(new Vector2Int(cell.x, cell.y - 1)) && !frontierCells.Contains(new Vector2Int(cell.x, cell.y - 1)))
            frontierCells.Add(new Vector2Int(cell.x, cell.y - 1));
    }

    private Vector2Int GetRandomVisitedNeighborCell(Vector2Int cell, List<Vector2Int> visitedCells)
    {
        List<Vector2Int> visitedNeighbours = new();

        // cell above
        if (cell.x - 1 >= 0 && visitedCells.Contains(new Vector2Int(cell.x - 1, cell.y)))
            visitedNeighbours.Add(new Vector2Int(cell.x - 1, cell.y));

        // cell right
        if (cell.y + 1 < setting.column && visitedCells.Contains(new Vector2Int(cell.x, cell.y + 1)))
            visitedNeighbours.Add(new Vector2Int(cell.x, cell.y + 1));

        // cell below
        if (cell.x + 1 < setting.row && visitedCells.Contains(new Vector2Int(cell.x + 1, cell.y)))
            visitedNeighbours.Add(new Vector2Int(cell.x + 1, cell.y));

        // cell left
        if (cell.y - 1 >= 0 && visitedCells.Contains(new Vector2Int(cell.x, cell.y - 1)))
            visitedNeighbours.Add(new Vector2Int(cell.x, cell.y - 1));

        // the cell cannot find any visited cell
        if (visitedNeighbours.Count == 0) return Vector2Int.zero;

        int randomIndex = Random.Range(0, visitedNeighbours.Count - 1);
        return visitedNeighbours[randomIndex];
    }

    private void CreatePassageway(Vector2Int cell1, Vector2Int cell2)
    {
        // Determine the orientation of the wall
        if (cell1.x == cell2.x) // same row
        {
            if (cell1.y < cell2.y) // cell1 at left, cell2 at right
            {
                DestroyWall(cell1, 1);
                DestroyWall(cell2, 3);
            }
            else
            {
                DestroyWall(cell1, 3);
                DestroyWall(cell2, 1);
            }
        }
        else // same column
        {
            if (cell1.x < cell2.x) // cell1 at above, cell2 at below
            {
                DestroyWall(cell1, 2);
                DestroyWall(cell2, 0);
            }
            else
            {
                DestroyWall(cell1, 0);
                DestroyWall(cell2, 2);
            }
        }
    }

    private void SetupUpStartingPointAndEndingPoint()
    {
        Vector2Int start = Vector2Int.zero;
        Vector2Int end = Vector2Int.zero;
        switch (setting.startingSide)
        {
            case MazeSetting.WallFrom.Up:
                start.y = setting.startingIndex - 1;
                setting.startWall = gridWallList[start.x, start.y][0];
                destroyedWallPosition = setting.startWall.transform.localPosition;
                DestroyWall(start, 0);
                break;
            case MazeSetting.WallFrom.Down:
                start.x = setting.row - 1;
                start.y = setting.startingIndex - 1;
                setting.startWall = gridWallList[start.x, start.y][2];
                destroyedWallPosition = setting.startWall.transform.localPosition;
                DestroyWall(start, 2);
                break;
            case MazeSetting.WallFrom.Left:
                start.x = setting.startingIndex - 1;
                setting.startWall = gridWallList[start.x, start.y][3];
                destroyedWallPosition = setting.startWall.transform.localPosition;
                DestroyWall(start, 3);
                break;
            case MazeSetting.WallFrom.Right:
                start.x = setting.startingIndex - 1;
                start.y = setting.column - 1;
                setting.startWall = gridWallList[start.x, start.y][1];
                destroyedWallPosition = setting.startWall.transform.localPosition;
                DestroyWall(start, 1);
                break;
        }
        switch (setting.endingSide)
        {
            case MazeSetting.WallFrom.Up:
                end.y = setting.endingIndex - 1;
                DestroyWall(end, 0);
                break;
            case MazeSetting.WallFrom.Down:
                end.x = setting.row - 1;
                end.y = setting.endingIndex - 1;
                DestroyWall(end, 2);
                break;
            case MazeSetting.WallFrom.Left:
                end.x = setting.endingIndex - 1;
                DestroyWall(end, 3);
                break;
            case MazeSetting.WallFrom.Right:
                end.x = setting.endingIndex - 1;
                end.y = setting.column - 1;
                DestroyWall(end, 1);
                break;
        }
        setting.startingPoint = start;
        setting.endingPoint = end;

        Vector3 gridStart = gridList[start.x, start.y];
        gridStart.x += (gridScale.x / 2);
        gridStart.z -= (gridScale.z / 2);
        startPosition = gridStart;

        Vector3 gridEnd = gridList[end.x, end.y];
        gridEnd.x += (gridScale.x / 2);
        gridEnd.z -= (gridScale.z / 2);
        endPosition = gridEnd;
    }

    void DestroyWall(Vector2Int grid, int side)
    {
        GameObject wallDestroy;
        wallDestroy = gridWallList[grid.x, grid.y][side];
        gridWallList[grid.x, grid.y][side] = null;
        Destroy(wallDestroy);
    }

    private void SetupMap()
    {
        // Create a new GameObject for the map canvas
        mapCanvas = new GameObject("MapCanvas");
        mapCanvas.transform.parent = transform;

        // Add a Canvas component to the map canvas GameObject
        Canvas canvas = mapCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(800, 600);
        canvas.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;

        // Add a RawImage component to the map canvas GameObject
        mapRawImage = new GameObject("MapRawImage");
        mapRawImage.AddComponent<RawImage>();
        mapRawImage.transform.SetParent(mapCanvas.transform);


        // Set the position and size of the RawImage
        RectTransform mapRectTransform = mapRawImage.GetComponent<RawImage>().rectTransform;
        mapRectTransform.sizeDelta = new Vector2(setting.mapWidth, setting.mapHeight); // Set map size

        Vector2 pos = Vector2.zero;
        switch (setting.mapPosition)
        {
            case MazeSetting.MapPosition.TopLeft:
                pos = Vector2.up;
                break;
            case MazeSetting.MapPosition.TopRight:
                pos = Vector2.one;
                break;
            case MazeSetting.MapPosition.BottomLeft:
                pos = Vector2.zero;
                break;
            case MazeSetting.MapPosition.BottomRight:
                pos = Vector2.right;
                break;
            default:
                break;
        }

        mapRectTransform.anchorMin = pos;
        mapRectTransform.anchorMax = pos;
        mapRectTransform.pivot = pos;
        mapRectTransform.anchoredPosition = pos;

        // Calculate the orthographic size of the camera based on the dimensions of the real maze
        float orthoSize;
        if (setting.height >= setting.width)
            orthoSize = setting.height / 2 + 1;
        else
            orthoSize = setting.width / 2 + 1;

        // Create a camera for rendering the map view
        mapCamera = new GameObject("MapCamera").AddComponent<Camera>();
        Camera setupCam = mapCamera.GetComponent<Camera>();
        setupCam.clearFlags = CameraClearFlags.SolidColor;
        setupCam.orthographic = true;
        setupCam.farClipPlane = 100;
        setupCam.nearClipPlane = 1f;
        setupCam.orthographicSize = orthoSize;
        setupCam.depth = -1;
        setupCam.targetTexture = setting.mapRender;

        mapCamera.transform.parent = transform;
        mapCamera.transform.SetPositionAndRotation(new Vector3(0, setting.depth * 3, 0), Quaternion.AngleAxis(90, new Vector3(1, 0, 0)));


        // Set the mapRenderTexture as the texture for the RawImage
        mapRawImage.GetComponent<RawImage>().texture = setting.mapRender;

        // Ensure the camera is active
        mapCamera.gameObject.SetActive(true);

    }

    private void SetupEnemy()
    {
        enemyAgent = new GameObject("EnemyAgent");
        enemyAgent.transform.parent = transform;
        enemyAgent.gameObject.SetActive(true);
        randomPosition = GetRandomValidPosition();
    }

    void Update()
    {
        if (setting.map)
        {
            // Render the map view
            mapCamera.Render();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SceneManager.LoadScene(0);
        }

        // ****** Temporarily for the camera to shoot on a prefab that is on the starting point *******
        if (target != null && mainCam != null)
        {
            mainCam.transform.position = new Vector3(target.transform.position.x, target.GetComponent<CapsuleCollider>().height, target.transform.position.z);
        }
    }
}
