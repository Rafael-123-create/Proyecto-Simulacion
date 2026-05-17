#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class SetupGameTools
{
    [MenuItem("Tools/Setup Basic Scene")]
    public static void Execute()
    {
        Debug.Log("Setup tool: Please create the scene manually and assign references in the Inspector.");
    }
}
#endif
