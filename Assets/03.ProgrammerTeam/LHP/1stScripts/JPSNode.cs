using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace JPS
{

    public enum JPSDir
    {
        Up, Down, Left, Right,
        UpRight, DownRight,
        UpLeft, DownLeft,
        None
    }

    public class JPSNode
    {
        public JPSNode parent;
        public Vector2Int pos;
        public JPSDir dir;
        private int cost;
        private int heuri;


        public JPSNode( JPSNode parent, Vector2Int pos, JPSDir dir, int cost, int huri )
        {
            this.parent = parent;
            this.pos = pos;
            this.dir = dir;
            this.cost = cost;
            this.heuri = huri;
        }

        public int GetPassedCost()
        {
            return cost;
        }
        public int GetExpectedCost()
        {
            return cost + heuri;
        }


        public override bool Equals( object obj )
        {
            return obj is JPSNode n && n.pos.x == this.pos.x && n.pos.y == this.pos.y;
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(pos.x, pos.y);
        }

    }

}