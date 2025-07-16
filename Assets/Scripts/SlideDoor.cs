using UnityEngine;
using Mirror;

public class SlideDoor : NetworkBehaviour
{
    [Header("Movement")]
    public float slideDistance = 3f;
    public float speed = 2f;

    [Header("Lever Rotation")]
    public float rotationAngle = 45f;

    [SyncVar(hook = nameof(OnDoorStateChanged))]
    private bool isOpened = false;

    private GameObject door;
    private Vector3 closedPosition;
    private Vector3 openedPosition;
    private Quaternion originalLeverRotation;
    private Quaternion activatedLeverRotation;

    private bool isMoving = false;
    private Vector3 targetPosition;

    [Header("Target Positions")]
    public Transform openTarget;
    public Transform closedTarget;


    void Start()
    {
        door = GameObject.FindGameObjectWithTag("SlideDoor");

        if (openTarget != null && closedTarget != null)
        {
            openedPosition = openTarget.position;
            closedPosition = closedTarget.position;
        }
        else
        {
            Debug.LogWarning("Target transforms not assigned!");
        }


        originalLeverRotation = transform.rotation;
        activatedLeverRotation = originalLeverRotation * Quaternion.Euler(0f, 0f, rotationAngle);
    }
    void FixedUpdate()
    {
        if (door == null || !isMoving) return;

        door.transform.position = Vector3.MoveTowards(door.transform.position, targetPosition, speed * Time.fixedDeltaTime);
        if (Vector3.Distance(door.transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
        }
    }

    void OnDoorStateChanged(bool oldState, bool newState)
    {
        Debug.Log("OnDoorStateChanged fired! New state: " + newState);

        if (door == null)
            door = GameObject.FindGameObjectWithTag("SlideDoor");

        if (openTarget != null && closedTarget != null)
        {
            openedPosition = openTarget.position;
            closedPosition = closedTarget.position;
        }

        targetPosition = newState ? openedPosition : closedPosition;
        isMoving = true;

        transform.rotation = newState ? activatedLeverRotation : originalLeverRotation;
    }


    [Command]
    public void CmdToggleDoor()
    {
        isOpened = !isOpened;
        Debug.Log($"[Server] Door is now {(isOpened ? "Opened" : "Closed")}");
    }

    public void ToggleDoor()
    {
        if (!isServer) return;

        isOpened = !isOpened;
        Debug.Log($"[Server] Door is now {(isOpened ? "Opened" : "Closed")}");
    }

}
