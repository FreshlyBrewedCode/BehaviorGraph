using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorGraph;

namespace BehaviorGraphEditor
{
    public static class BehaviorEditorExtension
    {
        public static bool IsDecorator(this Behavior b)
        {
            return b.GetType().BaseType.GetGenericTypeDefinition() == typeof(Decorator<>);
        }
    }
}
