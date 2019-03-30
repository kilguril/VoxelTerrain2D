using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace VoxelTerrain2D.Editor
{
    class TerrainBeforeBuildHook : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild( BuildReport report )
        {
            VoxelTerrainEditorManager.shouldProcessTerrains = false;
        }
    }


    class TerrainAfterBuildHook : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild( BuildReport report )
        {
            VoxelTerrainEditorManager.shouldProcessTerrains = true;
        }
    }


    [InitializeOnLoad]
    public class VoxelTerrainEditorManager
    {
        public static bool shouldProcessTerrains { get; set; }

        static VoxelTerrainEditorManager()
        {
            InitializeTerrains();
            shouldProcessTerrains = true;

            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
            Undo.undoRedoPerformed = InitializeTerrains;
        }


        private static void OnSceneOpened( UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode )
        {
            if ( shouldProcessTerrains )
            {
                InitializeTerrains();
            }
        }


        private static void OnPlaymodeStateChanged( PlayModeStateChange state )
        {
            if ( shouldProcessTerrains )
            {
                if ( state == PlayModeStateChange.EnteredEditMode )
                {
                    InitializeTerrains();
                }
            }
        }


        public static void InitializeTerrains()
        {
            VoxelTerrain[] terrains = Object.FindObjectsOfType< VoxelTerrain >();

            for( int i = 0; i < terrains.Length; i++ )
            {
                InitializeTerrain( terrains[ i ] );
            }
        }


        public static void InitializeTerrain( VoxelTerrain t )
        {
            if ( t.initialized == true )
            {
                t.Teardown();
            }

            if ( t.initialized == false && t.data != null )
            {
                bool hide     = t.hideChunks;
                bool threaded = t.threaded;

                t.threaded   = false;
                t.hideChunks = true;
                t.Initialize();

                t.threaded   = threaded;
                t.hideChunks = hide;
            }
        }
    }
}