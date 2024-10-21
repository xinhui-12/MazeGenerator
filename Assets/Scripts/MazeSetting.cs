using UnityEngine;

[CreateAssetMenu(fileName = "MazeSetting", menuName = "MazeSetting", order = 1)]
public class MazeSetting : ScriptableObject
{
    [Tooltip("The z value of the maze within 10 to 30.")]
    [Range(10, 30)]
    public int height = 10;

    [Tooltip("The x value of the maze within 10 to 30.")]
    [Range(10, 30)]
    public int width = 10;

    [Tooltip("The y value of the maze within 3 to 20.")]
    [Range(3, 20)]
    public int depth = 5;

    [Space]
    // Tooltip string need to setup up in editor as it will overwrite the tooltip that set in this script
    public int row = 10;
    public int column = 10;

    [Space]
    [Tooltip("A prefab object for the maze's wall.")]
    public GameObject wallPrefab = null;
    [Tooltip("A prefab object for the maze's floor.")]
    public GameObject floorPrefab = null;
    [Space]
    [Tooltip("A random value to generate the maze. Same seed value will generate the same pattern of the maze.")]
    public int seed;

    [Space]
    [Header("Starting Point")]
    public WallFrom startingSide;
    public int startingIndex = 0;

    // Not shown in inspector
    public Vector2Int startingPoint = Vector2Int.zero;
    public GameObject startWall;

    [Header("Ending Point")]
    public WallFrom endingSide;
    public int endingIndex = 0;

    // Not shown in inspector
    public Vector2Int endingPoint = Vector2Int.zero;

    [Header("2D Map")]
    [Tooltip("Checked to show a 2D map.")]
    public bool map = true;
    [Tooltip("Attached a render texture used for the 2D map.")]
    public RenderTexture mapRender;
    public MapPosition mapPosition;
    [Tooltip("The height of the 2D map.")]
    public int mapHeight = 100;
    [Tooltip("The width of the 2D map.")]
    public int mapWidth = 100;

    [Header("AI")]
    [Tooltip("A prefab as the player agent.")]
    public GameObject aiPlayerPrefab;
    [Tooltip("Checked to place enemy.")]
    public bool enableEnemy = true;
    [Tooltip("A prefab as the enemy agent.")]
    public GameObject aiEnemyPrefab;

    public enum MapPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum WallFrom
    {
        Up, Down, Left, Right
    }

}
