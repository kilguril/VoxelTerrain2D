namespace VoxelTerrain2D
{
    public interface IReadableDataset< T > where T : struct
    {
        IntPoint min { get; }
        IntPoint max { get; }

        T Sample( int x, int y );
    }
}