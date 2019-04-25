using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    CapsuleCollider myCollider = new CapsuleCollider();

    public Transform target;

    RaycastHit hitDown;
    RaycastHit hitInfo;

    public LayerMask ground;

    Vector3 currentNormal = Vector3.up;
    Vector3 currentAngle;
    Vector3 forward;
    Vector3 targetPosition;
    Vector3 surfaceNormal = Vector3.down;
    Vector3 colPoint;
    Vector3 normalizedDirection;
    Vector3 oldYPos;

    GameObject[] rampWaypoints;
    GameObject floor;
    GameObject currentRampPoint;

    Path path;
    public EnemyRampMovement enemyRampMovement;

    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveTreshold = 0.5f;

    public float slopeForce = 5;
    public float speed = 20;
    public float fallSpeed = 20;
    public float turnSpeed = 5;
    public float turnDistance = 5;
    public float stoppingDistance = 10;
    public float height = 1f;
    public float heightPadding = 0.1f;
    public float maxGroundAngle = 145;
    public float attackRange = 2;

    float normStoppingDist;
    float tempAttackRange;
    float groundAngle;
    float angle;
    float radius;
    float closestDistToEnemy;
    float closestDistToPlayer;

    string floorName;

    bool bGrounded;
    bool bMovingToPlayer = false;
    bool bMovingToRamp = false;
    bool bMovingToNextPoint = false;


    private void Start()
    {
        currentAngle = transform.eulerAngles;
        myCollider = GetComponent<CapsuleCollider>();
        radius = myCollider.radius * 0.9f;

        tempAttackRange = attackRange;
        normStoppingDist = stoppingDistance;

        //ramps = GameObject.FindGameObjectsWithTag("Ramp");
        rampWaypoints = GameObject.FindGameObjectsWithTag("Waypoint");
    }

    private void Update()
    {
        DrawDebugLines();
        CalculateForward();
        CheckGround();
        CalculateGroundAngle();
        ApplyGravity();

        if (!bMovingToPlayer && bGrounded && !bMovingToRamp)
        {
            StartCoroutine(UpdatePath());
            GetTargetToMoveTo();
        }

        if (targetPosition != target.position)
        {
            bMovingToPlayer = false;
            bMovingToRamp = true;

            RampMove();
        }
        else
        {
            StartCoroutine(UpdatePath());
        }
    }

    private void RampMove()
    {
        enemyRampMovement.MoveOnRamp(transform);

        if(Mathf.Abs(transform.position.y - target.position.y) <= 0.5f)
        {
            bMovingToRamp = false;
            targetPosition = target.position;
        }

        if (enemyRampMovement.OnRamp(transform))
        {
            Debug.Log(transform.name + " is on ramp");
            Vector3 newYPos = new Vector3(transform.position.x, hitInfo.point.y + height, transform.position.z);
            oldYPos = newYPos;
            transform.position = newYPos;
        }

        targetPosition = enemyRampMovement.GetTargetPosition();

        if(Vector3.Distance(transform.position, targetPosition) <= 0.5)
        {
            print(transform.name + " has arrived to ramp");
        }       
    }

    private void CalculateGroundAngle()
    {
        if (!bGrounded)
        {
            groundAngle = 90;
            return;
        }

        groundAngle = Vector3.Angle(hitDown.normal, transform.forward);
    }

    private void CheckGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, height + heightPadding, ground))
        {
            transform.up = hitInfo.normal;
            bGrounded = true;
        }
        else
        {
            bGrounded = false;
        }
    }

    private void ApplyGravity()
    {
        if (!bGrounded)
        {
            transform.position += Physics.gravity * Time.deltaTime;
        }
    }

    private void CalculateForward()
    {
        // if not grounded forward is transform.forward else change according to groundAngle
        if (!bGrounded)
        {
            forward = transform.forward;
            currentNormal = Vector3.up;
            return;
        }

        forward = Vector3.Cross(transform.right, hitInfo.normal);
    }

    private void OnPathFound(Vector3[] waypoints, bool bPathSuccessful)
    {
        if (bPathSuccessful)
        {
            path = new Path(waypoints, transform.position, turnDistance, stoppingDistance);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    private void GetTargetToMoveTo()
    {
        RaycastHit hit1;
        RaycastHit hit2;
        float castDistance = myCollider.height / 2;
        Vector3 dir = Vector3.down;

        //check if enemy is on different floor than player
        if (Physics.SphereCast(transform.position, radius, dir, out hit1, castDistance))
        {
            if (hit1.transform.parent != null)
            {
                floorName = hit1.transform.parent.name;
            }
            else
            {
                floorName = hit1.transform.name;
            }

            // check if enemy unit and player are on same floor
            if (Physics.SphereCast(target.position, radius, dir, out hit2, castDistance))
            {
                string parentFloorName;

                if (hit2.transform.parent != null)
                {
                    parentFloorName = hit2.transform.parent.name;
                }
                else
                {
                    parentFloorName = hit2.transform.name;
                }

                if (floorName == parentFloorName)
                {
                    targetPosition = target.position; // set player's position as targetPosition
                }
                else
                {
                    // floor is different - find ramp to wanted floor
                    targetPosition = enemyRampMovement.FindClosestRamp(this.transform);
                }
            }
        }
    }

    IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(0.3f);
        }

        //Request path from current position to target        
        PathRequestManager.RequestPath(new PathRequest(transform.position, targetPosition, OnPathFound));

        float sqrMoveTreshhold = pathUpdateMoveTreshold * pathUpdateMoveTreshold;
        Vector3 targetPosOld = targetPosition;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);

            if ((targetPosition - targetPosOld).sqrMagnitude > sqrMoveTreshhold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, targetPosition, OnPathFound));
                targetPosOld = targetPosition;
            }
        }
    }

    IEnumerator FollowPath()
    {
        bool bFollowingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        while (bFollowingPath)
        {
            Vector2 position2d = new Vector2(transform.position.x, transform.position.z);

            //check if crossed smoothing line for turning
            while (path.turnBoundaries[pathIndex].HasCrossedLine(position2d))
            {
                if (pathIndex == path.finishedLineIndex)
                {
                    bFollowingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            //move along the path
            if (bFollowingPath)
            {
                if (!bGrounded)
                {
                    bFollowingPath = false;
                    bMovingToPlayer = false;
                }

                // zero out stopping distance if moving to ramp or next rampPoint
                if (bMovingToRamp || bMovingToNextPoint)
                {
                    stoppingDistance = 0f;
                    attackRange = 0;
                }
                else
                {
                    stoppingDistance = normStoppingDist;
                    attackRange = tempAttackRange;
                }

                // start slowing if next point is target's position
                if (pathIndex >= path.slowDownIndex && stoppingDistance > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishedLineIndex].DistanceFromThePoint(position2d) / stoppingDistance);

                    if (speedPercent < 0.01f || Physics.Raycast(transform.position, targetPosition, attackRange))
                    {
                        bFollowingPath = false;

                        if (bMovingToPlayer)
                        {
                            print("say hello to death, mister wizard");
                            bMovingToPlayer = false;
                        }
                    }
                }

                // Rotate towards next pathPoint
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

                // Keep unit's own y - position while following path
                Vector3 targetPos = new Vector3(path.lookPoints[pathIndex].x, this.transform.position.y, path.lookPoints[pathIndex].z);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speed * speedPercent);

                normalizedDirection = (targetPos - transform.position).normalized;
                transform.position += normalizedDirection * speed * speedPercent * Time.deltaTime;
            }

            transform.LookAt(targetPosition);
            yield return null;
        }
    }

    private void DrawDebugLines()
    {
        Debug.DrawLine(transform.position, transform.position + forward * height * 2, Color.blue);
        Debug.DrawLine(transform.position, transform.position - Vector3.up * height, Color.green);
    }
}
