using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    public struct VoxelData
    {
        public const byte  CELL_MASK_SOLID        = 0x1;

        public const byte  EXTENT_MASK_LOW        = 0x0F;
        public const byte  EXTENT_MASK_HIGH       = 0xF0;
        public const int   EXTENT_SHIFT_HIGH      = 4;
        public const byte  EXTENT_MAX_RESOLUTION  = 15;
        public const float EXTENT_MAX_RESOLUTION_F = EXTENT_MAX_RESOLUTION * 1.0f;

        public byte cell;
        public byte extentHorizontal;
        public byte extentVertical;


        public bool GetSolidState()
        {
            return ( cell & CELL_MASK_SOLID ) > 0;
        }


        public void SetSolidState( bool state )
        { 
            if ( state ){ cell |= CELL_MASK_SOLID; }
            else{ cell &= ( ~CELL_MASK_SOLID & 0xFF ); }
        }


        public byte GetExtentLeft()
        {
            return (byte)( extentHorizontal & EXTENT_MASK_LOW );
        }


        public float GetExtentLeftNormalized()
        {
            return GetExtentLeft() / EXTENT_MAX_RESOLUTION_F;
        }


        public void SetExtentLeft( byte val )
        {
            extentHorizontal = (byte)(( extentHorizontal & EXTENT_MASK_HIGH ) | val);
        }


        public byte GetExtentRight()
        {
            return (byte)( ( extentHorizontal & EXTENT_MASK_HIGH ) >> EXTENT_SHIFT_HIGH );
        }


        public float GetExtentRightNormalized()
        {
            return GetExtentRight() / EXTENT_MAX_RESOLUTION_F;
        }


        public void SetExtentRight( byte val )
        {
            extentHorizontal = (byte)(( extentHorizontal & EXTENT_MASK_LOW ) | ( val << EXTENT_SHIFT_HIGH ) );
        }


        public byte GetExtentBottom()
        {
            return (byte)( extentVertical & EXTENT_MASK_LOW );
        }

        public float GetExtentBottomNormalized()
        {
            return GetExtentBottom() / EXTENT_MAX_RESOLUTION_F;
        }


        public void SetExtentBottom( byte val )
        {
            extentVertical = (byte)(( extentVertical & EXTENT_MASK_HIGH ) | val);
        }


        public byte GetExtentTop()
        {
            return (byte)( ( extentVertical & EXTENT_MASK_HIGH ) >> EXTENT_SHIFT_HIGH );
        }


        public float GetExtentTopNormalized()
        {
            return GetExtentTop() / EXTENT_MAX_RESOLUTION_F;
        }



        public void SetExtentTop( byte val )
        {
            extentVertical = (byte)(( extentVertical & EXTENT_MASK_LOW ) | ( val << EXTENT_SHIFT_HIGH ) );
        }

        public void SetExtentHorizontal( byte left, byte right )
        {
            extentHorizontal = ( byte )( ( left & EXTENT_MASK_LOW ) | ( ( right & EXTENT_MASK_LOW ) << 4 ) );
        }


        public void SetExtentVertical( byte bot, byte top )
        {
            extentVertical = ( byte )( ( bot & EXTENT_MASK_LOW ) | ( ( top & EXTENT_MASK_LOW ) << 4 ) );
        }


        public override bool Equals( object obj )
        {
            if ( !( obj is VoxelData ) )
            {
                return false;
            }

            var data = ( VoxelData )obj;
            return cell == data.cell &&
                     extentHorizontal == data.extentHorizontal &&
                     extentVertical == data.extentVertical;
        }


        public override int GetHashCode()
        {
            var hashCode = -406950879;
            hashCode = hashCode * -1521134295 + cell.GetHashCode();
            hashCode = hashCode * -1521134295 + extentHorizontal.GetHashCode();
            hashCode = hashCode * -1521134295 + extentVertical.GetHashCode();
            return hashCode;
        }


        public static bool operator ==(VoxelData v1, VoxelData v2) 
        {
            return v1.Equals(v2);
        }


        public static bool operator !=(VoxelData v1, VoxelData v2) 
        {
           return !v1.Equals(v2);
        }
    }
}