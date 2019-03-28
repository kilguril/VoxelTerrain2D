using System.Collections;
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

        [SerializeField]
        private bool m_hideChunks = default( bool );

        [Header("Voxel Settings")]
        [SerializeField]
        private int m_width        = default( int );

        [SerializeField]
        private int m_height       = default( int );

        [SerializeField]
        private float m_voxelSize  = default( int );

        [SerializeField]
        [Range(3,100)]
        private int m_chunkSize    = default( int );

        [Header("Contour Settings")]
        [SerializeField]
        private bool m_meshContour = default( bool );
        [SerializeField]
        private float m_contourInset = default( float );
        [SerializeField]
        private float m_contourOutset = default( float );
        [SerializeField]
        private float m_contourZbias = default( float );

        [Header("Physics Settings")]
        [SerializeField]
        private bool m_generateCollider = default( bool );

        [SerializeField]
        private float m_extrudeExtent = default( float );

        [SerializeField]
        private MeshColliderCookingOptions m_colliderCookingOptions = default( MeshColliderCookingOptions );

        [Header("Render Settings")]
        [SerializeField]
        private Material m_fillMaterial = default( Material );
        [SerializeField]
        private float    m_fillTileSize = default( float );

        [SerializeField]
        private Material m_outlineMaterial = default( Material );


        private ChunkedDataset< VoxelData > m_dataSource;
        private VoxelChunk[]                m_chunks;
   

        void Awake()
        {
            if ( m_initOnAwake ) { Initialize(); }
        }


        public void Initialize()
        {
            GeneratorSettings settings;
            settings.voxelSize              = m_voxelSize;
            settings.fillMaterial           = m_fillMaterial;
            settings.fillTileSize           = m_fillTileSize;
            settings.outlineMaterial        = m_outlineMaterial;
            settings.generateCollision      = m_generateCollider;
            settings.collisionExtrudeExtent = m_extrudeExtent;
            settings.colliderCookingOptions = m_colliderCookingOptions;
            settings.meshContour            = m_meshContour;
            settings.contourZbias           = m_contourZbias;
            settings.contourInset           = m_contourInset;
            settings.contourOutset          = m_contourOutset;

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
                        if ( m_hideChunks ) { go.hideFlags = HideFlags.HideAndDontSave; }

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
                        vchunk.Initialize( chunk, settings );
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