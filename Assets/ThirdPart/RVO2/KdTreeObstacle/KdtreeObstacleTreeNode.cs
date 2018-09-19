using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RVO2
{
    [System.Serializable]
    public class KdtreeObstacleTreeNode  {
        
        public int id;

        public int obstacleID;

        public int leftID;

        public int rightID;
        [NonSerialized]
        public KdtreeObstacle obstacle_;
        [NonSerialized]
        public KdtreeObstacleTreeNode left_;
        [NonSerialized]
        public KdtreeObstacleTreeNode right_;
    }
}




