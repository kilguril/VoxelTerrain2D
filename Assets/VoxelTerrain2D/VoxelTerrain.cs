﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    public class VoxelTerrain : MonoBehaviour
    {
        public ChunkedDataset< VoxelData > dataSource { get { return m_dataSource; } }
        public int                         width      { get { return m_width; } }
        public int                         height     { get { return m_height; } }
        public float                       voxelSize  { get { return m_voxelSize; } }

        [Header("General")]
        [SerializeField]
        private bool m_initOnAwake = default( bool );

        [SerializeField]
        private bool m_threadedMeshing = default( bool );

        [Header("Voxel Settings")]
        [SerializeField]
        private int m_width        = default( int );

        [SerializeField]
        private int m_height       = default( int );

        [SerializeField]
        private float m_voxelSize  = default( int );

        [SerializeField]
        [Range(2,100)]
        private int m_chunkSize    = default( int );

        [Header("Render Settings")]
        [SerializeField]
        private Material m_fillMaterial = default( Material );


        private ChunkedDataset< VoxelData > m_dataSource;
        private VoxelChunk[]                m_chunks;
   

        void Awake()
        {
            if ( m_initOnAwake ) { Initialize(); }
        }


        public void Initialize()
        {
            m_dataSource = new ChunkedDataset< VoxelData >( m_width, m_height, m_chunkSize );
            m_chunks     = new VoxelChunk[ m_dataSource.chunkCountX * m_dataSource.chunkCountY ];

            for( int y = 0; y < m_dataSource.chunkCountY; y++ )
            {
                for( int x = 0; x < m_dataSource.chunkCountX; x++ )
                {
                    var chunk = m_dataSource.GetDataChunk( x, y );

                    if ( chunk.width > 1 && chunk.height > 1 )
                    {
                        GameObject go = new GameObject(string.Format("Chunk[{0},{1}]", x, y ) );
                        go.hideFlags = HideFlags.HideAndDontSave;

                        go.transform.parent = transform;
                        go.transform.localRotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;

                        go.transform.localPosition = new Vector3(
                            x * ( m_chunkSize - 1 ) * m_voxelSize,
                            y * ( m_chunkSize - 1 ) * m_voxelSize,
                            0.0f
                        );

                        VoxelChunk vchunk = null;

                        if ( m_threadedMeshing == true )
                        {
                            vchunk = go.AddComponent<ThreadedVoxelChunk>();
                        }
                        else
                        {
                            vchunk = go.AddComponent<SimpleVoxelChunk>();
                        }

                        m_chunks[ y * m_dataSource.chunkCountX + x ] = vchunk;
                        vchunk.Initialize( chunk, voxelSize, m_fillMaterial );
                    }
                }
            }
        }


        public VoxelData GetValue( int x, int y )
        {
            return dataSource.Sample( x, y );
        }


        public void SetValue( int x, int y, VoxelData val )
        {
            VoxelData original = dataSource.Sample( x, y );
            if ( original != val )
            {
                dataSource.Set( x, y, val, true );
            }
        }
    }
}