using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RVO2
{
    public class LoadKdtree : MonoBehaviour {

        public KdtreeAsset asset;

        protected void Start()
        {
            this.Load(asset);
        }

        public void Load(KdtreeAsset treeasset)
        {
            if(treeasset != null)
            {
                float time = Time.realtimeSinceStartup;
                Simulator.Instance.CreateKdtreeFromAsset(treeasset);
               
                this.asset = treeasset;
            }
        }



    } 
}


