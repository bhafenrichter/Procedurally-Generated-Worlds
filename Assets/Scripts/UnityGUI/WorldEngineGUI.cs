using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldEngine))]
public class WorldEngineGUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldEngine myScript = (WorldEngine)target;
        if (GUILayout.Button("Generate World"))
        {
            // EventBus.Manager.Broadcast(EventBus.Actions.GENERATE_WORLD);
            GameObject.Find("WorldEngine").GetComponent<WorldEngine>().generateWorld();
        }
    }

}