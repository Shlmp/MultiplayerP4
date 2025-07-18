using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ColorPuzzleManager : NetworkBehaviour
{
    [SyncVar]
    private bool puzzleSolved = false;

    private List<Color> colorOptions = new List<Color>
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta
    };

    private Dictionary<string, Color> passwordColors = new Dictionary<string, Color>();
    private GameObject door;

    [Header("Assign in Inspector")]
    public List<GameObject> passwordCubes;   // PasswordCube1...5
    public List<GameObject> inputCubes;      // InputCube1...5

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Assign random colors to PasswordCubes
        for (int i = 0; i < passwordCubes.Count; i++)
        {
            GameObject cube = passwordCubes[i];
            if (cube != null)
            {
                string tagKey = "PasswordCube" + (i + 1);
                Color randomColor = colorOptions[Random.Range(0, colorOptions.Count)];
                passwordColors[tagKey] = randomColor;

                Renderer rend = cube.GetComponent<Renderer>();
                rend.material.color = randomColor;
            }
        }

        // Debug password
        Debug.Log("[ColorPuzzle] Password:");
        foreach (var pair in passwordColors)
            Debug.Log($"{pair.Key}: {pair.Value}");

        // Find first door
        GameObject[] doors = GameObject.FindGameObjectsWithTag("ColorDoor");
        if (doors.Length > 0)
        {
            door = doors[0];
        }
        else
        {
            Debug.LogWarning("[ColorPuzzle] Door with tag 'ColorDoor' not found.");
        }

        RpcActivateInputCubesForLocalCamera();
    }

    [Server]
    public void CheckIfPuzzleSolved()
    {
        for (int i = 0; i < inputCubes.Count; i++)
        {
            string passwordTag = "PasswordCube" + (i + 1);
            GameObject inputCube = inputCubes[i];

            if (inputCube == null || !passwordColors.ContainsKey(passwordTag))
                return;

            Color inputColor = inputCube.GetComponent<Renderer>().material.color;
            Color targetColor = passwordColors[passwordTag];

            if (inputColor != targetColor)
                return;
        }

        if (!puzzleSolved)
        {
            puzzleSolved = true;
            Debug.Log("[ColorPuzzle] Puzzle solved!");

            if (door != null)
            {
                SlideDoor slideDoor = door.GetComponent<SlideDoor>();
                if (slideDoor != null)
                    slideDoor.ToggleDoor();
            }
        }
    }

    [ClientRpc]
    void RpcActivateInputCubesForLocalCamera()
    {
        // Only run on local client
        if (!isLocalPlayerCameraAvailable())
            return;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[ColorPuzzle] No main camera found.");
            return;
        }

        for (int i = 0; i < inputCubes.Count; i++)
        {
            GameObject cube = inputCubes[i];
            if (cube == null) continue;

            // Check if cube is in front of camera (rough visibility check)
            Vector3 dir = cube.transform.position - cam.transform.position;
            if (Vector3.Dot(cam.transform.forward, dir.normalized) > 0.5f)
            {
                cube.SetActive(true);
            }
            else
            {
                cube.SetActive(false); // hide if not for this player
            }
        }
    }

    private bool isLocalPlayerCameraAvailable()
    {
        return Camera.main != null && Camera.main.CompareTag("MainCamera");
    }
}
