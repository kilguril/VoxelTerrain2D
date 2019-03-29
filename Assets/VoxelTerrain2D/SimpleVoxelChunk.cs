using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VoxelTerrain2D
{
    public sealed class SimpleVoxelChunk : VoxelChunk
    {
        protected override void OnInitialized()
        {
            InitializeBuffers();

            // Rebuild initial state
            RebuildMesh();
        }

        void LateUpdate()
        {
            m_worldPos = transform.position;

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
    }
}