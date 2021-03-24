using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SteeringController : MonoBehaviour
{
    public Rigidbody rb;
    public float maxSpeed = 5;

    public List<ISteeringBehaviour> steeringBehaviours = new List<ISteeringBehaviour>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        transform.GetComponents(steeringBehaviours);
    }

    void FixedUpdate()
    {
        var steeringForce = CalculateSteeringForces();
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxSpeed);
        var acceleration = steeringForce / rb.mass;
        var velocity = rb.velocity + acceleration * Time.deltaTime;

        rb.velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        if(rb.velocity.magnitude > 0.001f)
        {
            transform.LookAt(rb.velocity + transform.position);
        }
    }

    Vector3 CalculateSteeringForces()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);
        mousePos.y = 0;

        var outputForce = Vector3.zero;
        foreach(var steeringForce in steeringBehaviours)
        {
            outputForce += steeringForce.ApplyForce(mousePos);
        }

        return outputForce;
    }

    


}

public interface ISteeringBehaviour
{
    Vector3 ApplyForce(Vector3 target);
}

