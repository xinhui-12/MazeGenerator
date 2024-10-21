using Unity.MLAgents.Policies;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MazeUIController : MonoBehaviour
{
    public TMP_Dropdown dropdownDifficulty;
    public TMP_InputField seedText;
    public Toggle mapToggle;
    public Maze mazeScript;
    public Toggle enemyToggle;
    public Button playButton;
    public MoveToTheGoalAgent playerAgent;

    void Start()
    {
        seedText.text = mazeScript.setting.seed.ToString();
        mapToggle.isOn = mazeScript.setting.map;
        enemyToggle.isOn = mazeScript.setting.enableEnemy;
    }

    public void ToggleControlMode()
    {
        Debug.Log("ToggleControlMode called");

        if (playerAgent != null)
        {
            playerAgent.SetBehaviorType(BehaviorType.InferenceOnly);
            mazeScript.GenerateMaze();
            enemyToggle.isOn = mazeScript.setting.enableEnemy;
        }
    }

    public void DifficultyValueChanged(TMP_Dropdown dropdown)
    {
        mazeScript.currentSettingIndex = dropdown.value;
        mazeScript.setting = mazeScript.settingArray[mazeScript.currentSettingIndex];
        mazeScript.GenerateMaze();
        seedText.text = mazeScript.setting.seed.ToString();
        mapToggle.isOn = mazeScript.setting.map;
        enemyToggle.isOn = mazeScript.setting.enableEnemy;
        playerAgent.SetBehaviorType(BehaviorType.HeuristicOnly);
        if(dropdown.value != 0)
        {
            playButton.gameObject.SetActive(false); //Hide the play button
        }
        else
        {
            playButton.gameObject.SetActive(true); //Show the play button
        }
    }

    public void ValidateSeedValueChanged(TMP_InputField input)
    {
        if(input.text.Length > 0)
        {
            // Only can key in numbers
            if (!int.TryParse(input.text, out int intValue))
            {
                input.text = input.text.Substring(0, input.text.Length - 1);
            }
        }
    }

    public void ChangeSeedValue(TMP_InputField input)
    {
        mazeScript.setting.seed = int.Parse(input.text);
        mazeScript.GenerateMaze();
        enemyToggle.isOn = false;
        playerAgent.SetBehaviorType(BehaviorType.HeuristicOnly);
    }

    public void MapToggleChanged(Toggle mapToggle)
    {
        mazeScript.mapCanvas.SetActive(mapToggle.isOn);
        mazeScript.setting.map = mapToggle.isOn;
    }

    public void EnemyToggleChanged(Toggle enemyToggle)
    {
        // Update the setting in the Maze script based on the toggle state
        mazeScript.setting.enableEnemy = enemyToggle.isOn;

        if (enemyToggle.isOn)
        {
            // Toggle is checked, instantiate the enemy agent if it's not already instantiated
            if (mazeScript.setting.aiEnemyPrefab != null && mazeScript.enemyAgent == null)
            {
                Vector3 randomPosition = mazeScript.GetRandomValidPosition();
                mazeScript.enemyAgent = Instantiate(mazeScript.setting.aiEnemyPrefab, randomPosition, Quaternion.identity, mazeScript.transform);
            }
        }
        else
        {
            // Toggle is unchecked, destroy the enemy agent if it's instantiated
            if (mazeScript.enemyAgent != null)
            {
                Destroy(mazeScript.enemyAgent);
                mazeScript.enemyAgent = null;
            }
        }
    }


}
