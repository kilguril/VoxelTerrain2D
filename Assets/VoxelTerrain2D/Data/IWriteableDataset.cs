namespace VoxelTerrain2D
{
    public interface IWriteableDataset< T > where T : struct
    {
        int width   { get; }
        int height  { get; }

        void Set( int x, int y, T val, bool flagDirty = true );
    }
}