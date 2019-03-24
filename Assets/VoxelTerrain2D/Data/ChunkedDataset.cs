using System;
using UnityEngine;

namespace VoxelTerrain2D
{
    public class ChunkedDataset< T > : IReadableDataset< T >, IWriteableDataset< T > where T : struct
    {
        public class DataChunk< U > where U : struct
        {
            public U[]  data;
            public int  width;
            public int  height;
            public bool dirty;
        }

        public int    width      { get; private set; }
        public int    height     { get; private set; }

        public int    chunkCountX { get; private set; }
        public int    chunkCountY { get; private set; }
        
        public int    chunkSize   { get; private set; }

        private DataChunk< T >[]  m_data;


        public ChunkedDataset( int w, int h, int chunkSz )
        { 
            width    = w; 
            height   = h;
            chunkSize = chunkSz;

            chunkCountX = Mathf.CeilToInt( (float)( width - chunkSize ) / ( chunkSize - 1 ) ) + 1;
            chunkCountY = Mathf.CeilToInt( (float)( height - chunkSize ) / ( chunkSize - 1 ) ) + 1;

            m_data = new DataChunk<T>[ chunkCountX * chunkCountY ];

            for( int y = 0; y < chunkCountY; y++ )
            {
                for( int x = 0; x < chunkCountX; x ++ )
                {
                    int index       = y * chunkCountX + x;
                    int localWidth  = ( x < chunkCountX - 1 ) ? chunkSize : width - ( ( chunkSize - 1 ) * x );
                    int localHeight = ( y < chunkCountY - 1 ) ? chunkSize : height - ( ( chunkSize - 1 ) * y );

                    DataChunk< T > chunk = new DataChunk<T>();
                    chunk.width  = localWidth;
                    chunk.height = localHeight;
                    chunk.data   = new T[ localWidth * localHeight ];

                    m_data[ index ] = chunk;
                }
            }
        }


        public T Sample( int x, int y )
        {
            int chunkX = 0, chunkY = 0;

            if ( x == width - 1 ) { chunkX = chunkCountX - 1; }
            else if ( x > 0 ){ chunkX = x / ( chunkSize - 1 ); }

            if ( y == height - 1 ) { chunkY = chunkCountY - 1; }
            else if ( y > 0 ){ chunkY = y / ( chunkSize - 1 ); }

            int localX = x - ( chunkX * ( chunkSize - 1 ) );
            int localY = y - ( chunkY * ( chunkSize - 1 ) );

            DataChunk< T > chunk = m_data[ chunkY * chunkCountX + chunkX ];
            return chunk.data[ localY * chunk.width + localX ];
        }


        public void Set( int x, int y, T val, bool flagDirty = true )
        {
            bool lineX  = false, lineY = false;
            int  chunkX = 0, chunkY = 0;

            if ( x == width - 1 ) { chunkX = chunkCountX - 1; }
            else if ( x > 0 )
            { 
                chunkX = x / ( chunkSize - 1 );
                lineX = x % ( chunkSize - 1 ) == 0;
            }

            if ( y == height - 1 ) { chunkY = chunkCountY - 1; }
            else if ( y > 0 )
            {
                chunkY = y / ( chunkSize - 1 );
                lineY = y % ( chunkSize - 1 ) == 0;
            }

            int localX = x - ( chunkX * ( chunkSize - 1 ) );
            int localY = y - ( chunkY * ( chunkSize - 1 ) );

            DataChunk< T > chunk = m_data[ chunkY * chunkCountX + chunkX ];
            chunk.data[ localY * chunk.width + localX ] = val;

            if ( flagDirty ) { chunk.dirty = true; }

            if ( lineX == true )
            {
                int lx = x - ( ( chunkX - 1 ) * ( chunkSize - 1 ) );
                int ly = y - ( chunkY * ( chunkSize - 1 ) );

                DataChunk< T > c = m_data[ chunkY * chunkCountX + ( chunkX - 1 ) ];
                c.data[ ly * c.width + lx ] = val;

                if ( flagDirty ) { c.dirty = true; }
            }
            
            if ( lineY == true )
            {
                int lx = x - ( chunkX * ( chunkSize - 1 ) );
                int ly = y - ( ( chunkY - 1 ) * ( chunkSize - 1 ) );

                DataChunk< T > c = m_data[ ( chunkY - 1 ) * chunkCountX + chunkX ];
                c.data[ ly * c.width + lx ] = val;

                if ( flagDirty ) { c.dirty = true; }
            }

            if ( lineX == true && lineY == true )
            {
                int lx = x - ( ( chunkX - 1 ) * ( chunkSize - 1 ) );
                int ly = y - ( ( chunkY - 1 ) * ( chunkSize - 1 ) );

                DataChunk< T > c = m_data[ ( chunkY - 1 ) * chunkCountX + ( chunkX - 1 ) ];
                c.data[ ly * c.width + lx ] = val;

                if ( flagDirty ) { c.dirty = true; }
            }
        }


        public void SetDirty( int chunkX, int chunkY, bool state )
        {
            m_data[ chunkY * chunkCountX + chunkX ].dirty = state;
        }


        public bool GetDirty( int chunkX, int chunkY )
        {
            return m_data[ chunkY * chunkCountX + chunkX ].dirty;
        }


        public DataChunk< T > GetDataChunk( int chunkX, int chunkY )
        {
            return m_data[ chunkY * chunkCountX + chunkX ];
        }


        public DataChunk< T > GetDataChunk( int chunkIndex )
        {
            return m_data[ chunkIndex ];
        }


        public T[] GetRawBuffer( int chunkX, int chunkY )
        {
            return m_data[ chunkY * chunkCountX + chunkX ].data;
        }


        public T[] GetRawBuffer( int chunkIndex )
        {
            return m_data[ chunkIndex ].data;
        }


        public int GetChunkIndex( int x, int y )
        {
            int chunkX = 0, chunkY = 0;

            if ( x == width - 1 ) { chunkX = chunkCountX - 1; }
            else if ( x > 0 ){ chunkX = x / ( chunkSize - 1 ); }

            if ( y == height - 1 ) { chunkY = chunkCountY - 1; }
            else if ( y > 0 ){ chunkY = y / ( chunkSize - 1 ); }

            return chunkY * chunkCountX + chunkX;
        }
    }
}
