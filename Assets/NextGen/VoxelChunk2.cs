using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace VoxelTerrain2D
{
    public class VoxelChunk2 : MonoBehaviour
    {
        public static VoxelChunk2 Create( Transform parent, string name, GeneratorSettings settings, int maxWidth, int maxHeight )
        {
            GameObject  go    = new GameObject( name );
            go.transform.SetParent( parent, false );

            VoxelChunk2 chunk = go.AddComponent< VoxelChunk2 >();
            chunk.Initialize( settings, maxWidth, maxHeight );

            return chunk;
        }


        #if UNITY_EDITOR
        [SerializeField]
        private bool m_debugContour = default( bool );

        [SerializeField]
        private bool m_debugContourNormals = default( bool );
        #endif

        protected class MeshOutput
        {
            public List< Vector3 >          verts;
            public List< Vector3 >          normals;
            public List< int >              tris;
            public List< Vector2 >          uvs;
            public bool[]                   batched;

            public List< Vector3 >          contourVerts;
            public List< Vector3 >          contourNormals;
            public List< int >              contourTris;
            public List< Vector2 >          contourUVs;

            public List< List < Vector2 > > contours;

            public List< Vector3 >          collisionVerts;
            public List< int >              collisionTris;
        }

        public    Vector3           nextPosition { get; set; }

        protected GeneratorSettings m_settings;

        protected Mesh              m_mesh;
        protected MeshRenderer      m_renderer;

        protected Mesh              m_meshContour;
        protected MeshRenderer      m_rendererContour;

        protected Mesh              m_meshCollision;
        protected MeshCollider      m_collider;

        protected MeshOutput        m_meshOut;
        protected Vector3           m_worldPos;

        private Task                m_rebuildTask;
        private int                 m_maxWidth;
        private int                 m_maxHeight;


        public void Initialize( GeneratorSettings settings, int maxWidth, int maxHeight )
        {
            nextPosition = transform.position;

            m_maxWidth  = maxWidth;
            m_maxHeight = maxHeight;
            m_settings  = settings;

            // Initialize mesh
            m_mesh      = new Mesh();
            m_mesh.name = name + "_Mesh";
            m_mesh.MarkDynamic();

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

            // Initialize chunk data
            m_meshOut       = new MeshOutput();
            m_meshOut.verts = new List<Vector3>();
            m_meshOut.tris  = new List<int>();
            m_meshOut.uvs   = new List<Vector2>();

            if ( m_settings.generateNormals )
            {
                m_meshOut.normals = new List<Vector3>();
            }

            // Initialize temp buffers
            m_meshOut.batched  = new bool[ m_maxWidth * m_maxHeight ];
            m_meshOut.contours = new List<List<Vector2>>();

            // Contour buffers
            if ( m_settings.meshContour )
            {
                m_meshOut.contourVerts  = new List<Vector3>();
                m_meshOut.contourTris   = new List<int>();
                m_meshOut.contourUVs    = new List<Vector2>();

                if ( m_settings.generateNormals ){ m_meshOut.contourNormals = new List<Vector3>(); }
            }

            // Collision buffers
            if ( m_settings.generateCollision )
            {
                m_meshOut.collisionVerts = new List<Vector3>();
                m_meshOut.collisionTris = new List<int>();
            }
        }


        public void Rebuild( IReadableDataset< VoxelData > data, IntRect region )
        {
            if ( region.size.x <= 0 || region.size.y <= 0 )
            {
                ClearBuffers();
                AssignMeshData();
                transform.position = nextPosition;
                return;
            }

            m_rebuildTask = new Task(()=>{ 
                ClearBuffers();
                GenerateMesh( data, region, m_meshOut );
            });
            m_rebuildTask.Start();
        }


        void Update()
        {
            if ( m_rebuildTask != null )
            {
                m_rebuildTask.Wait();
                m_rebuildTask = null;

                AssignMeshData();
                transform.position = nextPosition;
            }
        }


        private void ClearBuffers()
        {
            m_meshOut.verts.Clear();
            m_meshOut.tris.Clear();
            m_meshOut.uvs.Clear();

            if ( m_settings.generateNormals )
            {
                m_meshOut.normals.Clear();
            }

            m_meshOut.contours.Clear();

            for ( int i = 0; i < m_meshOut.batched.Length; i++ ) { m_meshOut.batched[ i ] = false; }

            if ( m_settings.meshContour )
            {
                m_meshOut.contourVerts.Clear();
                m_meshOut.contourTris.Clear();
                m_meshOut.contourUVs.Clear();

                if ( m_settings.generateNormals ) { m_meshOut.contourNormals.Clear(); }
            }

            if ( m_settings.generateCollision )
            {
                m_meshOut.collisionVerts.Clear();
                m_meshOut.collisionTris.Clear();
            }
        }


        private void AssignMeshData()
        {
            m_mesh.Clear( true );
            m_mesh.SetVertices( m_meshOut.verts );
            m_mesh.SetTriangles( m_meshOut.tris, 0, false );
            m_mesh.SetUVs( 0, m_meshOut.uvs );

            if ( m_settings.generateNormals )
            {
                m_mesh.SetNormals( m_meshOut.normals );
            }

            if ( m_settings.meshContour )
            {
                m_meshContour.Clear( true );
                m_meshContour.SetVertices( m_meshOut.contourVerts );
                m_meshContour.SetTriangles( m_meshOut.contourTris, 0, false );
                m_meshContour.SetUVs( 0, m_meshOut.contourUVs );

                if ( m_settings.generateNormals )
                {
                    m_meshContour.SetNormals( m_meshOut.contourNormals );
                }
            }

            if ( m_settings.generateCollision )
            {
                m_meshCollision.Clear( true );
                m_meshCollision.SetVertices( m_meshOut.collisionVerts );
                m_meshCollision.SetTriangles( m_meshOut.collisionTris, 0, false );

                m_meshCollision.RecalculateBounds();

                m_collider.sharedMesh = null;
                m_collider.sharedMesh = m_meshCollision;
            }
        }


        private void GenerateMesh( IReadableDataset< VoxelData > data, IntRect region, MeshOutput o )
        {
            float voxelSize = m_settings.voxelSize;

            for( int y = 0; y < region.size.y - 1; y++ )
            {
                for( int x = 0; x < region.size.x - 1; x++ )
                {
                    if ( o.batched[ y * m_maxWidth + x ] == true ) { continue; } // Cell is already a part of existing batch, skip it...

                    VoxelData bottomLeft  = data.Sample( region.origin.x + x, region.origin.y + y );          // v = 1
                    VoxelData bottomRight = data.Sample( region.origin.x + x + 1, region.origin.y + y );      // v = 2
                    VoxelData topRight    = data.Sample( region.origin.x + x + 1, region.origin.y + y + 1 );  // v = 4
                    VoxelData topLeft     = data.Sample( region.origin.x + x, region.origin.y + y + 1 );      // v = 8

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v4 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v5 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v4 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v4 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v5 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v4 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v4 ) / m_settings.voxelSize );

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

                            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );
                            o.uvs.Add( ( m_worldPos + v4 ) / m_settings.voxelSize );

                            AppendContour( v4, v0, o );
                        }
                        break;

                        case 15:
                        {
                            if ( topLeft.CompareWithoutExtent( bottomLeft ) && topRight.CompareWithoutExtent( bottomRight ) && bottomLeft.CompareWithoutExtent( bottomRight ) )
                            {
                                GenerateGreedyQuad( data, region, x, y, o );
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

                                o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
                                o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
                                o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
                                o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );
                            }
                        }
                        break;
                    }
                }
            }

            CombineContours( o );

            if ( m_settings.generateCollision ){ BuildCollision( o ); }
            if ( m_settings.meshContour ){ BuildContourMesh( o ); }
            
            if ( m_settings.generateNormals )
            {
                for( int v = 0; v < o.verts.Count; v++ )
                {
                    o.normals.Add( Vector3.back );
                }
            }
        }


        private void GenerateGreedyQuad( IReadableDataset< VoxelData > data, IntRect region, int x, int y, MeshOutput o )
        {
            float voxelSize = m_settings.voxelSize;

            int startX = x;
            int startY = y;
            int endX   = x;
            int endY   = y;

            VoxelData compare = data.Sample( region.origin.x + x, region.origin.y + y );

            for( int xx = x + 1; xx < region.size.x - 1; xx++ )
            {
                if ( o.batched[ y * m_maxWidth + xx ] == true ) { break; } // Encountered another batch, stop expanding

                VoxelData a = data.Sample( region.origin.x + xx + 1, region.origin.y + y );
                VoxelData b = data.Sample( region.origin.x + xx + 1, region.origin.y + y + 1 );

                if ( compare.CompareWithoutExtent( a ) && compare.CompareWithoutExtent( b ) )
                {
                    endX = xx;
                }
                else{ break; } // Change in voxel data detected, stop extending
            }

            bool validUp = true;
            for( int yy = y + 1; yy < region.size.y - 1 && validUp == true; yy++ )
            {
                for( int xx = startX; xx <= endX && validUp == true; xx++ )
                {
                    if ( o.batched[ yy * m_maxWidth + xx ] == true ) { validUp = false; break; } // Encountered another batch, stop expanding

                    VoxelData a = data.Sample( region.origin.x + xx + 1, region.origin.y + yy + 1 );
                    VoxelData b = data.Sample( region.origin.x + xx, region.origin.y + yy + 1 );

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
                    o.batched[ yy * m_maxWidth + xx ] = true;
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

            o.uvs.Add( ( m_worldPos + v0 ) / m_settings.voxelSize );
            o.uvs.Add( ( m_worldPos + v1 ) / m_settings.voxelSize );
            o.uvs.Add( ( m_worldPos + v2 ) / m_settings.voxelSize );
            o.uvs.Add( ( m_worldPos + v3 ) / m_settings.voxelSize );
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

            if ( m_settings.generateNormals )
            {
                for( int v = 0; v < o.contourVerts.Count; v++ )
                {
                    o.contourNormals.Add( Vector3.back );
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