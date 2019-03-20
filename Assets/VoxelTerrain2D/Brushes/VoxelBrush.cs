using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D.Brushes
{
    public abstract class VoxelBrush
    {
        public abstract void Add( VoxelTerrain terrain, float x, float y );
        public abstract void Subtract( VoxelTerrain terrain, float x, float y );
    }
}