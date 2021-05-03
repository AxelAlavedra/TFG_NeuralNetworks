using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using System;

namespace Axel.NeuralNetworks
{
    public class CheckpointHandler : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [Tooltip("Path of the current circuit")]
        public PathCreator pathCreator;
        [Tooltip("Agent GameObject")]
        public GameObject agentGO;
        [Tooltip("Activate/Deactive debug drawing of checkpoints")]
        public float checkpointMinDistance = 4.0f;
        [Tooltip("Activate/Deactive debug drawing of checkpoints")]
        public bool debugDraw = false;
        [Tooltip("The radius of the debug sphere for the checkpoints")]
        public float debugSphereRadius = 1.0f;

        [Header("Metrics")]
        [Tooltip("The distance to the next checkpoint to reach")]
        [SerializeField]
        private float distanceToCheckpoint = 0.0f;
        [Tooltip("The direction to the next checkpoint to reach")]
        private Vector3 directionToCheckpoint;
        [Tooltip("The distance travelled so far")]
        [SerializeField]
        private float distanceTravelled = 0.0f;

        [Tooltip("The current checkpoint index")]
        [SerializeField]
        private int currentCheckpoint = 1;

        public float DistanceToCheckpoint { get { return distanceToCheckpoint; } }
        public Vector3 DirectionToCheckpoint { get { return (checkpoints[currentCheckpoint] - agentGO.transform.position).normalized; } }
        public delegate void OnCheckpointReached();
        public OnCheckpointReached checkpointReached = null;

        private List<Vector3> checkpoints;

        float lapTime = 0.0f;

        float totalTime = 0.0f;
        float bestLapTime = 999.0f;

        
        // Start is called before the first frame update
        void Awake()
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
            //distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(agentGO.transform.position);
            //currentPositionOnPath = pathCreator.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Loop);
            //directionOfPath = pathCreator.path.GetDirectionAtDistance(distanceTravelled, EndOfPathInstruction.Loop);
            //distanceToCheckpoint = Mathf.Abs(pathCreator.path.GetClosestDistanceAlongPath(checkpoints[currentCheckpoint]) - distanceTravelled);

            //Metrics calculation without pathcreator
            distanceToCheckpoint = Vector3.Distance(agentGO.transform.position, checkpoints[currentCheckpoint]);
            directionToCheckpoint = (checkpoints[currentCheckpoint] - agentGO.transform.position).normalized;

            //Check if the current checkpoint has been reached, if so, increment checkpoint
            if (distanceToCheckpoint < checkpointMinDistance)
            {
                CheckpointReached();
            }
        }

        private void FixedUpdate()
        {
            lapTime += Time.fixedDeltaTime;
            totalTime += Time.fixedDeltaTime;
        }

        internal void Reset()
        {
            lapTime = totalTime = bestLapTime = 0.0f;
            currentCheckpoint = 1;
        }

        private void CheckpointReached()
        {
            if(currentCheckpoint == 0)
            {
                if (lapTime < bestLapTime)
                    bestLapTime = lapTime;

                lapTime = 0.0f;
            }

            currentCheckpoint++;
            if (currentCheckpoint >= checkpoints.Count)
            {
                currentCheckpoint = 0;
            }

            checkpointReached?.Invoke();
        }

        internal void AssignRandomCheckpoint(out Vector3 newPosition, out Quaternion newRotation)
        {
            int newCheckpoint = UnityEngine.Random.Range(0, checkpoints.Count - 1);
            currentCheckpoint = newCheckpoint + 1;
            if (currentCheckpoint >= checkpoints.Count)
                currentCheckpoint = 0;

            float r = (checkpointMinDistance - 0.5f) * Mathf.Sqrt(UnityEngine.Random.Range(0.0f, 1.0f));
            float theta = UnityEngine.Random.Range(0.0f, 1.0f) * 2.0f * Mathf.PI;
            newPosition = checkpoints[newCheckpoint] + new Vector3(r * Mathf.Cos(theta), 0.0f, r * Mathf.Sin(theta));

            float checkpointDistance = pathCreator.path.GetClosestDistanceAlongPath(newPosition);
            Vector3 checkpointForward = pathCreator.path.GetDirectionAtDistance(checkpointDistance, EndOfPathInstruction.Loop);
            newRotation = Quaternion.LookRotation(checkpointForward);
        }

        internal void AssignStartCheckpoint(out Vector3 startPosition, out Quaternion startRotation)
        {
            currentCheckpoint = 1;
            startPosition = checkpoints[0];

            float checkpointDistance = 0;
            Vector3 checkpointForward = pathCreator.path.GetDirectionAtDistance(checkpointDistance, EndOfPathInstruction.Loop);
            startRotation = Quaternion.LookRotation(checkpointForward);
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
            /*bool outOfCircuit = Vector3.Distance(agentGO.transform.position, currentPositionOnPath) > checkpointMinDistance;
            Gizmos.color = outOfCircuit ? Color.red : Color.blue;
            Gizmos.DrawSphere(currentPositionOnPath, debugSphereRadius);
            Gizmos.DrawLine(currentPositionOnPath + Vector3.up * 0.5f, (currentPositionOnPath + Vector3.up * 0.5f) + (directionOfPath * 2f));*/

            /*for (int i = 0; i < pathCreator.bezierPath.NumAnchorPoints; i++)
              {
                  Vector3 wPos = pathCreator.transform.TransformPoint(pathCreator.bezierPath.GetPointsInSegment(i)[0]);
                  Gizmos.DrawSphere(wPos, debugSphereRadius);
              }*/


        }

        private void OnGUI()
        {
            string minutes = Mathf.Floor(lapTime / 60).ToString("00");
            string seconds = (lapTime % 60).ToString("00");
            GUI.Label(new Rect(10, 10, 150, 20), "Lap Time: " + string.Format("{0}:{1}", minutes, seconds));

            minutes = Mathf.Floor(totalTime / 60).ToString("00");
            seconds = (totalTime % 60).ToString("00");
            GUI.Label(new Rect(10, 40, 150, 20), "Total Time: " + string.Format("{0}:{1}", minutes, seconds));

            if(bestLapTime < 999.0f)
            {
                minutes = Mathf.Floor(bestLapTime / 60).ToString("00");
                seconds = (bestLapTime % 60).ToString("00");
                GUI.Label(new Rect(10, 70, 150, 20), "Best Lap: " + string.Format("{0}:{1}", minutes, seconds));
            }

        }
    }
}

