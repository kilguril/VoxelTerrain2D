namespace VoxelTerrain2D
{
    public struct IntRect
    {
        public IntPoint origin;
        public IntPoint size;
    }


    public struct IntPoint
    {
        public int x;
        public int y;

        // https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + x.GetHashCode();
                hash = hash * 23 + y.GetHashCode();
                return hash;
            }
        }
    }
}