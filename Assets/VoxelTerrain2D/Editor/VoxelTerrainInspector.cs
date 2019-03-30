using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using VoxelTerrain2D.Brushes;

namespace VoxelTerrain2D.Editor
{
    [CustomEditor( typeof( VoxelTerrain ) )]
    public class VoxelTerrainInspector : UnityEditor.Editor
    {
        enum Panel { Settings, Edit, Resize }
        enum Brush { Circle, Rect, Box }

        private int m_wantedWidth;
        private int m_wantedHeight;
        private Panel m_currentPanel;

        private float m_radius;
        private float m_rectWidth;
        private float m_rectHeight;

        private Brush m_brush;
        private RectBrush   m_rectBrush;
        private CircleBrush m_circleBrush;

        private bool    m_isBoxSelectDown;
        private bool    m_isBoxSelectAdd;
        private Vector3 m_boxSelectOrigin;

        private const float BRUSH_MINSIZE = 0.001f;
        private const float BRUSH_MAXSIZE = 100.0f;

        private const string HELP_MESSAGE = "LMB - Add Terrain\nRMB - Subtract Terrain\n\nCtrl + MMB - Toggle Brush\nCtrl + Scroll - Adjust Brush Size";


        void OnEnable()
        {
            m_radius     = 0.25f;
            m_rectWidth  = 0.5f;
            m_rectHeight = 0.5f;

            m_circleBrush = new CircleBrush();
            m_rectBrush = new RectBrush();
        }


        public override void OnInspectorGUI()
        {
            VoxelTerrain terrain = target as VoxelTerrain;
            if ( terrain == null ) { return; }

            EditorGUI.BeginChangeCheck();
            {
                terrain.data = ( VoxelTerrainData )EditorGUILayout.ObjectField( "Terrain Data", terrain.data, typeof( VoxelTerrainData ), false );
            }
            if ( EditorGUI.EndChangeCheck() )
            {
                VoxelTerrainEditorManager.InitializeTerrain( terrain );
            }

            if ( terrain.data == null ) { return; }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                if ( GUILayout.Button( "Settings" ) )
                {
                    m_currentPanel = Panel.Settings;
                    SceneView.RepaintAll();
                }

                if ( GUILayout.Button( "Resize" ) )
                {
                    m_currentPanel = Panel.Resize;
                    SceneView.RepaintAll();
                }

                if ( GUILayout.Button( "Edit Terrain" ) )
                {
                    m_currentPanel = Panel.Edit;
                    SceneView.RepaintAll();
                    
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            switch( m_currentPanel )
            {
                case Panel.Resize:
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField( "Current Terrain Size" );
                    EditorGUILayout.LabelField( string.Format( "[{0}x{1}]", terrain.data.width, terrain.data.height ) );
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField( "New Width" );
                    m_wantedWidth = EditorGUILayout.IntField( m_wantedWidth );
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField( "New Height" );
                    m_wantedHeight = EditorGUILayout.IntField( m_wantedHeight );
                    EditorGUILayout.EndHorizontal();

                    if ( GUILayout.Button("Confirm Resize" ) )
                    {
                        if ( m_wantedWidth > 1 &&  m_wantedHeight > 1  && ( m_wantedWidth != terrain.data.width || m_wantedHeight != terrain.data.height ) )
                        {
                            Undo.RecordObject( terrain.data, string.Format( "Resize Terrain {0}", terrain.data.name ) );
                            terrain.data.Resize( m_wantedWidth, m_wantedHeight );
                            VoxelTerrainEditorManager.InitializeTerrain( terrain );
                        }
                    }
                }
                break;

                case Panel.Settings:
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        base.OnInspectorGUI();
                    }
                    if ( EditorGUI.EndChangeCheck() )
                    {
                        VoxelTerrainEditorManager.InitializeTerrain( terrain );
                    }
                }
                break;

                case Panel.Edit:
                {
                    Event e = Event.current;
                    m_brush = (Brush)EditorGUILayout.EnumPopup( "Brush", m_brush );

                    switch( m_brush )
                    {
                        case Brush.Circle:
                            m_radius = EditorGUILayout.Slider( "Radius", m_radius, BRUSH_MINSIZE, BRUSH_MAXSIZE );
                        break;

                        case Brush.Rect:
                            m_rectWidth  = EditorGUILayout.Slider( "Width", m_rectWidth, BRUSH_MINSIZE, BRUSH_MAXSIZE );
                            m_rectHeight = EditorGUILayout.Slider( "Height", m_rectHeight, BRUSH_MINSIZE, BRUSH_MAXSIZE );
                        break;
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox( HELP_MESSAGE, MessageType.Info );

                    switch( e.type )
                    {
                        case EventType.MouseDown:
                        {
                            m_isBoxSelectDown = false;

                            if ( e.control && e.button == 2 )
                            {
                                CycleBrush();
                            }
                        }
                        break;

                        case EventType.MouseUp:
                        {
                            m_isBoxSelectDown = true;
                        }
                        break;

                        case EventType.ScrollWheel:
                        {
                            if ( e.control )
                            {
                                ResizeBrush( e.delta.y );
                                e.Use();
                            }
                        }
                        break;
                    }
                }
                break;
            }

        }


