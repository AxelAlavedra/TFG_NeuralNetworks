using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Axel.NeuralNetworks;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RLCarAgent : Agent
{
    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    public CheckpointHandler checkpointHandler;
    private KartMovement kartMovement;

    private float distanceTravelled = 0.0f;

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        kartMovement = GetComponent<KartMovement>();
        checkpointHandler.checkpointReached += OnCheckpointReached;

        // If not training mode, no max step, play forever
        if (!trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        Vector3 newPosition;
        Quaternion newRotation;

        if (trainingMode)
            checkpointHandler.AssignRandomCheckpoint(out newPosition, out newRotation);
        else
            checkpointHandler.AssignStartCheckpoint(out newPosition, out newRotation);

        kartMovement.Reset(newPosition, newRotation);
    }

    /// <summary>
    /// Called when an action is received from either player input or neural network
    /// 
    /// vectorAction[i] represents:
    /// Index 0: move vector y (+1 = up, -1 = down)
    /// Index 1: move vector x (+1 = right, -1 = left, 0 = no movement)
    /// </summary>
    /// <param name="vectorAction">The actions to take</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Pass the actions to the kart movement script as inputs
        kartMovement.SetInputs(actions.ContinuousActions[0], actions.ContinuousActions[1]);

        //Calculate dot to check if kart is moving in direction to the next checkpoint
        float reward = Vector3.Dot(kartMovement.sphere.velocity.normalized, checkpointHandler.DirectionToCheckpoint);

        // Add rewards if the agent is heading in the right direction
        AddReward(reward * 0.03f);

        // Add reward based on current velocity to promote faster driving
        AddReward((kartMovement.sphere.velocity.magnitude / 15.5f) * 0.02f);
    }

    /// <summary>
    /// Collect observations from environment
    /// </summary>
    /// <param name="sensor">Vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the agent's local rotation (4 observations)
        //sensor.AddObservation(transform.localRotation.normalized);

        // Observe the agent's speed (3 observations)
        sensor.AddObservation(kartMovement.sphere.velocity.normalized);

        // Observe distance to current checkpoint (1 observation)
        sensor.AddObservation(checkpointHandler.DistanceToCheckpoint);

        // Observe direction to current checkpoint (3 observations)
        sensor.AddObservation(checkpointHandler.DirectionToCheckpoint);

        // Observe dot product to indicate if moving to current checkpoint (1 observation)
        sensor.AddObservation(Vector3.Dot(kartMovement.sphere.velocity.normalized, checkpointHandler.DirectionToCheckpoint));

        // 8 total observations
    }

    /// <summary>
    /// When the behaviours type is set to "heuristic only" on the agent's behaviour parameters,
    /// this function will be called. 
    /// It's return values will be fed into <see cref="OnActionReceived(float[])"/> instead of using the neural network
    /// </summary>
    /// <param name="actionsOut">An output action array</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
    }

    public void OnCheckpointReached()
    {
        if(trainingMode)
        {
            // Add reward for getting to the checkpoint
            AddReward(2.5f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (trainingMode && (collision.collider.CompareTag("Wall") || collision.collider.CompareTag("Player")))
        {
            //If agent collides with wall or other player add negative reward and end episode
            AddReward(-1.0f);
            EndEpisode();
        }

    }
}
