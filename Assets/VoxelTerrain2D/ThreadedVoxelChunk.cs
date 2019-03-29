using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace VoxelTerrain2D
{
    public sealed class ThreadedVoxelChunk : VoxelChunk
    {
        private Task            m_remeshTask;

        protected override void OnInitialized()
        {
            InitializeBuffers();

            // Rebuild initial state
            ClearBuffers();
            GenerateMesh( m_data, m_meshOut );
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
            m_worldPos = transform.position;

            if ( m_data.dirty == true )
            {
                ClearBuffers();

                m_remeshTask = new Task( RebuildMesh );
                m_remeshTask.Start();

                m_data.dirty = false;
            }
        }

        private void RebuildMesh()
        {
            GenerateMesh( m_data, m_meshOut );
        }
    }
}