        void OnSceneGUI()
        {
            VoxelTerrain terrain = target as VoxelTerrain;
            if ( terrain == null ) { return; }

            if ( terrain.data == null ) { return; }

            float right = ( terrain.width - 1 ) * terrain.voxelSize;
            float up    = ( terrain.height - 1 ) * terrain.voxelSize;

            Vector3 p0 = terrain.transform.position;
            Vector3 p1 = terrain.transform.TransformPoint( new Vector3( 0.0f, up ) );
            Vector3 p2 = terrain.transform.TransformPoint( new Vector3( right, up ) );
            Vector3 p3 = terrain.transform.TransformPoint( new Vector3( right, 0.0f ) );

            Handles.color = m_currentPanel == Panel.Edit ? Color.yellow : Color.cyan;
            Handles.DrawAAPolyLine( p0, p1, p2, p3, p0 );

            if ( m_currentPanel != Panel.Edit ) { return; }

            SceneView view    = SceneView.lastActiveSceneView;
            Event     e       = Event.current;

            bool repaint      = false;
            bool brushVisible = false;

            // Get hovered position
            Plane plane = new Plane( terrain.transform.forward, terrain.transform.position );
            
            Vector3 brushPoint = Vector3.zero;
            Vector3 mouse      = e.mousePosition;
            brushVisible = view.camera.pixelRect.Contains( mouse );

            if ( brushVisible )
            {
                Ray ray = HandleUtility.GUIPointToWorldRay( mouse );
                float enter;

                if ( plane.Raycast( ray, out enter ) )
                {
                    brushPoint = ray.GetPoint( enter );
                    brushVisible = true;
                }
                else
                {
                    brushVisible = false;
                }
            }

            if ( brushVisible )
            {
                RenderBrush( brushPoint, terrain );
            }

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseMove:
                {
                    repaint = true;
                }
                break;

                case EventType.MouseDrag:
                {
                    repaint = true;

                    if ( e.modifiers == EventModifiers.None && e.button < 2 )
                    {
                        if ( brushVisible == true && m_brush != Brush.Box )
                        {
                            DoPaint( terrain, brushPoint, e.button == 0 );
                            e.Use();
                        }
                    }
                    else if ( m_isBoxSelectDown )
                    {
                        e.Use();
                    }
                }
                break;
                
                case EventType.MouseDown:
                {
                    if ( e.modifiers == EventModifiers.None && e.button < 2 )
                    {
                        GUIUtility.hotControl = controlID;
                        e.Use();
                        
                        repaint = true;

                        if ( brushVisible == true )
                        {
                            if ( m_brush != Brush.Box )
                            {
                                DoPaint( terrain, brushPoint, e.button == 0 );
                            }
                            else
                            {
                                if ( m_isBoxSelectDown ) { m_isBoxSelectDown = false; }
                                else
                                {
                                    m_isBoxSelectDown = true;
                                    m_isBoxSelectAdd = e.button == 0;
                                    m_boxSelectOrigin = brushPoint;
                                }
                            }
                        }
                    }
                    else if ( e.control && e.button == 2 )
                    {
                        CycleBrush();
                        e.Use();

                        repaint = true;
                    }
                }
                break;

                case EventType.MouseUp:
                {
                    if ( m_isBoxSelectDown == true && brushVisible == true )
                    {
                        DoBoxPaint( terrain, m_boxSelectOrigin, brushPoint, e.button == 0 );

                        GUIUtility.hotControl = 0;
                        e.Use();
                        repaint = true;
                    }

                    m_isBoxSelectDown = false;
                }
                break;

                case EventType.ScrollWheel:
                {
                    if ( e.control )
                    {
                        ResizeBrush( e.delta.y );
                        e.Use();

                        repaint = true;
                    }
                }
                break;
            }

