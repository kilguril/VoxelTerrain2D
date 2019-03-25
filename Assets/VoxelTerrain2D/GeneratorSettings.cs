using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    public struct GeneratorSettings
    {
        public float    voxelSize;

        public Material fillMaterial;

        public bool     generateCollision;
        public float    collisionExtrudeExtent;
        public MeshColliderCookingOptions colliderCookingOptions;
    }
}