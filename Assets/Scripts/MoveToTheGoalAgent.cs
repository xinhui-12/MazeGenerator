using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using Unity.MLAgents.Policies;

public class MoveToTheGoalAgent : Agent
{
    private BehaviorType currentBehaviourType = BehaviorType.HeuristicOnly;
    private MazeSetting mazeSetting = null;
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private float originalYPosition; // Store the original y position of the agent
    private Vector3 initialScale; //Store the initial scale of the agent
    private float previousDistance;
    private float currentDistance;
    private CameraControl cameraControl;
    private BehaviorParameters behaviorParameters;

    public override void Initialize()
    {
        mazeSetting = Maze.Instance.setting;
        originalYPosition = transform.localPosition.y;
        behaviorParameters = GetComponent<BehaviorParameters>();
        currentBehaviourType = behaviorParameters.BehaviorType;
        SetBehaviorType(currentBehaviourType);
        //Store the initial scale
        initialScale = transform.localScale;
        UpdateAgentScale();

        // Find and assign the FirstPersonCamera script attached to the main camera
        cameraControl = Camera.main.GetComponent<CameraControl>();
        // Set the player reference in the FirstPersonCamera script
        if (cameraControl != null)
        {
            cameraControl.playerAgent = transform; // Assign the agent's transform
        }
    }

    //Adjust  agent size based on the maze
    private void UpdateAgentScale()
    {
        if (Maze.Instance.setting != null)
        {
            float gridCellWidth = Maze.Instance.setting.width / Maze.Instance.setting.column;
            float gridCellHeight = Maze.Instance.setting.height / Maze.Instance.setting.row;

            float scaleFactor = Mathf.Min(gridCellWidth, gridCellHeight);

            initialScale = new Vector3(scaleFactor / 3, scaleFactor / 3, scaleFactor / 3);
        }
    }


    public override void OnEpisodeBegin()
    {
        transform.localPosition = Maze.Instance.startPosition;
        transform.rotation = Quaternion.identity; // Reset rotation to face forward
        visitedCells.Clear();

        //Initialize distance variable
        previousDistance = Vector3.Distance(transform.localPosition, Maze.Instance.endPosition);
        currentDistance = previousDistance;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position); //to get agent position
       sensor.AddObservation(Maze.Instance.endPosition); //to get the end point position
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float moveSpeed = 5f;

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        //Move the agent
        transform.position += moveDirection * Time.deltaTime * moveSpeed;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 180f);
        }

        //Update current distance to end point
        currentDistance = Vector3.Distance(transform.localPosition, Maze.Instance.endPosition);

        //Check if the agent is getting closer to the end point
        if (currentDistance < previousDistance)
        {
            AddReward(0.1f);
        }

        //Check the distance between agent and the end point
        float distanceToEndPoint = Vector3.Distance(transform.localPosition, Maze.Instance.endPosition);
        float distanceThreshold = 1.5f; //represent the max distance allow btw agent and end point

        if (distanceToEndPoint < distanceThreshold)
        {
            SetReward(+1f);
            EndEpisode();
            behaviorParameters.BehaviorType=  BehaviorType.HeuristicOnly;
            return;
        }

        //Check for collision with walls using raycasting
        RaycastHit hitEnemy;
        if (Physics.Raycast(transform.position, transform.forward, out hitEnemy, 1.0f))
        {
            if (hitEnemy.collider.CompareTag("Enemy"))
            {
                EndEpisode();
            }
        }

        //Check for collision with walls using raycasting
        RaycastHit hitWall;
        if (Physics.Raycast(transform.position, transform.forward, out hitWall, 0.7f))
        {
            if (hitWall.collider.CompareTag("Wall"))
            {
                SetReward(-1.0f);
                EndEpisode();
            }
        }

        // Reward for cisiting new cells
        Vector2Int newCell = new Vector2Int((int)transform.position.x, (int)transform.position.z);
        if (!visitedCells.Contains(newCell))
        {
            visitedCells.Add(newCell);
            AddReward(0.5f);
        }

        //Check if agent out of bounds
        if (transform.localPosition.y < originalYPosition - 1.0f)
        {
            SetReward(-1f);
            EndEpisode();
        }

        //Avoid agent fall out from the mazes
        float outBoundaryPoint = Vector3.Distance(transform.localPosition, Maze.Instance.destroyedWallPosition);
        if (outBoundaryPoint < 1.0f)
        {
            SetReward(-1f);
            EndEpisode();
        }

        //Update previous distance for the next step
        previousDistance = currentDistance;

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        // Map player input to continuous actions
        continuousActions[0] = Input.GetAxisRaw("Horizontal"); // X-axis movement
        continuousActions[1] = Input.GetAxisRaw("Vertical");   // Z-axis movement
    }

    public void SetBehaviorType(BehaviorType behaviorType)
    {
        currentBehaviourType = behaviorType;
        behaviorParameters = GetComponent<BehaviorParameters>();
        if (behaviorParameters != null)
        {
            behaviorParameters.BehaviorType = currentBehaviourType;
        }
    }

}