            if ( repaint ) 
            { 
                view.Repaint();
                Repaint();
            }
        }


        private void RenderBrush( Vector3 brushPoint, VoxelTerrain terrain )
        {
            Handles.color = Color.red;

            switch( m_brush )
            {
                case Brush.Circle:
                {
                    Handles.DrawWireDisc( brushPoint, -terrain.transform.forward, m_radius );
                }
                break;

                case Brush.Rect:
                {
                    float halfWidth = m_rectWidth / 2.0f;
                    float halfHeight = m_rectHeight / 2.0f;

                    Vector3 p0 = brushPoint + new Vector3( -halfWidth, -halfHeight );
                    Vector3 p1 = brushPoint + new Vector3( -halfWidth, halfHeight );
                    Vector3 p2 = brushPoint + new Vector3( halfWidth, halfHeight );
                    Vector3 p3 = brushPoint + new Vector3( halfWidth, -halfHeight );

                    Handles.DrawAAPolyLine( p0, p1, p2, p3, p0 );
                }
                break;

                case Brush.Box:
                {
                    if ( m_isBoxSelectDown )
                    {
                        Vector3 local0 = terrain.transform.InverseTransformPoint( m_boxSelectOrigin );
                        Vector3 local2 = terrain.transform.InverseTransformPoint( brushPoint );

                        Vector3 local1 = new Vector3( local0.x, local2.y );
                        Vector3 local3 = new Vector3( local2.x, local0.y );

                        Vector3 p0 = terrain.transform.TransformPoint( local0 );
                        Vector3 p1 = terrain.transform.TransformPoint( local1 );
                        Vector3 p2 = terrain.transform.TransformPoint( local2 );
                        Vector3 p3 = terrain.transform.TransformPoint( local3 );

                        Handles.DrawAAPolyLine( p0, p1, p2, p3, p0 );
                    }
                }
                break;
            }
        }


        private void CycleBrush()
        {
            Brush[] vals = (Brush[])System.Enum.GetValues( typeof( Brush ) );
            int next = System.Array.IndexOf( vals, m_brush) + 1;

            m_brush = ( next == vals.Length ) ? vals[ 0 ] : vals[ next ];

            Repaint();
            SceneView.RepaintAll();
        }


        private void ResizeBrush( float amount )
        {
            amount = ( amount / -3.0f ) * 0.1f; // Is this consistent in the editor?
            switch( m_brush )
            {
                case Brush.Rect:
                {
                    m_rectWidth  += amount;
                    m_rectHeight += amount;

                    m_rectWidth = Mathf.Clamp( m_rectWidth, BRUSH_MINSIZE, BRUSH_MAXSIZE );
                    m_rectHeight = Mathf.Clamp( m_rectHeight, BRUSH_MINSIZE, BRUSH_MAXSIZE );
                }
                break;

                case Brush.Circle:
                {
                    m_radius += amount;
                    m_radius = Mathf.Clamp( m_radius, BRUSH_MINSIZE, BRUSH_MAXSIZE );
                }
                break;
            }
        }


        private void DoPaint( VoxelTerrain terrain, Vector3 position, bool add )
        {
            Vector3 relativePosition = terrain.transform.InverseTransformPoint( position );
            float x = relativePosition.x;
            float y = relativePosition.y;

            switch( m_brush )
            {
                case Brush.Rect:
                {
                    m_rectBrush.width = m_rectWidth;
                    m_rectBrush.height = m_rectHeight;

                    if ( add ) { m_rectBrush.Add( terrain, x, y ); }
                    else { m_rectBrush.Subtract( terrain, x, y ); }
                }
                break;

                case Brush.Circle:
                {
                    m_circleBrush.radius = m_radius;
                    if ( add ) { m_circleBrush.Add( terrain, x, y ); }
                    else{ m_circleBrush.Subtract( terrain, x, y ); }
                }
                break;
            }

            CommitChanges( terrain );
        }


        private void DoBoxPaint( VoxelTerrain terrain, Vector3 origin, Vector3 dest, bool add )
        {
            Vector3 rel0  = terrain.transform.InverseTransformPoint( origin );
            Vector3 rel1  = terrain.transform.InverseTransformPoint( dest );
            Vector3 delta = rel1 - rel0;
            Vector3 mid   = rel0 + ( delta / 2.0f );

            float x = mid.x;
            float y = mid.y;
            float w = Mathf.Abs( delta.x );
            float h = Mathf.Abs( delta.y );

            m_rectBrush.width = w;
            m_rectBrush.height = h;

            if ( add ) { m_rectBrush.Add( terrain, x, y ); }
            else { m_rectBrush.Subtract( terrain, x, y ); }

            CommitChanges( terrain );
        }


        private void CommitChanges( VoxelTerrain terrain )
        {
            VoxelChunk[] chunks   = terrain.chunks;
            bool         anyDirty = false;

            for( int i = 0; i < chunks.Length; i++ )
            {
                SimpleVoxelChunk chunk = chunks[ i ] as SimpleVoxelChunk;
                if ( chunk != null )
                {
                    if ( chunk.data.dirty == true )
                    {
                        anyDirty = true;
                        break;
                    }
                }
            }

            if ( anyDirty )
            {
                Undo.RecordObject( terrain.data, "Modify Terrain" );

                for ( int i = 0; i < chunks.Length; i++ )
                {
                    SimpleVoxelChunk chunk = chunks[ i ] as SimpleVoxelChunk;
                    if ( chunk != null )
                    {
                        if ( chunk.data.dirty == true )
                        {
                            // Write Chunk
                            int chunkY = i / terrain.chunkedData.chunkCountX;
                            int chunkX = i - ( chunkY * terrain.chunkedData.chunkCountX );

                            int offsetX = chunkX * ( terrain.chunkedData.chunkSize - 1 );
                            int offsetY = chunkY * ( terrain.chunkedData.chunkSize - 1 );

                            for ( int y = 0; y < chunk.data.height; y++ )
                            {
                                for ( int x = 0; x < chunk.data.width; x++ )
                                {
                                    VoxelData v = chunk.data.data[ y * chunk.data.width + x ];
                                    terrain.data.Set( offsetX + x, offsetY + y, v );
                                }
                            }

                            // Rebuild
                            chunk.RebuildIfNeeded();
                        }
                    }
                }
            }
        }
    }
}