#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

public static class JackpotBoardLockHelper
{
    [System.Serializable]
    private class TierConfig { public string name; public bool rolling; }
    [System.Serializable]
    private class Config { public string gameFolderName; public string gameAbbreviation; public string gameFolder; public string prefabFolder; public string animFolder; public List<TierConfig> tiers; }

    private static Config LoadConfig()
    {
        string json = File.ReadAllText("Assets/SlotMaker/Editor/Tools/jackpot_board_config.json");
        return JsonUtility.FromJson<Config>(json);
    }

    [MenuItem("SlotMaker/Internal/Generate Jackpot Board Lock Objects")]
    public static void AddLockObjects()
    {
        Config config = LoadConfig();
        string spriteFolder = config.gameFolder + "/Sprites/UI/01/Jackpot";

        foreach (var tier in config.tiers)
        {
            string prefabPath = config.prefabFolder + "/" + tier.name + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;

            string assetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
            Transform animatorT = root.transform.Find("Animator");
            if (animatorT == null) { PrefabUtility.UnloadPrefabContents(root); continue; }
            if (animatorT.Find("Lock Anchor") != null) { PrefabUtility.UnloadPrefabContents(root); continue; }

            GameObject la = new GameObject("Lock Anchor"); la.layer = 8;
            la.transform.SetParent(animatorT, false);
            RectTransform laRT = la.AddComponent<RectTransform>();
            laRT.anchorMin = new Vector2(0.5f, 0.5f); laRT.anchorMax = new Vector2(0.5f, 0.5f);
            laRT.sizeDelta = new Vector2(100f, 100f); laRT.anchoredPosition = Vector2.zero;
            la.SetActive(false);

            CreateImageChild("Chain", la.transform, Vector2.zero, new Vector3(1.2f, 1.2f, 1f), FindSprite(spriteFolder, "Chain"));

            GameObject lockCtrl = new GameObject("Lock Controller"); lockCtrl.layer = 8;
            lockCtrl.transform.SetParent(la.transform, false);
            RectTransform lcRT = lockCtrl.AddComponent<RectTransform>();
            lcRT.anchorMin = new Vector2(0.5f, 0.5f); lcRT.anchorMax = new Vector2(0.5f, 0.5f);
            lcRT.sizeDelta = new Vector2(100f, 100f); lcRT.anchoredPosition = new Vector2(0f, -8.5f);

            CreateImageChild("Lock", lockCtrl.transform, new Vector2(0f, -22f), Vector3.one, FindSprite(spriteFolder, "Lock"));
            CreateImageChild("Lock Src", lockCtrl.transform, Vector2.zero, Vector3.one, FindSprite(spriteFolder, "Lock src"));

            PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log("[JackpotBoardLock] " + tier.name + ": Lock objects added");
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
    }

    [MenuItem("SlotMaker/Internal/Generate Jackpot Board Lock Anims")]
    public static void GenerateLockAnims()
    {
        Config config = LoadConfig();
        string cf = config.animFolder; string gn = config.gameAbbreviation;

        AnimationClip lockClip = CreateLockClip(gn, false);
        AnimationClip lockIdleClip = CreateLockIdleClip(gn);
        AnimationClip unlockClip = CreateLockClip(gn, true);
        AnimationClip unlockIdleClip = CreateUnlockIdleClip(gn);

        SaveClip(lockClip, cf + "/Jackpot Lock " + gn + ".anim");
        SaveClip(lockIdleClip, cf + "/Jackpot Lock Idle " + gn + ".anim");
        SaveClip(unlockClip, cf + "/Jackpot Unlock " + gn + ".anim");
        SaveClip(unlockIdleClip, cf + "/Jackpot Unlock Idle " + gn + ".anim");

        string ctrlPath = cf + "/Jackpot Lock Controller " + gn + ".controller";
        var ex = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
        if (ex != null) AssetDatabase.DeleteAsset(ctrlPath);

        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        ctrl.AddParameter("Lock", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        AnimatorState sLock = sm.AddState("Jackpot Lock"); sLock.motion = lockClip;
        AnimatorState sLockIdle = sm.AddState("Jackpot Lock Idle"); sLockIdle.motion = lockIdleClip;
        AnimatorState sUnlock = sm.AddState("Jackpot Unlock"); sUnlock.motion = unlockClip;
        AnimatorState sUnlockIdle = sm.AddState("Jackpot Unlock Idle"); sUnlockIdle.motion = unlockIdleClip;

        var t1 = sLockIdle.AddTransition(sUnlock);
        t1.AddCondition(AnimatorConditionMode.IfNot, 0, "Lock"); t1.hasExitTime = false; t1.duration = 0;
        var t2 = sUnlock.AddTransition(sUnlockIdle);
        t2.hasExitTime = true; t2.exitTime = 1f; t2.duration = 0;
        var t3 = sUnlockIdle.AddTransition(sLock);
        t3.AddCondition(AnimatorConditionMode.If, 0, "Lock"); t3.hasExitTime = false; t3.duration = 0;
        var t4 = sLock.AddTransition(sLockIdle);
        t4.hasExitTime = true; t4.exitTime = 1f; t4.duration = 0;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

        string grandPath = config.prefabFolder + "/" + config.tiers[0].name + ".prefab";
        GameObject gp = AssetDatabase.LoadAssetAtPath<GameObject>(grandPath);
        if (gp != null)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(grandPath);
            Transform at = root.transform.Find("Animator");
            if (at != null) { Animator a = at.GetComponent<Animator>(); if (a != null) a.runtimeAnimatorController = ctrl; }
            PrefabUtility.SaveAsPrefabAsset(root, grandPath);
            PrefabUtility.UnloadPrefabContents(root);
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("[JackpotBoardLock] Complete for " + gn);
    }

    private static void CreateImageChild(string name, Transform parent, Vector2 pos, Vector3 scale, Sprite sprite)
    {
        GameObject go = new GameObject(name); go.layer = 8;
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.localScale = scale;
        Image img = go.AddComponent<Image>(); img.raycastTarget = false;
        if (sprite != null) img.sprite = sprite;
        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static AnimationClip CreateLockClip(string gn, bool isUnlock)
    {
        AnimationClip clip = new AnimationClip();
        clip.name = (isUnlock ? "Jackpot Unlock " : "Jackpot Lock ") + gn;
        var s = AnimationUtility.GetAnimationClipSettings(clip); s.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        float d = 0.5f;
        if (!isUnlock) {
            SC(clip, "Lock Anchor", typeof(GameObject), "m_IsActive", new Keyframe(0f, 1f), new Keyframe(d, 1f));
            SC(clip, "Lock Anchor/Chain", typeof(RectTransform), "localScale.x", new Keyframe(0f, 0f), new Keyframe(d*0.6f, 1.3f), new Keyframe(d, 1.2f));
            SC(clip, "Lock Anchor/Chain", typeof(RectTransform), "localScale.y", new Keyframe(0f, 0f), new Keyframe(d*0.6f, 1.3f), new Keyframe(d, 1.2f));
            SC(clip, "Lock Anchor/Lock Controller/Lock", typeof(RectTransform), "localScale.x", new Keyframe(0f, 0f), new Keyframe(d*0.6f, 1.1f), new Keyframe(d, 1f));
            SC(clip, "Lock Anchor/Lock Controller/Lock", typeof(RectTransform), "localScale.y", new Keyframe(0f, 0f), new Keyframe(d*0.6f, 1.1f), new Keyframe(d, 1f));
        } else {
            SC(clip, "Lock Anchor/Lock Controller/Lock", typeof(RectTransform), "localEulerAnglesRaw.z", new Keyframe(0f, 0f), new Keyframe(d, -25f));
            SC(clip, "Lock Anchor/Lock Controller/Lock", typeof(RectTransform), "localScale.x", new Keyframe(0f, 1f), new Keyframe(d, 1.3f));
            SC(clip, "Lock Anchor/Lock Controller/Lock", typeof(RectTransform), "localScale.y", new Keyframe(0f, 1f), new Keyframe(d, 1.3f));
            SC(clip, "Lock Anchor/Chain", typeof(CanvasGroup), "m_Alpha", new Keyframe(0f, 1f), new Keyframe(d, 0f));
            SC(clip, "Lock Anchor", typeof(GameObject), "m_IsActive", new Keyframe(0f, 1f), new Keyframe(d-0.01f, 1f), new Keyframe(d, 0f));
        }
        return clip;
    }

    private static AnimationClip CreateLockIdleClip(string gn)
    {
        AnimationClip clip = new AnimationClip(); clip.name = "Jackpot Lock Idle " + gn;
        var s = AnimationUtility.GetAnimationClipSettings(clip); s.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        SC(clip, "Lock Anchor", typeof(GameObject), "m_IsActive", new Keyframe(0f, 1f), new Keyframe(1f, 1f));
        SC(clip, "Lock Anchor/Chain", typeof(RectTransform), "localScale.x", new Keyframe(0f, 1.2f), new Keyframe(1f, 1.2f));
        SC(clip, "Lock Anchor/Chain", typeof(RectTransform), "localScale.y", new Keyframe(0f, 1.2f), new Keyframe(1f, 1.2f));
        SC(clip, "Lock Anchor/Lock Controller/Lock", typeof(RectTransform), "localScale.x", new Keyframe(0f, 1f), new Keyframe(1f, 1f));
        SC(clip, "Lock Anchor/Lock Controller/Lock", typeof(RectTransform), "localScale.y", new Keyframe(0f, 1f), new Keyframe(1f, 1f));
        return clip;
    }

    private static AnimationClip CreateUnlockIdleClip(string gn)
    {
        AnimationClip clip = new AnimationClip(); clip.name = "Jackpot Unlock Idle " + gn;
        var s = AnimationUtility.GetAnimationClipSettings(clip); s.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        SC(clip, "Lock Anchor", typeof(GameObject), "m_IsActive", new Keyframe(0f, 0f), new Keyframe(1f, 0f));
        return clip;
    }

    private static void SC(AnimationClip clip, string path, System.Type type, string prop, params Keyframe[] keys)
    { clip.SetCurve(path, type, prop, new AnimationCurve(keys)); }

    private static void SaveClip(AnimationClip clip, string path)
    {
        string dir = Path.GetDirectoryName(path); if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var ex = AssetDatabase.LoadAssetAtPath<AnimationClip>(path); if (ex != null) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
    }

    private static Sprite FindSprite(string folder, string name)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(folder + "/" + name + ".png");
        if (assets != null) foreach (var obj in assets) if (obj is Sprite s) return s;
        return null;
    }
}
#endif