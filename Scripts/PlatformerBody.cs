using UnityEngine;

namespace JaeminPark.PlatformerKit
{
    [RequireComponent(typeof(PlatformerCollider)), AddComponentMenu("Platformer Kit/Platformer Body")]
    public class PlatformerBody : MonoBehaviour
    {
        [HideInInspector]
        public Vector2 velocity;
        public Vector2 gravity = new Vector2(0, -0.02f);
        public LayerMask solidLayer;
        public LayerMask platformLayer;

        // Very low number that should be treated as 0 threshold
        public const float almostZero = 0.01f;
        public const float slopeDescendDistance = 0.1f;
        public const float raycastUnit = 0.05f;

        protected PlatformerCollider coll;
        public bool downWall { get; private set; }
        public bool upWall { get; private set; }
        public bool leftWall { get; private set; }
        public bool rightWall { get; private set; }
        public bool leftStuck { get; private set; }
        public bool rightStuck { get; private set; }
        public bool downStuck { get; private set; }
        public bool upStuck { get; private set; }
        public bool isGround { get { return downWall || downStuck; } }

        public GameObject leftObject { get; private set; }
        public PlatformBase leftPlatform { get; private set; }
        public GameObject rightObject { get; private set; }
        public PlatformBase rightPlatform { get; private set; }
        public GameObject upObject { get; private set; }
        public PlatformBase upPlatform { get; private set; }
        public GameObject steppingObject { get; private set; }
        public PlatformBase steppingPlatform { get; private set; }

        public delegate void OnStuck(float gap);
        public event OnStuck onVerticalStuck;
        public event OnStuck onHorizontalStuck;
        
        private void Awake()
        {
            coll = GetComponent<PlatformerCollider>();
        }
        
        private void FixedUpdate()
        {
            UpdatePhysics();
        }

        protected virtual void UpdatePhysics()
        {
            UpdateVelocity();
            UpdateYAxis();
            UpdateXAxis();
            UpdatePlatform();
        }

        protected virtual void UpdateVelocity()
        {
            velocity += gravity;
        }

        protected void UpdateYAxis()
        {
            PlatformerHit pbDown = coll.RaycastPbDown(platformLayer);
            LayerMask downLayer = (pbDown.hit && pbDown.distance < almostZero) ? solidLayer : (LayerMask)(solidLayer + platformLayer);

            PlatformerHit up = coll.RaycastUp(solidLayer);
            PlatformerHit down = coll.RaycastDown(downLayer);
            PlatformerHit hbUp = coll.RaycastHbUp(solidLayer);
            PlatformerHit hbDown = coll.RaycastHbDown(downLayer);

            bool upCheck = (up.hit && up.distance <= velocity.y || upStuck && !downStuck) && velocity.y > gravity.y;
            bool downCheck = (down.hit && down.distance <= -velocity.y || downStuck && !upStuck) && velocity.y <= -gravity.y;
            bool upSlopeCheck = (hbUp.hit && hbUp.distance <= velocity.y && Mathf.Abs(hbUp.normal.x) > almostZero) && velocity.y > almostZero;
            bool downSlopeCheck = (hbDown.hit && hbDown.distance <= -velocity.y && Mathf.Abs(hbDown.normal.x) > almostZero) && velocity.y < -almostZero;

            downWall = false;
            upWall = false;
            rightStuck = false;
            leftStuck = false;

            float stickThreshold = Mathf.Sin(Vector2.Angle(Vector2.up, down.normal) * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
            if (up.hit && up.distance <= stickThreshold && down.hit && down.distance <= stickThreshold)
            {
                // 오른쪽, 왼쪽 끼임
                float stuckNormal = (up.normal.normalized + down.normal.normalized).normalized.x;
                if (stuckNormal < 0)
                    rightStuck = true;
                else if (stuckNormal > 0)
                    leftStuck = true;
                else
                    rightStuck = leftStuck = true;

                transform.position += Vector3.up * (up.distance - down.distance) / 2;

                if (onVerticalStuck != null)
                    onVerticalStuck.Invoke(coll.verticalHitbox.y + up.distance + down.distance);

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
                    );
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
                    );
            }
            else if (upCheck && !downCheck)
            {
                // Y축 위 충돌
                if (upStuck && !downStuck)
                {
                    transform.position += Mathf.Min(hbUp.distance, 0) * Vector3.up;
                    velocity.y = 0;
                }
                else
                {
                    transform.position += up.distance * Vector3.up;
                    upWall = true;
                    velocity.y = 0;
                }
            }
            else if (downCheck && !upCheck)
            {
                // Y축 아래 충돌
                if (downStuck && !upStuck)
                {
                    transform.position += Mathf.Min(hbDown.distance, 0) * Vector3.down;
                    velocity.y = 0;
                }
                else
                {
                    transform.position += down.distance * Vector3.down;

                    PlatformerHit left = coll.RaycastLeft(solidLayer);
                    PlatformerHit right = coll.RaycastRight(solidLayer);
                    if ((!right.hit || right.distance >= -almostZero) && (!left.hit || left.distance >= -almostZero))
                    {
                        velocity.y = 0;
                        downWall = true;
                    }
                }
            }
            else
            {
                // Y축 충돌하지 않음
                transform.position += Vector3.up * velocity.y;
            }

