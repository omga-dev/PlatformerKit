using UnityEngine;

namespace JaeminPark.PlatformerKit
{
    [RequireComponent(typeof(PlatformerCollider)), AddComponentMenu("Platformer Kit/Platformer Body")]
    public class PlatformerBody : MonoBehaviour
    {
        /// <summary>
        /// Linear velocity of the body per frame.
        /// </summary>
        [HideInInspector]
        public Vector2 velocity;
        
        /// <summary>
        /// Linear velocity at last frame.
        /// </summary>
        public Vector2 velocityAtLastFrame { get; private set; }

        /// <summary>
        /// Gravity applied to the body every frame.
        /// </summary>
        public Vector2 gravity = new Vector2(0, -0.02f);

        /// <summary>
        /// LayerMask for detecting Solid.
        /// </summary>
        public LayerMask solidLayer;

        /// <summary>
        /// LayerMask for detecting Platform: Solid that is detected only when player is falling.
        /// </summary>
        public LayerMask platformLayer;

        /// <summary>
        /// Velocity less than this value is treated the same with 0.
        /// </summary>
        public const float almostZero = 0.01f;

        internal const float raycastUnit = 0.05f;

        protected PlatformerCollider coll;

        /// <summary>
        /// Whether this body is detecting a collision on its left side.
        /// </summary>
        public bool isLeftWall { get; private set; }
        private bool wasLeftWall;
        public bool isLeftWallEntering { get { return !wasLeftWall && isLeftWall; } }
        public bool isLeftWallExitting { get { return wasLeftWall && !isLeftWall; } }

        /// <summary>
        /// Whether this body is detecting a collision on its right side.
        /// </summary>
        public bool isRightWall { get; private set; }
        private bool wasRightWall;
        public bool isRightWallEntering { get { return !isRightWall && wasRightWall; } }
        public bool isRightWallExitting { get { return isRightWall && !wasRightWall; } }

        /// <summary>
        /// Whether this body is detecting a collision on its bottom side.
        /// </summary>
        public bool isDownWall { get; private set; }
        private bool wasDownWall;
        public bool isDownWallEntering { get { return !isDownWall && wasDownWall; } }
        public bool isDownWallExitting { get { return isDownWall && !wasDownWall; } }

        /// <summary>
        /// Whether this body is detecting a collision on its upper side.
        /// </summary>
        public bool isUpWall { get; private set; }
        private bool wasUpWall;
        public bool isUpWallEntering { get { return !isUpWall && wasUpWall; } }
        public bool isUpWallExitting { get { return isUpWall && !wasUpWall; } }

        /// <summary>
        /// Whether this body is detecting a sandwiching on its left side.
        /// </summary>
        public bool isLeftSandwich { get; private set; }
        private bool wasLeftSandwich;
        public bool isLeftSandwichStarting { get { return !isLeftSandwich && wasLeftSandwich; } }
        public bool isLeftSandwichFinishing { get { return isLeftSandwich && !wasLeftSandwich; } }

        /// <summary>
        /// Whether this body is detecting a sandwiching on its right side.
        /// </summary>
        public bool isRightSandwich { get; private set; }
        private bool wasRightSandwich;
        public bool isRightSandwichStarting { get { return !isRightSandwich && wasRightSandwich; } }
        public bool isRightSandwichFinishing { get { return isRightSandwich && !wasRightSandwich; } }

        /// <summary>
        /// A gap between two colliders horizontally sandwiching the body. (Mathf.Infinity when body's not detecting a sandwich.)
        /// </summary>
        public float horizontalSandwichGap { get; private set; }
        public float horizontalSandwichGapLastFrame { get; private set; }

        /// <summary>
        /// Whether this body is detecting a sandwiching on its bottom side.
        /// </summary>
        public bool isDownSandwich { get; private set; }
        private bool wasDownSandwich;
        public bool isDownSandwichStarting { get { return !isDownSandwich && wasDownSandwich; } }
        public bool isDownSandwichFinishing { get { return isDownSandwich && !wasDownSandwich; } }

        /// <summary>
        /// Whether this body is detecting a sandwiching on its upper side.
        /// </summary>
        public bool isUpSandwich { get; private set; }
        private bool wasUpSandwich;
        public bool isUpSandwichStarting { get { return !isUpSandwich && wasUpSandwich; } }
        public bool isUpSandwichFinishing { get { return isUpSandwich && !wasUpSandwich; } }

        /// <summary>
        /// A gap between two colliders vertically sandwiching the body. (Mathf.Infinity when body's not detecting a sandwich.)
        /// </summary>
        public float verticalSandwichGap { get; private set; }
        public float verticalSandwichGapLastFrame { get; private set; }

