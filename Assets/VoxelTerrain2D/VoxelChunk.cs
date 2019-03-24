using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D
{
    public abstract class VoxelChunk : MonoBehaviour
    {
        protected ChunkedDataset< VoxelData >.DataChunk< VoxelData > m_data;
        protected float m_voxelSize;

        protected Mesh         m_mesh;
        protected MeshFilter   m_filter;
        protected MeshRenderer m_renderer;

        protected bool         m_init = false;



        public void Initialize( ChunkedDataset< VoxelData >.DataChunk< VoxelData > dataset, float voxelSize, Material fillMaterial )
        {
            m_data = dataset;
            m_voxelSize = voxelSize;

            // Initialize mesh
            m_mesh      = new Mesh();
            m_mesh.name = name + "_Mesh";
            m_mesh.MarkDynamic();

            float meshSizeX = dataset.width * voxelSize;
            float meshSizeY = dataset.height * voxelSize;
            m_mesh.bounds = new Bounds(
                new Vector3( meshSizeX / 2.0f, meshSizeY / 2.0f ),
                new Vector3( meshSizeX, meshSizeY )
            );

            m_filter   = gameObject.AddComponent< MeshFilter >();
            m_renderer = gameObject.AddComponent< MeshRenderer >();

            m_filter.sharedMesh       = m_mesh;
            m_renderer.sharedMaterial = fillMaterial;

            OnInitialized();
            m_init = true;
        }


        protected abstract void OnInitialized();


        protected void GenerateMesh( VoxelData[] dataset, int dataWidth, int dataHeight, List< Vector3 > verts, List< int > tris, bool[] batched )
        {
            for( int y = 0; y < dataHeight - 1; y++ )
            {
                for( int x = 0; x < dataWidth - 1; x++ )
                {
                    if ( batched[ y * dataWidth + x ] == true ) { continue; } // Cell is already a part of existing batch, skip it...

                    VoxelData bottomLeft  = dataset[ y * dataWidth + x ];              // v = 1
                    VoxelData bottomRight = dataset[ y * dataWidth + x + 1];           // v = 2
                    VoxelData topRight    = dataset[ ( y + 1 ) * dataWidth + x + 1 ];  // v = 4
                    VoxelData topLeft     = dataset[ ( y + 1 ) * dataWidth + x ];      // v = 8

                    byte index = 0;
                    if ( ( bottomLeft.cell & VoxelData.CELL_MASK_SOLID ) > 0 ) { index |= ( 1 << 0 ); }
                    if ( ( bottomRight.cell & VoxelData.CELL_MASK_SOLID ) > 0  ){ index |= ( 1 << 1 ); }
                    if ( ( topRight.cell & VoxelData.CELL_MASK_SOLID ) > 0  )   { index |= ( 1 << 2 ); }
                    if ( ( topLeft.cell & VoxelData.CELL_MASK_SOLID ) > 0  )    { index |= ( 1 << 3 ); }

                    int i =  y * ( dataWidth - 1 ) + x;
                    
                    float x0 = x * m_voxelSize;
                    float x1 = ( x + 1 ) * m_voxelSize;
                    float y0 = y * m_voxelSize;
                    float y1 = ( y + 1 ) * m_voxelSize;

                    switch( index )
                    {
                        case 0: break;  // Empty cell

                        case 1:
                        { 
                            float xstep = bottomLeft.GetExtentRightNormalized() * m_voxelSize;
                            float ystep = bottomLeft.GetExtentTopNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y0 + ystep );
                            Vector3 v2 = new Vector3( x0 + xstep, y0 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );
                        }
                        break;

                        case 2: 
                        { 
                            float xstep = bottomRight.GetExtentLeftNormalized() * m_voxelSize;
                            float ystep = bottomRight.GetExtentTopNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x1 - xstep, y0 );
                            Vector3 v1 = new Vector3( x1, y0 + ystep );
                            Vector3 v2 = new Vector3( x1, y0 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );
                        }
                        break;

                        case 3: 
                        {
                            float ystep = bottomLeft.GetExtentTopNormalized() * m_voxelSize;
                            float ystep2 = bottomRight.GetExtentTopNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y0 + ystep );
                            Vector3 v2 = new Vector3( x1, y0 + ystep2 );
                            Vector3 v3 = new Vector3( x1, y0);

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 2 );
                            tris.Add( vcount + 3 );
                        }
                        break;

                        case 4:
                        {
                            float xstep = topRight.GetExtentLeftNormalized() * m_voxelSize;
                            float ystep = topRight.GetExtentBottomNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x1, y1 );
                            Vector3 v1 = new Vector3( x1, y1 - ystep );
                            Vector3 v2 = new Vector3( x1 - xstep, y1 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );
                        }
                        break;

                        case 5:
                        {
                            float xstep  = bottomLeft.GetExtentRightNormalized() * m_voxelSize;
                            float ystep  = bottomLeft.GetExtentTopNormalized() * m_voxelSize;
                            float xstep2 = topRight.GetExtentLeftNormalized() * m_voxelSize;
                            float ystep2 = topRight.GetExtentBottomNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y0 + ystep );
                            Vector3 v2 = new Vector3( x0 + xstep, y0 );
                            Vector3 v3 = new Vector3( x1 - xstep2, y1 );
                            Vector3 v4 = new Vector3( x1, y1 );
                            Vector3 v5 = new Vector3( x1, y1 - ystep2 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );
                            verts.Add( v4 );
                            verts.Add( v5 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount + 3 );
                            tris.Add( vcount + 4 );
                            tris.Add( vcount + 5 );

                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 3 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount + 2 );
                            tris.Add( vcount + 3 );
                            tris.Add( vcount + 5 );
                        }
                        break;

                        case 6:
                        {
                            float xstep = topRight.GetExtentLeftNormalized() * m_voxelSize;
                            float xstep2 = bottomRight.GetExtentLeftNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x1 - xstep2, y0 );
                            Vector3 v1 = new Vector3( x1 - xstep, y1 );
                            Vector3 v2 = new Vector3( x1, y1 );
                            Vector3 v3 = new Vector3( x1, y0 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 2 );
                            tris.Add( vcount + 3 );
                        }
                        break;

                        case 7:
                        {
                            float xstep = topRight.GetExtentLeftNormalized() * m_voxelSize;
                            float ystep = bottomLeft.GetExtentTopNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y0 + ystep );
                            Vector3 v2 = new Vector3( x1 - xstep, y1 );
                            Vector3 v3 = new Vector3( x1, y1);
                            Vector3 v4 = new Vector3( x1, y0 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );
                            verts.Add( v4 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 4 );

                            tris.Add( vcount + 4 );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount + 4 );
                            tris.Add( vcount + 2 );
                            tris.Add( vcount + 3 );
                        }
                        break;

                        case 8:
                        {
                            float xstep = topLeft.GetExtentRightNormalized() * m_voxelSize;
                            float ystep = topLeft.GetExtentBottomNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y1 - ystep );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x0 + xstep, y1 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );
                        }
                        break;

                        case 9:
                        {
                            float xstep  = topLeft.GetExtentRightNormalized() * m_voxelSize;
                            float xstep2 = bottomLeft.GetExtentRightNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x0 + xstep, y1 );
                            Vector3 v3 = new Vector3( x0 + xstep2, y0 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 2 );
                            tris.Add( vcount + 3 );
                        }
                        break;

                        case 10:
                        {
                            float xstep  = topLeft.GetExtentRightNormalized() * m_voxelSize;
                            float ystep  = topLeft.GetExtentBottomNormalized() * m_voxelSize;
                            float xstep2 = bottomRight.GetExtentLeftNormalized() * m_voxelSize;
                            float ystep2 = bottomRight.GetExtentTopNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y1 - ystep );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x0 + xstep, y1 );
                            Vector3 v3 = new Vector3( x1 - xstep2, y0 );
                            Vector3 v4 = new Vector3( x1, y0 + ystep2 );
                            Vector3 v5 = new Vector3( x1, y0 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );
                            verts.Add( v4 );
                            verts.Add( v5 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount + 3 );
                            tris.Add( vcount + 4 );
                            tris.Add( vcount + 5 );

                            tris.Add( vcount + 3 );
                            tris.Add( vcount + 0 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount + 3 );
                            tris.Add( vcount + 2 );
                            tris.Add( vcount + 4 );
                        }
                        break;

                        case 11:
                        {
                            float xstep  = topLeft.GetExtentRightNormalized() * m_voxelSize;
                            float ystep  = bottomRight.GetExtentTopNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x0 + xstep, y1 );
                            Vector3 v3 = new Vector3( x1, y0 + ystep );
                            Vector3 v4 = new Vector3( x1, y0 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );
                            verts.Add( v4 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 2 );
                            tris.Add( vcount + 3 );

                            tris.Add( vcount );
                            tris.Add( vcount + 3 );
                            tris.Add( vcount + 4 );
                        }
                        break;

                        case 12:
                        {
                            float ystep  = topLeft.GetExtentBottomNormalized() * m_voxelSize;
                            float ystep2 = topRight.GetExtentBottomNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y1 - ystep );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x1, y1);
                            Vector3 v3 = new Vector3( x1, y1 - ystep2 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 2 );
                            tris.Add( vcount + 3 );
                        }
                        break;

                        case 13:
                        {
                            float xstep  = bottomLeft.GetExtentRightNormalized() * m_voxelSize;
                            float ystep  = topRight.GetExtentBottomNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x1, y1 );
                            Vector3 v3 = new Vector3( x1, y1 - ystep );
                            Vector3 v4 = new Vector3( x0 + xstep, y0 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );
                            verts.Add( v4 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 4 );

                            tris.Add( vcount + 4 );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 3 );

                            tris.Add( vcount + 3 );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );
                        }
                        break;

                        case 14:
                        {
                            float xstep  = bottomRight.GetExtentLeftNormalized() * m_voxelSize;
                            float ystep  = topLeft.GetExtentBottomNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y1 - ystep );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x1, y1 );
                            Vector3 v3 = new Vector3( x1, y0 );
                            Vector3 v4 = new Vector3( x1 - xstep, y0 );

                            int vcount = verts.Count;
            
                            verts.Add( v0 );
                            verts.Add( v1 );
                            verts.Add( v2 );
                            verts.Add( v3 );
                            verts.Add( v4 );

                            tris.Add( vcount );
                            tris.Add( vcount + 1 );
                            tris.Add( vcount + 2 );

                            tris.Add( vcount );
                            tris.Add( vcount + 2 );
                            tris.Add( vcount + 3 );

                            tris.Add( vcount );
                            tris.Add( vcount + 3 );
                            tris.Add( vcount + 4 );
                        }
                        break;

                        case 15:
                        {
                            if ( topLeft.CompareWithoutExtent( bottomLeft ) && topRight.CompareWithoutExtent( bottomRight ) && bottomLeft.CompareWithoutExtent( bottomRight ) )
                            {
                                GenerateGreedyQuad( dataset, dataWidth, dataHeight, x, y, verts, tris, batched );
                            }
                            else
                            {
                                Vector3 v0 = new Vector3( x0, y0 );
                                Vector3 v1 = new Vector3( x0, y1 );
                                Vector3 v2 = new Vector3( x1, y1 );
                                Vector3 v3 = new Vector3( x1, y0 );

                                int vcount = verts.Count;

                                verts.Add( v0 );
                                verts.Add( v1 );
                                verts.Add( v2 );
                                verts.Add( v3 );

                                tris.Add( vcount );
                                tris.Add( vcount + 1 );
                                tris.Add( vcount + 2 );

                                tris.Add( vcount );
                                tris.Add( vcount + 2 );
                                tris.Add( vcount + 3 );
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void GenerateGreedyQuad( VoxelData[] dataset, int dataWidth, int dataHeight, int x, int y, List< Vector3 > verts, List< int > tris, bool[] batched )
        {
            int startX = x;
            int startY = y;
            int endX   = x;
            int endY   = y;

            VoxelData   compare   = dataset[ y * dataWidth + x ];

            for( int xx = x + 1; xx < dataWidth - 1; xx++ )
            {
                if ( batched[ y * dataWidth + xx ] == true ) { break; } // Encountered another batch, stop expanding

                VoxelData a = dataset[ y * dataWidth + xx + 1];
                VoxelData b = dataset[ ( y + 1 ) * dataWidth + xx + 1 ];

                if ( compare.CompareWithoutExtent( a ) && compare.CompareWithoutExtent( b ) )
                {
                    endX = xx;
                }
                else{ break; } // Change in voxel data detected, stop extending
            }

            bool validUp = true;
            for( int yy = y + 1; yy < dataHeight - 1 && validUp == true; yy++ )
            {
                for( int xx = startX; xx <= endX && validUp == true; xx++ )
                {
                    if ( batched[ yy * dataWidth + xx ] == true ) { validUp = false; break; } // Encountered another batch, stop expanding

                    VoxelData a = dataset[ ( yy + 1 ) * dataWidth + xx + 1 ];
                    VoxelData b = dataset[ ( yy + 1 ) * dataWidth + xx ];

                    if ( compare.CompareWithoutExtent( a ) == false || compare.CompareWithoutExtent( b ) == false )
                    {
                        validUp = false;
                        break;
                    }
                }

                if ( validUp == true ) // Collect line
                {
                    endY = yy;
                }
            }

            // Mark batch
            for ( int yy = startY; yy <= endY; yy++ )
            {
                for ( int xx = startX; xx <= endX; xx++ )
                {
                    batched[ yy * dataWidth + xx ] = true;
                }
            }

            // Build quad mesh data
            float x0 = startX * m_voxelSize;
            float x1 = ( endX + 1 ) * m_voxelSize;
            float y0 = startY * m_voxelSize;
            float y1 = ( endY + 1 ) * m_voxelSize;

            Vector3 v0 = new Vector3( x0, y0 );
            Vector3 v1 = new Vector3( x0, y1 );
            Vector3 v2 = new Vector3( x1, y1 );
            Vector3 v3 = new Vector3( x1, y0 );

            int vcount = verts.Count;
            
            verts.Add( v0 );
            verts.Add( v1 );
            verts.Add( v2 );
            verts.Add( v3 );

            tris.Add( vcount );
            tris.Add( vcount + 1 );
            tris.Add( vcount + 2 );

            tris.Add( vcount );
            tris.Add( vcount + 2 );
            tris.Add( vcount + 3 );
        }
    }
}