using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D.Brushes
{
    public abstract class VoxelBrush
    {
        public abstract void Add( VoxelTerrain2 terrain, float x, float y );
        public abstract void Subtract( VoxelTerrain2 terrain2, float x, float y );
    }
}