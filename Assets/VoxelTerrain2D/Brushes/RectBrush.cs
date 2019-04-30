using System;
using UnityEngine;

namespace VoxelTerrain2D.Brushes
{
    public sealed class RectBrush : VoxelBrush
    {
        public float width  { get; set; }
        public float height { get; set; }

        private float    m_voxelSize;
        private IntPoint m_min;
        private IntPoint m_max;

        public RectBrush( float voxelSize, IntPoint lowerBounds, IntPoint upperBounds, Func< int, int, VoxelData > readMethod, Action< int, int, VoxelData > writeMethod ) : base( readMethod, writeMethod )
        {
            m_voxelSize = voxelSize;
            m_min = lowerBounds;
            m_max = upperBounds;
        }


        public override void Add(  float x, float y )
        {
            SetTerrainValue( x, y, true );
        }


        public override void Subtract( float x, float y )
        {

            SetTerrainValue( x, y, false );
        }


        private void SetTerrainValue( float x, float y, bool add )
        {
            Vector2 from = new Vector2( x - width / 2.0f, y - height / 2.0f );
            Vector2 to = new Vector2( x + width / 2.0f, y + height / 2.0f );

            int maxWidth  = Mathf.CeilToInt( width / m_voxelSize ) + 3;
            int maxHeight = Mathf.CeilToInt( height / m_voxelSize ) + 3;

            int firstX = Mathf.RoundToInt( from.x / m_voxelSize ) - 1;
            int firstY = Mathf.RoundToInt( from.y / m_voxelSize ) - 1;

            for( int j = 0; j < maxHeight; j++ )
            {
                int currY = firstY + j;
                if ( currY < m_min.y ) { continue; }
                if ( currY >= m_max.y ) { break; }

                for( int i = 0; i < maxWidth; i++)
                {
                    int currX = firstX + i;
                    if ( currX < m_min.x ) { continue; }
                    if ( currX >= m_max.x ) { break; }

                    float px = currX * m_voxelSize;
                    float py = currY * m_voxelSize;
                    
                    float dx = Mathf.Abs( px - x ) - width / 2.0f;
                    float dy = Mathf.Abs( py - y ) - height / 2.0f;

                    if ( add )
                    {
                        if ( dx < 0.0f && dy < 0.0f )
                        {
                            VoxelData existing = read( currX, currY );
                            VoxelData val      = default(VoxelData);

                            val.SetSolidState( true );

                            float right = x + width / 2.0f;
                            float left  = x - width / 2.0f;
                            float top   = y + height / 2.0f;
                            float bot   = y - height / 2.0f;

                            byte eleft  = (byte)( Mathf.Clamp01( ( px - left ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                            byte eright = (byte)( Mathf.Clamp01( ( right - px ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                            byte etop   = (byte)( Mathf.Clamp01( ( top - py ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                            byte ebot   = (byte)( Mathf.Clamp01( ( py - bot ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );

                            eleft  = Math.Max( eleft, existing.GetExtentLeft() );
                            eright = Math.Max( eright, existing.GetExtentRight() );
                            etop   = Math.Max( etop, existing.GetExtentTop() );
                            ebot   = Math.Max( ebot, existing.GetExtentBottom() );

                            val.SetExtentHorizontal( eleft, eright );
                            val.SetExtentVertical( ebot, etop );

                            write( currX, currY, val );
                        }
                    }
                    else
                    {
                        if ( dx < 0.0f && dy < 0.0f )
                        {
                            VoxelData val = default(VoxelData);
                            val.SetSolidState( false );
                            val.SetExtentHorizontal( 0, 0 );
                            val.SetExtentVertical( 0, 0 );

                            write( currX, currY, val );
                        }
                        else
                        {
                            VoxelData existing = read( currX, currY );

                            if ( dy < dx )
                            {
                                if ( dx < m_voxelSize )
                                {
                                    // Point is right of brush
                                    if ( px - x > 0.0f )
                                    {
                                        byte eleft = (byte)( Mathf.Clamp01( dx / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                        eleft = Math.Min( eleft, existing.GetExtentLeft() );
                                        existing.SetExtentLeft( eleft );
                                    }
                                    // Left of brush
                                    else if ( px - x < 0.0f )
                                    {
                                        byte eright = (byte)( Mathf.Clamp01( dx / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                        eright = Math.Min( eright, existing.GetExtentRight() );
                                        existing.SetExtentRight( eright );
                                    }
                                }
                            }
                            else
                            {
                                if ( dy < m_voxelSize )
                                {
                                    // Point is above brush
                                    if ( py - y > 0.0f )
                                    {
                                        byte ebot = (byte)( Mathf.Clamp01( dy / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                        ebot = Math.Min( ebot, existing.GetExtentBottom() );
                                        existing.SetExtentBottom( ebot );
                                    }
                                    // Below brush
                                    else if ( py - y < 0.0f )
                                    {
                                        byte etop = (byte)( Mathf.Clamp01( dy / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                        etop = Math.Min( etop, existing.GetExtentTop() );
                                        existing.SetExtentTop( etop );
                                    }
                                }
                            }

                            write( currX, currY, existing );
                        }
                    }
                }
            }
        }

    }
}