        /// <summary>
        /// Whether this body is standing on a ground.
        /// </summary>
        public bool isGround { get { return isDownWall || isDownSandwich; } }
        private bool wasGround;
        public bool isGroundEntering { get { return !isGround && wasGround; } }
        public bool isGroundExitting { get { return isGround && !wasGround; } }

        /// <summary>
        /// GameObject being detected on its left side.
        /// </summary>
        public GameObject leftObject { get; private set; }
        public GameObject leftObjectLastFrame { get; private set; }

        /// <summary>
        /// PlatformBase being detected on its left side.
        /// </summary>
        public PlatformBase leftPlatform { get; private set; }
        public PlatformBase leftPlatformLastFrame { get; private set; }

        /// <summary>
        /// GameObject being detected on its right side.
        /// </summary>
        public GameObject rightObject { get; private set; }
        public GameObject rightObjectLastFrame { get; private set; }

        /// <summary>
        /// PlatformBase being detected on its right side.
        /// </summary>
        public PlatformBase rightPlatform { get; private set; }
        public PlatformBase rightPlatformLastFrame { get; private set; }

        /// <summary>
        /// GameObject being detected on its bottom side.
        /// </summary>
        public GameObject downObject { get; private set; }
        public GameObject downObjectLastFrame { get; private set; }

        /// <summary>
        /// PlatformBase being detected on its bottom side.
        /// </summary>
        public PlatformBase downPlatform { get; private set; }
        public PlatformBase downPlatformLastFrame { get; private set; }

        /// <summary>
        /// GameObject being detected on its upper side.
        /// </summary>
        public GameObject upObject { get; private set; }
        public GameObject upObjectLastFrame { get; private set; }

        /// <summary>
        /// PlatformBase being detected on its upper side.
        /// </summary>
        public PlatformBase upPlatform { get; private set; }
        public PlatformBase upPlatformLastFrame { get; private set; }

        /// <summary>
        /// Delegate of the event called when the body's being sandwiched.
        /// </summary>
        /// <param name="gap">Gap between two sandwiching collider.</param>
        [System.Obsolete]
        public delegate void SandwichEvent(float gap);

        /// <summary>
        /// Event called when the body's being sandwiched vertically.
        /// </summary>
        [System.Obsolete]
        public event SandwichEvent onVerticalSandwich;

        /// <summary>
        /// Event called when the body's being sandwiched horizontally.
        /// </summary>
        [System.Obsolete]
        public event SandwichEvent onHorizontalSandwich;

        /// <summary>
        /// Delegate of the event called when the body has entered or exited from a platform collision.
        /// </summary>
        /// <param name="platformObject">detected GameObject.</param>
        /// <param name="platform">detected PlatformBase.</param>
        /// <param name="direction">Direction of collision. (e.g. Direction.Down when body's stepping on it.)</param>
        [System.Obsolete]
        public delegate void PlatformEvent(GameObject platformObject, PlatformBase platform, Direction direction);

        /// <summary>
        /// Event called when the body has entered a platform collision.
        /// </summary>
        [System.Obsolete]
        public event PlatformEvent onPlatformEnter;

        /// <summary>
        /// Event called when the body has exited from a platform collision.
        /// </summary>
        [System.Obsolete]
        public event PlatformEvent onPlatformExit;

        private void Awake()
        {
            coll = GetComponent<PlatformerCollider>();
        }

        private void FixedUpdate()
        {
            UpdatePhysics();
        }

        private void UpdatePhysics()
        {
            UpdateFrame();
            UpdateVelocity();
            UpdateYAxis();
            UpdateXAxis();
            UpdatePlatform();
        }

        private void UpdateFrame()
        {
            velocityAtLastFrame = velocity;
            wasLeftWall = isLeftWall;
            wasRightWall = isRightWall;
            wasDownWall = isDownWall;
            wasUpWall = isUpWall;
            wasLeftSandwich = isLeftSandwich;
            wasRightSandwich = isRightSandwich;
            verticalSandwichGapLastFrame = verticalSandwichGap;
            wasDownSandwich = isDownSandwich;
            wasUpSandwich = isUpSandwich;
            horizontalSandwichGapLastFrame = horizontalSandwichGap;
            wasGround = isGround;
            leftObjectLastFrame = leftObject;
            rightObjectLastFrame = rightObject;
            downObjectLastFrame = downObject;
            upObjectLastFrame = upObject;
            leftPlatformLastFrame = leftPlatform;
            rightPlatformLastFrame = rightPlatform;
            downPlatformLastFrame = downPlatform;
            upPlatformLastFrame = upPlatform;
        }

