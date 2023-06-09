﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Artngame.GIBLI.TreeMaker
{
    [CustomEditor(typeof(TreeGenerator))]
    public class TreeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TreeGenerator tree = (TreeGenerator)target;

            if (GUILayout.Button("Build"))
            {
                tree.Build();
            }
        }
    }
}