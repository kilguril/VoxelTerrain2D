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
        public float    voxelSize;

        [Header("Contour Settings")]
        [SerializeField]
        public bool     meshContour;
        public float    contourZbias;
        public float    contourInset;
        public float    contourOutset;

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