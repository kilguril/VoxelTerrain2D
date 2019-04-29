using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelTerrain2D.Brushes;

namespace VoxelTerrain2D.Samples.Utils
{
    public class VoxelBrushGameObject : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_rectMarker = default( GameObject );
        [SerializeField]
        private GameObject m_circleMarker = default( GameObject );

        private bool m_rectMode;
        RectBrush    m_rectBrush;
        CircleBrush  m_circleBrush;

        void Awake()
        {
            if ( m_circleMarker != null )
            {
                m_circleMarker.SetActive( false );
            }

            if ( m_rectMarker != null )
            {
                m_rectMarker.SetActive( true );
            }

            m_rectMode          = true;

            m_rectBrush         = new RectBrush();
            m_rectBrush.width   = 1.0f;
            m_rectBrush.height  = 1.0f;

            m_circleBrush        = new CircleBrush();
            m_circleBrush.radius = 0.5f;
        }

        void Update()
        {
            if ( Input.GetMouseButtonDown( 2 ) )
            {
                ToggleMode();
            }


            VoxelTerrain terrain = FindObjectOfType< VoxelTerrain >();
            if ( terrain == null )
            {
                gameObject.SetActive( false );
                return;
            }

            transform.rotation = terrain.transform.rotation;

            Camera cam = Camera.main;
            if ( cam == null )
            {
                cam = FindObjectOfType< Camera >();
                if ( cam == null )
                {
                    gameObject.SetActive( false );
                    return;
                }
            }

            Plane plane = new Plane( terrain.transform.forward, terrain.transform.position );
            
            Vector3 mouse = Input.mousePosition;
            Ray     ray   = cam.ScreenPointToRay( mouse );

            float enter;
            if ( plane.Raycast( ray, out enter ) )
            {
                Vector3 p = ray.GetPoint( enter );
                transform.position = p;
            }
            else
            {
                return;
            }
            

            float scrollY = Input.mouseScrollDelta.y;
            float scaleX  = transform.localScale.x;
            float scaleY  = transform.localScale.y;

            scaleX += scrollY * 0.1f;
            scaleY += scrollY * 0.1f;

            scaleX = Mathf.Clamp( scaleX, 0.1f, 10.0f );
            scaleY = Mathf.Clamp( scaleY, 0.1f, 10.0f );

            m_rectBrush.width    = scaleX;
            m_rectBrush.height   = scaleY;
            m_circleBrush.radius = scaleX / 2.0f;

            transform.localScale = new Vector3( scaleX, scaleY, transform.localScale.z );


            Vector3 relativePosition = terrain.transform.InverseTransformPoint( transform.position );

            if ( Input.GetMouseButton( 0 ) )
            {
                if ( m_rectMode )
                {
                    m_rectBrush.Add( terrain, relativePosition.x, relativePosition.y );
                }
                else
                {
                    m_circleBrush.Add( terrain, relativePosition.x, relativePosition.y );
                }
            }
            else if ( Input.GetMouseButton( 1 ) )
            {
                if ( m_rectMode )
                {
                    m_rectBrush.Subtract( terrain, relativePosition.x, relativePosition.y );
                }
                else
                {
                    m_circleBrush.Subtract( terrain, relativePosition.x, relativePosition.y );
                }
            }
        }


        private void ToggleMode()
        {
            if ( m_rectMode )
            {
                m_rectMode = false;
                if ( m_rectMarker != null ) { m_rectMarker.SetActive( false ); }
                if ( m_circleMarker != null ) { m_circleMarker.SetActive( true ); }
            }
            else
            {
                m_rectMode = true;
                if ( m_rectMarker != null ) { m_rectMarker.SetActive( true ); }
                if ( m_circleMarker != null ) { m_circleMarker.SetActive( false ); }
            }
        }

    }
}