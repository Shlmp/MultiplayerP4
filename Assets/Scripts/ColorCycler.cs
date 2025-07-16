using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ColorCycler : NetworkBehaviour
{
    public List<Color> colorOptions = new List<Color>
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta
    };

    private int currentColorIndex = 0;
    private Renderer rend;
    private ColorPuzzleManager puzzleManager;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = colorOptions[currentColorIndex];

        if (isServer)
        {
            puzzleManager = FindObjectOfType<ColorPuzzleManager>();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdCycleColor()
    {
        currentColorIndex = (currentColorIndex + 1) % colorOptions.Count;
        Color newColor = colorOptions[currentColorIndex];

        RpcApplyColor(newColor);

        if (puzzleManager == null)
            puzzleManager = FindObjectOfType<ColorPuzzleManager>();

        if (puzzleManager != null)
            puzzleManager.CheckIfPuzzleSolved();
    }

    [ClientRpc]
    private void RpcApplyColor(Color color)
    {
        if (rend == null)
            rend = GetComponent<Renderer>();

        rend.material.color = color;
    }
}
