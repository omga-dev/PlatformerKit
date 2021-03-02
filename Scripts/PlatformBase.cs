using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace JaeminPark.PlatformerKit
{
    public enum Direction { Left, Right, Down, Up }

    public abstract class PlatformBase : MonoBehaviour
    {
        public virtual void OnBodyEnter(PlatformerBody body, Direction direction) { }
        public virtual void OnBodyStay(PlatformerBody body, Direction direction) { }
        public virtual void OnBodyExit(PlatformerBody body, Direction direction) { }
    }
}
