using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VoxelTerrain2D.Samples.Data
{
    public class RandomData : VoxelTerrainData
    {
        protected override void InitializeData()
        {
            // Initialize Data..
            for( int y = 0; y < m_height; y++ )
            {
                for( int x = 0; x < m_width; x ++ )
                {
                    VoxelData random;
                    random.cell             = Random.value > 0.5f ? (byte)1 : (byte)0;
                    random.extentHorizontal = (byte)( Random.value * 255.0f );
                    random.extentVertical   = (byte)( Random.value * 255.0f );

                    m_data[ y * m_width + x ] = random;
                }
            }
        }
    }
}