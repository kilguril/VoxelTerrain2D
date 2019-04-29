using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    public class VoxelTerrain : MonoBehaviour
    {
        public float                            voxelSize  { get { return m_settings.voxelSize; } }
        public Vector2                          renderSize { get { return m_renderSize; } }
        public IReadableDataset< VoxelData >    readable   { get; private set; }
        public IWriteableDataset< VoxelData >   writeable  { get; private set; }

        [SerializeField]
        private Vector2           m_renderSize = default( Vector2 );
        [SerializeField]
        private int               m_chunkSize = default( int );
        [SerializeField]
        private GeneratorSettings m_settings = default( GeneratorSettings );

        private SlidingWindow   m_renderWindow;
        private IntRect         m_rect;

        private IntPoint        m_windowShift;
        private IntPoint        m_windowSize;
        private IntPoint        m_cyclicHead;

        private bool[]         m_dirty;
        private VoxelChunk[]   m_chunks;


        void Awake()
        {
            float chunkWorldSize = m_settings.voxelSize * m_chunkSize;
            Initialize( Vector2.zero, m_renderSize, chunkWorldSize );
        }


        public void SetData( IReadableDataset< VoxelData > readableData, IWriteableDataset< VoxelData > writeableData )
        {
            readable  = readableData;
            writeable = writeableData;

            for ( int i = 0; i < m_dirty.Length; i++ ) { m_dirty[ i ] = true; }
        }


        public void Initialize( Vector2 position, Vector2 windowSize, float chunkWorldSize )
        {
            m_renderWindow = new SlidingWindow( position, windowSize, Vector2.zero, Vector2.one * chunkWorldSize );
            
            m_rect = m_renderWindow.GetWindow();
            m_windowSize = m_rect.size;

            m_chunks = new VoxelChunk[ m_windowSize.x * m_windowSize.y ];
            m_dirty  = new bool[ m_chunks.Length ];

            for( int y = 0; y < m_windowSize.y; y++ )
            {
                for( int x = 0; x < m_windowSize.x; x++ )
                {
                    int cx = m_rect.origin.x + x;
                    int cy = m_rect.origin.y + y;

                    VoxelChunk chunk = VoxelChunk.Create( transform, string.Format( "[{0},{1}]", cx, cy ), m_settings, m_chunkSize + 1, m_chunkSize + 1 );

                    float chunkX = cx * m_settings.voxelSize * m_chunkSize;
                    float chunkY = cy * m_settings.voxelSize * m_chunkSize;

                    chunk.nextPosition = new Vector3( chunkX, chunkY );

                    m_chunks[ y * m_windowSize.x + x ] = chunk;
                }
            }
        }


        public void Slide( Vector2 newPosition )
        {
            IntPoint delta = m_renderWindow.Slide( newPosition );
            m_rect = m_renderWindow.GetWindow();

            m_windowShift.x += delta.x;
            m_windowShift.y += delta.y;
        }


        public void SetValue( int x, int y, VoxelData val )
        {
            writeable.Set( x, y, val );

            int xx = x / m_chunkSize;
            int yy = y / m_chunkSize;

            xx = ( m_rect.origin.x + ( xx + ( m_windowSize.x - m_cyclicHead.x ) ) % m_windowSize.x ) - m_rect.origin.x;
            yy = ( m_rect.origin.y + ( yy + ( m_windowSize.y - m_cyclicHead.y ) ) % m_windowSize.y ) - m_rect.origin.y;

            bool xline = x % m_chunkSize == 0;
            bool yline = y % m_chunkSize == 0;

            SetDirty( xx, yy );

            if ( xline )
            {
                SetDirty( xx - 1, yy );
            }
            if ( yline )
            {
                SetDirty( xx, yy - 1 );
            }
            if ( xline && yline )
            {
                SetDirty( xx - 1, yy - 1 );
            }
        }


        public void SetDirty( int x, int y )
        {
            if ( x >= 0 && x < m_windowSize.x )
            {
                if ( y >= 0 && y < m_windowSize.y )
                {
                    int index = GetChunkIndex( x, y );
                    m_dirty[ index ] = true;
                }
            }
        }


        void LateUpdate()
        {
            // Ensure terrain doesn't move
            transform.position = Vector3.zero;

            // Shift window
            if ( m_windowShift.x != 0 ) { ShiftHorizontal( m_windowShift.x ); }
            if ( m_windowShift.y != 0 ) { ShiftVertical( m_windowShift.y ); }

            m_windowShift.x = 0;
            m_windowShift.y = 0;

            // Rebuild dirty
            m_windowSize = m_rect.size;

            // Can't rebuild - no data
            if ( readable == null ) { return; }

            for( int y = 0; y < m_windowSize.y; y++ )
            {
                for( int x = 0; x < m_windowSize.x; x++ )
                {
                    if ( m_dirty[ y * m_windowSize.x + x ] == true )
                    {
                        int ox = ( m_rect.origin.x + ( x + ( m_windowSize.x - m_cyclicHead.x ) ) % m_windowSize.x ) * m_chunkSize;
                        int oy = ( m_rect.origin.y + ( y + ( m_windowSize.y - m_cyclicHead.y ) ) % m_windowSize.y ) * m_chunkSize;
                        int dx = ox + m_chunkSize + 1;
                        int dy = oy + m_chunkSize + 1;

                        if ( ox < readable.min.x ){ ox = readable.min.x; }
                        if ( oy < readable.min.y ){ oy = readable.min.y; }
                        if ( dx > readable.max.x ){ dx = readable.max.x; }
                        if ( dy > readable.max.y ){ dy = readable.max.y; }

                        IntRect region;
                        region.origin.x = ox;
                        region.origin.y = oy;
                        region.size.x   = dx - ox;
                        region.size.y   = dy - oy;

                        VoxelChunk chunk = m_chunks[ y * m_windowSize.x + x ];
                        chunk.Rebuild( readable, region );

                        m_dirty[ y * m_windowSize.x + x ] = false;
                    }
                }
            }
        }


        private void ShiftHorizontal( int amount )
        {
            int direction = System.Math.Sign( amount );
            amount = System.Math.Min( System.Math.Abs( amount ), m_rect.size.x );

            if( direction > 0 )
            {
                for ( int y = 0; y < m_rect.size.y; y++ )
                {
                    for ( int x = 0; x < amount; x++ )
                    {
                        int cindex = GetChunkIndex( x, y );
                        VoxelChunk chunk = m_chunks[ cindex ];

                        int cx = m_rect.origin.x + m_rect.size.x - x - 1;
                        int cy = m_rect.origin.y + y;
                        float chunkX = cx * m_settings.voxelSize * m_chunkSize;
                        float chunkY = cy * m_settings.voxelSize * m_chunkSize;

                        chunk.name = string.Format( "[{0},{1}]", cx, cy );
                        chunk.nextPosition = new Vector3( chunkX, chunkY, 0.0f );

                        m_dirty[ cindex ] = true;
                    }
                }

                m_cyclicHead.x = WrapIndex( m_cyclicHead.x + amount, m_windowSize.x );
            }
            else
            {
                for ( int y = 0; y < m_rect.size.y; y++ )
                {
                    for ( int x = 0; x < amount; x++ )
                    {
                        int xx = m_windowSize.x - 1 - x;

                        int cindex = GetChunkIndex( xx, y );
                        VoxelChunk chunk = m_chunks[ cindex ];

                        int cx = m_rect.origin.x + x;
                        int cy = m_rect.origin.y + y;
                        float chunkX = cx * m_settings.voxelSize * m_chunkSize;
                        float chunkY = cy * m_settings.voxelSize * m_chunkSize;

                        chunk.name = string.Format( "[{0},{1}]", cx, cy );
                        chunk.nextPosition = new Vector3( chunkX, chunkY, 0.0f );

                        m_dirty[ cindex ] = true;
                    }
                }

                m_cyclicHead.x = WrapIndex( m_cyclicHead.x - amount, m_windowSize.x );
            }
        }


        private void ShiftVertical( int amount )
        {
            int direction = System.Math.Sign( amount );
            amount = System.Math.Min( System.Math.Abs( amount ), m_rect.size.y );

            if( direction > 0 )
            {
                for ( int y = 0; y < amount; y++ )
                {
                    for ( int x = 0; x < m_rect.size.x; x++ )
                    {
                        int cindex = GetChunkIndex( x, y );
                        VoxelChunk chunk = m_chunks[ cindex ];

                        int cx = m_rect.origin.x + x;
                        int cy = m_rect.origin.y + m_rect.size.y - y - 1;
                        float chunkX = cx * m_settings.voxelSize * m_chunkSize;
                        float chunkY = cy * m_settings.voxelSize * m_chunkSize;

                        chunk.name = string.Format( "[{0},{1}]", cx, cy );
                        chunk.nextPosition = new Vector3( chunkX, chunkY, 0.0f );

                        m_dirty[ cindex ] = true;
                    }
                }

                m_cyclicHead.y = WrapIndex( m_cyclicHead.y + amount, m_windowSize.y );
            }
            else
            {
                for ( int y = 0; y < amount; y++ )
                {
                    for ( int x = 0; x < m_rect.size.x; x++ )
                    {
                        int yy = m_windowSize.y - 1 - y;

                        int cindex = GetChunkIndex( x, yy );
                        VoxelChunk chunk = m_chunks[ cindex ];

                        int cx = m_rect.origin.x + x;
                        int cy = m_rect.origin.y + y;
                        float chunkX = cx * m_settings.voxelSize * m_chunkSize;
                        float chunkY = cy * m_settings.voxelSize * m_chunkSize;

                        chunk.name = string.Format( "[{0},{1}]", cx, cy );
                        chunk.nextPosition = new Vector3( chunkX, chunkY, 0.0f );

                        m_dirty[ cindex ] = true;
                    }
                }

                m_cyclicHead.y = WrapIndex( m_cyclicHead.y - amount, m_windowSize.y );
            }
        }


        private int GetChunkIndex( int x, int y )
        {
            return ( ( y + m_cyclicHead.y ) % m_windowSize.y ) * m_windowSize.x + ( ( x + m_cyclicHead.x ) % m_windowSize.x );
        }


        // https://codereview.stackexchange.com/questions/57923/index-into-array-as-if-it-is-circular
        private int WrapIndex( int i, int size )
        {
            bool wasNegative = false;
            if (i < 0) {
                wasNegative = true;
                i = -i;
            }
            int offset = i % size;
            return (wasNegative) ? (size - offset) : (offset);
        }


        #region Debug Rendering
        private void OnDrawGizmosSelected()
        {
            SlidingWindow window = m_renderWindow;
            if ( window == null ){ window = new SlidingWindow( Vector2.zero, m_renderSize, Vector2.zero, Vector2.one * m_settings.voxelSize * m_chunkSize ); }
            IntRect rect = window.GetWindow();
        
            for( int j = 0; j < rect.size.y && j < 100; j++ )
            {
                for( int i = 0; i < rect.size.x && i < 100; i++ )
                {
                    int chunkX = rect.origin.x + i;
                    int chunkY = rect.origin.y + j;

                    Vector2 a = new Vector2( chunkX * m_settings.voxelSize * m_chunkSize, chunkY * m_settings.voxelSize * m_chunkSize );
                    Vector2 b = a + Vector2.one * m_settings.voxelSize * m_chunkSize;

                    DrawRect( a, b, Color.yellow );

                    string debug = string.Format( "Chunk[{0},{1}]\n({2}->{3})", chunkX, chunkY, a, b );
                    DrawString( debug, a + ( b - a ) / 2.0f, Color.yellow );
                }
            }

            DrawRect( window.position, window.position + window.size, Color.red );
            DrawString( "Window", window.position, Color.red );
        }

        private void DrawRect( Vector3 a, Vector3 b, Color color )
        {
            Vector3 c = new Vector3( a.x, b.y, 0.0f );
            Vector3 d = new Vector3( b.x, a.y, 0.0f );

            Gizmos.color = color;
            Gizmos.DrawLine( a, c );
            Gizmos.DrawLine( c, b );
            Gizmos.DrawLine( b, d );
            Gizmos.DrawLine( d, a );
        }


        private void DrawString(string text, Vector3 worldPos, Color? color = null) 
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};

            UnityEditor.Handles.BeginGUI();
     
            var restoreColor = GUI.color;
     
            if (color.HasValue) GUI.color = color.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
     
            if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
            {
                GUI.color = restoreColor;
                UnityEditor.Handles.EndGUI();
                return;
            }
     
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text, style);
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
        }
        #endregion
    }
}