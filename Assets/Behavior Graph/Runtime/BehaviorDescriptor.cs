using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorGraph
{
    [CreateAssetMenu(menuName = "Behavior Graph/Behavior Descriptor", fileName = "newBehaviorDescriptor")]
    public class BehaviorDescriptor : ScriptableObject
    {
        public string title;
        public string editorPath;
        public Texture2D icon;
        public Color color = Color.clear;
        public BehaviorPortCapacity inputCapacity = BehaviorPortCapacity.Single;
        public BehaviorPortCapacity outputCapacity = BehaviorPortCapacity.Multiple;
    }

    public enum BehaviorPortCapacity
    {
        None = -1, Single = 0, Multiple = 1
    }
}
