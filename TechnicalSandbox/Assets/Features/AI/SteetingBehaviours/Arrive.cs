using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrive : MonoBehaviour, ISteeringBehaviour
{
    public enum ArrivalSpeed { 
        slow = 3,
        medium = 2,
        fast = 1
    }

    public ArrivalSpeed arrivalSpeed;

    SteeringController controller;

    void Start()
    {
        controller = GetComponent<SteeringController>();
    }

    public Vector3 ApplyForce(Vector3 target)
    {
        float dist = Vector3.Distance(target, transform.position);


        if(dist > 0)
        {
            const float decelerationTweak = 0.3f;

            float speed = dist / ((float)arrivalSpeed + decelerationTweak);
            speed = Mathf.Min(speed, controller.maxSpeed);

            Vector3 desiredVelocity = (target - transform.position).normalized * speed;

            return desiredVelocity - controller.rb.velocity;
        }


        return Vector3.zero;
    }
}
