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

    [SyncVar]
    private int currentColorIndex = 0;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        UpdateColor();
    }

    [Command(requiresAuthority = false)]
    public void CmdCycleColor()
    {
        currentColorIndex = (currentColorIndex + 1) % colorOptions.Count;
        RpcUpdateColor(currentColorIndex);

        // Optionally re-check puzzle after color is changed
        ColorPuzzleManager manager = FindObjectOfType<ColorPuzzleManager>();
        if (manager != null)
            manager.CheckIfPuzzleSolved();
    }

    [ClientRpc]
    private void RpcUpdateColor(int index)
    {
        currentColorIndex = index;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        rend.material.color = colorOptions[currentColorIndex];
    }

    public int GetCurrentColorIndex()
    {
        return currentColorIndex;
    }
}
