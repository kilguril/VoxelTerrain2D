using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VoxelTerrain2D.Samples.Data
{
    public class PerlinNoiseData : VoxelTerrainData
    {
        [SerializeField]
        private Vector2 m_offset = default( Vector2 );

        [SerializeField]
        private Vector2 m_scale = default( Vector2 );

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float m_cutoff = default( float );

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float m_maxExtent = default( float );

        protected override void InitializeData()
        {
            GenerateData();
        }


        [ContextMenu("Generate Data")]
        private void GenerateData()
        {
            for( int y = 0; y < m_height; y++ )
            {
                for( int x = 0; x < m_width; x ++ )
                {
                    VoxelData val;
                    val.cell             = 0;
                    val.extentHorizontal = 0;
                    val.extentVertical   = 0;

                    float v = SamplePerlin( x, y );

                    if ( v >= m_cutoff )
                    {
                        byte eleft = (byte)( ComputeExtent( v, SamplePerlin( x - 1, y ) ) * VoxelData.EXTENT_MAX_RESOLUTION );
                        byte eright = (byte)( ComputeExtent( v, SamplePerlin( x + 1, y ) ) * VoxelData.EXTENT_MAX_RESOLUTION );
                        byte ebot = (byte)( ComputeExtent( v, SamplePerlin( x, y - 1 ) ) * VoxelData.EXTENT_MAX_RESOLUTION );
                        byte etop = (byte)( ComputeExtent( v, SamplePerlin( x, y + 1 ) ) * VoxelData.EXTENT_MAX_RESOLUTION );

                        val.cell = 1;
                        val.SetExtentHorizontal( eleft, eright );
                        val.SetExtentVertical( ebot, etop );
                    }

                    m_data[ y * m_width + x ] = val;
                }
            }
        }


        private float ComputeExtent( float val, float neighbor )
        {
            if ( val < m_cutoff ) { return 0.0f; }
            if ( neighbor >= m_cutoff ){ return 1.0f; }
            
            float avg = ( val + neighbor ) / 2.0f;
            return Mathf.Clamp( avg / m_cutoff, 0.0f, m_maxExtent );
        }


        private float SamplePerlin( int x, int y )
        {
            float xcoord = m_offset.x + ( ( (float)x / m_width ) * m_scale.x );
            float ycoord = m_offset.y + ( ( (float)y / m_height ) * m_scale.y );
            float v = Mathf.Clamp01( Mathf.PerlinNoise( xcoord, ycoord ) );

            return v;
        }
    }
}