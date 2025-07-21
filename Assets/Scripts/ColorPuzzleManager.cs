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
    public GameObject door;

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
        RpcActivateAllInputCubes();
    }

    [Server]
    public void CheckIfPuzzleSolved()
    {
        for (int i = 1; i <= 5; i++)
        {
            string inputTag = "InputCube" + i;
            string passwordTag = "PasswordCube" + i;

            GameObject inputCube = GameObject.FindGameObjectWithTag(inputTag);
            if (inputCube == null || !passwordColors.ContainsKey(passwordTag))
                return;

            Color targetColor = passwordColors[passwordTag];
            int targetIndex = colorOptions.IndexOf(targetColor);

            ColorCycler cycler = inputCube.GetComponent<ColorCycler>();
            if (cycler == null)
                return;

            int currentIndex = cycler.GetCurrentColorIndex();

            if (currentIndex != targetIndex)
            {
                door.SetActive(true);
                puzzleSolved = false;
                return;
            }
        }

        if (!puzzleSolved)
        {
            puzzleSolved = true;
            Debug.Log("[ColorPuzzle] Puzzle solved!");
            door.SetActive(false);
        }
    }


    [ClientRpc]
    void RpcActivateAllInputCubes()
    {
        for (int i = 1; i <= 5; i++)
        {
            string tag = "InputCube" + i;
            GameObject obj = GameObject.FindGameObjectWithTag(tag);
            if (obj != null)
                obj.SetActive(true);
        }
    }

    bool ColorsAreEqual(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

}
