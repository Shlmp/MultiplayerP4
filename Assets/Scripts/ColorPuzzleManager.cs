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

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Set password colors
        for (int i = 1; i <= 5; i++)
        {
            string tag = "PasswordCube" + i;
            GameObject cube = GameObject.FindGameObjectWithTag(tag);
            if (cube != null)
            {
                Color randomColor = colorOptions[Random.Range(0, colorOptions.Count)];
                passwordColors[tag] = randomColor;

                Renderer rend = cube.GetComponent<Renderer>();
                rend.material.color = randomColor;
            }
        }

        // Debug password
        Debug.Log("[ColorPuzzle] Password:");
        foreach (var pair in passwordColors)
            Debug.Log($"{pair.Key}: {pair.Value}");

        // Find door
        door = GameObject.FindGameObjectWithTag("ColorDoor");
        if (door == null)
            Debug.LogWarning("[ColorPuzzle] Door with tag 'ColorDoor' not found.");
    }

    [Server]
    public void CheckIfPuzzleSolved()
    {
        for (int i = 1; i <= 5; i++)
        {
            string inputTag = "InputCube" + i;
            string passwordTag = "PasswordCube" + i;

            GameObject[] inputCube = GameObject.FindGameObjectsWithTag(inputTag);
            if (inputCube == null || !passwordColors.ContainsKey(passwordTag))
                return;

            Color targetColor = passwordColors[passwordTag];
            foreach (GameObject obj in inputCube)
            {
                Color inputColor = obj.GetComponent<Renderer>().material.color;
                if (inputColor != targetColor)
                    return;
            }


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
}
