using UnityEngine;

namespace VoxelTerrain2D.Utils
{
    [RequireComponent( typeof( VoxelTerrain ) )]
    public class TerrainTrackObject : MonoBehaviour
    {
        [SerializeField]
        private Transform    m_trackedObject = default( Transform );
        private VoxelTerrain m_terrain;


        void Awake()
        {
            m_terrain = GetComponent< VoxelTerrain >();
        }


        void Update()
        {
            Vector2 pos = m_trackedObject.position;
            pos -= ( m_terrain.renderSize / 2.0f );

            m_terrain.Slide( pos );
        }
    }
}