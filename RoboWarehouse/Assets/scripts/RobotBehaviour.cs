using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RobotBehaviour : MonoBehaviour
{
    public enum RobotState
    {
        Moving,
        WaitingAtDestination
    }

    [Header("Route")]
    [Tooltip("Add the pallets in order: Pallet 0, Pallet 1, etc.")]
    public Transform[] pallets;
    public Transform collectionPoint;
    public Transform deliveryPoint;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float arrivalDistance = 0.1f;
    public float stopDuration = 1f;

    [Header("Pathfinding")]
    [Tooltip("How far from a pallet/collection point to look for a walkable NavMesh position.")]
    public float destinationSearchRadius = 2f;

    [Header("Status Display")]
    public float textHeight = 2f;
    public float textSize = 0.15f;
    public Color textColor = Color.green;

    [Header("Current Behaviour")]
    public RobotState currentState = RobotState.Moving;

    private NavMeshAgent agent;
    private Transform target;
    private float waitTimer;
    private TextMesh statusText;
    private string targetName;

    void Start()
    {
        if (pallets == null || pallets.Length == 0 || pallets[0] == null || collectionPoint == null || deliveryPoint == null)
        {
            Debug.LogError("Assign Pallet 0 and the Collection Point at minimum.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < pallets.Length; i++)
        {
            if (pallets[i] == null)
            {
                Debug.LogError("Missing reference for Pallet " + i, this);
                enabled = false;
                return;
            }
        }

        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = Mathf.Max(0f, arrivalDistance);
        agent.updateRotation = true;

        // Begin at Pallet 0
        NavMeshHit startHit;
        if (!NavMesh.SamplePosition(pallets[0].position, out startHit,
            destinationSearchRadius, NavMesh.AllAreas))
        {
            Debug.LogError("Pallet 0 is not near a baked NavMesh.", this);
            enabled = false;
            return;
        }

        if (!agent.Warp(startHit.position))
        {
            Debug.LogError("Could not place robot on the NavMesh. Check the baked surface.", this);
            enabled = false;
            return;
        }

        CreateStatusDisplay();
        SetRandomTarget(pallets[0]);
    }

    void Update()
    {
        if (target == null || !agent.isOnNavMesh)
            return;

        agent.speed = speed;
        agent.stoppingDistance = Mathf.Max(0f, arrivalDistance);

        if (currentState == RobotState.Moving)
        {
            MoveTowardsTarget();
        }
        else if (currentState == RobotState.WaitingAtDestination)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= stopDuration)
                GoToNextDestination();
        }
    }

    void MoveTowardsTarget()
    {
        if (agent.pathPending)
            return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid ||
            agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.LogWarning("No complete path to " + targetName + ". Choosing another destination.", this);
            SetRandomTarget(target);
            return;
        }

        // follows corners
        if (agent.remainingDistance <= agent.stoppingDistance + 0.05f &&
            (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f))
        {
            agent.isStopped = true;
            currentState = RobotState.WaitingAtDestination;
            waitTimer = 0f;
            SetStatus("Arrived at " + targetName, Color.yellow);
            Debug.Log("Arrived at " + targetName);
        }
    }

    void GoToNextDestination()
    {
        SetRandomTarget(target);
    }

    void SetRandomTarget(Transform excludeTarget)
    {
        int total = pallets.Length + 2;
        int firstChoice = Random.Range(0, total);

        for (int attempt = 0; attempt < total; attempt++)
        {
            int index = (firstChoice + attempt) % total;
            Transform candidate = index < pallets.Length
                ? pallets[index]
                : (index == pallets.Length ? collectionPoint : deliveryPoint);

            if (candidate == excludeTarget)
                continue;

            if (SetTarget(candidate, GetDestinationName(candidate)))
                return;
        }

        target = null;
        agent.isStopped = true;
        SetStatus("No reachable destinations", Color.red);
        Debug.LogWarning("Robot cannot reach any other destination. Check NavMesh connections and pallet positions.", this);
    }

    bool SetTarget(Transform newTarget, string destinationName)
    {
        NavMeshHit destinationHit;
        if (!NavMesh.SamplePosition(newTarget.position, out destinationHit,
            destinationSearchRadius, agent.areaMask))
            return false;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(destinationHit.position, path) ||
            path.status != NavMeshPathStatus.PathComplete)
            return false;

        target = newTarget;
        targetName = destinationName;
        currentState = RobotState.Moving;
        agent.isStopped = false;
        agent.SetPath(path);

        SetStatus("Moving to " + targetName, textColor);
        Debug.Log("Moving to " + targetName);
        return true;
    }

    string GetDestinationName(Transform destination)
    {
        for (int i = 0; i < pallets.Length; i++)
        {
            if (pallets[i] == destination)
                return "Pallet " + i;
        }

        if (destination == collectionPoint)
            return "Collection Point";

        if (destination == deliveryPoint)
            return "Delivery Point";

        return destination.name;
    }

    void CreateStatusDisplay()
    {
        GameObject label = new GameObject("Robot Status Text");
        label.transform.SetParent(transform, false);
        label.transform.localPosition = Vector3.up * textHeight;

        statusText = label.AddComponent<TextMesh>();
        statusText.text = "Starting";
        statusText.fontSize = 48;
        statusText.characterSize = textSize;
        statusText.anchor = TextAnchor.MiddleCenter;
        statusText.alignment = TextAlignment.Center;
        statusText.color = textColor;

        MeshRenderer renderer = label.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 10;
    }

    void SetStatus(string message, Color colour)
    {
        if (statusText == null)
            return;

        statusText.text = message;
        statusText.color = colour;
    }

    void LateUpdate()
    {
        if (statusText != null && Camera.main != null)
        {
            Transform labelTransform = statusText.transform;
            labelTransform.rotation = Quaternion.LookRotation(
                labelTransform.position - Camera.main.transform.position);
        }
    }
}