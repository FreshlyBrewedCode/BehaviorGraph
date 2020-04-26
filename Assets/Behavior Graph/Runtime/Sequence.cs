using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorGraph
{
    public class Sequence : Composite<Sequence.Context>
    {
        public class Context : BehaviorContext
        {
            public int currentChild;
        }

        public Sequence() : base()
        {

        }

        protected override void OnStart(Context context)
        {
            context.currentChild = 0;
        }

        protected override BehaviorStatus OnUpdate(Context context)
        {
            // Keep going until all children are proccessed
            while (context.currentChild < children.Count)
            {
                // Tick the current child
                var status = children[context.currentChild].Tick(context);

                // only continue if the child succeeded otherwise simply return its status (i.e. Failed or Running)
                if (status != BehaviorStatus.Success)
                {
                    return status;
                }

                // Increment the current child
                context.currentChild++;
            }

            // If we exit the loop it means all children returned success
            return BehaviorStatus.Success;
        }

    }
}
