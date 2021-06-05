using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.AI.FSM
{
    public class Entity : MonoBehaviour
    {
        public int teamID = 0;
        public int health = 100;

        public Vector3 targetPosition;

        FiniteStateMachine brain = null;

        // Start is called before the first frame update
        void Start()
        {
            brain = new FiniteStateMachine(this);
        }

        // Update is called once per frame
        void Update()
        {
            //brain.Update();
        }
    }
}