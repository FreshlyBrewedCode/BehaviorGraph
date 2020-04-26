using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Behavior Graph/Behavior Descriptor", fileName = "newBehaviorDescriptor")]
public class BehaviorDescriptor : ScriptableObject
{
    public string title;
    public string editorPath;
    public Texture2D icon;
    public Color color = Color.white;
}
