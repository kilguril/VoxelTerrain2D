using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace VoxelTerrain2D
{
    public sealed class ThreadedVoxelChunk : VoxelChunk
    {
        private List< Vector3 > m_verts;
        private List< int >     m_tris;
        private bool[]          m_batched;

        private VoxelData[]     m_readBuffer;
        private Task            m_remeshTask;


        protected override void OnInitialized()
        {
            // Initialize chunk data
            m_verts = new List< Vector3 >();
            m_tris  = new List< int >();

            // Initialize temp buffers
            m_readBuffer = new VoxelData[ m_data.width * m_data.height ];
            m_batched = new bool[ m_data.width * m_data.height ];

            // Rebuild initial state
            ClearBuffers();
            GenerateMesh( m_data.data, m_data.width, m_data.height, m_verts, m_tris, m_batched );
            AssignMeshData();
        }


        void Update()
        {
            if ( m_remeshTask != null )
            {
                m_remeshTask.Wait();
                m_remeshTask = null;

                AssignMeshData();
            }
        }


        void LateUpdate()
        {
            if ( m_data.dirty == true )
            {
                ClearBuffers();
                Array.Copy( m_data.data, m_readBuffer, m_data.data.Length );

                m_remeshTask = new Task( RebuildMesh );
                m_remeshTask.Start();

                m_data.dirty = false;
            }
        }

        private void RebuildMesh()
        {
            GenerateMesh( m_readBuffer, m_data.width, m_data.height, m_verts, m_tris, m_batched );
        }


        private void ClearBuffers()
        {
            m_verts.Clear();
            m_tris.Clear();

            for ( int i = 0; i < m_batched.Length; i++ ) { m_batched[ i ] = false; }
        }


        private void AssignMeshData()
        {
            m_mesh.Clear( true );
            m_mesh.SetVertices( m_verts );
            m_mesh.SetTriangles( m_tris, 0, false );
        }
    }
}