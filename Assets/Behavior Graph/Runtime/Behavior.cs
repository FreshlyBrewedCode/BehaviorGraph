using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorGraph
{
    public class Behavior : ScriptableObject
    {
        [SerializeField]
        public BehaviorMetaData metaData;

        public virtual int ChildCount => 0;

        // Called when the running status is entered
        protected virtual void OnStart(BehaviorContext context) { }

        // Called during the running status
        protected virtual BehaviorStatus OnUpdate(BehaviorContext context) { return BehaviorStatus.Success; }

        // Called once the behavior finished with success or failure
        protected virtual void OnTerminate(BehaviorContext context, BehaviorStatus status) { }

        protected virtual BehaviorContext CreateContext()
        {
            return new BehaviorContext();
        }

        protected virtual void DestoryContext(BehaviorContext context)
        {
            context.status = BehaviorStatus.Invalid;
        }

        public Behavior()
        {

        }

        // Tick function used to make sure we always invoke onStart, onUpdate and onTerminate according to state 
        public virtual BehaviorStatus Tick(BehaviorContext parentContext)
        {
            if (parentContext.ChildContext == null || parentContext.ChildContext.status == BehaviorStatus.Invalid)
            {
                parentContext.ChildContext = CreateContext();
                OnStart(parentContext.ChildContext);
            }

            var status = OnUpdate(parentContext.ChildContext);
            parentContext.ChildContext.status = status;

            if (status != BehaviorStatus.Running)
            {
                OnTerminate(parentContext.ChildContext, status);
                DestoryContext(parentContext.ChildContext);
            }

            return status;
        }

        public virtual Behavior GetChild(int index = 0)
        {
            return null;
        }

        public virtual void SetChild(Behavior child, int index = 0) { }

        public virtual void RemoveChild(Behavior child) { }
    }

    public enum BehaviorStatus
    {
        Invalid,
        Running,
        Success,
        Failed
    }

    [System.Serializable]
    public struct BehaviorMetaData
    {
        public Vector2 editorPosition;
    }
}
