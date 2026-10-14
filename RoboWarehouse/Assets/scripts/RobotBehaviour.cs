using UnityEngine;

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

    [Header("Movement Settings")]
    public float speed = 2f;
    public float arrivalDistance = 0.1f;
    public float stopDuration = 1f;

    [Header("Status Display")]
    public float textHeight = 2f;
    public float textSize = 0.15f;
    public Color textColor = Color.green;

    [Header("Current Behaviour")]
    public RobotState currentState = RobotState.Moving;

    private Transform target;
    private int routeIndex = 1;
    private float waitTimer;
    private TextMesh statusText;
    private string targetName;

    void Start()
    {
        if (pallets == null || pallets.Length == 0 ||
            pallets[0] == null || collectionPoint == null)
        {
            Debug.LogError(
                "Assign Pallet 0 and the Collection Point at minimum.");
            enabled = false;
            return;
        }

        // Validate every pallet in the route.
        for (int i = 0; i < pallets.Length; i++)
        {
            if (pallets[i] == null)
            {
                Debug.LogError("Missing reference for Pallet " + i);
                enabled = false;
                return;
            }
        }

        transform.position = pallets[0].position;

        CreateStatusDisplay();

        SetRandomTarget(pallets[0]);
    }

    void Update()
    {
        if (target == null)
            return;

        if (currentState == RobotState.Moving)
        {
            MoveTowardsTarget();
        }
        else if (currentState == RobotState.WaitingAtDestination)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= stopDuration)
            {
                GoToNextDestination();
            }
        }
    }

    void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // Face direction moving
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Vector3.Distance(transform.position, target.position)
            <= arrivalDistance)
        {
            transform.position = target.position;

            currentState = RobotState.WaitingAtDestination;
            waitTimer = 0f;

            SetStatus("Arrived at " + targetName, Color.yellow);

            Debug.Log("Arrived at " + targetName);
        }
    }

    void GoToNextDestination()
    {
        currentState = RobotState.Moving;

        SetRandomTarget(target);
    }

    void SetRandomTarget(Transform excludeTarget)
    {
        // Build a list of all destinations.
        Transform[] destinations =
            new Transform[pallets.Length + 1];

        for (int i = 0; i < pallets.Length; i++)
        {
            destinations[i] = pallets[i];
        }

        destinations[pallets.Length] = collectionPoint;

        // ignores the last destination visited
        int availableCount = 0;

        foreach (Transform destination in destinations)
        {
            if (destination != excludeTarget)
                availableCount++;
        }

        if (availableCount == 0)
            return;

        // Select one pallet randomly.
        int choice = Random.Range(0, availableCount);
        Transform nextTarget = null;

        foreach (Transform destination in destinations)
        {
            if (destination == excludeTarget)
                continue;

            if (choice == 0)
            {
                nextTarget = destination;
                break;
            }

            choice--;
        }

        if (nextTarget != null)
        {
            SetTarget(nextTarget, GetDestinationName(nextTarget));
        }
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

        return destination.name;
    }

    void CreateStatusDisplay()
    {
        GameObject label = new GameObject("Robot Status Text");

        label.transform.SetParent(transform, false);
        label.transform.localPosition = Vector3.up * textHeight;

        statusText = label.AddComponent<TextMesh>();
        statusText.text = "Starting...";
        statusText.fontSize = 48;
        statusText.characterSize = textSize;
        statusText.anchor = TextAnchor.MiddleCenter;
        statusText.alignment = TextAlignment.Center;
        statusText.color = textColor;

        // Draw the text in front of other objects
        MeshRenderer renderer = label.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 10;
    }

    void SetTarget(Transform newTarget, string destinationName)
    {
        target = newTarget;
        targetName = destinationName;

        currentState = RobotState.Moving;

        SetStatus("Moving to " + targetName, textColor);

        Debug.Log("Moving to " + targetName);
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
        // Keep the label facing the camera.
        if (statusText != null && Camera.main != null)
        {
            Transform labelTransform = statusText.transform;

            labelTransform.rotation = Quaternion.LookRotation(
                labelTransform.position - Camera.main.transform.position
            );
        }
    }
}