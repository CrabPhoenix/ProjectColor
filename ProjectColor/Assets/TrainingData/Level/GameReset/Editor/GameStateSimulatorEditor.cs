using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameStateSimulator))]
public class GameStateSimulatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameStateSimulator simulator = (GameStateSimulator)target;

        if(GUILayout.Button("Simulate Start"))
        {
            Debug.Log("Game Start");
            GameEvent.GameStart();
        }
        if(GUILayout.Button("Simulate Restart"))
        {
            Debug.Log("Game Restart");
            GameEvent.GameRestart();
        }
        if(GUILayout.Button("Simulate Game Lose"))
        {
            Debug.Log("Game Lose");
            GameEvent.GameLose();
        }
        if(GUILayout.Button("Simulate Game Win"))
        {
            Debug.Log("Game Win");
            GameEvent.GameWin();
        }

    }


}
