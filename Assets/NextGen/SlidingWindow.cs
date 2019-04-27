using UnityEngine;

namespace VoxelTerrain2D
{
    public class SlidingWindow
    {
        public Vector2 position     { get { return m_windowPosition; } }
        public Vector2 size         { get { return m_windowSize; } }

        private Vector2 m_origin;
        private Vector2 m_chunkSize;

        private Vector2 m_windowSize;
        private Vector2 m_windowPosition;

        private IntRect m_window;


        public SlidingWindow( Vector2 position, Vector2 size, Vector2 origin, Vector2 chunkSize )
        {
            Set( position, size, origin, chunkSize );
        }


        public IntRect GetWindow()
        {
            return m_window;
        }


        public void Set( Vector2 position, Vector2? size = null, Vector2? origin = null, Vector2? chunkSize = null )
        {
            if ( origin.HasValue ) { m_origin = origin.Value; }
            if ( chunkSize.HasValue ){ m_chunkSize = chunkSize.Value; }
            if ( size.HasValue ) { m_windowSize = size.Value; }

            m_windowPosition = position;
            m_window = ComputeWindow();
        }


        public IntPoint Slide( Vector2 newPosition )
        {
            IntRect oldWindow = m_window;
            Set( newPosition );

            IntPoint delta;
            delta.x = m_window.origin.x - oldWindow.origin.x;
            delta.y = m_window.origin.y - oldWindow.origin.y;

            delta.x = Mathf.Clamp( delta.x, -m_window.size.x, m_window.size.x );
            delta.y = Mathf.Clamp( delta.y, -m_window.size.y, m_window.size.y );

            return delta;
        }


        private IntRect ComputeWindow()
        {
            IntRect window;

            Vector2 offset = m_windowPosition - m_origin;

            offset = offset / m_chunkSize;

            window.origin.x = Mathf.RoundToInt( offset.x );
            window.origin.y = Mathf.RoundToInt( offset.y );

            Vector2 size = m_windowSize / m_chunkSize;

            window.size.x = Mathf.RoundToInt( size.x );
            window.size.y = Mathf.RoundToInt( size.y );

            return window;
        }
    }
}