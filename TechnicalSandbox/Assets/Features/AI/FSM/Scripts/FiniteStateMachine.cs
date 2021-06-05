using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.AI.FSM
{
    public class FiniteStateMachine
    {
        Entity entity;

        IState previousState;
        IState currentState;

        public FiniteStateMachine(Entity e)
        {
            entity = e;
            currentState = PatrolState.GetInstance();
        }

        void Update()
        {
            if(currentState != null)
            {
                currentState.Update(entity);
            }
            else
            {
                Debug.LogError("Current state is null!");
            }
        }

        void ChangeState(IState newState)
        {
            if (newState == currentState) return;

            previousState = currentState;
            if(previousState != null)
                previousState.Exit(entity);

            if(currentState != null)
            {
                currentState.Enter(entity);
            }
            currentState = newState;

        }
    }
}