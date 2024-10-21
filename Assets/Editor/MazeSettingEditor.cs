
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeSetting))]
public class MazeSettingEditor : Editor
{
    SerializedProperty height;
    SerializedProperty width;
    SerializedProperty depth;
    SerializedProperty row;
    SerializedProperty column;
    SerializedProperty wallPrefab;
    SerializedProperty floorPrefab;
    SerializedProperty seed;

    SerializedProperty startingSide;
    SerializedProperty startingIndex;
    SerializedProperty endingSide;
    SerializedProperty endingIndex;

    SerializedProperty map;
    SerializedProperty mapRender;
    SerializedProperty mapPosition;
    SerializedProperty mapHeight;
    SerializedProperty mapWidth;

    SerializedProperty aiPlayerPrefab;
    SerializedProperty aiEnemyPrefab;
    SerializedProperty enableEnemy;

    readonly GUIContent rowString = new("Row", "The number of rows should able to dividing completely with the width given above.");
    readonly GUIContent columnString = new("Column", "The number of columns should able to dividing completely with the height given above.");
    readonly GUIContent startingSideString = new("Starting Side", "The border that will indicate as the starting.");
    readonly GUIContent startingIndexString = new("Starting Index", "According to the starting side, select which cell in sequence to be the starting index.");
    readonly GUIContent endingSideString = new("Ending Side", "The border that will indicate as the ending.");
    readonly GUIContent endingIndexString = new("Ending Index", "According to the ending side, select which cell in sequence to be the ending index.");
    readonly GUIContent mapPositionString = new("Map Position", "The position of the 2D map shown.");
    readonly GUIContent aiPlayerPrefabString = new("AI Player Prefab", "A player prefab object for the AI.");
    readonly GUIContent aiEnemyPrefabString = new("AI Enemy Prefab", "An enemy prefab object for the AI.");

    void OnEnable()
    {
        height = serializedObject.FindProperty("height");
        width = serializedObject.FindProperty("width");
        depth = serializedObject.FindProperty("depth");
        row = serializedObject.FindProperty("row");
        column = serializedObject.FindProperty("column");
        wallPrefab = serializedObject.FindProperty("wallPrefab");
        floorPrefab = serializedObject.FindProperty("floorPrefab");
        seed = serializedObject.FindProperty("seed");

        startingSide = serializedObject.FindProperty("startingSide");
        startingIndex = serializedObject.FindProperty("startingIndex");
        endingSide = serializedObject.FindProperty("endingSide");
        endingIndex = serializedObject.FindProperty("endingIndex");

        map = serializedObject.FindProperty("map");
        mapRender = serializedObject.FindProperty("mapRender");
        mapPosition = serializedObject.FindProperty("mapPosition");
        mapHeight = serializedObject.FindProperty("mapHeight");
        mapWidth = serializedObject.FindProperty("mapWidth");

        aiPlayerPrefab = serializedObject.FindProperty("aiPlayerPrefab");
        aiEnemyPrefab = serializedObject.FindProperty("aiEnemyPrefab");
        enableEnemy = serializedObject.FindProperty("enableEnemy");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(depth);

        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();
        int maxRowNum = height.intValue;
        row.intValue = EditorGUILayout.IntSlider(rowString, row.intValue, 1, height.intValue);
        int maxColumnNum = width.intValue;
        column.intValue = EditorGUILayout.IntSlider(columnString, column.intValue, 1, maxColumnNum);
        if (EditorGUI.EndChangeCheck())
        {
            row.intValue = Mathf.Clamp(row.intValue, 1, maxRowNum);
            column.intValue = Mathf.Clamp(column.intValue, 1, maxColumnNum);
            int gridScaleRow = height.intValue % row.intValue;
            int gridScaleColumn = width.intValue % column.intValue;
            if (gridScaleRow != 0)
            {
                int changedScaleRow = height.intValue / row.intValue;
                row.intValue = height.intValue / changedScaleRow;
            }
            if (gridScaleColumn != 0)
            {
                int changeScaleColumn = width.intValue / column.intValue;
                column.intValue = width.intValue / changeScaleColumn;
            }

        }

        EditorGUILayout.PropertyField(wallPrefab);
        EditorGUILayout.PropertyField(floorPrefab);
        EditorGUILayout.PropertyField(seed);

        EditorGUILayout.Space();

        startingSide.enumValueIndex = EditorGUILayout.Popup(startingSideString, startingSide.enumValueIndex, startingSide.enumDisplayNames);

        // Display startingIndex property with maximum range based on row if the startingSide is left/right
        if (startingSide.enumValueIndex == (int)MazeSetting.WallFrom.Left || startingSide.enumValueIndex == (int)MazeSetting.WallFrom.Right)
        {
            EditorGUI.BeginChangeCheck();
            startingIndex.intValue = EditorGUILayout.IntSlider(startingIndexString, startingIndex.intValue, 1, row.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                startingIndex.intValue = Mathf.Clamp(startingIndex.intValue, 1, row.intValue);
            }
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            startingIndex.intValue = EditorGUILayout.IntSlider(startingIndexString, startingIndex.intValue, 1, column.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                startingIndex.intValue = Mathf.Clamp(startingIndex.intValue, 1, column.intValue);
            }
        }

        endingSide.enumValueIndex = EditorGUILayout.Popup(endingSideString, endingSide.enumValueIndex, endingSide.enumDisplayNames);

        // Display endingIndex property with maximum range based on row if the endingSide is left/right
        if (endingSide.enumValueIndex == (int)MazeSetting.WallFrom.Left || endingSide.enumValueIndex == (int)MazeSetting.WallFrom.Right)
        {
            EditorGUI.BeginChangeCheck();
            endingIndex.intValue = EditorGUILayout.IntSlider(endingIndexString, endingIndex.intValue, 1, row.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                endingIndex.intValue = Mathf.Clamp(endingIndex.intValue, 1, row.intValue);
            }
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            endingIndex.intValue = EditorGUILayout.IntSlider(endingIndexString, endingIndex.intValue, 1, column.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                endingIndex.intValue = Mathf.Clamp(endingIndex.intValue, 1, column.intValue);
            }
        }


        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(map);

        if (map.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(mapRender);
            mapPosition.enumValueIndex = EditorGUILayout.Popup(mapPositionString, mapPosition.enumValueIndex, mapPosition.enumDisplayNames);
            EditorGUILayout.PropertyField(mapHeight);
            EditorGUILayout.PropertyField(mapWidth);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(aiPlayerPrefab);
        EditorGUILayout.PropertyField(enableEnemy);
        if (enableEnemy.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(aiEnemyPrefab);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

}