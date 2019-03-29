using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DataChunk = VoxelTerrain2D.ChunkedDataset< VoxelTerrain2D.VoxelData >.DataChunk< VoxelTerrain2D.VoxelData >;

namespace VoxelTerrain2D
{
    public abstract class VoxelChunk : MonoBehaviour
    {
        #if UNITY_EDITOR
        [SerializeField]
        private bool m_debugContour = default( bool );

        [SerializeField]
        private bool m_debugContourNormals = default( bool );
        #endif

        protected class MeshOutput
        {
            public List< Vector3 >          verts;
            public List< int >              tris;
            public bool[]                   batched;

            public List< Vector3 >          contourVerts;
            public List< int >              contourTris;
            public List< Vector2 >          contourUVs;

            public List< List < Vector2 > > contours;

            public List< Vector3 >          collisionVerts;
            public List< int >              collisionTris;
        }

        protected DataChunk         m_data;
        protected GeneratorSettings m_settings;

        protected Mesh         m_mesh;
        protected MeshRenderer m_renderer;

        protected Mesh         m_meshContour;
        protected MeshRenderer m_rendererContour;

        protected Mesh         m_meshCollision;
        protected MeshCollider m_collider;

        protected MeshOutput   m_meshOut;

        protected bool         m_init = false;



        public void Initialize( DataChunk dataset, GeneratorSettings settings )
        {
            m_data     = dataset;
            m_settings = settings;

            // Initialize mesh
            m_mesh      = new Mesh();
            m_mesh.name = name + "_Mesh";
            m_mesh.MarkDynamic();

            float meshSizeX = dataset.width * m_settings.voxelSize;
            float meshSizeY = dataset.height * m_settings.voxelSize;
            m_mesh.bounds = new Bounds(
                new Vector3( meshSizeX / 2.0f, meshSizeY / 2.0f ),
                new Vector3( meshSizeX, meshSizeY )
            );

            MeshFilter filter = gameObject.AddComponent< MeshFilter >();
            m_renderer        = gameObject.AddComponent< MeshRenderer >();

            filter.sharedMesh = m_mesh;
            m_renderer.sharedMaterial = m_settings.fillMaterial;

            if ( m_settings.generateCollision )
            {
                m_meshCollision = new Mesh();
                m_meshCollision.name = name + "_CollisionMesh";
                m_meshCollision.MarkDynamic();

                m_meshCollision.bounds = m_mesh.bounds;

                m_collider = gameObject.AddComponent< MeshCollider >();
                m_collider.sharedMesh = m_meshCollision;
                m_collider.convex = false;
                m_collider.cookingOptions = m_settings.colliderCookingOptions;
            }

            if ( m_settings.meshContour )
            {
                GameObject contour = new GameObject("Contour");
                contour.transform.parent = transform;
                contour.transform.localPosition = Vector3.zero;
                contour.transform.localScale    = Vector3.one;
                contour.transform.localRotation = Quaternion.identity;

                m_meshContour = new Mesh();
                m_meshContour.name = name + "_MeshContour";
                m_meshContour.MarkDynamic();

                m_meshContour.bounds = m_mesh.bounds;

                MeshFilter contourFilter = contour.AddComponent< MeshFilter >();
                contourFilter.sharedMesh = m_meshContour;

                m_rendererContour = contour.AddComponent< MeshRenderer >();
                m_rendererContour.sharedMaterial = settings.outlineMaterial;
            }

            OnInitialized();
            m_init = true;
        }


        protected abstract void OnInitialized();


