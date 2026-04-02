#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

public static class JackpotBoardAnimHelper
{
    [System.Serializable]
    private class TierConfig
    {
        public string name;
        public bool rolling;
    }

    [System.Serializable]
    private class Config
    {
        public string gameFolderName;
        public string gameAbbreviation;
        public string animFolder;
        public float fadeDuration;
        public float displayDuration;
        public List<TierConfig> tiers;
    }

    [MenuItem("SlotMaker/Internal/Generate Jackpot Board Anims")]
    public static void Generate()
    {
        string configPath = "Assets/SlotMaker/Editor/Tools/jackpot_board_config.json";
        string json = File.ReadAllText(configPath);
        Config config = JsonUtility.FromJson<Config>(json);

        string clipPath = config.animFolder + "/Jackpot Board Rolling " + config.gameAbbreviation + ".anim";
        string controllerPath = config.animFolder + "/Jackpot Board Controller " + config.gameAbbreviation + ".controller";

        List<TierConfig> rollingTiers = new List<TierConfig>();
        foreach (var t in config.tiers)
            if (t.rolling) rollingTiers.Add(t);

        float stepTime = config.displayDuration + config.fadeDuration;
        float cycleEnd = rollingTiers.Count * stepTime;

        AnimationClip clip = new AnimationClip();
        clip.name = "Jackpot Board Rolling " + config.gameAbbreviation;

        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        string dir = Path.GetDirectoryName(clipPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        AssetDatabase.CreateAsset(clip, clipPath);

        foreach (var tier in config.tiers)
        {
            string path = "Anchor/" + tier.name;
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, typeof(CanvasGroup), "m_Alpha");

            List<Keyframe> keys = new List<Keyframe>();

            if (!tier.rolling)
            {
                keys.Add(new Keyframe(0f, 1f));
                keys.Add(new Keyframe(cycleEnd, 1f));
            }
            else
            {
                int rollingIndex = rollingTiers.IndexOf(tier);
                if (rollingIndex == 0)
                {
                    float showEnd = config.displayDuration;
                    float fadeEnd = showEnd + config.fadeDuration;
                    float reappearStart = cycleEnd - config.fadeDuration;
                    keys.Add(new Keyframe(0f, 1f));
                    keys.Add(new Keyframe(showEnd, 1f));
                    keys.Add(new Keyframe(fadeEnd, 0f));
                    keys.Add(new Keyframe(reappearStart, 0f));
                    keys.Add(new Keyframe(cycleEnd, 1f));
                }
                else
                {
                    float showStart = rollingIndex * stepTime;
                    float showEnd = showStart + config.displayDuration;
                    float fadeEnd = showEnd + config.fadeDuration;
                    float fadeInStart = showStart - config.fadeDuration;
                    keys.Add(new Keyframe(0f, 0f));
                    if (fadeInStart > 0f) keys.Add(new Keyframe(fadeInStart, 0f));
                    keys.Add(new Keyframe(showStart, 1f));
                    keys.Add(new Keyframe(showEnd, 1f));
                    keys.Add(new Keyframe(fadeEnd, 0f));
                    if (fadeEnd < cycleEnd) keys.Add(new Keyframe(cycleEnd, 0f));
                }
            }

            AnimationCurve curve = new AnimationCurve(keys.ToArray());
            AnimationUtility.SetEditorCurve(clip, binding, curve);

            AnimationCurve setCurve = AnimationUtility.GetEditorCurve(clip, binding);
            if (setCurve != null)
            {
                for (int i = 0; i < setCurve.length; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(setCurve, i, AnimationUtility.TangentMode.ClampedAuto);
                    AnimationUtility.SetKeyRightTangentMode(setCurve, i, AnimationUtility.TangentMode.ClampedAuto);
                }
                AnimationUtility.SetEditorCurve(clip, binding, setCurve);
            }
        }

        EditorUtility.SetDirty(clip);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Off", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Up", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Down", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("info", AnimatorControllerParameterType.Int);

        AnimatorControllerLayer layer = controller.layers[0];
        AnimatorStateMachine sm = layer.stateMachine;
        AnimatorState state = sm.AddState("Jackpot Board Controller " + config.gameAbbreviation);
        state.motion = clip;
        sm.defaultState = state;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[JackpotBoardAnimHelper] Generated: " + config.gameAbbreviation);
    }
}
#endif