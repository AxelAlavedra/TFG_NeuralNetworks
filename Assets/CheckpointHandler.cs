using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CheckpointHandler : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("Path of the current circuit")]
    public PathCreator pathCreator;
    [Tooltip("Activate/Deactive debug drawing of checkpoints")]
    public float checkpointMinDistance = 4.0f;
    [Tooltip("Activate/Deactive debug drawing of checkpoints")]
    public bool debugDraw = false;
    [Tooltip("The radius of the debug sphere for the checkpoints")]
    public float debugSphereRadius = 1.0f;

    [Header("Checkpoint Metrics")]
    [Tooltip("The distance to the next checkpoint to reach")]
    [SerializeField]
    private float distanceToCheckpoint = 0.0f;
    [Tooltip("The distance travelled so far")]
    [SerializeField]
    private float distanceTravelled = 0.0f;
    [Tooltip("The current checkpoint index")]
    [SerializeField]
    private int currentCheckpoint = 0;


    private List<Vector3> checkpoints;
    private Vector3 currentPositionOnPath;
    private Vector3 directionOfPath;

    //Maybe not needed


    // Start is called before the first frame update
    void Start()
    {
        checkpoints = new List<Vector3>();

        if (pathCreator)
        {
            for (int i = 0; i < pathCreator.bezierPath.NumAnchorPoints; i++)
            {
                Vector3 checkpointWorldPos = pathCreator.transform.TransformPoint(pathCreator.bezierPath.GetPointsInSegment(i)[0]);
                checkpoints.Add(checkpointWorldPos);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Metrics calculation with pathcreator
        //distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        //currentPositionOnPath = pathCreator.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Loop);
        //directionOfPath = pathCreator.path.GetDirectionAtDistance(distanceTravelled, EndOfPathInstruction.Loop);
        //distanceToCheckpoint = Mathf.Abs(pathCreator.path.GetClosestDistanceAlongPath(checkpoints[currentCheckpoint]) -  distanceTravelled);

        //Metrics calculation without pathcreator
        //distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        distanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[currentCheckpoint]);

        //Check if the current checkpoint has been reached, if so, increment checkpoint
        if (distanceToCheckpoint < checkpointMinDistance)
        {
            currentCheckpoint++;
            if (currentCheckpoint >= checkpoints.Count)
                currentCheckpoint = 0;
        }
    }

    private void OnDrawGizmos()
    {
        if (!debugDraw)
            return;

        //Iterate over anchor points of the current path selected and draw the checkpoints with their radius
        for (int i = 0; i < pathCreator.bezierPath.NumAnchorPoints; i++)
        {
            Vector3 checkpointWorldPos = pathCreator.transform.TransformPoint(pathCreator.bezierPath.GetPointsInSegment(i)[0]);

            Gizmos.color = i == currentCheckpoint ? Color.green : Color.red;
            Gizmos.DrawSphere(checkpointWorldPos, debugSphereRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(checkpointWorldPos, checkpointMinDistance);
        }

        bool outOfCircuit = Vector3.Distance(transform.position, currentPositionOnPath) > checkpointMinDistance;
        Gizmos.color = outOfCircuit? Color.red : Color.blue;
        Gizmos.DrawSphere(currentPositionOnPath, debugSphereRadius);
        Gizmos.DrawLine(currentPositionOnPath + Vector3.up * 0.5f, (currentPositionOnPath + Vector3.up * 0.5f) + (directionOfPath * 2f));

        /*for (int i = 0; i < pathCreator.bezierPath.NumAnchorPoints; i++)
          {
              Vector3 wPos = pathCreator.transform.TransformPoint(pathCreator.bezierPath.GetPointsInSegment(i)[0]);
              Gizmos.DrawSphere(wPos, debugSphereRadius);
          }*/


    }
}
