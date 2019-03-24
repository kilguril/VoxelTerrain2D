namespace VoxelTerrain2D
{
    public interface IReadableDataset< T > where T : struct
    {
        int width   { get; }
        int height  { get; }

        T Sample( int x, int y );
    }
}