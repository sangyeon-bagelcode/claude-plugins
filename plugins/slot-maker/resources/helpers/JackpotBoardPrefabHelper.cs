#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using TMPro;
using System.IO;
using System.Collections.Generic;

public static class JackpotBoardPrefabHelper
{
    [System.Serializable]
    private class TierConfig { public string name; public bool rolling; }
    [System.Serializable]
    private class Config
    {
        public string gameFolderName;
        public string gameAbbreviation;
        public string gameFolder;
        public string prefabFolder;
        public string animFolder;
        public List<TierConfig> tiers;
    }

    private static Config LoadConfig()
    {
        string json = File.ReadAllText("Assets/SlotMaker/Editor/Tools/jackpot_board_config.json");
        return JsonUtility.FromJson<Config>(json);
    }

    [MenuItem("SlotMaker/Internal/Generate Jackpot Board Tiers")]
    public static void GenerateTiers()
    {
        Config config = LoadConfig();
        string baseTierName = config.tiers[0].name;
        GameObject sceneObj = BuildItemHierarchy(config, baseTierName);
        string path = config.prefabFolder + "/" + baseTierName + ".prefab";
        bool success;
        PrefabUtility.SaveAsPrefabAsset(sceneObj, path, out success);
        Object.DestroyImmediate(sceneObj);
        Debug.Log("[JackpotBoard] Pass 1: " + baseTierName + " " + (success ? "OK" : "FAILED"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("SlotMaker/Internal/Generate Jackpot Board Variants")]
    public static void GenerateVariants()
    {
        Config config = LoadConfig();
        string basePath = config.prefabFolder + "/" + config.tiers[0].name + ".prefab";
        GameObject grandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
        if (grandPrefab == null) { Debug.LogError("[JackpotBoard] Grand not found"); return; }

        string spriteFolder = config.gameFolder + "/Sprites/UI/01/Jackpot";
        for (int i = 1; i < config.tiers.Count; i++)
        {
            string tierName = config.tiers[i].name;
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(grandPrefab);
            inst.name = tierName;
            Transform jtT = inst.transform.Find("Animator/Anchor/Jackpot Txt");
            if (jtT != null)
            {
                Image jtImage = jtT.GetComponent<Image>();
                Sprite tierSprite = FindSprite(spriteFolder, tierName);
                if (jtImage != null && tierSprite != null) jtImage.sprite = tierSprite;
            }
            string variantPath = config.prefabFolder + "/" + tierName + ".prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(inst, variantPath, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(inst);
            Debug.Log("[JackpotBoard] Variant: " + tierName);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("SlotMaker/Internal/Generate Jackpot Board Assembly")]
    public static void GenerateBoard()
    {
        Config config = LoadConfig();
        string controllerPath = config.animFolder + "/Jackpot Board Controller " + config.gameAbbreviation + ".controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        GameObject root = new GameObject("Jackpot Board"); root.layer = 8;
        root.AddComponent<RectTransform>();
        SetRectTransformDefaults(root.GetComponent<RectTransform>());

        GameObject animatorGO = CreateChild("Animator", root.transform);
        Animator animator = animatorGO.AddComponent<Animator>();
        if (controller != null) animator.runtimeAnimatorController = controller;

        GameObject anchor = CreateChild("Anchor", animatorGO.transform);
        anchor.AddComponent<CanvasGroup>();

        int count = 0;
        foreach (var tier in config.tiers)
        {
            string tierPath = config.prefabFolder + "/" + tier.name + ".prefab";
            GameObject tierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(tierPath);
            if (tierPrefab != null)
            {
                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(tierPrefab);
                inst.transform.SetParent(anchor.transform, false);
                count++;
            }
            else Debug.LogError("[JackpotBoard] NOT FOUND: " + tierPath);
        }

        string boardPath = config.prefabFolder + "/Jackpot Board.prefab";
        bool success;
        PrefabUtility.SaveAsPrefabAsset(root, boardPath, out success);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[JackpotBoard] Pass 3: Board " + count + "/" + config.tiers.Count + " " + (success ? "OK" : "FAILED"));
    }

    private static GameObject BuildItemHierarchy(Config config, string tierName)
    {
        string spriteFolder = config.gameFolder + "/Sprites/UI/01/Jackpot";
        Sprite frameSprite = FindSprite(spriteFolder, "Jackpot Board");
        Sprite tierSprite = FindSprite(spriteFolder, tierName);

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            config.gameFolder + "/Fonts/Impact/Impact SDF.asset");
        if (font == null)
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/Contents/Contents Group 1/_Common/Fonts/Impact/Impact SDF.asset");

        GameObject root = new GameObject(tierName); root.layer = 8;
        root.AddComponent<RectTransform>();
        SetRectTransformDefaults(root.GetComponent<RectTransform>());
        root.AddComponent<CanvasGroup>();

        GameObject animatorGO = CreateChild("Animator", root.transform);
        animatorGO.AddComponent<Animator>();

        GameObject anchor = CreateChild("Anchor", animatorGO.transform);
        anchor.AddComponent<CanvasGroup>();

        GameObject frameAnchor = CreateChild("Frame Anchor", anchor.transform);

        GameObject frameL = CreateChild("Frame L", frameAnchor.transform);
        Image flImage = frameL.AddComponent<Image>(); flImage.raycastTarget = false;
        if (frameSprite != null) flImage.sprite = frameSprite;
        ContentSizeFitter flCSF = frameL.AddComponent<ContentSizeFitter>();
        flCSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        flCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        frameL.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);

        GameObject frameR = CreateChild("Frame R", frameAnchor.transform);
        frameR.transform.localScale = new Vector3(-1f, 1f, 1f);
        Image frImage = frameR.AddComponent<Image>(); frImage.raycastTarget = false;
        if (frameSprite != null) frImage.sprite = frameSprite;
        ContentSizeFitter frCSF = frameR.AddComponent<ContentSizeFitter>();
        frCSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        frCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        frameR.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);

        GameObject credit = CreateChild("Credit", frameAnchor.transform);
        TextMeshProUGUI tmp = credit.AddComponent<TextMeshProUGUI>();
        tmp.text = "1,000,000,000";
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 10;
        tmp.fontSizeMax = 55;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;

        RectTransform creditRT = credit.GetComponent<RectTransform>();
        creditRT.pivot = new Vector2(1f, 0.5f);
        creditRT.sizeDelta = new Vector2(370f, 60f);
        creditRT.anchoredPosition = new Vector2(273f, 3.45f);

        GameObject jackpotTxt = CreateChild("Jackpot Txt", anchor.transform);
        Image jtImage = jackpotTxt.AddComponent<Image>(); jtImage.raycastTarget = false;
        if (tierSprite != null) jtImage.sprite = tierSprite;
        ContentSizeFitter jtCSF = jackpotTxt.AddComponent<ContentSizeFitter>();
        jtCSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        jtCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        RectTransform jtRT = jackpotTxt.GetComponent<RectTransform>();
        jtRT.pivot = new Vector2(0f, 0.5f);
        jtRT.anchoredPosition = new Vector2(-273f, 0f);

        return root;
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        GameObject go = new GameObject(name); go.layer = 8;
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        SetRectTransformDefaults(rt);
        return go;
    }

    private static void SetRectTransformDefaults(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(100f, 100f);
        rt.anchoredPosition = Vector2.zero;
    }

    private static Sprite FindSprite(string folder, string name)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(folder + "/" + name + ".png");
        if (assets != null) foreach (var obj in assets) if (obj is Sprite s) return s;
        return null;
    }
}
#endif