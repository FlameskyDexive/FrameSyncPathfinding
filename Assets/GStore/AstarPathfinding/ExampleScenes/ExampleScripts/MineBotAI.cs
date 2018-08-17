using UnityEngine;

namespace Pathfinding.Examples
{
    /** AI controller specifically made for the spider robot.
	 * \deprecated This script has been replaced by Pathfinding.Examples.MineBotAnimation. Any uses of this script in the Unity editor will be automatically replaced by one AIPath component and one MineBotAnimation component.
	 */
    [RequireComponent(typeof(Seeker))]
    [System.Obsolete("This script has been replaced by Pathfinding.Examples.MineBotAnimation. Any uses of this script in the Unity editor will be automatically replaced by one AIPath component and one MineBotAnimation component.")]
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_mine_bot_a_i.php")]
    public class MineBotAI : AIPath
    {
        /** Animation component.
		 * Should hold animations "awake" and "forward"
		 */
        public Animation anim;

        /** Minimum velocity for moving */
        public float sleepVelocity = 0.4F;

        /** Speed relative to velocity with which to play animations */
        public float animationSpeed = 0.2F;

        /** Effect which will be instantiated when end of path is reached.
		 * \see OnTargetReached */
        public GameObject endOfPathEffect;

#if UNITY_EDITOR
        protected override int OnUpgradeSerializedData(int version, bool unityThread)
        {
            if (unityThread)
            {
                var components = gameObject.GetComponents<Component>();
                int index = System.Array.IndexOf(components, this);
                foreach (System.Type newType in new[] { typeof(AIPath), typeof(MineBotAnimation) })
                {
                    var newComp = gameObject.AddComponent(newType);
                    foreach (var field in newComp.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
                    {
                        var oldField = this.GetType().GetField(field.Name);
                        if (oldField != null) field.SetValue(newComp, oldField.GetValue(this));
                    }
                    for (int i = components.Length - 1; i > index; i--) UnityEditorInternal.ComponentUtility.MoveComponentUp(newComp);
                }
                GameObject.DestroyImmediate(this);
                return 0;
            }
            else
            {
                return base.OnUpgradeSerializedData(version, unityThread);
            }
        }
#endif
    }
}
