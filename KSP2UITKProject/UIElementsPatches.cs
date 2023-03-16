using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.UIR;
using UnityEngine.UIElements.UIR.Implementation;

namespace KSP2UITK;

public static class UIElementsPatches
{
    #region UnityEngine.UIElements patches
    
    [HarmonyPatch(typeof(PanelSettings), nameof(PanelSettings.InitializeShaders))]
    [HarmonyPrefix]
    public static bool PanelSettings_InitializeShaders(PanelSettings __instance)
    {
        KSP2UITKPlugin.Logger.LogInfo("Entered PanelSettings_InitializeShaders");

        if (__instance.m_AtlasBlitShader == null)
        {
            __instance.m_AtlasBlitShader = KSP2UITKPlugin.Shaders[Shaders.k_AtlasBlit];
        }
        if (__instance.m_RuntimeShader == null)
        {
            __instance.m_RuntimeShader = KSP2UITKPlugin.Shaders[Shaders.k_Runtime];
        }
        if (__instance.m_RuntimeWorldShader == null)
        {
            __instance.m_RuntimeWorldShader = KSP2UITKPlugin.Shaders[Shaders.k_RuntimeWorld];
        }
        __instance.m_PanelAccess.SetTargetTexture();
        
        return false;
    }
    
    [HarmonyPatch(typeof(VisualElement), nameof(VisualElement.getRuntimeMaterial))]
    [HarmonyPrefix]
    public static bool VisualElement_getRuntimeMaterial(ref Material __result)
    {
        KSP2UITKPlugin.Logger.LogInfo("Entered VisualElement_getRuntimeMaterial");

        if (VisualElement.s_runtimeMaterial != null)
        {
            return VisualElement.s_runtimeMaterial;
        }
        Shader shader = KSP2UITKPlugin.Shaders[UIRUtility.k_DefaultShaderName];
        if (shader != null)
        {
            shader.hideFlags |= HideFlags.DontSaveInEditor;
            Material material = new Material(shader);
            material.hideFlags |= HideFlags.DontSaveInEditor;
            VisualElement.s_runtimeMaterial = material;
            __result = material;
            return false;
        }
        __result = null;
        return false;
    }
    
    [HarmonyPatch(typeof(TextureBlitter), nameof(TextureBlitter.BeginBlit))]
    [HarmonyPrefix]
    public static bool TextureBlitter_BeginBlit(TextureBlitter __instance, RenderTexture dst)
    {
        KSP2UITKPlugin.Logger.LogInfo("Entered TextureBlitter_BeginBlit");

        if (__instance.m_BlitMaterial == null)
        {
            Shader shader = KSP2UITKPlugin.Shaders[Shaders.k_AtlasBlit];
            __instance.m_BlitMaterial = new Material(shader);
            __instance.m_BlitMaterial.hideFlags |= HideFlags.DontSaveInEditor;
        }
        if (__instance.m_Properties == null)
        {
            __instance.m_Properties = new MaterialPropertyBlock();
        }
        __instance.m_Viewport = Utility.GetActiveViewport();
        __instance.m_PrevRT = RenderTexture.active;
        GL.LoadPixelMatrix(0f, (float)dst.width, 0f, (float)dst.height);
        Graphics.SetRenderTarget(dst);
        __instance.m_BlitMaterial.SetPass(0);
        return false;
    }

    #endregion
    
    #region UnityEngine.UIElements.UIR patches

    [HarmonyPatch(typeof(UIRenderDevice), nameof(UIRenderDevice.vertexTexturingIsAvailable), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool UIRenderDevice_get_vertexTexturingIsAvailable(ref bool __result)
    {
        KSP2UITKPlugin.Logger.LogInfo("Entered UIRenderDevice_get_vertexTexturingIsAvailable");

        if (UIRenderDevice.s_VertexTexturingIsAvailable == null)
        {
            Material material = new Material(KSP2UITKPlugin.Shaders[UIRUtility.k_DefaultShaderName]);
            material.hideFlags |= HideFlags.DontSaveInEditor;
            string tag = material.GetTag("UIE_VertexTexturingIsAvailable", false);
            UIRUtility.Destroy(material);
            UIRenderDevice.s_VertexTexturingIsAvailable = new bool?(tag == "1");
        }

        __result = UIRenderDevice.s_VertexTexturingIsAvailable.Value;
        return false;
    }

    [HarmonyPatch(typeof(UIRenderDevice), nameof(UIRenderDevice.shaderModelIs35), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool UIRenderDevice_get_shaderModelIs35(ref bool __result)
    {
        KSP2UITKPlugin.Logger.LogInfo("Entered UIRenderDevice_get_shaderModelIs35");

        if (UIRenderDevice.s_ShaderModelIs35 == null)
        {
            var defaultName = UIRUtility.k_DefaultShaderName;
            var shader = KSP2UITKPlugin.Shaders[defaultName];
            Material material = new Material(shader);
            material.hideFlags |= HideFlags.DontSaveInEditor;
            string tag = material.GetTag("UIE_ShaderModelIs35", false);
            UIRUtility.Destroy(material);
            UIRenderDevice.s_ShaderModelIs35 = tag == "1";
        }

        __result = UIRenderDevice.s_ShaderModelIs35.Value;
        return false;
    }

    #endregion

    #region UnityEngine.UIElements.UIR.Implementation patches

    [HarmonyPatch(typeof(RenderEvents), nameof(RenderEvents.CreateBlitShader))]
    [HarmonyPrefix]
    public static bool RenderEvents_CreateBlitShader(float colorConversion, ref Material __result)
    {
        KSP2UITKPlugin.Logger.LogInfo("Entered RenderEvents_CreateBlitShader");

        if (RenderEvents.s_blitShader == null)
        {
            RenderEvents.s_blitShader = KSP2UITKPlugin.Shaders["Hidden/UIE-ColorConversionBlit"];
        }
        Material material = new Material(RenderEvents.s_blitShader);
        material.hideFlags |= HideFlags.DontSaveInEditor;
        material.SetFloat("_ColorConversion", colorConversion);
        __result = material;
        return false;
    }

    #endregion
}