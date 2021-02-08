﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySensor : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Amount of rays that will be casted")]
    private int rays = 5;
    [SerializeField]
    [Tooltip("Amount of rays that will be casted")]
    private int rayLength = 5;
    [SerializeField]
    [Tooltip("The amount of degrees where rays will be casted in")]
    private int degrees = 90;
    [SerializeField]
    [Tooltip("LayerMask used for hit detection")]
    private LayerMask layerMask;

    float[] rayInputs;

    private void Start()
    {
        rayInputs = new float[rays + 1];
    }

    public void AnalyzeSensors()
    {
        if (rays == 1)
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, rayLength, layerMask.value))
                rayInputs[0] = (10 - hit.distance) / rayLength;
            else
                rayInputs[0] = 0;
        }
        else
        {
            float anglePerRay = degrees / (rays - 1);
            float currentAngle = degrees * 0.5f;

            for (int i = 0; i < rays; i++)
            {
                Vector3 direction = Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward;
                RaycastHit hit;
                if (Physics.Raycast(transform.position, direction, out hit, rayLength, layerMask.value))
                    rayInputs[i] = (10 - hit.distance) / rayLength;
                else
                    rayInputs[i] = 0;

                currentAngle -= anglePerRay;
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (rays == 1)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * rayLength);
            return;
        }

        float anglePerRay = degrees / (rays - 1);
        float currentAngle = degrees * 0.5f;

        for (int i = 0; i < rays; i++)
        {
            Vector3 direction = (Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward) * rayLength;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, direction);

            currentAngle -= anglePerRay;
        }
    }
}
