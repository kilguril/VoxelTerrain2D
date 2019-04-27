using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    [System.Serializable]
    public class GeneratorSettings
    {
        [Header("Voxel Settings")]
        [SerializeField]
        public float    voxelSize = 1.0f;

        [Header("Contour Settings")]
        [SerializeField]
        public bool     meshContour   = true;
        public float    contourZbias  = -0.01f;
        public float    contourInset  = 0.25f;
        public float    contourOutset = 0.25f;

        [Header("Physics Settings")]
        [SerializeField]
        public bool     generateCollision;
        public float    collisionExtrudeExtent;
        public MeshColliderCookingOptions colliderCookingOptions;

        [Header("Render Settings")]
        [SerializeField]
        public bool     generateNormals;
        public Material fillMaterial;
        public Material outlineMaterial;
    }
}