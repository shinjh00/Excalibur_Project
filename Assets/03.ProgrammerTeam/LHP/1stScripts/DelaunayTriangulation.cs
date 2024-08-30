using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public class DelaunayTriangulation : MonoBehaviour
    {
        private static Triangle CalcSuperTriangle( IEnumerable<Vertex> vertices )
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach ( var v in vertices )
            {
                minX = Mathf.Min(minX, v.x);
                maxX = Mathf.Max(maxX, v.x);
                minY = Mathf.Min(minY, v.y);
                maxY = Mathf.Max(maxY, v.y);
            }

            int dx = ( maxX - minX + 1 );

            Vertex v1 = new Vertex(( minX - dx - 1 ) * 2, minY - 1 - ( maxY + ( maxY - minY ) + 1 ));
            Vertex v2 = new Vertex(( minX + maxX ) / 2, ( maxY + ( maxY - minY ) + 1 ) * 2);
            Vertex v3 = new Vertex(( maxX + dx + 1 ) * 2, minY - 1 - ( maxY + ( maxY - minY ) + 1 ));

            return new Triangle(v1, v2, v3);
        }

        public static HashSet<Triangle> Triangulate( IEnumerable<Vertex> vertices )
        {
            Triangle superTriangle = CalcSuperTriangle(vertices);
            HashSet<Triangle> triangulation = new HashSet<Triangle>
            {
                superTriangle
            };

            foreach ( var vertex in vertices )
            {
                HashSet<Triangle> badTriangles = new HashSet<Triangle>();
                foreach ( var triangle in triangulation )
                {
                    if ( triangle.IsInCircumCircle(vertex) )
                        badTriangles.Add(triangle);
                }

                HashSet<Edge> polygon = new HashSet<Edge>();
                foreach ( var badTriangle in badTriangles )
                {
                    foreach ( var edge in badTriangle.edges )
                    {
                        bool isShared = false;
                        foreach ( var otherTriangle in badTriangles )
                        {
                            if ( badTriangle == otherTriangle )
                                continue;
                            if ( otherTriangle.HasEdge(edge) )
                            {
                                isShared = true;
                            }
                        }
                        if ( !isShared )
                            polygon.Add(edge);
                    }
                }

                triangulation.ExceptWith(badTriangles);

                foreach ( var edge in polygon )
                {
                    triangulation.Add(new Triangle(vertex, edge.a, edge.b));
                }
            }

            triangulation.RemoveWhere(( Triangle t ) => t.HasSameVertex(superTriangle));

            return triangulation;
        }
    }
}