        private void UpdateVelocity()
        {
            velocity += gravity * Time.timeScale;
        }

        private void UpdateYAxis()
        {
            PlatformerHit pbDown = coll.RaycastPbDown(platformLayer, -velocity.y);
            LayerMask downLayer = (pbDown.hit && pbDown.distance < almostZero) ? solidLayer : (LayerMask)(solidLayer + platformLayer);

            PlatformerHit up = coll.RaycastUp(solidLayer, velocity.y);
            PlatformerHit down = coll.RaycastDown(downLayer, -velocity.y);
            PlatformerHit hbUp = coll.RaycastHbUp(solidLayer, velocity.y);
            PlatformerHit hbDown = coll.RaycastHbDown(downLayer, -velocity.y);

            bool upCheck = (up.hit && up.distance <= velocity.y || isUpSandwich && !isDownSandwich) && velocity.y > gravity.y;
            bool downCheck = (down.hit && down.distance <= -velocity.y || isDownSandwich && !isUpSandwich) && velocity.y <= -gravity.y;
            bool upSlopeCheck = (hbUp.hit && hbUp.distance <= velocity.y && Mathf.Abs(hbUp.normal.x) > almostZero) && velocity.y > almostZero;
            bool downSlopeCheck = (hbDown.hit && hbDown.distance <= -velocity.y && Mathf.Abs(hbDown.normal.x) > almostZero) && velocity.y < -almostZero;

            isDownWall = false;
            isUpWall = false;
            isRightSandwich = false;
            isLeftSandwich = false;
            verticalSandwichGap = 0;

            float stickThreshold = Mathf.Sin(Vector2.Angle(Vector2.up, down.normal) * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
            if (up.hit && up.distance <= stickThreshold && down.hit && down.distance <= stickThreshold)
            {
                // 오른쪽, 왼쪽 끼임
                float stuckNormal = (up.normal.normalized + down.normal.normalized).normalized.x;
                if (stuckNormal < 0)
                    isRightSandwich = true;
                else if (stuckNormal > 0)
                    isLeftSandwich = true;
                else
                    isRightSandwich = isLeftSandwich = true;

                transform.position += Vector3.up * (up.distance - down.distance) / 2;
                
                onVerticalSandwich?.Invoke(coll.verticalHitbox.y + up.distance + down.distance);
                verticalSandwichGap = coll.verticalHitbox.y + up.distance + down.distance;

                velocity.y = 0;
            }
            else if (upSlopeCheck && !upCheck)
            {
                // Y축 위 경사로
                float angle = Vector2.Angle(hbUp.normal, Vector2.up);
                float ySpeed = Mathf.Abs(velocity.y);
                float ySign = Mathf.Sign(velocity.y);

                transform.position += new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * ySpeed * Mathf.Sign(hbUp.normal.x),
                        Mathf.Sin(angle * Mathf.Deg2Rad) * ySpeed * ySign
                    ) * Time.timeScale;
            }
            else if (downSlopeCheck && !downCheck)
            {
                // Y축 아래 경사로
                float angle = Vector2.Angle(hbDown.normal, Vector2.up);
                float ySpeed = Mathf.Abs(velocity.y);
                float ySign = Mathf.Sign(velocity.y);

                transform.position += new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * ySpeed * Mathf.Sign(hbDown.normal.x),
                        Mathf.Sin(angle * Mathf.Deg2Rad) * ySpeed * ySign
                    ) * Time.timeScale;
            }
            else if (upCheck && !downCheck)
            {
                // Y축 위 충돌
                if (isUpSandwich && !isDownSandwich)
                {
                    transform.position += Mathf.Min(hbUp.distance, 0) * Vector3.up;
                    velocity.y = 0;
                }
                else
                {
                    transform.position += up.distance * Vector3.up;
                    isUpWall = true;
                    velocity.y = 0;
                }
            }
            else if (downCheck && !upCheck)
            {
                // Y축 아래 충돌
                if (isDownSandwich && !isUpSandwich)
                {
                    transform.position += Mathf.Min(hbDown.distance, 0) * Vector3.down;
                    velocity.y = 0;
                }
                else
                {
                    transform.position += down.distance * Vector3.down;

                    PlatformerHit left = coll.RaycastLeft(solidLayer, -velocity.x);
                    PlatformerHit right = coll.RaycastRight(solidLayer, velocity.x);
                    if ((!right.hit || right.distance >= -almostZero) && (!left.hit || left.distance >= -almostZero))
                    {
                        velocity.y = 0;
                        isDownWall = true;
                    }
                }
            }
            else
            {
                // Y축 충돌하지 않음
                transform.position += Vector3.up * velocity.y * Time.timeScale;
            }

