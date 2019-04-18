using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRampMovement : MonoBehaviour
{
    public Transform target;

    public float height = 1f;
    public float heightPadding = 0.1f;
    public float speed = 5;

    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeForceRayLenght;

    private float closestDistToEnemy;
    private float closestDistToPlayer;

    private string floorName;

    RaycastHit hitDown;
    RaycastHit hitInfo;

    public LayerMask ground;

    private bool bGrounded;
    private bool bMovingToRamp = false;

    private Vector3 targetPosition;
    private Vector3 normalizedDirection;
    private Vector3 oldYPos;

    GameObject[] rampWaypoints;
    GameObject floor;
    GameObject currentRampPoint;


    private void Start()
    {
        rampWaypoints = GameObject.FindGameObjectsWithTag("Waypoint");
    }

    private void Update()
    {
        CheckGround();
        ApplyGravity();

        if (!bMovingToRamp)
        {
            FindClosestRamp(transform);
        }

        if (bMovingToRamp && bGrounded)
        {
            MoveOnRamp(transform);
        }

    }

    public void MoveOnRamp(Transform enemyUnit)
    {
        normalizedDirection = (targetPosition - enemyUnit.position).normalized;
        enemyUnit.position += normalizedDirection * speed * Time.deltaTime;

        if (OnRamp())
        {
            Vector3 newYPos = new Vector3(enemyUnit.position.x, hitInfo.point.y + height, enemyUnit.position.z);
            oldYPos = newYPos;
            enemyUnit.position = newYPos;
        }

        if (Vector3.Distance(enemyUnit.position, targetPosition) <= 0.5f)
        {
            FindNextRampPoint(currentRampPoint);
            bMovingToRamp = false;
        }
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

    private bool OnRamp()
    {
        if (!bGrounded)
        {
            return false;
        }

        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, height + heightPadding))
        {
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }

        return false;
    }

    public Vector3 FindClosestRamp(Transform enemyUnit)
    {
        bMovingToRamp = true;
        Transform closest = enemyUnit;

        float distance = Mathf.Infinity;

        closestDistToEnemy = Vector3.Distance(enemyUnit.position, target.position);
        closestDistToPlayer = closestDistToEnemy;

        //go through each ramp to find closest one to both enemy and player
        for (int i = 0; i < rampWaypoints.Length; i++)
        {

            distance = Vector3.Distance(enemyUnit.position, rampWaypoints[i].transform.position);
            print(transform.name + " is looking for closest ramp to player");

            
            if (distance < closestDistToEnemy && Mathf.Abs(enemyUnit.position.y - rampWaypoints[i].transform.position.y) <= 1f && currentRampPoint != rampWaypoints[i])
            {
                
                var ramp = rampWaypoints[i].transform.parent;

                
                foreach (Transform child in ramp)
                {
                    if (child.transform != rampWaypoints[i].transform && Mathf.Abs(target.position.y - child.position.y) <= 1)
                    {
                        closestDistToEnemy = distance;
                        closest = rampWaypoints[i].transform;
                        currentRampPoint = rampWaypoints[i];
                    }
                }

                //TODO: find way from ramp to ramp if there is level between enemy and player - necessary?
            }
            else
            {
                print(transform.name + " is just going to closest ramp point");
                closest = rampWaypoints[i].transform;
            }
        }

        targetPosition = closest.transform.position;
        return targetPosition;
    }

    public void FindNextRampPoint(GameObject currentRampPoint)
    {
        // find ramp enemy is on
        var ramp = currentRampPoint.transform.parent;

        GameObject nextRampPoint = null;

        foreach (Transform child in ramp)
        {
            if (child != currentRampPoint)
            {
                nextRampPoint = child.gameObject;
            }
            else
            {
                return;
            }
        }

        targetPosition = nextRampPoint.transform.position;
    }

    public Vector3 GetTargetPosition()
    {
        return targetPosition;
    }
}