            // 플랫폼 처리
            if (upCheck)
            {
                if (upObject != up.gameObject)
                {
                    if (upPlatform != null) upPlatform.OnBodyExit(this, PlatformBase.Direction.Up);
                    upObject = up.gameObject;
                    upPlatform = upObject.GetComponent<PlatformBase>();
                    if (upPlatform != null) upPlatform.OnBodyEnter(this, PlatformBase.Direction.Up);
                }
            }
            else
            {
                if (steppingPlatform != null) steppingPlatform.OnBodyExit(this, PlatformBase.Direction.Up);
                steppingObject = null;
                steppingPlatform = null;
            }

            if (downCheck)
            {
                if (steppingObject != down.gameObject)
                {
                    if (steppingPlatform != null) steppingPlatform.OnBodyExit(this, PlatformBase.Direction.Down);
                    steppingObject = down.gameObject;
                    steppingPlatform = steppingObject.GetComponent<PlatformBase>();
                    if (steppingPlatform != null) steppingPlatform.OnBodyEnter(this, PlatformBase.Direction.Down);
                }
            }
            else
            {
                if (steppingPlatform != null) steppingPlatform.OnBodyExit(this, PlatformBase.Direction.Down);
                steppingObject = null;
                steppingPlatform = null;
            }
        }

