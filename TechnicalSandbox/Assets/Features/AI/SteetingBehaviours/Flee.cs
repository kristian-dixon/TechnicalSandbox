using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flee : MonoBehaviour, ISteeringBehaviour
{
    public float panicDistance = 10f;
    SteeringController controller;

    void Start()
    {
        controller = GetComponent<SteeringController>();
    }

    public Vector3 ApplyForce(Vector3 target)
    {
        float dist = Vector3.Distance(target, transform.position);
        if(dist > panicDistance)
        {
            return Vector3.zero;
        }

        Vector3 desiredVelocity = ((transform.position - target).normalized * controller.maxSpeed);

        return desiredVelocity - controller.rb.velocity;
    }
}
