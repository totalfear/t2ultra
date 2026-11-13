using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[BepInPlugin("t2ultra", "t2ultra", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    // The aspect ratio the game's HUD was designed for.
    const float BaseAspect = 16f / 9f;

    // Name of the HUD's root object in the game (contains the health bars, score, etc.).
    const string HudRootName = "in_game";

    // [General]
    ConfigEntry<bool> spanHud;
    ConfigEntry<KeyboardShortcut> toggleKey;

    // [HUD Scale]
    ConfigEntry<float> matchAt21x9;
    ConfigEntry<float> matchAt32x9;

    // [HUD Spread]
    ConfigEntry<float> spreadStrength;

    bool span;
    int lastW, lastH;
    float nextPoll;

    readonly Dictionary<RectTransform, Vector2> spreadOrig = new Dictionary<RectTransform, Vector2>();
    readonly Dictionary<RectTransform, float> appliedDx = new Dictionary<RectTransform, float>();

    void Awake()
    {
        var zeroToOne = new AcceptableValueRange<float>(0f, 1f);

        spanHud = Config.Bind("General", "SpanHud", true,
            "true = spread the HUD out to the screen edges.\nfalse = keep the HUD in its original 16:9 layout.");
        toggleKey = Config.Bind("General", "ToggleKey", new KeyboardShortcut(KeyCode.F8),
            "Press this key in-game to switch between the spread and original HUD layouts.");

        matchAt21x9 = Config.Bind("HUD Scale", "MatchAt21:9", 0.4f,
            new ConfigDescription(
                "HUD size at 21:9 (and narrower screens).\n0 = scale the HUD to the screen width, 1 = scale to the screen height.\nHigher values make the HUD smaller and move it down; lower values make it larger.", zeroToOne));
        matchAt32x9 = Config.Bind("HUD Scale", "MatchAt32:9", 0.7f,
            new ConfigDescription(
                "HUD size at 32:9. Screens between 21:9 and 32:9 get a blend of the two values.\nSame scale as MatchAt21:9.", zeroToOne));

        spreadStrength = Config.Bind("HUD Spread", "SpreadStrength", 1f,
            new ConfigDescription(
                "How far left and right HUD elements move toward the screen edges.\n0 = stay in the 16:9 area, 1 = go all the way to the edges.\nTry 0.5 to 0.8 if the edges feel too far at 32:9.", zeroToOne));

        span = spanHud.Value;

        new Harmony("t2ultra").PatchAll();
        Logger.LogInfo("t2ultra 1.0.0 loaded.");

        SceneManager.sceneLoaded += (_, __) =>
        {
            CleanDead();
            Fix();
        };
    }

    void Update()
    {
        if (toggleKey.Value.IsDown())
        {
            span = !span;
            UpdateHud();
        }

        // The HUD may be inactive at load or spawned later, so re-check once a second.
        if (span && Time.unscaledTime >= nextPoll)
        {
            nextPoll = Time.unscaledTime + 1f;
            UpdateHud();
        }

        // Re-apply when the screen resolution changes (e.g. picked in the options menu).
        if (Screen.width != lastW || Screen.height != lastH)
        {
            lastW = Screen.width;
            lastH = Screen.height;
            Fix();
        }
    }

    void CleanDead()
    {
        RemoveDead(spreadOrig);
        RemoveDead(appliedDx);
    }

    static void RemoveDead<T>(Dictionary<RectTransform, T> dict)
    {
        var dead = new List<RectTransform>();
        foreach (var k in dict.Keys) if (k == null) dead.Add(k);
        foreach (var k in dead) dict.Remove(k);
    }

    // Blend between the two configured values as the screen goes from 21:9 to 32:9.
    float CurrentMatch()
    {
        float aspect = (float)Screen.width / Screen.height;
        float lo = 21f / 9f, hi = 32f / 9f;
        float t = Mathf.Clamp01((aspect - lo) / (hi - lo));
        return Mathf.Lerp(matchAt21x9.Value, matchAt32x9.Value, t);
    }

    void Fix()
    {
        float m = CurrentMatch();
        foreach (var scaler in FindObjectsOfType<CanvasScaler>())
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = m;
        }
        UpdateHud();
    }

    // Spread the HUD out (span = true) or put everything back (span = false).
    void UpdateHud()
    {
        if (!span)
        {
            RestoreHud();
            return;
        }

        foreach (var rt in Resources.FindObjectsOfTypeAll<RectTransform>())
        {
            if (rt.name != HudRootName) continue;
            if (!rt.gameObject.scene.IsValid()) continue; // skip prefabs and assets
            SpreadChildren(rt);
        }
    }

    void RestoreHud()
    {
        foreach (var kv in spreadOrig)
        {
            if (kv.Key == null) continue;
            kv.Key.anchoredPosition = kv.Value;
        }
        appliedDx.Clear();
    }

    // Horizontal centre (in root's local space) of everything visible under c.
    bool ContentCenterX(RectTransform root, RectTransform c, out float x)
    {
        float min = float.MaxValue, max = float.MinValue;
        bool any = false;
        var corners = new Vector3[4];
        foreach (var g in c.GetComponentsInChildren<Graphic>(false))
        {
            g.rectTransform.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++)
            {
                float lx = root.InverseTransformPoint(corners[i]).x;
                if (lx < min) min = lx;
                if (lx > max) max = lx;
                any = true;
            }
        }
        if (!any)
        {
            foreach (var t in c.GetComponentsInChildren<RectTransform>(false))
            {
                float lx = root.InverseTransformPoint(t.position).x;
                if (lx < min) min = lx;
                if (lx > max) max = lx;
                any = true;
            }
        }
        x = any ? (min + max) * 0.5f : 0f;
        return any;
    }

    // Move each direct child of the HUD root toward the left or right screen edge,
    // depending on which side of the HUD's centre its visible content is on.
    void SpreadChildren(RectTransform root)
    {
        var canvas = root.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        canvas = canvas.rootCanvas;
        var canvasRect = ((RectTransform)canvas.transform).rect;

        // extra width (in canvas units) beyond the HUD's design aspect ratio
        float baseWidth = canvasRect.height * BaseAspect;
        float extra = Mathf.Max(0f, canvasRect.width - baseWidth);
        float shift = extra * 0.5f * spreadStrength.Value;

        // convert canvas units to the HUD root's local units
        float k = canvas.transform.lossyScale.x / Mathf.Max(0.0001f, root.lossyScale.x);
        shift *= k;
        float dead = 0.05f * baseWidth * k; // elements this close to the middle stay put

        Rect rr = root.rect;
        foreach (Transform ch in root)
        {
            var c = ch as RectTransform;
            if (c == null) continue;

            Vector2 orig;
            if (!spreadOrig.TryGetValue(c, out orig))
            {
                orig = c.anchoredPosition;
                spreadOrig[c] = orig;
            }

            // Judge left/right by where the element's visible content actually is, not by its own
            // position (many elements sit at 0,0 and place their content through child objects).
            float prev;
            appliedDx.TryGetValue(c, out prev);
            float cx;
            float rel;
            bool measured = ContentCenterX(root, c, out cx);
            if (measured) rel = cx - prev - rr.center.x;
            else
            {
                float anchorX = rr.xMin + (c.anchorMin.x + c.anchorMax.x) * 0.5f * rr.width;
                rel = anchorX + orig.x - rr.center.x;
            }

            float dx = 0f;
            if (rel < -dead) dx = -shift;
            else if (rel > dead) dx = shift;

            appliedDx[c] = dx;
            c.anchoredPosition = new Vector2(orig.x + dx, orig.y);
        }
    }
}

// When the game applies a 16:9 resolution from its options menu,
// keep the chosen height but widen it to match the desktop's aspect ratio.
[HarmonyPatch]
static class SetResolutionPatch
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var m in typeof(Screen).GetMethods(BindingFlags.Public | BindingFlags.Static))
            if (m.Name == "SetResolution")
                yield return m;
    }

    static void Prefix(ref int width, int height)
    {
        if (height <= 0) return;
        float requested = (float)width / height;
        if (Mathf.Abs(requested - 16f / 9f) > 0.02f) return;
        float desktop = (float)Display.main.systemWidth / Display.main.systemHeight;
        width = Mathf.RoundToInt(height * desktop);
    }
}
