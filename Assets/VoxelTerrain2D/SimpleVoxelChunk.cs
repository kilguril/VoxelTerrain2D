using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VoxelTerrain2D
{
    public sealed class SimpleVoxelChunk : VoxelChunk
    {
        private List< Vector3 > m_verts;
        private List< int >     m_tris;

        protected override void OnInitialized()
        {
            // Initialize chunk data
            m_verts = new List< Vector3 >();
            m_tris  = new List< int >();

            // Rebuild initial state
            RebuildMesh();
        }


        void LateUpdate()
        {
            if ( m_data.dirty == true )
            {
                RebuildMesh();
                m_data.dirty = false;
            }
        }

        private void RebuildMesh()
        {
            ClearBuffers();
            GenerateMesh();
            AssignMeshData();
        }


        private void ClearBuffers()
        {
            m_verts.Clear();
            m_tris.Clear();
        }


        private void GenerateMesh()
        {
            int         dataWidth = m_data.width;
            VoxelData[] dataset   = m_data.data;

            for( int y = 0; y < m_data.height - 1; y++ )
            {
                for( int x = 0; x < m_data.width - 1; x++ )
                {
                    VoxelData bottomLeft  = dataset[ y * dataWidth + x ];              // v = 1
                    VoxelData bottomRight = dataset[ y * dataWidth + x + 1];           // v = 2
                    VoxelData topRight    = dataset[ ( y + 1 ) * dataWidth + x + 1 ];  // v = 4
                    VoxelData topLeft     = dataset[ ( y + 1 ) * dataWidth + x ];      // v = 8

                    byte index = 0;
                    if ( ( bottomLeft.cell & VoxelData.CELL_MASK_SOLID ) > 0 ) { index |= ( 1 << 0 ); }
                    if ( ( bottomRight.cell & VoxelData.CELL_MASK_SOLID ) > 0  ){ index |= ( 1 << 1 ); }
                    if ( ( topRight.cell & VoxelData.CELL_MASK_SOLID ) > 0  )   { index |= ( 1 << 2 ); }
                    if ( ( topLeft.cell & VoxelData.CELL_MASK_SOLID ) > 0  )    { index |= ( 1 << 3 ); }

                    int i =  y * ( m_data.width - 1 ) + x;
                    
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );
                        }
                        break;

                        case 2: 
                        { 
                            float xstep = bottomRight.GetExtentLeftNormalized() * m_voxelSize;
                            float ystep = bottomRight.GetExtentTopNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x1 - xstep, y0 );
                            Vector3 v1 = new Vector3( x1, y0 + ystep );
                            Vector3 v2 = new Vector3( x1, y0 );

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 3 );
                        }
                        break;

                        case 4:
                        {
                            float xstep = topRight.GetExtentLeftNormalized() * m_voxelSize;
                            float ystep = topRight.GetExtentBottomNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x1, y1 );
                            Vector3 v1 = new Vector3( x1, y1 - ystep );
                            Vector3 v2 = new Vector3( x1 - xstep, y1 );

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );
                            m_verts.Add( v4 );
                            m_verts.Add( v5 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount + 3 );
                            m_tris.Add( vcount + 4 );
                            m_tris.Add( vcount + 5 );

                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 3 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 3 );
                            m_tris.Add( vcount + 5 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 3 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );
                            m_verts.Add( v4 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 4 );

                            m_tris.Add( vcount + 4 );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount + 4 );
                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 3 );
                        }
                        break;

                        case 8:
                        {
                            float xstep = topLeft.GetExtentRightNormalized() * m_voxelSize;
                            float ystep = topLeft.GetExtentBottomNormalized() * m_voxelSize;

                            Vector3 v0 = new Vector3( x0, y1 - ystep );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x0 + xstep, y1 );

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 3 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );
                            m_verts.Add( v4 );
                            m_verts.Add( v5 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount + 3 );
                            m_tris.Add( vcount + 4 );
                            m_tris.Add( vcount + 5 );

                            m_tris.Add( vcount + 3 );
                            m_tris.Add( vcount + 0 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount + 3 );
                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 4 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );
                            m_verts.Add( v4 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 3 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 3 );
                            m_tris.Add( vcount + 4 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 3 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );
                            m_verts.Add( v4 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 4 );

                            m_tris.Add( vcount + 4 );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 3 );

                            m_tris.Add( vcount + 3 );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );
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

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );
                            m_verts.Add( v4 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 3 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 3 );
                            m_tris.Add( vcount + 4 );
                        }
                        break;

                        case 15:
                        {
                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x1, y1 );
                            Vector3 v3 = new Vector3( x1, y0 );

                            int vcount = m_verts.Count;
            
                            m_verts.Add( v0 );
                            m_verts.Add( v1 );
                            m_verts.Add( v2 );
                            m_verts.Add( v3 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 1 );
                            m_tris.Add( vcount + 2 );

                            m_tris.Add( vcount );
                            m_tris.Add( vcount + 2 );
                            m_tris.Add( vcount + 3 );
                        }
                        break;
                    }
                }
            }
        }


        private void AssignMeshData()
        {
            m_mesh.Clear( true );
            m_mesh.SetVertices( m_verts );
            m_mesh.SetTriangles( m_tris, 0, false );
        }
    }
}