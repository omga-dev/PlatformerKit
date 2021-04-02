using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace JaeminPark.PlatformerKit
{
    public enum Direction { Left, Right, Down, Up }

    public abstract class PlatformBase : MonoBehaviour
    {
        /// <summary>
        /// An event called when PlatformerBody has entered to the platform's collider.
        /// </summary>
        /// <param name="body">Detected PlatformerBody.</param>
        /// <param name="direction">Direction of the platform from the detected body. (e.g. Direction.Down when player is stepping on it.)</param>
        public virtual void OnBodyEnter(PlatformerBody body, Direction direction) { }

        /// <summary>
        /// An event called when PlatformerBody is staying on the platform's collider.
        /// </summary>
        /// <param name="body">Detected PlatformerBody.</param>
        /// <param name="direction">Direction of the platform from the detected body. (e.g. Direction.Down when player is stepping on it.)</param>
        public virtual void OnBodyStay(PlatformerBody body, Direction direction) { }

        /// <summary>
        /// An event called when PlatformerBody has exited from the platform's collider.
        /// </summary>
        /// <param name="body">Detected PlatformerBody.</param>
        /// <param name="direction">Direction of the platform from the detected body. (e.g. Direction.Down when player is stepping on it.)</param>
        public virtual void OnBodyExit(PlatformerBody body, Direction direction) { }
    }
}
