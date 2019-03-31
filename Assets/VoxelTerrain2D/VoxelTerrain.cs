using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    public class VoxelTerrain : MonoBehaviour
    {
        public bool                        initialized { get; private set; }
        public VoxelTerrainData            data        { get { return m_data; } set { m_data = value; } }
        public ChunkedDataset< VoxelData > chunkedData { get { return m_chunkedData; } }
        public VoxelChunk[]                chunks      { get { return m_chunks; } }
        public int                         width       { get { return m_data.width; } }
        public int                         height      { get { return m_data.height; } }
        public float                       voxelSize   { get { return m_settings.voxelSize; } }

        public bool threaded   { get { return m_threadedMeshing; } set { m_threadedMeshing = value; } }
        public bool hideChunks { get { return m_hideChunks; } set { m_hideChunks = value; } }

        [SerializeField]
        [HideInInspector]
        private VoxelTerrainData m_data = default( VoxelTerrainData );
        [SerializeField]
        [Range( 2, 100 )]
        private int m_chunkSize        = 20;
        [SerializeField]
        private bool m_initOnAwake     = true;
        [SerializeField]
        private bool m_threadedMeshing = true;
        [SerializeField]
        private bool m_hideChunks      = true;
        [Space]
        [SerializeField]
        private GeneratorSettings m_settings = default( GeneratorSettings );


        private ChunkedDataset< VoxelData > m_chunkedData;
        private VoxelChunk[]                m_chunks;
   

        void Awake()
        {
            if ( m_initOnAwake ) { Initialize(); }
        }


        public void Initialize()
        {
            Initialize( m_settings );
        }


        public void Initialize( GeneratorSettings settings )
        {
            if ( m_data == null ) { throw new System.ArgumentNullException( "Trying to initialize null terrain" ); }
            
            m_chunkedData = new ChunkedDataset< VoxelData >( m_data.width, m_data.height, m_chunkSize, m_data.Sample );
            m_chunks      = new VoxelChunk[ m_chunkedData.chunkCountX * m_chunkedData.chunkCountY ];

            for( int y = 0; y < m_chunkedData.chunkCountY; y++ )
            {
                for( int x = 0; x < m_chunkedData.chunkCountX; x++ )
                {
                    var chunk = m_chunkedData.GetDataChunk( x, y );

                    if ( chunk.width > 1 && chunk.height > 1 )
                    {
                        GameObject go = new GameObject(string.Format("Chunk[{0},{1}]", x, y ) );
                        if ( m_hideChunks ) { go.hideFlags = HideFlags.HideAndDontSave; }
                        else { go.hideFlags = HideFlags.DontSave; }

                        go.transform.parent = transform;
                        go.transform.localRotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;

                        go.transform.localPosition = new Vector3(
                            x * ( m_chunkSize - 1 ) * settings.voxelSize,
                            y * ( m_chunkSize - 1 ) * settings.voxelSize,
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

                        m_chunks[ y * m_chunkedData.chunkCountX + x ] = vchunk;
                        vchunk.Initialize( chunk, settings );
                    }
                }
            }

            initialized = true;
        }


        public void Teardown()
        {
            if ( initialized == true )
            {
                if ( m_chunks != null )
                {
                    for ( int i = 0; i < m_chunks.Length; i++ )
                    {
                        if ( m_chunks[ i ] == null || m_chunks[ i ].gameObject == null ) { continue; }

#if UNITY_EDITOR
                        DestroyImmediate( m_chunks[ i ].gameObject );
#else
                        Destroy( m_chunks[ i ].gameObject );
#endif
                    }
                }

                m_chunks     = null;
                m_chunkedData = null;
                initialized  = false;
            }
        }


        public VoxelData GetValue( int x, int y )
        {
            return chunkedData.Sample( x, y );
        }


        public void SetValue( int x, int y, VoxelData val )
        {
            VoxelData original = chunkedData.Sample( x, y );
            if ( original != val )
            {
                chunkedData.Set( x, y, val, true );
            }
        }
    }
}