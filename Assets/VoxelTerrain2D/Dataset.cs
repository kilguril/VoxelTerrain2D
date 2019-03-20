using System;
using UnityEngine;

namespace VoxelTerrain2D
{
    [System.Serializable]
    public class Dataset< T > where T : struct
    {
        public T[] data;
        public int width;
        public int height;

        public Dataset() { width = 2; height = 2; data = new T[ 2 * 2 ]; }
        public Dataset( int w, int h ){ width = w; height = h; data = new T[ w * h ]; }


        public void Resize( int w, int h )
        {
            if ( w <= 1 || h <= 1 ) { return; }

            T[] original = data;
            data = new T[ w * h ];

            int minW = Mathf.Min( w, width );
            int minH = Mathf.Min( h, height );

            for( int i = 0; i < minH; i++ )
            {
                Array.Copy( original, i * width, data, i * w, minW );
            }

            width  = w;
            height = h;
        }


        public T SampleRaw( int x, int y )
        {
            return data[ y * width + x ];
        }


        public void SetRaw( int x, int y, T val )
        {
            data[ y * width + x ] = val;
        }
    }
}
