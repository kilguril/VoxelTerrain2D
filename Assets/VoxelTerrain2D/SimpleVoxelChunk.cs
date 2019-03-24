﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VoxelTerrain2D
{
    public sealed class SimpleVoxelChunk : VoxelChunk
    {
        private List< Vector3 > m_verts;
        private List< int >     m_tris;
        private bool[]          m_batched;

        protected override void OnInitialized()
        {
            // Initialize chunk data
            m_verts = new List< Vector3 >();
            m_tris  = new List< int >();

            // Initialize temp buffers
            m_batched = new bool[ m_data.width * m_data.height ];

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
            GenerateMesh( m_data.data, m_data.width, m_data.height, m_verts, m_tris, m_batched );
            AssignMeshData();
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