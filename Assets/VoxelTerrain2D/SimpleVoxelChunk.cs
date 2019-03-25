using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VoxelTerrain2D
{
    public sealed class SimpleVoxelChunk : VoxelChunk
    {
        private MeshOutput      m_meshOut;

        protected override void OnInitialized()
        {
            // Initialize chunk data
            m_meshOut       = new MeshOutput();
            m_meshOut.verts = new List< Vector3 >();
            m_meshOut.tris  = new List< int >();

            // Initialize temp buffers
            m_meshOut.batched  = new bool[ m_data.width * m_data.height ];
            m_meshOut.contours = new List<List<Vector2>>();

            // Collision buffers
            if ( m_settings.generateCollision )
            {
                m_meshOut.collisionVerts = new List<Vector3>();
                m_meshOut.collisionTris = new List<int>();
            }

            // Rebuild initial state
            RebuildMesh();
        }


        void LateUpdate()
        {
            if ( m_data.dirty == true )
            {
                RebuildMesh();
                m_data.dirty = false;
            }
        }

        private void RebuildMesh()
        {
            ClearBuffers();
            GenerateMesh( m_data, m_meshOut );
            AssignMeshData();
        }


        private void ClearBuffers()
        {
            m_meshOut.verts.Clear();
            m_meshOut.tris.Clear();

            m_meshOut.contours.Clear();

            for ( int i = 0; i < m_meshOut.batched.Length; i++ ) { m_meshOut.batched[ i ] = false; }

            if ( m_settings.generateCollision )
            {
                m_meshOut.collisionVerts.Clear();
                m_meshOut.collisionTris.Clear();
            }
        }


        private void AssignMeshData()
        {
            m_mesh.Clear( true );
            m_mesh.SetVertices( m_meshOut.verts );
            m_mesh.SetTriangles( m_meshOut.tris, 0, false );

            if ( m_settings.generateCollision )
            {
                m_meshCollision.Clear( true );
                m_meshCollision.SetVertices( m_meshOut.collisionVerts );
                m_meshCollision.SetTriangles( m_meshOut.collisionTris, 0, false );

                m_meshCollision.RecalculateBounds();
                m_collider.sharedMesh = null;
                m_collider.sharedMesh = m_meshCollision;
            }
        }
    }
}