        protected void GenerateMesh( DataChunk i, MeshOutput o )
        {
            VoxelData[] dataset = i.data;
            int   dataWidth     = i.width;
            int   dataHeight    = i.height;
            float voxelSize     = m_settings.voxelSize;

            for( int y = 0; y < dataHeight - 1; y++ )
            {
                for( int x = 0; x < dataWidth - 1; x++ )
                {
                    if ( o.batched[ y * dataWidth + x ] == true ) { continue; } // Cell is already a part of existing batch, skip it...

                    VoxelData bottomLeft  = dataset[ y * dataWidth + x ];              // v = 1
                    VoxelData bottomRight = dataset[ y * dataWidth + x + 1];           // v = 2
                    VoxelData topRight    = dataset[ ( y + 1 ) * dataWidth + x + 1 ];  // v = 4
                    VoxelData topLeft     = dataset[ ( y + 1 ) * dataWidth + x ];      // v = 8

                    byte index = 0;
                    if ( ( bottomLeft.cell & VoxelData.CELL_MASK_SOLID ) > 0 ) { index |= ( 1 << 0 ); }
                    if ( ( bottomRight.cell & VoxelData.CELL_MASK_SOLID ) > 0  ){ index |= ( 1 << 1 ); }
                    if ( ( topRight.cell & VoxelData.CELL_MASK_SOLID ) > 0  )   { index |= ( 1 << 2 ); }
                    if ( ( topLeft.cell & VoxelData.CELL_MASK_SOLID ) > 0  )    { index |= ( 1 << 3 ); }

                    float x0 = x * voxelSize;
                    float x1 = ( x + 1 ) * voxelSize;
                    float y0 = y * voxelSize;
                    float y1 = ( y + 1 ) * voxelSize;

                    switch( index )
                    {
                        case 0: break;  // Empty cell

                        case 1:
                        { 
                            float xstep = bottomLeft.GetExtentRightNormalized() * voxelSize;
                            float ystep = bottomLeft.GetExtentTopNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y0 + ystep );
                            Vector3 v2 = new Vector3( x0 + xstep, y0 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            AppendContour( v1, v2, o );
                        }
                        break;

                        case 2: 
                        { 
                            float xstep = bottomRight.GetExtentLeftNormalized() * voxelSize;
                            float ystep = bottomRight.GetExtentTopNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x1 - xstep, y0 );
                            Vector3 v1 = new Vector3( x1, y0 + ystep );
                            Vector3 v2 = new Vector3( x1, y0 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            AppendContour( v0, v1, o );
                        }
                        break;

                        case 3: 
                        {
                            float ystep = bottomLeft.GetExtentTopNormalized() * voxelSize;
                            float ystep2 = bottomRight.GetExtentTopNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y0 + ystep );
                            Vector3 v2 = new Vector3( x1, y0 + ystep2 );
                            Vector3 v3 = new Vector3( x1, y0);

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 2 );
                            o.tris.Add( vcount + 3 );

                            AppendContour( v1, v2, o );
                        }
                        break;

                        case 4:
                        {
                            float xstep = topRight.GetExtentLeftNormalized() * voxelSize;
                            float ystep = topRight.GetExtentBottomNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x1, y1 );
                            Vector3 v1 = new Vector3( x1, y1 - ystep );
                            Vector3 v2 = new Vector3( x1 - xstep, y1 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            AppendContour( v1, v2, o );
                        }
                        break;

                        case 5:
                        {
                            float xstep  = bottomLeft.GetExtentRightNormalized() * voxelSize;
                            float ystep  = bottomLeft.GetExtentTopNormalized() * voxelSize;
                            float xstep2 = topRight.GetExtentLeftNormalized() * voxelSize;
                            float ystep2 = topRight.GetExtentBottomNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y0 + ystep );
                            Vector3 v2 = new Vector3( x0 + xstep, y0 );
                            Vector3 v3 = new Vector3( x1 - xstep2, y1 );
                            Vector3 v4 = new Vector3( x1, y1 );
                            Vector3 v5 = new Vector3( x1, y1 - ystep2 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );
                            o.verts.Add( v4 );
                            o.verts.Add( v5 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount + 3 );
                            o.tris.Add( vcount + 4 );
                            o.tris.Add( vcount + 5 );

                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 3 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount + 2 );
                            o.tris.Add( vcount + 3 );
                            o.tris.Add( vcount + 5 );

                            AppendContour( v1, v3, o );
                            AppendContour( v5, v2, o );
                        }
                        break;

                        case 6:
                        {
                            float xstep = topRight.GetExtentLeftNormalized() * voxelSize;
                            float xstep2 = bottomRight.GetExtentLeftNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x1 - xstep2, y0 );
                            Vector3 v1 = new Vector3( x1 - xstep, y1 );
                            Vector3 v2 = new Vector3( x1, y1 );
                            Vector3 v3 = new Vector3( x1, y0 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 2 );
                            o.tris.Add( vcount + 3 );

                            AppendContour( v0, v1, o );
                        }
                        break;

                        case 7:
                        {
                            float xstep = topRight.GetExtentLeftNormalized() * voxelSize;
                            float ystep = bottomLeft.GetExtentTopNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y0 + ystep );
                            Vector3 v2 = new Vector3( x1 - xstep, y1 );
                            Vector3 v3 = new Vector3( x1, y1);
                            Vector3 v4 = new Vector3( x1, y0 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );
                            o.verts.Add( v4 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 4 );

