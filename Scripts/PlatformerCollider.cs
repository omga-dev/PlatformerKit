using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JaeminPark.PlatformerKit
{
    public struct PlatformerHit
    {
        public bool hit { get; private set; }
        public float distance { get; private set; }
        public Vector2 normal { get; private set; }
        public GameObject gameObject { get; private set; }
        public readonly static PlatformerHit NoHit = new PlatformerHit(false, Mathf.Infinity, Vector2.zero, null);

        public PlatformerHit(bool hit, float distance, Vector2 normal, GameObject gameObject)
        {
            this.hit = hit;
            this.distance = distance;
            this.normal = normal;
            this.gameObject = gameObject;
        }
    }

    [AddComponentMenu("Platformer Kit/Platformer Collider")]
    public class PlatformerCollider : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _horizontalHitbox = new Vector2(1f, 0.75f);
        [SerializeField]
        private Vector2 _verticalHitbox = new Vector2(0.75f, 1f);
        [SerializeField]
        private float _platformCheckOffset = 0.05f;
        [SerializeField]
        private float _slopeCheckOffset = 0.05f;

        /// <summary>
        /// Hitbox on checking Y axis.
        /// </summary>
        public Vector2 horizontalHitbox { get { return _horizontalHitbox; } set { _horizontalHitbox = value; UpdateHBPosition(); } }

        /// <summary>
        /// Hitbox on checking Y axis.
        /// </summary>
        public Vector2 verticalHitbox { get { return _verticalHitbox; } set { _verticalHitbox = value; UpdateVBPosition(); } }

        /// <summary>
        /// Hitbox on checking Y axis.
        /// </summary>
        public float platformCheckOffset { get { return _platformCheckOffset; } set { _platformCheckOffset = value; UpdatePBPosition(); } }

        /// <summary>
        /// Hitbox on checking Y axis.
        /// </summary>
        public float slopeCheckOffset { get { return _slopeCheckOffset; } set { _slopeCheckOffset = value; } }

        private Vector2 hbDownLeft, hbLeftDown, hbDownRight, hbRightDown, hbUpLeft, hbLeftUp, hbUpRight, hbRightUp,
            vbDownLeft, vbLeftDown, vbDownRight, vbRightDown, vbUpLeft, vbLeftUp, vbUpRight, vbRightUp,
            pbLeftDown, pbRightDown;
        
        private Transform tf;

        private void Awake()
        {
            tf = transform;
        }

        private void Start()
        {
            UpdateHBPosition();
            UpdateVBPosition();
            UpdatePBPosition();
        }

        private void UpdateHBPosition()
        {
            hbDownLeft = _horizontalHitbox * new Vector2(-0.5f, -0.5f) - Vector2.down * 0.01f;
            hbLeftDown = _horizontalHitbox * new Vector2(-0.5f, -0.5f) - Vector2.left * 0.01f;
            hbDownRight = _horizontalHitbox * new Vector2(0.5f, -0.5f) - Vector2.down * 0.01f;
            hbRightDown = _horizontalHitbox * new Vector2(0.5f, -0.5f) - Vector2.right * 0.01f;
            hbUpLeft = _horizontalHitbox * new Vector2(-0.5f, 0.5f) - Vector2.up * 0.01f;
            hbLeftUp = _horizontalHitbox * new Vector2(-0.5f, 0.5f) - Vector2.left * 0.01f;
            hbUpRight = _horizontalHitbox * new Vector2(0.5f, 0.5f) - Vector2.up * 0.01f;
            hbRightUp = _horizontalHitbox * new Vector2(0.5f, 0.5f) - Vector2.right * 0.01f;
        }

        private void UpdateVBPosition()
        {
            vbDownLeft = _verticalHitbox * new Vector2(-0.5f, -0.5f) - Vector2.down * 0.01f;
            vbLeftDown = _verticalHitbox * new Vector2(-0.5f, -0.5f) - Vector2.left * 0.01f;
            vbDownRight = _verticalHitbox * new Vector2(0.5f, -0.5f) - Vector2.down * 0.01f;
            vbRightDown = _verticalHitbox * new Vector2(0.5f, -0.5f) - Vector2.right * 0.01f;
            vbUpLeft = _verticalHitbox * new Vector2(-0.5f, 0.5f) - Vector2.up * 0.01f;
            vbLeftUp = _verticalHitbox * new Vector2(-0.5f, 0.5f) - Vector2.left * 0.01f;
            vbUpRight = _verticalHitbox * new Vector2(0.5f, 0.5f) - Vector2.up * 0.01f;
            vbRightUp = _verticalHitbox * new Vector2(0.5f, 0.5f) - Vector2.right * 0.01f;
        }

        private void UpdatePBPosition()
        {
            pbLeftDown = _verticalHitbox * new Vector2(-0.5f, -0.5f) - Vector2.down * _platformCheckOffset;
            pbRightDown = _verticalHitbox * new Vector2(0.5f, -0.5f) - Vector2.down * _platformCheckOffset;
        }

        internal PlatformerHit RaycastLeft(LayerMask layer, float distance)
        {
            Vector2 pos = tf.position;
            return Raycast(pos + hbDownLeft, pos + hbUpLeft, _horizontalHitbox.x / 2, distance, Vector2.left, layer);
        }

        internal PlatformerHit RaycastRight(LayerMask layer, float distance)
        {
            Vector2 pos = tf.position;
            return Raycast(pos + hbDownRight, pos + hbUpRight, _horizontalHitbox.x / 2, distance, Vector2.right, layer);
        }

        internal PlatformerHit RaycastVbLeft(LayerMask layer, float distance)
        {
            Vector2 pos = tf.position;
            return Raycast(pos + vbDownLeft, pos + vbUpLeft, _verticalHitbox.x, distance, Vector2.left, layer);
        }

        internal PlatformerHit RaycastVbRight(LayerMask layer, float distance)
        {
            Vector2 pos = tf.position;
            return Raycast(pos + vbDownRight, pos + vbUpRight, _verticalHitbox.x, distance, Vector2.right, layer);
        }

        internal PlatformerHit RaycastDown(LayerMask layer, float distance)
        {
            Vector2 pos = tf.position;
            return Raycast(pos + vbLeftDown, pos + vbRightDown, _verticalHitbox.y / 2, distance, Vector2.down, layer);
        }

        internal PlatformerHit RaycastUp(LayerMask layer, float distance)
        {
            Vector2 pos = tf.position;
            return Raycast(pos + vbLeftUp, pos + vbRightUp, _verticalHitbox.y / 2, distance, Vector2.up, layer);
        }

        internal PlatformerHit RaycastHbDown(LayerMask layer, float distance)
        {
            Vector2 pos = tf.position;
            return Raycast(pos + hbLeftDown, pos + hbRightDown, _horizontalHitbox.y, distance, Vector2.down, layer);
        }

        internal PlatformerHit RaycastHbUp(LayerMask layer, float distance)
        {
            Vector2 pos = tf.position;
            return Raycast(pos + hbLeftUp, pos + hbRightUp, _horizontalHitbox.y, distance, Vector2.up, layer);
        }

        internal PlatformerHit RaycastPbDown(LayerMask layer, float distance)
        {
            Vector2 pos = tf.position;
            return Raycast(pos + pbLeftDown, pos + pbRightDown, _horizontalHitbox.y - platformCheckOffset, distance, Vector2.down, layer, false);
        }

        private PlatformerHit Raycast(Vector2 from, Vector2 to, float skin, float distance, Vector2 dir, LayerMask layer, bool ignoreZeroDistance = true)
        {
            if (!enabled)
                return PlatformerHit.NoHit;

            int count = Mathf.CeilToInt((from - to).magnitude / PlatformerBody.raycastUnit);

            PlatformerHit min = PlatformerHit.NoHit;

            if (dir == Vector2.zero)
                return min;

            for (int i = 0; i <= count; i++)
            {
                float r = i / (float)count;
                Vector2 origin = Vector2.Lerp(from, to, r);
                PlatformerHit hit = Raycast(origin, skin, distance, dir, layer, ignoreZeroDistance);
                if (hit.hit && hit.distance <= min.distance)
                {
                    min = hit;
                }
            }

            return min;
        }

        private PlatformerHit Raycast(Vector2 origin, float skin, float distance, Vector2 dir, LayerMask layer, bool ignoreZeroDistance)
        {
            if (distance < PlatformerBody.almostZero)
                distance = PlatformerBody.almostZero;

            RaycastHit2D hit = Physics2D.Raycast(origin - dir * skin, dir, distance + skin, layer);
            if (hit && (!ignoreZeroDistance || hit.distance > PlatformerBody.almostZero))
                return new PlatformerHit(true, hit.distance - skin, hit.normal, hit.transform.gameObject);
            else
                return PlatformerHit.NoHit;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector2 position = transform.position;
            Vector2[] box = { new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0.5f) };
            for (int i = 0; i < 4; i++)
                Debug.DrawLine(position + _horizontalHitbox * box[i], position + _horizontalHitbox * box[(i + 1) % 4], new Color(0.992f, 0.349f, 0.349f));
            for (int i = 0; i < 4; i++)
                Debug.DrawLine(position + _verticalHitbox * box[i], position + _verticalHitbox * box[(i + 1) % 4], new Color(0.694f, 0.992f, 0.349f));

            Debug.DrawLine(position + _verticalHitbox * box[0] - Vector2.down * _platformCheckOffset, position + _verticalHitbox * box[1] - Vector2.down * _platformCheckOffset, new Color(0.694f, 0.992f, 0.349f));
            Debug.DrawLine(position + _verticalHitbox * box[0] + Vector2.down * slopeCheckOffset, position + _verticalHitbox * box[1] + Vector2.down * slopeCheckOffset, new Color(0.694f, 0.992f, 0.349f));
        }
#endif
    }
}