using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D.Brushes
{
    public class CircleBrush : VoxelBrush
    {
        public float radius  { get; set; }


        public override void Add( VoxelTerrain terrain, float x, float y )
        {
            SetTerrainValue( terrain, x, y, true );
        }


        public override void Subtract( VoxelTerrain terrain, float x, float y )
        {

            SetTerrainValue( terrain, x, y, false );
        }


        private void SetTerrainValue( VoxelTerrain terrain, float x, float y, bool add )
        {
            float vsize2 = terrain.voxelSize * 2.0f;

            Vector2 from  = new Vector2( x - radius - vsize2, y - radius - vsize2 );
            Vector2 to    = new Vector2( x + radius + vsize2, y + radius + vsize2 );
            Vector2 center = new Vector2( x, y );

            int maxWidth  = Mathf.CeilToInt( ( to.x - from.x ) / terrain.voxelSize );
            int maxHeight = Mathf.CeilToInt( ( to.y - from.y ) / terrain.voxelSize );

            int firstX = Mathf.RoundToInt( from.x / terrain.voxelSize );
            int firstY = Mathf.RoundToInt( from.y / terrain.voxelSize );

            for ( int j = 0; j < maxHeight; j++ )
            {
                int currY = firstY + j;
                if ( currY < 0 ) { continue; }
                if ( currY >= terrain.height ) { break; }

                for ( int i = 0; i < maxWidth; i++ )
                {
                    int currX = firstX + i;
                    if ( currX < 0 ) { continue; }
                    if ( currX >= terrain.width ) { break; }

                    float px = currX * terrain.voxelSize;
                    float py = currY * terrain.voxelSize;

                    Vector2 p = new Vector2( px, py );
                    float d = ( p - center ).magnitude;

                    if ( add )
                    {
                        if ( d <= radius )
                        {
                            VoxelData existing = terrain.GetValue( currX, currY );
                            VoxelData val      = default(VoxelData);

                            val.SetSolidState( true );

                            if ( d < radius - terrain.voxelSize )
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

                                byte eleft  = (byte)( Mathf.Clamp01( ( p.x - left.x) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                byte eright = (byte)( Mathf.Clamp01( ( right.x - p.x ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                byte etop   = (byte)( Mathf.Clamp01( ( up.y - p.y ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                byte ebot   = (byte)( Mathf.Clamp01( ( p.y - down.y ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );

                                eleft  = System.Math.Max( eleft, existing.GetExtentLeft() );
                                eright = System.Math.Max( eright, existing.GetExtentRight() );
                                etop   = System.Math.Max( etop, existing.GetExtentTop() );
                                ebot   = System.Math.Max( ebot, existing.GetExtentBottom() );

                                val.SetExtentHorizontal( eleft, eright );
                                val.SetExtentVertical( ebot, etop );
                            }

                            terrain.SetValue( currX, currY, val );
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

                            terrain.SetValue( currX, currY, val );
                        }
                        else if ( d <= radius + terrain.voxelSize )
                        {
                            VoxelData existing = terrain.GetValue( currX, currY );

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
                                    etop = System.Math.Min( etop, (byte)( Mathf.Clamp01( Mathf.Abs( topbot.y - p.y ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION ) );
                                }
                                else if ( p.y > topbot.y )
                                {
                                    ebot = System.Math.Min( ebot, (byte)( Mathf.Clamp01( Mathf.Abs( topbot.y - p.y ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION ) );
                                }
                            }

                            if ( i2 > 0 )
                            {
                                Vector2 leftright;
                                if ( i2 == 1 ){ leftright = left; }
                                else{ leftright = NearestPoint( p, left, right ); }

                                if ( p.x < leftright.x )
                                {
                                    eright = System.Math.Min( eright, (byte)( Mathf.Clamp01( Mathf.Abs( leftright.x - p.x ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION ) );
                                }
                                else if ( p.x > leftright.x )
                                {
                                    eleft = System.Math.Min( eleft, (byte)( Mathf.Clamp01( Mathf.Abs( leftright.x - p.x ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION ) );
                                }
                            }

                            existing.SetExtentHorizontal( eleft, eright );
                            existing.SetExtentVertical( ebot, etop );
                            terrain.SetValue( currX, currY, existing );
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