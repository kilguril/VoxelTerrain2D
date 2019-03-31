using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VoxelTerrain2D.Editor
{
    public class CreateTerrainData
    {
        #if UNITY_EDITOR
        [MenuItem("Assets/Create/Voxel Terrain 2D/Voxel Terrain Data", priority = 0)]
        public static void CreateMyAsset()
        {
            VoxelTerrainData asset = ScriptableObject.CreateInstance< VoxelTerrainData >();
            asset.ClearData();

            AssetDatabase.CreateAsset(asset, "Assets/VoxelTerrainData.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        #endif
    }
}