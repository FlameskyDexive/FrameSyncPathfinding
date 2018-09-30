using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pathfinding
{
    public class GSeeker
    {
        public bool drawGizmos = true;
        public bool detailedGizmos;
        [HideInInspector] public int traversableTags = -1;
        [HideInInspector] public int[] tagPenalties = new int[32];
        [HideInInspector] public int graphMask = -1;

        /** Callback for when a path is completed.
         * Movement scripts should register to this delegate.\n
         * A temporary callback can also be set when calling StartPath, but that delegate will only be called for that path
         */
        public OnPathDelegate pathCallback;

        /** Called before pathfinding is started */
        public OnPathDelegate preProcessPath;

        /** Called after a path has been calculated, right before modifiers are executed.
         */
        public OnPathDelegate postProcessPath;

        /** Used for drawing gizmos */
        //Good Game
        [System.NonSerialized]
        //List<Vector3> lastCompletedVectorPath;
        List<VInt3> lastCompletedVectorPath;

        /** Used for drawing gizmos */
        [System.NonSerialized] List<GraphNode> lastCompletedNodePath;

        /** The current path */
        [System.NonSerialized] protected Path path;

        /** Previous path. Used to draw gizmos */
        [System.NonSerialized] private Path prevPath;

        /** Cached delegate to avoid allocating one every time a path is started */
        private readonly OnPathDelegate onPathDelegate;

        /** Cached delegate to avoid allocating one every time a path is started */
        private readonly OnPathDelegate onPartialPathDelegate;

        /** Temporary callback only called for the current path. This value is set by the StartPath functions */
        private OnPathDelegate tmpPathCallback;

        /** The path ID of the last path queried */
        protected uint lastPathID;

        /** Internal list of all modifiers */
        readonly List<IPathModifier> modifiers = new List<IPathModifier>();

        public enum ModifierPass
        {
            PreProcess,

            // An obsolete item occupied index 1 previously
            PostProcess = 2,
        }

        public GSeeker()
        {
            onPathDelegate = OnPathComplete;
            onPartialPathDelegate = OnPartialPathComplete;
        }
        

        /** Path that is currently being calculated or was last calculated.
		 * You should rarely have to use this. Instead get the path when the path callback is called.
		 *
		 * \see pathCallback
		 */
        public Path GetCurrentPath()
        {
            return path;
        }

        /** Stop calculating the current path request.
		 * If this Seeker is currently calculating a path it will be canceled.
		 * The callback (usually to a method named OnPathComplete) will soon be called
		 * with a path that has the 'error' field set to true.
		 *
		 * This does not stop the character from moving, it just aborts
		 * the path calculation.
		 *
		 * \param pool If true then the path will be pooled when the pathfinding system is done with it.
		 */
        public void CancelCurrentPathRequest(bool pool = true)
        {
            if (!IsDone())
            {
                path.FailWithError("Canceled by script (Seeker.CancelCurrentPathRequest)");
                if (pool)
                {
                    // Make sure the path has had its reference count incremented and decremented once.
                    // If this is not done the system will think no pooling is used at all and will not pool the path.
                    // The particular object that is used as the parameter (in this case 'path') doesn't matter at all
                    // it just has to be *some* object.
                    path.Claim(path);
                    path.Release(path);
                }
            }
        }

        /** Cleans up some variables.
		 * Releases any eventually claimed paths.
		 * Calls OnDestroy on the #startEndModifier.
		 *
		 * \see ReleaseClaimedPath
		 * \see startEndModifier
		 */
        public void OnDestroy()
        {
            ReleaseClaimedPath();
            //startEndModifier.OnDestroy(this);
            //if (seeker != null)
            //{
            //DeregisterModifier(this);
            //}
        }

        /** Releases the path used for gizmos (if any).
		 * The seeker keeps the latest path claimed so it can draw gizmos.
		 * In some cases this might not be desireable and you want it released.
		 * In that case, you can call this method to release it (not that path gizmos will then not be drawn).
		 *
		 * If you didn't understand anything from the description above, you probably don't need to use this method.
		 *
		 * \see \ref pooling
		 */
        public void ReleaseClaimedPath()
        {
            if (prevPath != null)
            {
                prevPath.Release(this, true);
                prevPath = null;
            }
        }
        

        /** Post Processes the path.
		 * This will run any modifiers attached to this GameObject on the path.
		 * This is identical to calling RunModifiers(ModifierPass.PostProcess, path)
		 * \see RunModifiers
		 * \since Added in 3.2
		 */
        public void PostProcess(Path path)
        {
            RunModifiers(ModifierPass.PostProcess, path);
        }

        /** Runs modifiers on a path */
        public void RunModifiers(ModifierPass pass, Path path)
        {
            if (pass == ModifierPass.PreProcess)
            {
                if (preProcessPath != null) preProcessPath(path);

                //for (int i = 0; i < modifiers.Count; i++) modifiers[i].PreProcess(path);
            }
            else if (pass == ModifierPass.PostProcess)
            {
                Profiler.BeginSample("Running Path Modifiers");
                // Call delegates if they exist
                if (postProcessPath != null) postProcessPath(path);

                // Loop through all modifiers and apply post processing
                //for (int i = 0; i < modifiers.Count; i++) modifiers[i].Apply(path);
                ApplyFunnelModifier(path);
                Profiler.EndSample();
            }
        }

        /** Is the current path done calculating.
		 * Returns true if the current #path has been returned or if the #path is null.
		 *
		 * \note Do not confuse this with Pathfinding.Path.IsDone. They usually return the same value, but not always
		 * since the path might be completely calculated, but it has not yet been processed by the Seeker.
		 *
		 * \since Added in 3.0.8
		 * \version Behaviour changed in 3.2
		 */
        public bool IsDone()
        {
            return path == null || path.PipelineState >= PathState.Returned;
        }

        /** Called when a path has completed.
		 * This should have been implemented as optional parameter values, but that didn't seem to work very well with delegates (the values weren't the default ones)
		 * \see OnPathComplete(Path,bool,bool)
		 */
        void OnPathComplete(Path path)
        {
            OnPathComplete(path, true, true);
        }

        /** Called when a path has completed.
		 * Will post process it and return it by calling #tmpPathCallback and #pathCallback
		 */
        void OnPathComplete(Path p, bool runModifiers, bool sendCallbacks)
        {
            if (p != null && p != path && sendCallbacks)
            {
                return;
            }

            if (this == null || p == null || p != path)
                return;

            if (!path.error && runModifiers)
            {
                // This will send the path for post processing to modifiers attached to this Seeker
                RunModifiers(ModifierPass.PostProcess, path);
            }

            if (sendCallbacks)
            {
                p.Claim(this);

                lastCompletedNodePath = p.path;
                lastCompletedVectorPath = p.vectorPath;

                // This will send the path to the callback (if any) specified when calling StartPath
                if (tmpPathCallback != null)
                {
                    tmpPathCallback(p);
                }

                // This will send the path to any script which has registered to the callback
                if (pathCallback != null)
                {
                    pathCallback(p);
                }

                // Recycle the previous path to reduce the load on the GC
                if (prevPath != null)
                {
                    prevPath.Release(this, true);
                }

                prevPath = p;

                // If not drawing gizmos, then storing prevPath is quite unecessary
                // So clear it and set prevPath to null
                if (!drawGizmos) ReleaseClaimedPath();
            }
        }

        /** Called for each path in a MultiTargetPath.
		 * Only post processes the path, does not return it.
		 * \astarpro */
        void OnPartialPathComplete(Path p)
        {
            OnPathComplete(p, true, false);
        }

        /** Call this function to start calculating a path.
		 * Since this method does not take a callback parameter, you should set the #pathCallback field before calling this method.
		 *
		 * \param start		The start point of the path
		 * \param end		The end point of the path
		 */
        //Good Game
        //public Path StartPath (Vector3 start, Vector3 end) {
        public Path StartPath(VInt3 start, VInt3 end)
        {
            return StartPath(start, end, null);
        }

        /** Call this function to start calculating a path.
		 *
		 * \param start		The start point of the path
		 * \param end		The end point of the path
		 * \param callback	The function to call when the path has been calculated
		 *
		 * \a callback will be called when the path has completed.
		 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed) */
        //Good Game
        //public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback) {
        public Path StartPath(VInt3 start, VInt3 end, OnPathDelegate callback)
        {
            return StartPath(ABPath.Construct(start, end, null), callback);
        }

        /** Call this function to start calculating a path.
		 *
		 * \param start		The start point of the path
		 * \param end		The end point of the path
		 * \param callback	The function to call when the path has been calculated
		 * \param graphMask	Mask used to specify which graphs should be searched for close nodes. See #Pathfinding.NNConstraint.graphMask. This will override #graphMask for this path request.
		 *
		 * \a callback will be called when the path has completed.
		 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed) */
        //Good Game
        //public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback, int graphMask) {
        public Path StartPath(VInt3 start, VInt3 end, OnPathDelegate callback, int graphMask)
        {
            return StartPath(ABPath.Construct(start, end, null), callback, graphMask);
        }

        /** Call this function to start calculating a path.
		 *
		 * \param p			The path to start calculating
		 * \param callback	The function to call when the path has been calculated
		 *
		 * The \a callback will be called when the path has been calculated (which may be several frames into the future).
		 * The \a callback will not be called if a new path request is started before this path request has been calculated.
		 *
		 * \version Since 3.8.3 this method works properly if a MultiTargetPath is used.
		 * It now behaves identically to the StartMultiTargetPath(MultiTargetPath) method.
		 *
		 * \version Since 4.1.x this method will no longer overwrite the graphMask on the path unless it is explicitly passed as a parameter (see other overloads of this method).
		 */
        public Path StartPath(Path p, OnPathDelegate callback = null)
        {
            // Set the graph mask only if the user has not changed it from the default value.
            // This is not perfect as the user may have wanted it to be precisely -1
            // however it is the best detection that I can do.
            // The non-default check is primarily for compatibility reasons to avoid breaking peoples existing code.
            // The StartPath overloads with an explicit graphMask field should be used instead to set the graphMask.
            if (p.nnConstraint.graphMask == -1) p.nnConstraint.graphMask = graphMask;
            StartPathInternal(p, callback);
            return p;
        }

        /** Call this function to start calculating a path.
		 *
		 * \param p			The path to start calculating
		 * \param callback	The function to call when the path has been calculated
		 * \param graphMask	Mask used to specify which graphs should be searched for close nodes. See #Pathfinding.NNConstraint.graphMask. This will override #graphMask for this path request.
		 *
		 * The \a callback will be called when the path has been calculated (which may be several frames into the future).
		 * The \a callback will not be called if a new path request is started before this path request has been calculated.
		 *
		 * \version Since 3.8.3 this method works properly if a MultiTargetPath is used.
		 * It now behaves identically to the StartMultiTargetPath(MultiTargetPath) method.
		 */
        public Path StartPath(Path p, OnPathDelegate callback, int graphMask)
        {
            p.nnConstraint.graphMask = graphMask;
            StartPathInternal(p, callback);
            return p;
        }

        /** Internal method to start a path and mark it as the currently active path */
        void StartPathInternal(Path p, OnPathDelegate callback)
        {
            var mtp = p as MultiTargetPath;

            if (mtp != null)
            {
                // TODO: Allocation, cache
                /*var callbacks = new OnPathDelegate[mtp.targetPoints.Length];

                for (int i = 0; i < callbacks.Length; i++)
                {
                    callbacks[i] = onPartialPathDelegate;
                }

                mtp.callbacks = callbacks;
                p.callback += OnMultiPathComplete;*/
            }
            else
            {
                p.callback += onPathDelegate;
            }

            p.enabledTags = traversableTags;
            p.tagPenalties = tagPenalties;

            // Cancel a previously requested path is it has not been processed yet and also make sure that it has not been recycled and used somewhere else
            if (path != null && path.PipelineState <= PathState.Processing && path.CompleteState != PathCompleteState.Error && lastPathID == path.pathID)
            {
                path.FailWithError("Canceled path because a new one was requested.\n" +
                    "This happens when a new path is requested from the seeker when one was already being calculated.\n" +
                    "For example if a unit got a new order, you might request a new path directly instead of waiting for the now" +
                    " invalid path to be calculated. Which is probably what you want.\n" +
                    "If you are getting this a lot, you might want to consider how you are scheduling path requests.");
                // No callback will be sent for the canceled path
            }

            // Set p as the active path
            path = p;
            tmpPathCallback = callback;

            // Save the path id so we can make sure that if we cancel a path (see above) it should not have been recycled yet.
            lastPathID = path.pathID;

            // Pre process the path
            RunModifiers(ModifierPass.PreProcess, path);

            // Send the request to the pathfinder
            AstarPath.StartPath(path);
        }

        public void ApplyFunnelModifier(Path p)
        {
            List<GraphNode> path = p.path;
            List<VInt3> vectorPath = p.vectorPath;
            if (((path != null) && (path.Count != 0)) && ((vectorPath != null) && (vectorPath.Count == path.Count)))
            {
                List<VInt3> funnelPath = ListPool<VInt3>.Claim();
                List<VInt3> left = ListPool<VInt3>.Claim(path.Count + 1);
                List<VInt3> right = ListPool<VInt3>.Claim(path.Count + 1);
                left.Add(vectorPath[0]);
                right.Add(vectorPath[0]);
                for (int i = 0; i < (path.Count - 1); i++)
                {
                    bool flag = path[i].GetPortal(path[i + 1], left, right, false);
                    bool flag2 = false;
                    if (!flag && !flag2)
                    {
                        left.Add(path[i].position);
                        right.Add(path[i].position);
                        left.Add(path[i + 1].position);
                        right.Add(path[i + 1].position);
                    }
                }
                left.Add(vectorPath[vectorPath.Count - 1]);
                right.Add(vectorPath[vectorPath.Count - 1]);
                if (!this.RunFunnel(left, right, funnelPath))
                {
                    funnelPath.Add(vectorPath[0]);
                    funnelPath.Add(vectorPath[vectorPath.Count - 1]);
                }
                ListPool<VInt3>.Release(p.vectorPath);
                p.vectorPath = funnelPath;
                //PositionsLog(funnelPath);
                ListPool<VInt3>.Release(left);
                ListPool<VInt3>.Release(right);
            }
        }

        

        public bool RunFunnel(List<VInt3> left, List<VInt3> right, List<VInt3> funnelPath)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }
            if (funnelPath == null)
            {
                throw new ArgumentNullException("funnelPath");
            }
            if (left.Count != right.Count)
            {
                throw new ArgumentException("left and right lists must have equal length");
            }
            if (left.Count <= 3)
            {
                return false;
            }
            while ((left[1] == left[2]) && (right[1] == right[2]))
            {
                left.RemoveAt(1);
                right.RemoveAt(1);
                if (left.Count <= 3)
                {
                    return false;
                }
            }
            VInt3 c = left[2];
            if (c == left[1])
            {
                c = right[2];
            }
            while (VectorMath.IsColinearXZ(left[0], left[1], right[1]) || (VectorMath.RightOrColinearXZ(left[1], right[1], c) == VectorMath.RightOrColinearXZ(left[1], right[1], left[0])))
            {
                left.RemoveAt(1);
                right.RemoveAt(1);
                if (left.Count <= 3)
                {
                    return false;
                }
                c = left[2];
                if (c == left[1])
                {
                    c = right[2];
                }
            }
            if (!VectorMath.IsClockwiseXZ(left[0], left[1], right[1]) && !VectorMath.IsColinearXZ(left[0], left[1], right[1]))
            {
                List<VInt3> list = left;
                left = right;
                right = list;
            }
            funnelPath.Add(left[0]);
            VInt3 a = left[0];
            VInt3 b = left[1];
            VInt3 num4 = right[1];
            int num5 = 0;
            int num6 = 1;
            int num7 = 1;
            for (int i = 2; i < left.Count; i++)
            {
                if (funnelPath.Count > 0x7d0)
                {
                    Debug.LogWarning("Avoiding infinite loop. Remove this check if you have this long paths.");
                    break;
                }
                VInt3 num9 = left[i];
                VInt3 num10 = right[i];
                if (VectorMath.SignedTriangleAreaTimes2XZ(a, num4, num10) >= 0L)
                {
                    if ((a == num4) || (VectorMath.SignedTriangleAreaTimes2XZ(a, b, num10) <= 0L))
                    {
                        num4 = num10;
                        num6 = i;
                    }
                    else
                    {
                        funnelPath.Add(b);
                        a = b;
                        num5 = num7;
                        b = a;
                        num4 = a;
                        num7 = num5;
                        num6 = num5;
                        i = num5;
                        continue;
                    }
                }
                if (VectorMath.SignedTriangleAreaTimes2XZ(a, b, num9) <= 0L)
                {
                    if ((a == b) || (VectorMath.SignedTriangleAreaTimes2XZ(a, num4, num9) >= 0L))
                    {
                        b = num9;
                        num7 = i;
                    }
                    else
                    {
                        funnelPath.Add(num4);
                        a = num4;
                        num5 = num6;
                        b = a;
                        num4 = a;
                        num7 = num5;
                        num6 = num5;
                        i = num5;
                    }
                }
            }
            funnelPath.Add(left[left.Count - 1]);
            return true;
        }

        public void OnDrawGizmos()
        {
            if (lastCompletedNodePath == null || !drawGizmos)
            {
                return;
            }

            if (detailedGizmos)
            {
                Gizmos.color = new Color(0.7F, 0.5F, 0.1F, 0.5F);

                if (lastCompletedNodePath != null)
                {
                    for (int i = 0; i < lastCompletedNodePath.Count - 1; i++)
                    {
                        Gizmos.DrawLine((Vector3)lastCompletedNodePath[i].position, (Vector3)lastCompletedNodePath[i + 1].position);
                    }
                }
            }

            Gizmos.color = new Color(0, 1F, 0, 1F);

            if (lastCompletedVectorPath != null)
            {
                for (int i = 0; i < lastCompletedVectorPath.Count - 1; i++)
                {
                    //Good Game
                    //Gizmos.DrawLine(lastCompletedVectorPath[i], lastCompletedVectorPath[i+1]);
                    Gizmos.DrawLine((Vector3)lastCompletedVectorPath[i], (Vector3)lastCompletedVectorPath[i + 1]);
                }
            }
        }
    }
}
