using System;
using UnityEngine;

namespace Delaunay
{
    public class Vertex : IComparable<Vertex>
    {
        public readonly int x;
        public readonly int y;

        public Vertex( int x, int y )
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals( object obj )
        {
            return obj is Vertex v && v.x == x && v.y == y;
        }

        public override string ToString()
        {
            return "( " + x + ", " + y + ")";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public int CompareTo( Vertex other )
        {
            if ( x < other.x ) return -1;
            if ( x > other.x ) return 1;
            if ( y < other.y ) return -1;
            if ( y > other.y ) return 1;
            return 0;
        }

        public static bool operator <( Vertex lv, Vertex rv )
        {
            return ( lv.x < rv.x ) || ( ( lv.x == rv.x ) && ( lv.y < rv.y ) );
        }

        public static bool operator >( Vertex lv, Vertex rv )
        {
            return ( lv.x > rv.x ) || ( ( lv.x == rv.x ) && ( lv.y > rv.y ) );
        }
    }
}
