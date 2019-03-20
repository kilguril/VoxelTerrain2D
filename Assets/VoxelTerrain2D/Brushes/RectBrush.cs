using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D.Brushes
{
    public class RectBrush : VoxelBrush
    {
        public float width  { get; set; }
        public float height { get; set; }


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
            Vector2 from = new Vector2( x - width / 2.0f, y - height / 2.0f );
            Vector2 to = new Vector2( x + width / 2.0f, y + height / 2.0f );

            int maxWidth  = Mathf.CeilToInt( width / terrain.voxelSize ) + 3;
            int maxHeight = Mathf.CeilToInt( height / terrain.voxelSize ) + 3;

            int firstX = Mathf.RoundToInt( from.x / terrain.voxelSize ) - 1;
            int firstY = Mathf.RoundToInt( from.y / terrain.voxelSize ) - 1;

            for( int j = 0; j < maxHeight; j++ )
            {
                int currY = firstY + j;
                if ( currY < 0 ) { continue; }
                if ( currY >= terrain.height ) { break; }

                for( int i = 0; i < maxWidth; i++)
                {
                    int currX = firstX + i;
                    if ( currX < 0 ) { continue; }
                    if ( currX >= terrain.width ) { break; }

                    float px = currX * terrain.voxelSize;
                    float py = currY * terrain.voxelSize;
                    
                    float dx = Mathf.Abs( px - x ) - width / 2.0f;
                    float dy = Mathf.Abs( py - y ) - height / 2.0f;

                    if ( add )
                    {
                        if ( dx < 0.0f && dy < 0.0f )
                        {
                            VoxelData existing = terrain.GetValue( currX, currY );
                            VoxelData val      = default(VoxelData);

                            val.SetSolidState( true );

                            float right = x + width / 2.0f;
                            float left  = x - width / 2.0f;
                            float top   = y + height / 2.0f;
                            float bot   = y - height / 2.0f;

                            byte eleft  = (byte)( Mathf.Clamp01( ( px - left ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                            byte eright = (byte)( Mathf.Clamp01( ( right - px ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                            byte etop   = (byte)( Mathf.Clamp01( ( top - py ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                            byte ebot   = (byte)( Mathf.Clamp01( ( py - bot ) / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );

                            eleft  = System.Math.Max( eleft, existing.GetExtentLeft() );
                            eright = System.Math.Max( eright, existing.GetExtentRight() );
                            etop   = System.Math.Max( etop, existing.GetExtentTop() );
                            ebot   = System.Math.Max( ebot, existing.GetExtentBottom() );

                            val.SetExtentHorizontal( eleft, eright );
                            val.SetExtentVertical( ebot, etop );

                            terrain.SetValue( currX, currY, val );
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

                            terrain.SetValue( currX, currY, val );
                        }
                        else
                        {
                            VoxelData existing = terrain.GetValue( currX, currY );

                            if ( dy < dx )
                            {
                                if ( dx < terrain.voxelSize )
                                {
                                    // Point is right of brush
                                    if ( px - x > 0.0f )
                                    {
                                        byte eleft = (byte)( Mathf.Clamp01( dx / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                        eleft = System.Math.Min( eleft, existing.GetExtentLeft() );
                                        existing.SetExtentLeft( eleft );
                                    }
                                    // Left of brush
                                    else if ( px - x < 0.0f )
                                    {
                                        byte eright = (byte)( Mathf.Clamp01( dx / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                        eright = System.Math.Min( eright, existing.GetExtentRight() );
                                        existing.SetExtentRight( eright );
                                    }
                                }
                            }
                            else
                            {
                                if ( dy < terrain.voxelSize )
                                {
                                    // Point is above brush
                                    if ( py - y > 0.0f )
                                    {
                                        byte ebot = (byte)( Mathf.Clamp01( dy / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                        ebot = System.Math.Min( ebot, existing.GetExtentBottom() );
                                        existing.SetExtentBottom( ebot );
                                    }
                                    // Below brush
                                    else if ( py - y < 0.0f )
                                    {
                                        byte etop = (byte)( Mathf.Clamp01( dy / terrain.voxelSize ) * VoxelData.EXTENT_MAX_RESOLUTION );
                                        etop = System.Math.Min( etop, existing.GetExtentTop() );
                                        existing.SetExtentTop( etop );
                                    }
                                }
                            }

                            terrain.SetValue( currX, currY, existing );
                        }
                    }
                }
            }
        }

    }
}