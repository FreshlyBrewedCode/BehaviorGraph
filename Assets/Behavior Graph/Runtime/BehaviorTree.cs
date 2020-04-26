using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorGraph
{
    [CreateAssetMenu(menuName = "Behavior Graph/Behavior Tree")]
    public class BehaviorTree : ScriptableObject
    {
        public Behavior entryNode;
    }
}
