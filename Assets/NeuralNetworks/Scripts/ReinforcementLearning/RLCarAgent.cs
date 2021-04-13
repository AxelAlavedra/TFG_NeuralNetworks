using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Axel.NeuralNetworks;
using Unity.MLAgents.Sensors;

public class RLCarAgent : Agent
{
    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    public CheckpointHandler checkpointHandler;
    private new Rigidbody rigidbody;
    private CarMovement carMovement;

    private Vector3 startPosition;
    private Quaternion startRotation;

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();
        carMovement = GetComponent<CarMovement>();
        checkpointHandler.checkpointReached += OnCheckpointReached;

        startPosition = transform.position;
        startRotation = transform.rotation;

        // If not training mode, no max step, play forever
        if (!trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //Reset checkpoint
        checkpointHandler.Reset();

        //Reset movement
        carMovement.Reset(startPosition, startRotation);
    }

    /// <summary>
    /// Called when an action is received from either player input or neural network
    /// 
    /// vectorAction[i] represents:
    /// Index 0: move vector y (+1 = up, -1 = down)
    /// Index 1: move vector x (+1 = right, -1 = left, 0 = no movement)
    /// </summary>
    /// <param name="vectorAction">The actions to take</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        // Don't take action if frozen
        //if (frozen) return;
        carMovement.Move(vectorAction[0], vectorAction[1], 0.0f);
    }

    /// <summary>
    /// Collect observations from environment
    /// </summary>
    /// <param name="sensor">Vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);

        // Observe the agent's velocity (3 observations)
        sensor.AddObservation(rigidbody.velocity);

        // Observe distance to current checkpoint (1 observation)
        sensor.AddObservation(checkpointHandler.DistanceToCheckpoint);

        // Observe direction to current checkpoint (3 observations)
        sensor.AddObservation(checkpointHandler.DirectionToCheckpoint);

        // 11 total observations
    }

    /// <summary>
    /// When the behaviours type is set to "heuristic only" on the agent's behaviour parameters,
    /// this function will be called. 
    /// It's return values will be fed into <see cref="OnActionReceived(float[])"/> instead of using the neural network
    /// </summary>
    /// <param name="actionsOut">An output action array</param>
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
        actionsOut[2] = 0.0f;
    }

    public void OnCheckpointReached()
    {
        if(trainingMode)
        {
            // Calculate reward for getting to the checkpoint
            //float bonus = 2.0f * Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, -nearestFlower.FlowerUpVector.normalized));
            AddReward(2.5f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (trainingMode && collision.collider.CompareTag("Wall"))
            AddReward(-.5f);
    }
}
