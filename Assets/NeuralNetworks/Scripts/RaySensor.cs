using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySensor : MonoBehaviour
{
    [Tooltip("Amount of rays that will be casted each side")]
    public int rays = 2;
    [Tooltip("Amount of rays that will be casted")]
    public int rayLength = 5;
    [Tooltip("The amount of degrees for each side")]
    public int degrees = 90;

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.forward * rayLength);

        if (rays <= 0)
            return;

        float anglePerRay = degrees / rays;
        float currentAngle = -degrees;

        for (int i = 0; i < rays * 2; i++)
        {
            Vector3 direction = (Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward) * rayLength;
            Gizmos.DrawRay(transform.position, direction);

            currentAngle += anglePerRay;

            if (i == rays - 1) //Change side if half of rays are done
                currentAngle = anglePerRay;
        }
    }
}
