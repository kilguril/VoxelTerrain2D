using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;

namespace VoxelTerrain2D.Editor
{
    [CustomEditor( typeof( VoxelTerrain ) )]
    public class VoxelTerrainEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            VoxelTerrain source = target as VoxelTerrain;

            int size = source.width * source.height;
            double mem = size * Marshal.SizeOf( typeof( VoxelData ) );
            EditorGUILayout.HelpBox( "Dataset Mem Usage: " + ToFileSize( mem ), MessageType.Info );
        }

        // Prety memory size format utility
        // Source: http://csharphelper.com/blog/2014/07/format-file-sizes-in-kb-mb-gb-and-so-forth-in-c/

        private string ToFileSize( double value )
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB",
                "TB", "PB", "EB", "ZB", "YB"};
            for ( int i = 0; i < suffixes.Length; i++ )
            {
                if ( value <= ( System.Math.Pow( 1024, i + 1 ) ) )
                {
                    return ThreeNonZeroDigits( value /
                        System.Math.Pow( 1024, i ) ) +
                        " " + suffixes[ i ];
                }
            }

            return ThreeNonZeroDigits( value /
                System.Math.Pow( 1024, suffixes.Length - 1 ) ) +
                " " + suffixes[ suffixes.Length - 1 ];
        }

        private static string ThreeNonZeroDigits( double value )
        {
            if ( value >= 100 )
            {
                // No digits after the decimal.
                return value.ToString( "0" );
            }
            else if ( value >= 10 )
            {
                // One digit after the decimal.
                return value.ToString( "0.0" );
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString( "0.00" );
            }
        }
    }
}