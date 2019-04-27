using UnityEngine;

namespace VoxelTerrain2D
{
    [RequireComponent( typeof( VoxelTerrain2 ) ) ]
    public class VoxelTerrainData2 : MonoBehaviour, IReadableDataset<VoxelData>, IWriteableDataset<VoxelData>
    {
        public IntPoint min { get; private set; }
        public IntPoint max { get; private set; }

        [SerializeField]
        private int m_width = 10;
        [SerializeField]
        private int m_height = 10;

        private VoxelData[] m_data;


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

            // Initialize Data..
            for( int y = 0; y < m_height; y++ )
            {
                for( int x = 0; x < m_width; x ++ )
                {
                    VoxelData random;
                    random.cell             = Random.value > 0.5f ? (byte)1 : (byte)0;
                    random.extentHorizontal = (byte)( Random.value * 255.0f );
                    random.extentVertical   = (byte)( Random.value * 255.0f );

                    m_data[ y * m_width + x ] = random;
                }
            }

            VoxelTerrain2 terrain = GetComponent< VoxelTerrain2 >();
            terrain.SetData( this, this );
        }
    }
}