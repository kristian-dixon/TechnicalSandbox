using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Sandbox.AI.FSM
{
    public class PatrolState : Singleton<PatrolState>, IState
    {
        public void Enter(Entity entity)
        {
            Debug.Log(entity.gameObject.name + ": Beginning Patrol!");
        }

        public void Exit(Entity entity)
        {
        }

        public void Update(Entity entity)
        {
            //If at target - set new target
            if(Vector3.Distance(entity.transform.position, entity.targetPosition) < 0.01)
            {

                //TODO:: Smarter pathfinding
                //entity.movementController.MoveTowardsTarget(0.5f);
            }

            //Move towards target
            //Scan for enemies
        }
    }

    public class CombatState : Singleton<CombatState>, IState
    {
        public void Enter(Entity entity)
        {
            Debug.Log(entity.gameObject.name + ": Entering Combat!");
        }

        public void Exit(Entity entity)
        {
        }

        public void Update(Entity entity)
        {
            //Check if can target enemy
                //Check if aimed at enemy
                    //Shoot
                //else
                    //Move aim towards enemy
            //Else
                //Move to a better position
                //Begin countdown for switch to search state
        }
    }

    public class SearchState : IState
    {
        public void Enter(Entity entity)
        {
            Debug.Log(entity.gameObject.name + ": Beginning Search!");
        }

        public void Exit(Entity entity)
        {
        }

        public void Update(Entity entity)
        {
            //Move towards target scanning around more
        }
    }
}
