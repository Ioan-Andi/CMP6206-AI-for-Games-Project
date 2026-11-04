
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotController : MonoBehaviour
{
    public AStarPathfinder pathfinder;

    public Transform collectionPoint;
    public Transform deliveryPoint;

    public float movementSpeed = 2f;
    public float rotationSpeed = 180f;
    public float collectionTime = 2f;
    public float deliveryTime = 2f;

    private bool carryingPallet = false;

    void Start()
    {
        StartCoroutine(RunDelivery());
    }

    IEnumerator RunDelivery()
    {
        while (true)
        {
            yield return StartCoroutine(
                FollowPath(collectionPoint.position)
            );

            Debug.Log("Collecting pallet...");

            yield return new WaitForSeconds(collectionTime);

            carryingPallet = true;
            Debug.Log("Pallet collected!");

            yield return StartCoroutine(
                FollowPath(deliveryPoint.position)
            );

            Debug.Log("Delivering pallet...");

            yield return new WaitForSeconds(deliveryTime);

            carryingPallet = false;
            Debug.Log("Pallet delivered!");

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator FollowPath(Vector3 destination)
    {
        List<Vector3> path = pathfinder.FindPath(
            transform.position,
            destination
        );

        if (path == null || path.Count == 0)
        {
            Debug.LogError("Robot cannot find a route!");
            yield break;
        }

        foreach (Vector3 waypoint in path)
        {
            Vector3 target = new Vector3(
                waypoint.x,
                transform.position.y,
                waypoint.z
            );

            while (Vector3.Distance(
                transform.position, target) > 0.05f)
            {
                Vector3 direction =
                    (target - transform.position).normalized;

                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation =
                        Quaternion.LookRotation(direction);

                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    movementSpeed * Time.deltaTime
                );

                yield return null;
            }
        }
    }
}
