using System;

namespace VoxelTerrain2D.Brushes
{
    public abstract class VoxelBrush
    {
        protected Func< int, int, VoxelData >   read;
        protected Action< int, int, VoxelData > write;

        public VoxelBrush( Func< int, int, VoxelData > readMethod, Action< int, int, VoxelData > writeMethod )
        {
            read  = readMethod;
            write = writeMethod;
        }

        public abstract void Add( float x, float y );
        public abstract void Subtract( float x, float y );
    }
}