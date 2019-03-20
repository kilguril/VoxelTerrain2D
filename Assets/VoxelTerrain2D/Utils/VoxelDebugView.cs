using System;
using UnityEngine;

namespace VoxelTerrain2D.Utils
{
    [ RequireComponent( typeof( VoxelTerrain ) ) ]
    public class VoxelDebugView : MonoBehaviour
    {
        [SerializeField]
        private float m_gizmoSize = default( float );

        [SerializeField]
        private bool m_wireframe = default( bool );

        [SerializeField]
        private bool m_renderWhenDeselected = default( bool );


        void OnDrawGizmos()
        {
            if ( m_renderWhenDeselected == true )
            {
                RenderDebugView();
            }
        }


        void OnDrawGizmosSelected()
        {
            RenderDebugView();
        }


        private void RenderDebugView()
        {
            VoxelTerrain terrain = GetComponent< VoxelTerrain >();
            if ( terrain == null ) { return; }
            if ( terrain.dataSource == null ) { return; }

            float                voxelSize = terrain.voxelSize;
            Dataset< VoxelData > data      = terrain.dataSource;

            Vector3 origin    = transform.position;
            Vector3 size      = Vector3.one * m_gizmoSize;

            Action< Vector3, Vector3 > renderFunc = Gizmos.DrawCube;
            if ( m_wireframe == true ) { renderFunc = Gizmos.DrawWireCube; }

            Color32 offColor = Color.black;
            Color32 onColor  = Color.white;

            Vector3 left  = -transform.right;
            Vector3 right = transform.right;
            Vector3 up    = transform.up;
            Vector3 down  = -transform.up;

            for( int y = 0; y < data.height; y++ )
            {
                for( int x = 0; x < data.width; x++ )
                {
                    Vector3 pos = new Vector3(
                        origin.x + x * voxelSize,
                        origin.y + y * voxelSize,
                        origin.z
                    );

                    VoxelData val  = terrain.GetValue( x, y );
                    bool      on   = val.GetSolidState();
                    Color     col  = on ? onColor : offColor;

                    Gizmos.color = col;
                    renderFunc( pos, size );

                    if ( on )
                    {
                        byte extentLeft  = val.GetExtentLeft();
                        byte extentRight = val.GetExtentRight();

                        byte extentBottom = val.GetExtentBottom();
                        byte extentTop    = val.GetExtentTop();

                        Gizmos.color = Color.green;
                        if ( extentLeft > 0 && extentLeft < VoxelData.EXTENT_MAX_RESOLUTION ){ Gizmos.DrawRay( pos, left * voxelSize * ( extentLeft / VoxelData.EXTENT_MAX_RESOLUTION_F ) ); }
                        if ( extentRight > 0 && extentRight < VoxelData.EXTENT_MAX_RESOLUTION ){ Gizmos.DrawRay( pos, right * voxelSize * ( extentRight / VoxelData.EXTENT_MAX_RESOLUTION_F ) ); }
                        if ( extentBottom > 0 && extentBottom < VoxelData.EXTENT_MAX_RESOLUTION ){ Gizmos.DrawRay( pos, down * voxelSize * ( extentBottom / VoxelData.EXTENT_MAX_RESOLUTION_F ) ); }
                        if ( extentTop > 0 && extentTop < VoxelData.EXTENT_MAX_RESOLUTION ){ Gizmos.DrawRay( pos, up * voxelSize * ( extentTop / VoxelData.EXTENT_MAX_RESOLUTION_F ) ); }
                    }
                }
            }
        }
    }
}