        protected void UpdateXAxis()
        {
            PlatformerHit pbDown = coll.RaycastPbDown(platformLayer);
            LayerMask downLayer = (pbDown.hit && pbDown.distance < almostZero) ? solidLayer : (LayerMask)(solidLayer + platformLayer);

            PlatformerHit left = coll.RaycastLeft(solidLayer);
            PlatformerHit right = coll.RaycastRight(solidLayer);
            PlatformerHit vbLeft = coll.RaycastVbLeft(downLayer);
            PlatformerHit vbRight = coll.RaycastVbRight(downLayer);

            bool rightCheck = (right.hit && right.distance <= velocity.x || rightStuck && !leftStuck) && velocity.x >= -almostZero;
            bool leftCheck = (left.hit && left.distance <= -velocity.x || leftStuck && !rightStuck) && velocity.x <= almostZero;
            bool rightSlopeCheck = (vbRight.hit && vbRight.distance <= velocity.x) && velocity.x > almostZero;
            bool leftSlopeCheck = (vbLeft.hit && vbLeft.distance <= -velocity.x) && velocity.x < -almostZero;

            leftWall = false;
            rightWall = false;
            upStuck = false;
            downStuck = false;

            if (right.hit && right.distance <= 0 && left.hit && left.distance <= 0)
            {
                // 위, 아래 끼임
                float stuckNormal = (left.normal.normalized + right.normal.normalized).normalized.y;
                if (stuckNormal < 0)
                    upStuck = true;
                else if (stuckNormal > 0)
                    downStuck = true;
                else
                    upStuck = downStuck = true;

                if (onHorizontalStuck != null)
                    onHorizontalStuck.Invoke(coll.horizontalHitbox.x + right.distance + left.distance);

                transform.position += Vector3.right * (right.distance - left.distance) / 2;
            }
            else if (rightCheck && !leftCheck)
            {
                // X축 오른쪽 충돌
                if (rightStuck && right.distance > 0 && velocity.x >= -almostZero)
                {
                    // 끼임
                    transform.position += Mathf.Min(vbRight.distance, 0) * Vector3.right;
                    velocity.x = 0;
                }
                else if (rightSlopeCheck && right.distance >= -almostZero)
                {
                    // 경사로
                    float angle = Vector2.Angle(vbRight.normal, Vector2.up);
                    float xSpeed = Mathf.Abs(velocity.x);
                    float xSign = Mathf.Sign(velocity.x);

                    transform.position += new Vector3(
                            Mathf.Cos(angle * Mathf.Deg2Rad) * xSpeed * xSign,
                            Mathf.Sin(angle * Mathf.Deg2Rad) * xSpeed
                        );
                }
                else
                {
                    // 벽면
                    transform.position += right.distance * Vector3.right;
                    velocity.x = 0;
                    rightWall = true;
                }
            }
            else if (leftCheck && !rightCheck)
            {
                // X축 왼쪽 충돌
                if (leftStuck && left.distance > 0 && velocity.x <= almostZero)
                {
                    // 끼임
                    transform.position += Mathf.Min(vbLeft.distance, 0) * Vector3.left;
                    velocity.x = 0;
                }
                else if (leftSlopeCheck && left.distance >= -almostZero)
                {
                    // 경사로
                    float angle = Vector2.Angle(vbLeft.normal, Vector2.up);
                    float xSpeed = Mathf.Abs(velocity.x);
                    float xSign = Mathf.Sign(velocity.x);

                    transform.position += new Vector3(
                            Mathf.Cos(angle * Mathf.Deg2Rad) * xSpeed * xSign,
                            Mathf.Sin(angle * Mathf.Deg2Rad) * xSpeed
                        );
                }
                else
                {
                    // 벽면
                    transform.position += left.distance * Vector3.left;
                    velocity.x = 0;
                    leftWall = true;
                }
            }
            else
            {
                // X축 충돌하지 않음
                transform.position += Vector3.right * velocity.x;
            }

            PlatformerHit descSlope = coll.RaycastDown(downLayer);
            bool descSlopeHit = descSlope.hit && descSlope.distance <= slopeDescendDistance && velocity.y == 0;

            if (descSlopeHit && !(leftStuck && rightStuck))
            {
                // 아래에 내려가는 경사면이 있어 붙어서 가야 할 때
                transform.position += descSlope.distance * Vector3.down;
                velocity.y = 0;
            }

            // 플랫폼 처리
            if (rightCheck)
            {
                if (rightObject != right.gameObject)
                {
                    if (rightPlatform != null) rightPlatform.OnBodyExit(this, PlatformBase.Direction.Right);
                    rightObject = right.gameObject;
                    rightPlatform = rightObject.GetComponent<PlatformBase>();
                    if (rightPlatform != null) rightPlatform.OnBodyEnter(this, PlatformBase.Direction.Right);
                }
            }
            else
            {
                if (rightPlatform != null) rightPlatform.OnBodyExit(this, PlatformBase.Direction.Right);
                rightObject = null;
                rightPlatform = null;
            }

            if (leftCheck)
            {
                if (leftObject != left.gameObject)
                {
                    if (leftPlatform != null) leftPlatform.OnBodyExit(this, PlatformBase.Direction.Left);
                    leftObject = left.gameObject;
                    leftPlatform = leftObject.GetComponent<PlatformBase>();
                    if (leftPlatform != null) leftPlatform.OnBodyEnter(this, PlatformBase.Direction.Left);
                }
            }
            else
            {
                if (leftPlatform != null) leftPlatform.OnBodyExit(this, PlatformBase.Direction.Left);
                leftObject = null;
                leftPlatform = null;
            }
        }

        private void UpdatePlatform()
        {
            if (leftPlatform != null)
                leftPlatform.OnBodyStay(this, PlatformBase.Direction.Left);
            if (rightPlatform != null)
                rightPlatform.OnBodyStay(this, PlatformBase.Direction.Right);
            if (steppingPlatform != null)
                steppingPlatform.OnBodyStay(this, PlatformBase.Direction.Down);
            if (upPlatform != null)
                upPlatform.OnBodyStay(this, PlatformBase.Direction.Up);
        }
    }
}
