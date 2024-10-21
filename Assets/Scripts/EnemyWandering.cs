using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;

public class EnemyAgent : Agent
{
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private MazeSetting mazeSetting = null;
    private float originalYPosition;
    private Vector3 startPosition;
    private Vector3 endPosition;

    public override void OnEpisodeBegin()
    {
        mazeSetting = Maze.Instance.setting;
        transform.localPosition = Maze.Instance.randomPosition;
        transform.rotation = Quaternion.identity;
        visitedCells.Clear();
        startPosition = Maze.Instance.startPosition;
        endPosition = Maze.Instance.endPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position); // Agent position
        sensor.AddObservation(Maze.Instance.endPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float moveSpeed = 2f;

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // Move the agent
        transform.position += moveDirection * Time.deltaTime * moveSpeed;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 180f);
        }

        // Check if agent reaches the start point or end point
        float distanceToStart = Vector3.Distance(transform.localPosition, startPosition);
        float distanceToEnd = Vector3.Distance(transform.localPosition, endPosition);
        float distanceThreshold = 1.5f;

        if (distanceToEnd < distanceThreshold)
        {
            SetReward(-1f); // Penalize agent for reaching start or end point
            EndEpisode();
        }

        if (distanceToStart < distanceThreshold )
        {
            SetReward(-1f); // Penalize agent for reaching start or end point
            EndEpisode();
        }

        // Reward for visiting new cells
        Vector2Int newCell = new Vector2Int((int)transform.position.x, (int)transform.position.z);
        if (!visitedCells.Contains(newCell))
        {
            visitedCells.Add(newCell);
            AddReward(1.0f);
        }

        // Check for collision with walls using raycasting
        RaycastHit hitWall;
        if (Physics.Raycast(transform.position, transform.forward, out hitWall, 0.5f))
        {
            if (hitWall.collider.CompareTag("Wall"))
            {
                SetReward(-0.5f);
                EndEpisode();
            }
        }

        // Avoid falling out from the maze
        float outBoundaryPoint = Vector3.Distance(transform.localPosition, Maze.Instance.destroyedWallPosition);
        if (outBoundaryPoint < 1.0f)
        {
            SetReward(-1f);
            EndEpisode();
        }

        // Check if agent is out of bounds
        if (transform.localPosition.y < originalYPosition - 1.0f)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
}