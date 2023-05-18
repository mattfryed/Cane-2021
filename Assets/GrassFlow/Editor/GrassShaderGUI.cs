using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

using PropType = UnityEditor.MaterialProperty.PropType;
using PropFlags = UnityEditor.MaterialProperty.PropFlags;

namespace GrassFlow {
    public class GrassShaderGUI : ShaderGUI {

        bool m_FirstTimeApply = true;


        static AnimBool testAnim;

        static Dictionary<string, AnimBool> foldoutDict;
        static Stack<bool> nestedFoldouts;
        static Stack<string> nestedFoldoutProps;


        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            //this is really stupid but by default shaders are created in the standard shader
            //which sets this to 0.5, which we dont want
            material.SetFloat("_Cutoff", 0);
        }


        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props) {

            Material mat = materialEditor.target as Material;

            MaterialProperty mainTexProp = FindProperty("_MainTex", props);

            EditorGUIUtility.fieldWidth = 50;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * 0.75f;

            if (m_FirstTimeApply) {
                //UpdateMaterial(mat);
                UpdateBools(materialEditor, props);
                m_FirstTimeApply = false;
            }


            PropertyInfo proInfo = typeof(EditorGUIUtility).GetProperty("skinIndex",
                BindingFlags.Static | BindingFlags.NonPublic);


            EditorGUILayout.HelpBox("Check the documentation for information on material settings", MessageType.Info, true);

            bool hideIf = false;

            int propIdx = -1;
            foreach (MaterialProperty prop in props) {
                propIdx++;

                bool hideProp = prop.flags.hasFlag(PropFlags.HideInInspector);

                if (prop.name == "_IncIndent") {
                    EditorGUI.indentLevel++;
                    continue;
                }

                if (prop.name == "_DecIndent") {
                    EditorGUI.indentLevel--;
                    continue;
                }


                if (prop.name.StartsWith("_EndHideIf")) {
                    hideIf = false;
                }

                if (hideIf) {
                    continue;
                }

                if (prop.name.StartsWith("_CollapseEnd") &&
                    nestedFoldoutProps.Peek() == prop.displayName) {

                    if (prop.name == "_CollapseEnd_Maps" && nestedFoldouts.Peek()) {
                        materialEditor.TextureScaleOffsetProperty(mainTexProp);
                    }

                    EditorGUILayout.EndFadeGroup();
                    //Debug.Log("pop: " + prop.name);
                    nestedFoldouts.Pop();
                    nestedFoldoutProps.Pop();
                    EditorGUI.indentLevel--;
                    GUILayout.Space(4);
                    continue;
                }

                if (nestedFoldouts.Count != 0 && !nestedFoldouts.Peek()) {
                    continue;
                }

                GUIContent content = new GUIContent(prop.displayName);
                string tooltip;
                if (shaderPropTooltipDict.TryGetValue(prop.name, out tooltip)) {
                    content.tooltip = tooltip;
                }


                if (prop.type == PropType.Texture) {

                    var nextProp1 = (propIdx + 1 < props.Length) ? props[propIdx + 1] : null;
                    bool nextPropHidden = (nextProp1 != null) && (nextProp1.type != PropType.Texture) &&
                        nextProp1.flags.hasFlag(PropFlags.HideInInspector);

                    EditorGUI.BeginChangeCheck();
                    if (nextPropHidden) {
                        var nextProp2 = (propIdx + 2 < props.Length) ? props[propIdx + 2] : null;
                        bool nextPropHidden2 = (nextProp2 != null) && (nextProp2.type != PropType.Texture) &&
                            nextProp2.flags.hasFlag(PropFlags.HideInInspector);

                        if (nextPropHidden2) {
                            materialEditor.TexturePropertySingleLine(content,
                                prop, nextProp1, nextProp2);
                        } else {
                            materialEditor.TexturePropertySingleLine(content,
                                prop, nextProp1);
                        }
                    } else {
                        if (!hideProp) {
                            if (prop.flags.hasFlag(PropFlags.NoScaleOffset)) {
                                materialEditor.TexturePropertySingleLine(content, prop);
                            } else {
                                var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 2);
                                materialEditor.TextureScaleOffsetProperty(rect, prop);
                                materialEditor.TexturePropertySingleLine(content, prop);
                            }
                        }
                    }

                    if (EditorGUI.EndChangeCheck()) {
                        HandleTexFeatureKeyword(mat, prop);
                    }

                    if (prop.name == "_EmissionMap") {
                        //materialEditor.LightmapEmissionProperty(2);
                        MaterialEditor.FixupEmissiveFlag(mat);
                        materialEditor.LightmapEmissionFlagsProperty(2, true);
                        //mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.
                    }

                    continue;
                }



                if (prop.name.StartsWith("_CollapseStart")) {
                    //drawProps = GetPropDefined(prop);

                    //GUILayout.Space(15);
                    EditorGUILayout.BeginHorizontal();

                    AnimBool animBool = CheckFoldoutDict(prop.displayName);
                    if (animBool == null) {
                        UpdateBools(materialEditor, props);
                        animBool = CheckFoldoutDict(prop.displayName);
                    }

                    EditorGUI.BeginChangeCheck();

                    animBool.target = EditorGUILayout.Foldout(
                        animBool.target,
                        prop.displayName,
                        true,
                        new GUIStyle(EditorStyles.foldout) {
                            fontStyle = FontStyle.Bold, fontSize = 12
                        }
                    );

                    if (EditorGUI.EndChangeCheck()) {
                        SetPref(prop, animBool.target);
                    }

                    //GUILayout.Label(prop.displayName, EditorStyles.boldLabel);

                    if (!hideProp) {
                        materialEditor.ShaderProperty(prop, content);
                    }


                    EditorGUILayout.EndHorizontal();


                    nestedFoldouts.Push(EditorGUILayout.BeginFadeGroup(animBool.faded));
                    nestedFoldoutProps.Push(prop.displayName);
                    EditorGUI.indentLevel++;
                    //Debug.Log("push: " + prop.name + " : " + nestedFoldouts.Peek().ToString());

                    continue;
                }

                if (prop.name.StartsWith("_HideIf")) {
                    hideIf = !prop.GetPropSetOrEnabled();
                }

                if (prop.name.StartsWith("_Space")) {
                    GUILayout.Space(15);
                    continue;
                }


                if (prop.name.StartsWith("_header")) {
                    DrawHeader(prop.displayName);
                    continue;
                }



                //Just a normal prop

                if (hideProp) {
                    continue;
                }




                switch (prop.type) {

                    case PropType.Texture:
                        materialEditor.TexturePropertySingleLine(content, prop);
                        if (!prop.flags.hasFlag(PropFlags.NoScaleOffset)) {
                            materialEditor.TextureScaleOffsetProperty(prop);
                        }
                        break;


                    default:

                        if (prop.type == PropType.Vector) {
                            GUILayout.Space(5);
                            EditorGUILayout.PrefixLabel(content);
                            content.text = "";
                        }

                        materialEditor.ShaderProperty(prop, content);
                        break;
                }
            }

            GUILayout.Space(10);
            DrawHeader("Other");
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
        }

        static AnimBool CheckFoldoutDict(string key) {
            if (foldoutDict.ContainsKey(key)) {
                return foldoutDict[key];
            } else {
                return null;
            }
        }

        static void UpdateBools(MaterialEditor matEdit, MaterialProperty[] props) {

            foldoutDict = new Dictionary<string, AnimBool>();
            nestedFoldouts = new Stack<bool>();
            nestedFoldoutProps = new Stack<string>();

            foreach (MaterialProperty prop in props) {
                if (prop.name.StartsWith("_CollapseStart")) {

                    AnimBool aBool = new AnimBool(GetPref(prop));
                    aBool.valueChanged.AddListener(matEdit.Repaint);
                    aBool.speed *= 2;
                    foldoutDict.Add(prop.displayName, aBool);
                }
            }


            if (testAnim == null) testAnim = new AnimBool(false);
            else testAnim.valueChanged.RemoveAllListeners();
            testAnim.valueChanged.AddListener(matEdit.Repaint);
        }




        //
        //UTILITY
        //

        const string prefsfix = "GFGUI_";

        static bool GetPref(MaterialProperty prop) { return EditorPrefs.GetBool(prefsfix + prop.displayName, true); }

        static void SetPref(MaterialProperty prop, bool val) { EditorPrefs.SetBool(prefsfix + prop.displayName, val); }


        void DrawHeader(string text) {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }



        void HandleTexFeatureKeyword(Material mat, MaterialProperty prop) {
            mat.SetKeyword(prop.name.ToUpper(), prop.GetPropSetOrEnabled());
        }



        static Dictionary<string, string> shaderPropTooltipDict = new Dictionary<string, string>() {
            {"bladeSharp", "Controls sharpness of grass blades, 0 is perfect point, 1 is rectangular."},
            {"seekSun", "Controls how much the grass aligns to the surface normal. 0 aligns all the way, 1 points up."},
            {"topViewPush", "Attempts to add a slight offset to the grass when viewed from above which can help to give more depth and density when looking down."},
            {"flatnessMult", "Controls how \"flat\" the grass is pushed when using the flatness channel of the parameter map."},
            {"_BILLBOARD", "Whether or not the grass should always face the camera."},
            {"variance", "These four values control how randomized the grass is in certain ways. The values are: X = Position, Y = Height, Z = Color, W = Width"},
            {"_AO", "Controls how dark the bottom of the grass blades are, 0 is darker, 1 is no darkness. "},
            {"ambientCO", "Controls how dark the shading can be."},
            {"ambientCOShadow", "On top of the light source shadow strength setting, this allows you to further tune received shadow strength."},
            {"edgeLight", "Controls strength of added brightness when the light direction is edge on to the grass blades."},
            {"edgeLightSharp", "Controls sharpness of the added edge on light brightness."},
            {"_ALPHA_TO_MASK", "If enabled, AlphaToMask is turned on in the shader. And the performance of this is quite complicated. Sometimes grass looks better with it enabled and sometimes it doesn't."},
            {"widthLODscale", "Controls how the width of blades grows as distance from camera increases. This helps less grass cover the same area while not being very noticeable."},
            {"grassFade", "distance the grass visually fades at. NOTE: This does NOT control lod settings, those must be set separately from the GrassFlow component, this setting is visual only."},
            {"grassFadeSharpness", "Sharpness of the grass fade."},
            {"windTint", "Color the grass is tinted when the wind affects them strongly, alpha controls strength."},
            {"_noiseScale", "Scale of the noise sampling for wind, Sort of controls wind gust size."},
            {"_noiseSpeed", "How fast the noise scrolls accross the grass to change wind patterns. Sort of acts like wind speed but you'll need to adjust wind strength to match."},
            {"windDir  ", "Direction the wind blows, the size of these values determines strength essentially."},
            {"windDir2", "Same as wind direction but controls secondary wind direction, helps give more variety to the wind instead of always being blown in one direction."},
            {"_MULTI_SEGMENT", "Adds extra segments to each grass blade, allowing it to bend either from the wind, or from curvature. " +
                "The minimum and maximum number of segments can be changed by adjusting the number at the top of the GrassFlow/Shaders/GrassStructsVars.cginc file. " +
                "Based on the LOD settings the number of grass segments is reduced over distance."},
            {"bladeLateralCurve", "How much natural bend the grass has."},
            {"bladeVerticalCurve", "Sort've pulls the grass down towards the surface."},
            {"bladeStiffness", "Controls how much the grass bends in response to wind/ripples."},
            {"_SEMI_TRANSPARENT", "Enables use of textures with alpha."},
            {"alphaClip", "Controls how sensitive the clipping of transparent textures is."},
            {"numTextures", "Set this to the number of textures in the type map texture atlas. Only used when using a type map."},
            {"textureAtlasScalingCutoff", "Texture index for the type map at which LOD width scaling is turned off. For example: set it to 3 and scaling would only apply to the first three textures in the atlas. " +
                "Only used when using a type map."},
            {" _SpecMap", "Specular map for deferred rendering."},
            {"_OccMap", "Occlusion map for deferred rendering. "},
            {"colorMap", "Color map for GrassFlow. Usually this is set by the GrassFlowRenderer, don't touch this unless you know what you're doing."},
            {"dhfParamMap", "Parameter map for GrassFlow. Usually this is set by the GrassFlowRenderer, don't touch this unless you know what you're doing."},
            {"typeMap", "Type map for GrassFlow. Usually this is set by the GrassFlowRenderer, don't touch this unless you know what you're doing."},
        };

    }

    static class GrassGUIExtensions {
        public static void SetKeyword(this Material m, string keyword, bool state) {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }

        public static bool GetPropSetOrEnabled(this MaterialProperty prop) {
            switch (prop.type) {

                case PropType.Range:
                case PropType.Float:
                    return prop.floatValue != 0;

                case PropType.Texture:
                    return prop.textureValue;

                case PropType.Vector:
                    return prop.vectorValue != Vector4.zero;

                case PropType.Color:
                    return prop.colorValue != Color.clear;

                default:
                    return true;
            }
        }

        public static bool hasFlag(this Enum variable, Enum value) {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            if (!Enum.IsDefined(variable.GetType(), value)) {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);

        }
    }

}