using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    public struct GeneratorSettings
    {
        public float    voxelSize;

        public bool     meshContour;
        public float    contourZbias;
        public float    contourInset;
        public float    contourOutset;

        public bool     generateCollision;
        public float    collisionExtrudeExtent;
        public MeshColliderCookingOptions colliderCookingOptions;

        public Material fillMaterial;
        public Material outlineMaterial;
    }
}