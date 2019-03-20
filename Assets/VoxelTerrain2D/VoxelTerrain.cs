using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    public class VoxelTerrain : MonoBehaviour
    {
        public Dataset< VoxelData > dataSource { get { return m_dataSource; } }
        public int                  width      { get { return m_width; } }
        public int                  height     { get { return m_height; } }
        public float                voxelSize  { get { return m_voxelSize; } }

        [Header("General")]
        [SerializeField]
        private bool m_initOnAwake = default( bool );

        [Header("Voxel Settings")]
        [SerializeField]
        private int m_width = default( int );

        [SerializeField]
        private int m_height = default( int );

        [SerializeField]
        private float m_voxelSize = default( int );

        [SerializeField]
        [Range(10,100)]
        private int m_chunkSize = default( int );

        [Header("Render Settings")]
        [SerializeField]
        private Material m_fillMaterial = default( Material );

        private Dataset< VoxelData > m_dataSource;
        private VoxelChunk[]         m_chunks;
        private int                  m_chunkWidth;
        private int                  m_chunkHeight;

        void Awake()
        {
            if ( m_initOnAwake ) { Initialize(); }
        }


        public void Initialize()
        {
            m_dataSource = new Dataset< VoxelData >( m_width, m_height );

            // Derpy traversal because I can't figure the algebric method of doing this
            m_chunkWidth  = 1;
            m_chunkHeight = 1;

            int nextW = m_chunkSize - 1;
            int nextH = m_chunkSize - 1;

            while( dataSource.width - nextW > 1)
            {
                nextW += ( m_chunkSize - 1 );
                m_chunkWidth++;
            }

            while( dataSource.height - nextH > 1 )
            {
                nextH += ( m_chunkSize - 1 );
                m_chunkHeight++;
            }

            m_chunks = new VoxelChunk[ m_chunkWidth * m_chunkHeight ];

            for( int y = 0; y < m_chunkHeight; y++ )
            {
                for( int x = 0; x < m_chunkWidth; x++ )
                {
                    int fromX = x * ( m_chunkSize - 1 );
                    int sizeX = Mathf.Min( m_chunkSize, dataSource.width - fromX );
                    int fromY = y * ( m_chunkSize - 1 );
                    int sizeY = Mathf.Min( m_chunkSize, dataSource.height - fromY );

                    if ( sizeX > 1 && sizeY > 1 )
                    {
                        GameObject go = new GameObject(string.Format("Chunk[{0},{1}]", x, y ) );

                        go.transform.parent = transform;
                        go.transform.localRotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;

                        go.transform.localPosition = new Vector3(
                            x * ( m_chunkSize - 1 ) * m_voxelSize,
                            y * ( m_chunkSize - 1 ) * m_voxelSize,
                            0.0f
                        );

                        VoxelChunk chunk = null;
                        chunk = go.AddComponent<SimpleVoxelChunk>();

                        m_chunks[ y * m_chunkWidth + x ] = chunk;
                        chunk.Initialize( dataSource, fromX, sizeX, fromY, sizeY, voxelSize, m_fillMaterial );
                    }
                }
            }
        }


        public VoxelData GetValue( int x, int y )
        {
            return dataSource.SampleRaw( x, y );
        }


        public void SetValue( int x, int y, VoxelData val )
        {
            VoxelData original = dataSource.SampleRaw( x, y );
            if ( original != val )
            {
                dataSource.SetRaw( x, y, val );
                MarkDirty( x, y );
            }
        }


        private void MarkDirty( int x, int y )
        {
            int chunkX =  x / ( m_chunkSize - 1 );
            int chunkY =  y / ( m_chunkSize - 1 );

            bool lineX = x % ( m_chunkSize - 1 ) == 0;
            bool lineY = y % ( m_chunkSize - 1 ) == 0;

            int index = chunkY * m_chunkWidth + chunkX;
            if ( index < m_chunks.Length )
            {
                m_chunks[ index ].dirty = true;
            }
                
            if ( lineX && chunkX > 0 && ( index - 1 ) < m_chunks.Length )
            {
                m_chunks[ index - 1 ].dirty = true;
            }

            if ( lineY && chunkY > 0 && ( index - m_chunkWidth ) < m_chunks.Length )
            {
                m_chunks[ index - m_chunkWidth ].dirty = true;
            }

            if ( lineX && chunkX > 0 && lineY && chunkY > 0 && ( index - m_chunkWidth - 1 ) < m_chunks.Length )
            {
                m_chunks[ index - m_chunkWidth - 1 ].dirty = true;
            }
        }
    }
}