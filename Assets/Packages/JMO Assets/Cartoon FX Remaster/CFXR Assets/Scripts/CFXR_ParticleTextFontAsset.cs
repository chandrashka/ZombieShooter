﻿//--------------------------------------------------------------------------------------------------------------------------------
// Cartoon FX
// (c) 2012-2020 Jean Moreno
//--------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CartoonFX
{
    public class CFXR_ParticleTextFontAsset : ScriptableObject
    {
        public enum LetterCase
        {
            Both,
            Upper,
            Lower
        }

        public string CharSequence = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!?-.#@$ ";
        public LetterCase letterCase = LetterCase.Upper;
        public Sprite[] CharSprites;
        public Kerning[] CharKerningOffsets;

        private void OnValidate()
        {
            hideFlags = HideFlags.None;

            if (CharKerningOffsets == null || CharKerningOffsets.Length != CharSequence.Length)
            {
                CharKerningOffsets = new Kerning[CharSequence.Length];
                for (var i = 0; i < CharKerningOffsets.Length; i++)
                    CharKerningOffsets[i] = new Kerning { name = CharSequence[i].ToString() };
            }
        }

        public bool IsValid()
        {
            var valid = !string.IsNullOrEmpty(CharSequence) && CharSprites != null &&
                        CharSprites.Length == CharSequence.Length && CharKerningOffsets != null &&
                        CharKerningOffsets.Length == CharSprites.Length;

            if (!valid) Debug.LogError(string.Format("Invalid ParticleTextFontAsset: '{0}'\n", name), this);

            return valid;
        }

#if UNITY_EDITOR
        // [MenuItem("Tools/Create font asset")]
        private static void CreateFontAsset()
        {
            var instance = CreateInstance<CFXR_ParticleTextFontAsset>();
            AssetDatabase.CreateAsset(instance, "Assets/Font.asset");
        }
#endif

        [Serializable]
        public class Kerning
        {
            public string name = "A";
            public float pre;
            public float post;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CFXR_ParticleTextFontAsset))]
    public class ParticleTextFontAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Kerning"))
            {
                var ptfa = target as CFXR_ParticleTextFontAsset;
                var path = EditorUtility.SaveFilePanel("Export Kerning Settings", Application.dataPath,
                    ptfa.name + " kerning", ".txt");
                if (!string.IsNullOrEmpty(path))
                {
                    var output = "";
                    foreach (var k in ptfa.CharKerningOffsets) output += k.name + "\t" + k.pre + "\t" + k.post + "\n";
                    File.WriteAllText(path, output);
                }
            }

            if (GUILayout.Button("Import Kerning"))
            {
                var path = EditorUtility.OpenFilePanel("Import Kerning Settings", Application.dataPath, "txt");
                if (!string.IsNullOrEmpty(path))
                {
                    var text = File.ReadAllText(path);
                    var split = text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var ptfa = target as CFXR_ParticleTextFontAsset;
                    Undo.RecordObject(ptfa, "Import Kerning Settings");
                    var kerningList = new List<CFXR_ParticleTextFontAsset.Kerning>(ptfa.CharKerningOffsets);
                    for (var i = 0; i < split.Length; i++)
                    {
                        var data = split[i].Split('\t');

                        foreach (var cko in kerningList)
                            if (cko.name == data[0])
                            {
                                cko.pre = float.Parse(data[1]);
                                cko.post = float.Parse(data[2]);
                                break;
                            }
                    }

                    ptfa.CharKerningOffsets = kerningList.ToArray();
                }
            }

            GUILayout.EndHorizontal();
        }
    }
#endif
}