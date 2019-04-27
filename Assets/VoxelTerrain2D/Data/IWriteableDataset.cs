namespace VoxelTerrain2D
{
    public interface IWriteableDataset< T > where T : struct
    {
        IntPoint min { get; }
        IntPoint max { get; }

        void Set( int x, int y, T val );
    }
}