                            o.tris.Add( vcount + 4 );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount + 4 );
                            o.tris.Add( vcount + 2 );
                            o.tris.Add( vcount + 3 );

                            AppendContour( v1, v2, o );
                        }
                        break;

                        case 8:
                        {
                            float xstep = topLeft.GetExtentRightNormalized() * voxelSize;
                            float ystep = topLeft.GetExtentBottomNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y1 - ystep );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x0 + xstep, y1 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            AppendContour( v2, v0, o );
                        }
                        break;

                        case 9:
                        {
                            float xstep  = topLeft.GetExtentRightNormalized() * voxelSize;
                            float xstep2 = bottomLeft.GetExtentRightNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x0 + xstep, y1 );
                            Vector3 v3 = new Vector3( x0 + xstep2, y0 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 2 );
                            o.tris.Add( vcount + 3 );

                            AppendContour( v2, v3, o );
                        }
                        break;

                        case 10:
                        {
                            float xstep  = topLeft.GetExtentRightNormalized() * voxelSize;
                            float ystep  = topLeft.GetExtentBottomNormalized() * voxelSize;
                            float xstep2 = bottomRight.GetExtentLeftNormalized() * voxelSize;
                            float ystep2 = bottomRight.GetExtentTopNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y1 - ystep );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x0 + xstep, y1 );
                            Vector3 v3 = new Vector3( x1 - xstep2, y0 );
                            Vector3 v4 = new Vector3( x1, y0 + ystep2 );
                            Vector3 v5 = new Vector3( x1, y0 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );
                            o.verts.Add( v4 );
                            o.verts.Add( v5 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount + 3 );
                            o.tris.Add( vcount + 4 );
                            o.tris.Add( vcount + 5 );

                            o.tris.Add( vcount + 3 );
                            o.tris.Add( vcount + 0 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount + 3 );
                            o.tris.Add( vcount + 2 );
                            o.tris.Add( vcount + 4 );

                            AppendContour( v3, v0, o );
                            AppendContour( v2, v4, o );
                        }
                        break;

                        case 11:
                        {
                            float xstep  = topLeft.GetExtentRightNormalized() * voxelSize;
                            float ystep  = bottomRight.GetExtentTopNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x0 + xstep, y1 );
                            Vector3 v3 = new Vector3( x1, y0 + ystep );
                            Vector3 v4 = new Vector3( x1, y0 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );
                            o.verts.Add( v4 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 2 );
                            o.tris.Add( vcount + 3 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 3 );
                            o.tris.Add( vcount + 4 );

                            AppendContour( v2, v3, o );
                        }
                        break;

                        case 12:
                        {
                            float ystep  = topLeft.GetExtentBottomNormalized() * voxelSize;
                            float ystep2 = topRight.GetExtentBottomNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y1 - ystep );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x1, y1);
                            Vector3 v3 = new Vector3( x1, y1 - ystep2 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 2 );
                            o.tris.Add( vcount + 3 );

                            AppendContour( v3, v0, o );
                        }
                        break;

                        case 13:
                        {
                            float xstep  = bottomLeft.GetExtentRightNormalized() * voxelSize;
                            float ystep  = topRight.GetExtentBottomNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y0 );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x1, y1 );
                            Vector3 v3 = new Vector3( x1, y1 - ystep );
                            Vector3 v4 = new Vector3( x0 + xstep, y0 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );
                            o.verts.Add( v4 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 4 );

                            o.tris.Add( vcount + 4 );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 3 );

                            o.tris.Add( vcount + 3 );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            AppendContour( v3, v4, o );
                        }
                        break;

                        case 14:
                        {
                            float xstep  = bottomRight.GetExtentLeftNormalized() * voxelSize;
                            float ystep  = topLeft.GetExtentBottomNormalized() * voxelSize;

                            Vector3 v0 = new Vector3( x0, y1 - ystep );
                            Vector3 v1 = new Vector3( x0, y1 );
                            Vector3 v2 = new Vector3( x1, y1 );
                            Vector3 v3 = new Vector3( x1, y0 );
                            Vector3 v4 = new Vector3( x1 - xstep, y0 );

                            int vcount = o.verts.Count;
            
                            o.verts.Add( v0 );
                            o.verts.Add( v1 );
                            o.verts.Add( v2 );
                            o.verts.Add( v3 );
                            o.verts.Add( v4 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 1 );
                            o.tris.Add( vcount + 2 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 2 );
                            o.tris.Add( vcount + 3 );

                            o.tris.Add( vcount );
                            o.tris.Add( vcount + 3 );
                            o.tris.Add( vcount + 4 );

                            AppendContour( v4, v0, o );
                        }
                        break;

                        case 15:
                        {
                            if ( topLeft.CompareWithoutExtent( bottomLeft ) && topRight.CompareWithoutExtent( bottomRight ) && bottomLeft.CompareWithoutExtent( bottomRight ) )
                            {
                                GenerateGreedyQuad( i, x, y, o );
                            }
                            else
                            {
                                Vector3 v0 = new Vector3( x0, y0 );
                                Vector3 v1 = new Vector3( x0, y1 );
                                Vector3 v2 = new Vector3( x1, y1 );
                                Vector3 v3 = new Vector3( x1, y0 );

                                int vcount = o.verts.Count;

                                o.verts.Add( v0 );
                                o.verts.Add( v1 );
                                o.verts.Add( v2 );
                                o.verts.Add( v3 );

                                o.tris.Add( vcount );
                                o.tris.Add( vcount + 1 );
                                o.tris.Add( vcount + 2 );

                                o.tris.Add( vcount );
                                o.tris.Add( vcount + 2 );
                                o.tris.Add( vcount + 3 );
                            }
                        }
                        break;
                    }
                }
            }

            CombineContours( o );

            if ( m_settings.generateCollision ){ BuildCollision( o ); }
            if ( m_settings.meshContour ){ BuildContourMesh( o ); }
        }


        private void GenerateGreedyQuad( DataChunk i, int x, int y, MeshOutput o )
        {
            VoxelData[] dataset = i.data;
            int   dataWidth     = i.width;
            int   dataHeight    = i.height;
            float voxelSize     = m_settings.voxelSize;

            int startX = x;
            int startY = y;
            int endX   = x;
            int endY   = y;

            VoxelData   compare   = dataset[ y * dataWidth + x ];

            for( int xx = x + 1; xx < dataWidth - 1; xx++ )
            {
                if ( o.batched[ y * dataWidth + xx ] == true ) { break; } // Encountered another batch, stop expanding

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
                    if ( o.batched[ yy * dataWidth + xx ] == true ) { validUp = false; break; } // Encountered another batch, stop expanding

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
                    o.batched[ yy * dataWidth + xx ] = true;
                }
            }

            // Build quad mesh data
            float x0 = startX * voxelSize;
            float x1 = ( endX + 1 ) * voxelSize;
            float y0 = startY * voxelSize;
            float y1 = ( endY + 1 ) * voxelSize;

            Vector3 v0 = new Vector3( x0, y0 );
            Vector3 v1 = new Vector3( x0, y1 );
            Vector3 v2 = new Vector3( x1, y1 );
            Vector3 v3 = new Vector3( x1, y0 );

            int vcount = o.verts.Count;
            
            o.verts.Add( v0 );
            o.verts.Add( v1 );
            o.verts.Add( v2 );
            o.verts.Add( v3 );

            o.tris.Add( vcount );
            o.tris.Add( vcount + 1 );
            o.tris.Add( vcount + 2 );

            o.tris.Add( vcount );
            o.tris.Add( vcount + 2 );
            o.tris.Add( vcount + 3 );
        }


        private bool AppendContour( Vector2 from, Vector2 to, MeshOutput o )
        {
            for ( int i = 0; i < o.contours.Count; i++ )
            {
                if ( o.contours[ i ].Count > 1 )
                {
                    Vector2 first  = o.contours[ i ][ 0 ];
                    Vector2 second = o.contours[ i ][ 1 ];

                    Vector2 last       = o.contours[ i ][ o.contours[ i ].Count - 1 ];
                    Vector2 beforeLast = o.contours[ i ][ o.contours[ i ].Count - 2 ];

                    if ( Mathf.Approximately( last.x, from.x ) && Mathf.Approximately( last.y, from.y ) )
                    {
                        Vector2 ab = to - from;
                        Vector2 cd = last - beforeLast;
                        float   a  = Vector2.Angle( ab, cd );

                        if ( Mathf.Approximately( a, 0.0f ) )
                        {
                            o.contours[ i ][ o.contours[ i ].Count - 1 ] = to;
                        }
                        else
                        {
                            o.contours[ i ].Add( to );
                        }

                        return true;
                    }
                    else if ( Mathf.Approximately( first.x, to.x ) && Mathf.Approximately( first.y, to.y ) )
                    {
                        Vector2 ab = to - from;
                        Vector2 cd = second - first;
                        float   a  = Vector2.Angle( ab, cd );

                        if ( Mathf.Approximately( a, 0.0f ) )
                        {
                            o.contours[ i ][ 0 ] = from;
                        }
                        else
                        {
                            o.contours[ i ].Insert( 0, from );
                        }
                        return true;
                    }
                }
            }

            List< Vector2 > ctr = new List<Vector2>();
            ctr.Add( from );
            ctr.Add( to );

            o.contours.Add( ctr );

            return false;
        }


        private void CombineContours( MeshOutput o )
        {
            for( int i = o.contours.Count - 1; i > 0; i-- )
            {
                for( int k = i - 1; k >= 0; k-- )
                {
                    bool merged = false;

                    Vector2 ifirst = o.contours[ i ][ 0 ];
                    Vector2 ilast = o.contours[ i ][ o.contours[ i ].Count - 1 ];

                    Vector2 kfirst = o.contours[ k ][ 0 ];
                    Vector2 klast = o.contours[ k ][ o.contours[ k ].Count - 1 ];

                    if ( Mathf.Approximately( klast.x, ifirst.x ) && Mathf.Approximately( klast.y, ifirst.y ) )
                    {
                        o.contours[ k ].RemoveAt( o.contours[ k ].Count - 1 );
                        o.contours[ k ].AddRange( o.contours[ i ] );
                        merged = true;
                    }
                    else if ( Mathf.Approximately( kfirst.x, ilast.x ) && Mathf.Approximately( kfirst.y, ilast.y ) )
                    {
                        o.contours[ i ].RemoveAt( o.contours[ i ].Count - 1 );
                        o.contours[ i ].AddRange( o.contours[ k ] );
                        o.contours[ k ] = o.contours[ i ];
                        merged = true;
                    }

                    if ( merged == true ) { o.contours.RemoveAt( i ); break; }
                }
            }
        }


        private void BuildContourMesh( MeshOutput o )
        {
            float inset  = m_settings.contourInset;
            float outset = m_settings.contourOutset;
            float zbias  = m_settings.contourZbias;
            int   verts  = 0;

            Vector2 textureCutoff = ( Vector2.left + Vector2.up ).normalized;

            for( int i = 0; i < o.contours.Count; i++ )
            {
                List< Vector2 > contour = o.contours[ i ];
                if ( contour.Count < 2 ) { continue; }

                int vstart = verts;

                // Repeat segments
                for( int p = 0; p < contour.Count - 1; p++ )
                {
                    Vector2 a = contour[ p ];
                    Vector2 b = contour[ p + 1 ];

                    Vector2 ab = b - a;
                    Vector2 n  = new Vector2( -ab.y, ab.x ).normalized;

                    Vector3 p1 = a - n * inset;
                    Vector3 p2 = a + n * outset;
                    Vector3 p3 = b - n * inset;
                    Vector3 p4 = b + n * outset;

                    p1.z = zbias;
                    p2.z = zbias;
                    p3.z = zbias;
                    p4.z = zbias;

                    if ( p > 0 ) // Instead of proper mitering smooth joints by averaging vertices
                    {
                        p1 = ( o.contourVerts[ verts - 2 ] + p1 ) / 2.0f;
                        p2 = ( o.contourVerts[ verts - 1 ] + p2 ) / 2.0f;

                        o.contourVerts[ verts - 2 ] = p1;
                        o.contourVerts[ verts - 1 ] = p2;
                    }

                    o.contourVerts.Add( p1 );
                    o.contourVerts.Add( p2 );
                    o.contourVerts.Add( p3 );
                    o.contourVerts.Add( p4 );

                    o.contourTris.Add( verts     );
                    o.contourTris.Add( verts + 1 );
                    o.contourTris.Add( verts + 2 );

                    o.contourTris.Add( verts + 2 );
                    o.contourTris.Add( verts + 1 );
                    o.contourTris.Add( verts + 3 );

                    verts += 4;

                    float angle = Vector2.SignedAngle( textureCutoff, ab ) + 180.0f;
                    if ( angle >= 360.0f ){ angle -= 360.0f; }
                    int face = Mathf.FloorToInt( angle / 90 );

                    float u0 = ( face == 0 || face == 2 ) ? a.x / m_settings.voxelSize : a.y / m_settings.voxelSize;
                    float u1 = ( face == 0 || face == 2 ) ? b.x / m_settings.voxelSize : b.y / m_settings.voxelSize;
                    float v0 = face * 0.25f;
                    float v1 = ( face + 1 ) * 0.25f;

                    o.contourUVs.Add( new Vector2( u0, v0 ) );
                    o.contourUVs.Add( new Vector2( u0, v1 ) );
                    o.contourUVs.Add( new Vector2( u1, v0 ) );
                    o.contourUVs.Add( new Vector2( u1, v1 ) );
                }

                Vector2 last  = contour[ contour.Count - 1 ];
                Vector2 first = contour[ 0 ];

                // If contour line loops average start/end vertices
                if ( Mathf.Approximately( first.x, last.x ) && Mathf.Approximately( first.y, last.y ) )
                {
                    Vector3 avg1 = ( o.contourVerts[ vstart ] + o.contourVerts[ verts - 2 ] ) / 2.0f;
                    Vector3 avg2 = ( o.contourVerts[ vstart + 1 ] + o.contourVerts[ verts - 1 ] ) / 2.0f;

                    o.contourVerts[ vstart ] = avg1;
                    o.contourVerts[ verts - 2 ] = avg1;

                    o.contourVerts[ vstart + 1 ] = avg2;
                    o.contourVerts[ verts - 1 ] = avg2;
                }
            }
        }


        private void BuildCollision( MeshOutput o )
        {
            float extent = m_settings.collisionExtrudeExtent;
            int   verts  = 0;

            for( int i = 0; i < o.contours.Count; i++ )
            {
                List< Vector2 > ctr = o.contours[ i ];
                
                Vector2 prev = ctr[ 0 ];
                for( int p = 1; p < ctr.Count; p++ )
                {
                    Vector2 next = ctr[ p ];

                    Vector3 afar  = new Vector3( prev.x, prev.y, extent );
                    Vector3 anear = new Vector3( prev.x, prev.y, -extent );

                    Vector3 bfar  = new Vector3( next.x, next.y, extent );
                    Vector3 bnear = new Vector3( next.x, next.y, -extent );

                    o.collisionVerts.Add( anear );
                    o.collisionVerts.Add( afar );
                    o.collisionVerts.Add( bnear );
                    o.collisionVerts.Add( bfar );

                    o.collisionTris.Add( verts );
                    o.collisionTris.Add( verts + 1 );
                    o.collisionTris.Add( verts + 2 );

                    o.collisionTris.Add( verts + 2 );
                    o.collisionTris.Add( verts + 1 );
                    o.collisionTris.Add( verts + 3 );

                    verts += 4;
                    prev = next;
                }
            }
        }

        #if UNITY_EDITOR
        // Contour debug view
        static List< Color > debugContourColor;

        void OnDrawGizmosSelected()
        {
            if ( m_debugContour )
            {
                if ( debugContourColor == null ) { debugContourColor = new List<Color>(); }

                for ( int i = 0; i < m_meshOut.contours.Count; i++ )
                {
                    if ( i >= debugContourColor.Count ) { debugContourColor.Add( UnityEngine.Random.ColorHSV( 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f ) ); }

                    Gizmos.color = debugContourColor[ i ];
                    List< Vector2 > ctr = m_meshOut.contours[ i ];

                    for ( int p = 0; p < ctr.Count - 1; p++ )
                    {
                        Vector2 a = transform.TransformPoint( ctr[ p ] );
                        Vector2 b = transform.TransformPoint( ctr[ p + 1 ] );

                        Gizmos.DrawLine( a, b );


                        if ( m_debugContourNormals )
                        {
                            Vector2 ab = b - a;
                            Vector2 c  = a + ab / 2.0f;
                            Vector2 n  = new Vector2( -ab.y, ab.x );
                            Vector2 d  = c + n.normalized * m_settings.voxelSize * 0.5f;

                            Gizmos.DrawLine( c, d );

                            if ( p == 0 )
                            {
                                Gizmos.DrawWireSphere( d, 0.1f );
                            }
                        }
                    }
                }
            }
        }
        #endif
    }
}