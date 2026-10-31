using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Update only the turn state's speed binding, preserving the open scene and all transitions.
public static class ConfigureFastFoodTurnSpeed
{
    public static string Apply()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
            "Assets/Animations/Player/FastFood/EvaFastFoodAnimatorController.controller");
        string speed = AnimatorFloat.TurnSpeed.ToString();
        var turn = controller.layers[0].stateMachine.states.Single(s => s.state.name == "Turn").state;
        if (!controller.parameters.Any(p => p.name == speed))
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = speed, type = AnimatorControllerParameterType.Float, defaultFloat = 1f
            });
        turn.speedParameter = speed;
        turn.speedParameterActive = true;
        EditorUtility.SetDirty(turn);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssetIfDirty(controller);
        return "TurnSpeed controls only the Turn state; scene and existing transitions preserved.";
    }
}
