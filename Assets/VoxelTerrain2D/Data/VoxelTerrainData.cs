using UnityEngine;

namespace VoxelTerrain2D
{
    public class VoxelTerrainData : ScriptableObject
    {
        [HideInInspector] public int      width;
        [HideInInspector] public int      height;
        [HideInInspector] public byte[]   raw;

        private const int RAW_DATA_SIZE = 3;

        public void ClearData()
        {
            width  = 0;
            height = 0;
            raw    = new byte[ 0 ];
        }


        public void Resize( int w, int h )
        {
            int     oldWidth  = width;
            int     oldHeight = height;
            byte[]  oldData   = raw;

            width  = w;
            height = h;
            raw = new byte[ width * height * RAW_DATA_SIZE ];

            int minW = Mathf.Min( oldWidth, width );
            int minH = Mathf.Min( oldHeight, height );

            for( int y = 0; y < minH; y++ )
            {
                int src = ( y * oldWidth ) * RAW_DATA_SIZE;
                int dst = ( y * width ) * RAW_DATA_SIZE;
                int len = minW * RAW_DATA_SIZE;

                System.Buffer.BlockCopy( oldData, src, raw, dst, len );
            }
        }


        public VoxelData Sample( int x, int y )
        {
            int index = ( y * width + x ) * RAW_DATA_SIZE;

            VoxelData val;
            val.cell             = raw[ index ];
            val.extentHorizontal = raw[ index + 1 ];
            val.extentVertical   = raw[ index + 2 ];

            return val;
        }


        public void Set( int x, int y, VoxelData val )
        {
            int index        = ( y * width + x ) * RAW_DATA_SIZE;

            raw[ index ]     = val.cell;
            raw[ index + 1 ] = val.extentHorizontal;
            raw[ index + 2 ] = val.extentVertical;
        }
    }
}