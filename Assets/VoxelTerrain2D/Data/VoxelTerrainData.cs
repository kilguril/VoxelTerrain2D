using UnityEngine;

namespace VoxelTerrain2D
{
    [RequireComponent( typeof( VoxelTerrain ) ) ]
    public class VoxelTerrainData : MonoBehaviour, IReadableDataset< VoxelData >, IWriteableDataset< VoxelData >
    {
        public IntPoint min { get; private set; }
        public IntPoint max { get; private set; }

        [SerializeField]
        protected int m_width = 10;
        [SerializeField]
        protected int m_height = 10;

        protected VoxelData[] m_data;


        public VoxelData Sample( int x, int y )
        {
            return m_data[ y * m_width + x ];
        }


        public void Set( int x, int y, VoxelData val )
        {
            m_data[ y * m_width + x ] = val;
        }


        void Start()
        {
            m_data = new VoxelData[ m_width * m_height ];
            min = new IntPoint { x = 0, y = 0 };
            max = new IntPoint { x = m_width, y = m_height };

            InitializeData();

            VoxelTerrain terrain = GetComponent< VoxelTerrain >();
            terrain.SetData( this, this );
        }


        protected virtual void InitializeData(){}
    }
}