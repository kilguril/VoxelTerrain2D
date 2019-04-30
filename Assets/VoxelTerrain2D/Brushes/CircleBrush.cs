using System;
using UnityEngine;

namespace VoxelTerrain2D.Brushes
{
    public class CircleBrush : VoxelBrush
    {
        public float radius  { get; set; }

        private float    m_voxelSize;
        private IntPoint m_min;
        private IntPoint m_max;

        public CircleBrush( float voxelSize, IntPoint lowerBounds, IntPoint upperBounds, Func< int, int, VoxelData > readMethod, Action< int, int, VoxelData > writeMethod ) : base( readMethod, writeMethod )
        {
            m_voxelSize = voxelSize;
            m_min = lowerBounds;
            m_max = upperBounds;
        }

        public override void Add( float x, float y )
        {
            SetTerrainValue( x, y, true );
        }


        public override void Subtract( float x, float y )
        {

            SetTerrainValue( x, y, false );
        }


        private void SetTerrainValue( float x, float y, bool add )
        {
            float vsize2 = m_voxelSize * 2.0f;

            Vector2 from  = new Vector2( x - radius - vsize2, y - radius - vsize2 );
            Vector2 to    = new Vector2( x + radius + vsize2, y + radius + vsize2 );
            Vector2 center = new Vector2( x, y );

            int maxWidth  = Mathf.CeilToInt( ( to.x - from.x ) / m_voxelSize );
            int maxHeight = Mathf.CeilToInt( ( to.y - from.y ) / m_voxelSize );

            int firstX = Mathf.RoundToInt( from.x / m_voxelSize );
            int firstY = Mathf.RoundToInt( from.y / m_voxelSize );

            for ( int j = 0; j < maxHeight; j++ )
            {
                int currY = firstY + j;
                if ( currY < m_min.y ) { continue; }
                if ( currY >= m_max.y ) { break; }

                for ( int i = 0; i < maxWidth; i++ )
                {
                    int currX = firstX + i;
                    if ( currX < m_min.x ) { continue; }
                    if ( currX >= m_max.x ) { break; }

                    float px = currX * m_voxelSize;
                    float py = currY * m_voxelSize;

                    Vector2 p = new Vector2( px, py );
                    float d = ( p - center ).magnitude;

                    if ( add )
                    {
                        if ( d <= radius )
                        {
                            VoxelData existing = read( currX, currY );
                            VoxelData val      = default(VoxelData);

                            val.SetSolidState( true );

                            if ( d < radius - m_voxelSize )
                            {
                                val.SetExtentHorizontal( 15, 15 );
                                val.SetExtentVertical( 15, 15 );
                            }
                            else
                            {
                                Vector2 up = default( Vector2 ), down = default( Vector2 );
                                Vector2 left = default( Vector2 ), right = default( Vector2 );

                                IntersectCircle( p, p + Vector2.up, center, radius, out up, out down );
                                IntersectCircle( p, p + Vector2.left, center, radius, out left, out right );

                                byte eleft  = (byte)( Mathf.Clamp01( ( p.x - left.x) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                byte eright = (byte)( Mathf.Clamp01( ( right.x - p.x ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                byte etop   = (byte)( Mathf.Clamp01( ( up.y - p.y ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                byte ebot   = (byte)( Mathf.Clamp01( ( p.y - down.y ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );

                                eleft  = Math.Max( eleft, existing.GetExtentLeft() );
                                eright = Math.Max( eright, existing.GetExtentRight() );
                                etop   = Math.Max( etop, existing.GetExtentTop() );
                                ebot   = Math.Max( ebot, existing.GetExtentBottom() );

                                val.SetExtentHorizontal( eleft, eright );
                                val.SetExtentVertical( ebot, etop );
                            }

                            write( currX, currY, val );
                        }
                    }
                    else
                    {
                        if ( d <= radius )
                        {
                            VoxelData val = default(VoxelData);

                            val.SetSolidState( false );
                            val.SetExtentHorizontal( 0, 0 );
                            val.SetExtentVertical( 0, 0 );

                            write( currX, currY, val );
                        }
                        else if ( d <= radius + m_voxelSize )
                        {
                            VoxelData existing = read( currX, currY );

                            int i1 = 0, i2 = 0;
                            Vector2 up = default( Vector2 ), down = default( Vector2 );
                            Vector2 left = default( Vector2 ), right = default( Vector2 );

                            i1 = IntersectCircle( p, p + Vector2.up, center, radius, out up, out down );
                            i2 = IntersectCircle( p, p + Vector2.left, center, radius, out left, out right );

                            byte eleft  = existing.GetExtentLeft();
                            byte eright = existing.GetExtentRight();
                            byte etop   = existing.GetExtentTop();
                            byte ebot   = existing.GetExtentBottom();

                            if ( i1 > 0 )
                            {
                                Vector2 topbot;
                                if ( i1 == 1 ){ topbot = up; }
                                else{ topbot = NearestPoint( p, up, down ); }

                                if ( p.y < topbot.y )
                                {
                                    etop = Math.Min( etop, (byte)( Mathf.Clamp01( Mathf.Abs( topbot.y - p.y ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION ) );
                                }
                                else if ( p.y > topbot.y )
                                {
                                    ebot = Math.Min( ebot, (byte)( Mathf.Clamp01( Mathf.Abs( topbot.y - p.y ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION ) );
                                }
                            }

                            if ( i2 > 0 )
                            {
                                Vector2 leftright;
                                if ( i2 == 1 ){ leftright = left; }
                                else{ leftright = NearestPoint( p, left, right ); }

                                if ( p.x < leftright.x )
                                {
                                    eright = Math.Min( eright, (byte)( Mathf.Clamp01( Mathf.Abs( leftright.x - p.x ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION ) );
                                }
                                else if ( p.x > leftright.x )
                                {
                                    eleft = Math.Min( eleft, (byte)( Mathf.Clamp01( Mathf.Abs( leftright.x - p.x ) / m_voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION ) );
                                }
                            }

                            existing.SetExtentHorizontal( eleft, eright );
                            existing.SetExtentVertical( ebot, etop );
                            write( currX, currY, existing );
                        }
                    }
                }
            }
        }


        public Vector2 NearestPoint( Vector2 c, Vector2 a, Vector2 b )
        {
            float da = ( a - c ).sqrMagnitude;
            float db = ( b - c ).sqrMagnitude;

            if ( da < db ) { return a; }
            return b;
        }


        public static int IntersectCircle( Vector2 pA, Vector2 pB, Vector2 c, float r, out Vector2 outA, out Vector2 outB )
        {
            outA = Vector2.zero;
            outB = Vector2.zero;

            Vector2 nA  = pB - pA;
            Vector2 nB  = c - pA;

            float   d       = Vector2.Dot( nA, nB );
            Vector2 project = nA * ( d / nA.sqrMagnitude );

            Vector2 mid     = pA + project;
            Vector2 cMid    = mid - c;
            float   dsqr    = cMid.sqrMagnitude;

            if ( dsqr > r * r ){ return 0; }
            if ( dsqr == r * r )
            {
                outA = mid;
                return 1;
            }

            float intersectDist = 0.0f;

            if ( dsqr == 0.0f )
            {
                intersectDist = r;
            }
            else
            {
                intersectDist = Mathf.Sqrt( r * r - dsqr );
            }

            nA.Normalize();
            nA *= intersectDist;

            outA = mid + nA;
            outB = mid - nA;
            return 2;
        }

    }
}