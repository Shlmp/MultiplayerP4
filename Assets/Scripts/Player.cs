using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class Player : NetworkBehaviour
{
    #region Parameters
    public Rigidbody rb;
    private Camera playerCamera;

    public float speed = 6f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 100f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    private bool isGrounded;
    private float xRotation = 0f;


    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Add your validation code here after the base.OnValidate(); call.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
    }

    // NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.
    void Awake()
    {
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCamera = GetComponentInChildren<Camera>();
        if (!isLocalPlayer) playerCamera.enabled = false;
    }

    void Update()
    {
        HandleMouseLook();
        Move();
        HandleJumping();

        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.T))
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, 3f))
            {
                Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red, 1f);
                Debug.Log("Raycast hit: " + hit.transform.name);

                // Handle SlideDoor (Lever)
                SlideDoor door = hit.transform.GetComponent<SlideDoor>();
                if (door != null)
                {
                    NetworkIdentity netId = door.GetComponent<NetworkIdentity>();
                    if (netId != null)
                    {
                        Debug.Log("Sending command to SlideDoor...");
                        CmdRequestDoorToggle(netId);
                    }
                    return; // skip rest
                }

                // Handle InputCube (ColorCycler)
                ColorCycler cube = hit.transform.GetComponent<ColorCycler>();
                if (cube != null)
                {
                    Debug.Log("Sending command to ColorCycler...");
                    cube.CmdCycleColor();
                }
            }
        }



    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        Vector3 velocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

        rb.linearVelocity = velocity;
    }

    void HandleJumping()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down * 0.5f, groundCheckDistance, groundMask);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    [Command]
    void CmdRequestDoorToggle(NetworkIdentity leverNetId)
    {
        Debug.Log("Command received on server");

        SlideDoor lever = leverNetId.GetComponent<SlideDoor>();
        if (lever != null)
        {
            lever.ToggleDoor();
        }
        else
        {
            Debug.LogWarning("Lever not found on server.");
        }
    }


    #endregion

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        bool isPlayerA = isServer; // host = Player A
        string[] hideTags = isPlayerA
            ? new[] { "InputCube1", "InputCube2", "InputCube3", "InputCube4", "InputCube5" }
            : new[] { "PasswordCube1", "PasswordCube2", "PasswordCube3", "PasswordCube4", "PasswordCube5" };

        foreach (string tag in hideTags)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects)
            {
                obj.SetActive(false);
                Debug.Log($"[OnStartLocalPlayer] Deactivated object with tag {tag}: {obj.name}");
            }
        }
    }




    /// <summary>
    /// Called when the local player object is being stopped.
    /// <para>This happens before OnStopClient(), as it may be triggered by an ownership message from the server, or because the player object is being destroyed. This is an appropriate place to deactivate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStopLocalPlayer() {}

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion
}