            // 플랫폼 처리
            if (upCheck)
            {
                if (upObject != up.gameObject)
                {
                    upPlatform?.OnBodyExit(this, Direction.Up);
                    upObject = up.gameObject;
                    upPlatform = upObject.GetComponent<PlatformBase>();
                    upPlatform?.OnBodyEnter(this, Direction.Up);

                    onPlatformEnter?.Invoke(upObject, upPlatform, Direction.Up);
                }
            }
            else if (upObject)
            {
                onPlatformExit?.Invoke(upObject, upPlatform, Direction.Up);
                upPlatform?.OnBodyExit(this, Direction.Up);
                upObject = null;
                upPlatform = null;
            }

            if (downCheck)
            {
                if (downObject != down.gameObject)
                {
                    downPlatform?.OnBodyExit(this, Direction.Down);
                    downObject = down.gameObject;
                    downPlatform = downObject.GetComponent<PlatformBase>();
                    downPlatform?.OnBodyEnter(this, Direction.Down);

                    onPlatformEnter?.Invoke(downObject, downPlatform, Direction.Down);
                }
            }
            else if (downObject)
            {
                onPlatformExit?.Invoke(downObject, downPlatform, Direction.Down);
                downPlatform?.OnBodyExit(this, Direction.Down);
                downObject = null;
                downPlatform = null;
            }
        }

