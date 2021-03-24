using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : MonoBehaviour, ISteeringBehaviour
{
    SteeringController controller;

    void Start()
    {
        controller = GetComponent<SteeringController>();
    }

    public Vector3 ApplyForce(Vector3 target)
    {
        Vector3 desiredVelocity = ((target - transform.position).normalized * controller.maxSpeed);

        return desiredVelocity - controller.rb.velocity;
    }
}
