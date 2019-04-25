using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRampMovement : MonoBehaviour
{
    GameObject[] rampWaypoints;
    GameObject floor;
    GameObject currentRampPoint;

    public Transform target;

    RaycastHit hitDown;
    RaycastHit hitInfo;

    private Vector3 targetPosition;
    private Vector3 normalizedDirection;
    private Vector3 oldYPos;

    public float height = 1f;
    public float heightPadding = 0.1f;
    public float speed = 5;

    private float closestDistToEnemy;
    private float closestDistToPlayer;

    private string floorName;

    public LayerMask ground;

    private bool bGrounded;
    private bool bMovingToRamp = false;
    bool bArrivedToRamp = false;

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
        if (!bArrivedToRamp) // TODO: THIS IS NOT NEEDED WHEN CALLING FROM ENEMY SCRIPT? - FIGURE OUT WHY
        {
            FindClosestRamp(enemyUnit);
        }

        if (OnRamp(transform))
        {
            Vector3 newYPos = new Vector3(enemyUnit.position.x, hitInfo.point.y + height, enemyUnit.position.z);
            oldYPos = newYPos;
            enemyUnit.position = newYPos;
        }

        if (Vector3.Distance(enemyUnit.position, targetPosition) <= 1f)
        {
            FindNextRampPoint();

            if (!bArrivedToRamp)
            {
                bArrivedToRamp = true;
            }
            else
            {
                bArrivedToRamp = false;
            }
        }

        normalizedDirection = (targetPosition - enemyUnit.position).normalized;
        enemyUnit.position += normalizedDirection * speed * Time.deltaTime;
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

    public bool OnRamp(Transform enemyTransform)
    {
        if (!bGrounded)
        {
            return false;
        }

        RaycastHit hit;

        if (Physics.Raycast(enemyTransform.position, Vector3.down, out hit, height + heightPadding))
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

        //TODO: does this find closest to player and not enemy? - it seems next ramp point changes to one closer to enemy

        Transform closest = enemyUnit;

        float distance = Mathf.Infinity;

        var dir = enemyUnit.position - target.position;
        closestDistToEnemy = dir.magnitude;
        closestDistToPlayer = closestDistToEnemy;

        //go through each ramp to find closest one to both enemy and player
        for (int i = 0; i < rampWaypoints.Length; i++)
        {

            distance = Vector3.Distance(enemyUnit.position, rampWaypoints[i].transform.position);

            if (distance < closestDistToEnemy && Mathf.Abs(enemyUnit.position.y - rampWaypoints[i].transform.position.y) <= 2f && currentRampPoint != rampWaypoints[i])
            {

                var ramp = rampWaypoints[i].transform.parent;

                foreach (Transform child in ramp)
                {
                    if (child.transform != rampWaypoints[i].transform && Mathf.Abs(target.position.y - child.position.y) <= 1) //TODO: this finds ramp point on same floor as player - need to find point on enemy's floor and make it targetPosition
                    {
                        closestDistToEnemy = distance;
                        closest = rampWaypoints[i].transform;
                        currentRampPoint = rampWaypoints[i];
                    }
                }
            }
        }

        targetPosition = closest.transform.position;

        return targetPosition;
    }

    public void FindNextRampPoint()
    {
        bArrivedToRamp = false;

        var ramp = currentRampPoint.transform.parent;

        GameObject nextRampPoint = null;

        foreach (Transform child in ramp)
        {
            if (child != currentRampPoint)
            {
                nextRampPoint = child.gameObject;
            }
        }

        targetPosition = nextRampPoint.transform.position;
    }

    public Vector3 GetTargetPosition()
    {
        return targetPosition;
    }
}