        private void UpdateXAxis()
        {
            float upCheckDist = Mathf.Max(velocity.y, almostZero);
            float downCheckDist = Mathf.Max(-velocity.y, almostZero);
            float rightCheckDist = Mathf.Max(velocity.x, almostZero);
            float leftCheckDist = Mathf.Max(-velocity.x, almostZero);

            PlatformerHit pbDown = coll.RaycastPbDown(platformLayer, -velocity.y);
            LayerMask downLayer = (pbDown.hit && pbDown.distance < almostZero) ? solidLayer : (LayerMask)(solidLayer + platformLayer);

            PlatformerHit left = coll.RaycastLeft(solidLayer, -velocity.x);
            PlatformerHit right = coll.RaycastRight(solidLayer, velocity.x);
            PlatformerHit vbLeft = coll.RaycastVbLeft(downLayer, -velocity.x);
            PlatformerHit vbRight = coll.RaycastVbRight(downLayer, velocity.x);

            bool rightCheck = (right.hit && right.distance <= velocity.x || isRightSandwich && !isLeftSandwich) && velocity.x >= -almostZero;
            bool leftCheck = (left.hit && left.distance <= -velocity.x || isLeftSandwich && !isRightSandwich) && velocity.x <= almostZero;
            bool rightSlopeCheck = (vbRight.hit && vbRight.distance <= velocity.x) && velocity.x > almostZero;
            bool leftSlopeCheck = (vbLeft.hit && vbLeft.distance <= -velocity.x) && velocity.x < -almostZero;

            isLeftWall = false;
            isRightWall = false;
            isUpSandwich = false;
            isDownSandwich = false;
            horizontalSandwichGap = 0;

            if (right.hit && right.distance <= 0 && left.hit && left.distance <= 0)
            {
                // 위, 아래 끼임
                float stuckNormal = (left.normal.normalized + right.normal.normalized).normalized.y;
                if (stuckNormal < 0)
                    isUpSandwich = true;
                else if (stuckNormal > 0)
                    isDownSandwich = true;
                else
                    isUpSandwich = isDownSandwich = true;
                
                onHorizontalSandwich?.Invoke(coll.horizontalHitbox.x + right.distance + left.distance);
                horizontalSandwichGap = coll.horizontalHitbox.x + right.distance + left.distance;

                transform.position += Vector3.right * (right.distance - left.distance) / 2;
            }
            else if (rightCheck && !leftCheck)
            {
                float angle = Vector2.Angle(vbRight.normal, Vector2.up);

                // X축 오른쪽 충돌
                if (isRightSandwich && right.distance > 0 && velocity.x >= -almostZero)
                {
                    // 끼임
                    transform.position += Mathf.Min(vbRight.distance, 0) * Vector3.right;
                    velocity.x = 0;
                }
                else if (rightSlopeCheck && right.distance >= -almostZero && angle <= coll.maxHorizontalSlopeAngle)
                {
                    // 경사로
                    float xSpeed = Mathf.Abs(velocity.x);
                    float xSign = Mathf.Sign(velocity.x);

                    transform.position += new Vector3(
                            Mathf.Cos(angle * Mathf.Deg2Rad) * xSpeed * xSign,
                            Mathf.Sin(angle * Mathf.Deg2Rad) * xSpeed
                        ) * Time.timeScale;
                }
                else
                {
                    // 벽면
                    transform.position += right.distance * Vector3.right;
                    isRightWall = true;
                    velocity.x = 0;
                }
            }
            else if (leftCheck && !rightCheck)
            {
                float angle = Vector2.Angle(vbLeft.normal, Vector2.up);

                // X축 왼쪽 충돌
                if (isLeftSandwich && left.distance > 0 && velocity.x <= almostZero)
                {
                    // 끼임
                    transform.position += Mathf.Min(vbLeft.distance, 0) * Vector3.left;
                    velocity.x = 0;
                }
                else if (leftSlopeCheck && left.distance >= -almostZero && angle <= coll.maxHorizontalSlopeAngle)
                {
                    // 경사로
                    float xSpeed = Mathf.Abs(velocity.x);
                    float xSign = Mathf.Sign(velocity.x);

                    transform.position += new Vector3(
                            Mathf.Cos(angle * Mathf.Deg2Rad) * xSpeed * xSign,
                            Mathf.Sin(angle * Mathf.Deg2Rad) * xSpeed
                        ) * Time.timeScale;
                }
                else
                {
                    // 벽면
                    transform.position += left.distance * Vector3.left;
                    isLeftWall = true;
                    velocity.x = 0;
                }
            }
            else
            {
                transform.position += Vector3.right * velocity.x * Time.timeScale;

                float xSpeed = Mathf.Abs(velocity.x);
                float xSign = Mathf.Sign(velocity.x);
                float slopeCheckOffset = coll.slopeCheckRate * xSpeed + almostZero;
                PlatformerHit slope = coll.RaycastDown(downLayer, slopeCheckOffset);
                bool isSlopeOpposite = velocity.x < -almostZero && slope.normal.x < 0 || velocity.x > almostZero && slope.normal.x > 0;
                if (slope.hit && velocity.x != 0 && velocity.y == 0 && !(isLeftSandwich && isRightSandwich))
                {
                    float angle = Vector2.Angle(slope.normal, Vector2.up);
                    
                    if (isSlopeOpposite && angle <= coll.maxHorizontalSlopeAngle)
                    {
                        // 아래에 내려가는 경사면이 있어 붙어서 가야 할 때
                        transform.position -= Vector3.right * velocity.x * Time.timeScale;
                        transform.position += new Vector3(
                                Mathf.Cos(angle * Mathf.Deg2Rad) * xSpeed * Mathf.Sign(slope.normal.x),
                                Mathf.Sin(angle * Mathf.Deg2Rad) * -xSpeed
                            ) * Time.timeScale;
                    }
                }
            }

            // 플랫폼 처리
            if (right.hit && right.distance <= velocity.x + almostZero)
            {
                if (rightObject != right.gameObject)
                {
                    rightPlatform?.OnBodyExit(this, Direction.Right);
                    rightObject = right.gameObject;
                    rightPlatform = rightObject.GetComponent<PlatformBase>();
                    rightPlatform?.OnBodyEnter(this, Direction.Right);

                    onPlatformEnter?.Invoke(rightObject, rightPlatform, Direction.Right);
                }
            }
            else if (rightObject)
            {
                onPlatformExit?.Invoke(rightObject, rightPlatform, Direction.Right);
                rightPlatform?.OnBodyExit(this, Direction.Right);
                rightObject = null;
                rightPlatform = null;
            }

            if (left.hit && left.distance <= -velocity.x + almostZero)
            {
                if (leftObject != left.gameObject)
                {
                    leftPlatform?.OnBodyExit(this, Direction.Left);
                    leftObject = left.gameObject;
                    leftPlatform = leftObject.GetComponent<PlatformBase>();
                    leftPlatform?.OnBodyEnter(this, Direction.Left);

                    onPlatformEnter?.Invoke(leftObject, leftPlatform, Direction.Left);
                }
            }
            else if (leftObject)
            {
                onPlatformExit?.Invoke(leftObject, leftPlatform, Direction.Left);
                leftPlatform?.OnBodyExit(this, Direction.Left);
                leftObject = null;
                leftPlatform = null;
            }
        }

        private void UpdatePlatform()
        {
            leftPlatform?.OnBodyStay(this, Direction.Left);
            rightPlatform?.OnBodyStay(this, Direction.Right);
            downPlatform?.OnBodyStay(this, Direction.Down);
            upPlatform?.OnBodyStay(this, Direction.Up);
        }
    }
}
