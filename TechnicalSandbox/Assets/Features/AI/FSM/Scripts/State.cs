using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.AI.FSM
{
    public interface IState 
    {
        void Enter(Entity entity);
        void Update(Entity entity);
        void Exit(Entity entity);
    }
}