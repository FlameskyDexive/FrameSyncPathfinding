﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace RVO2
{
    [System.Serializable]
    public class KdtreeObstacle  {

        public int nextID;
        public int previousID;
        public KInt2 direction_;
        public KInt2 point_;
        public int id_;
        public bool convex_;
    } 
}




