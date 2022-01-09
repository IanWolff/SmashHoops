using System;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Represents the current vital statistics of some game entity.
    /// </summary>
    public class Percent : MonoBehaviour
    {
        /// <summary>
        /// The maximum hit points for the entity.
        /// </summary>
        public int maxPT = 999;

        int currentPT;

        /// <summary>
        /// Increment the HP of the entity.
        /// </summary>
        public void Increment(int damage)
        {
            currentPT = Mathf.Clamp(currentPT + damage, 0, maxPT);
        }

        /// <summary>
        /// Decrement the HP of the entity.
        /// </summary>
        public void Decrement(int heal)
        {
            currentPT = Mathf.Clamp(currentPT - heal, 0, maxPT);
        }

        /// <summary>
        /// Reset the HP of the entity.
        /// </summary>
        public void Reset()
        {
            currentPT = 0;
        }

        void Awake()
        {
            currentPT = 0;
        }
    }
}
