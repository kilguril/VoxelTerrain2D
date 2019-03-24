using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    public abstract class VoxelChunk : MonoBehaviour
    {
        protected ChunkedDataset< VoxelData >.DataChunk< VoxelData > m_data;
        protected float m_voxelSize;

        protected Mesh         m_mesh;
        protected MeshFilter   m_filter;
        protected MeshRenderer m_renderer;

        protected bool         m_init = false;



        public void Initialize( ChunkedDataset< VoxelData >.DataChunk< VoxelData > dataset, float voxelSize, Material fillMaterial )
        {
            m_data = dataset;
            m_voxelSize = voxelSize;

            // Initialize mesh
            m_mesh      = new Mesh();
            m_mesh.name = name + "_Mesh";
            m_mesh.MarkDynamic();

            float meshSizeX = dataset.width * voxelSize;
            float meshSizeY = dataset.height * voxelSize;
            m_mesh.bounds = new Bounds(
                new Vector3( meshSizeX / 2.0f, meshSizeY / 2.0f ),
                new Vector3( meshSizeX, meshSizeY )
            );

            m_filter   = gameObject.AddComponent< MeshFilter >();
            m_renderer = gameObject.AddComponent< MeshRenderer >();

            m_filter.sharedMesh       = m_mesh;
            m_renderer.sharedMaterial = fillMaterial;

            OnInitialized();
            m_init = true;
        }

        protected abstract void OnInitialized();
    }
}