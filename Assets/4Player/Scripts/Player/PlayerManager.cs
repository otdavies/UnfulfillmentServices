using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class PlayerManager : MonoBehaviour
{
    private bool[] controllersConnected;

    private List<XboxController> connectedButNotPlaying = new List<XboxController>();
    private List<XboxController> connectedAndPlaying = new List<XboxController>();
    private List<Player> players = new List<Player>();

    private Dictionary<int, XboxController> intToXboxController = new Dictionary<int, XboxController>();

    private void Start()
    {
        controllersConnected = new bool[4];
        intToXboxController.Add(0, XboxController.First);
        intToXboxController.Add(1, XboxController.Second);
        intToXboxController.Add(2, XboxController.Third);
        intToXboxController.Add(3, XboxController.Fourth);

        StartCoroutine(UpdateControllerList());
    }

    private void Update()
    {
        for (int i = 0; i < connectedButNotPlaying.Count; i++)
        {
            XboxController controller = connectedButNotPlaying[i];
            if (XCI.GetButtonDown(XboxButton.Start, controller))
            {
                connectedAndPlaying.Add(controller);
                connectedButNotPlaying.Remove(controller);
            }
        }
    }

    private IEnumerator UpdateControllerList()
    {
        while (true)
        {
            for (int i = 0; i < 4; i++)
            {
                bool recent = XCI.IsPluggedIn(i+1);
                if (controllersConnected[i] != recent) ControllerListChange(i, recent);

                controllersConnected[i] = recent;
            }
            yield return new WaitForSeconds(1);
        }
    }

    private void ControllerListChange(int id, bool state)
    {
        if (state) connectedButNotPlaying.Add(intToXboxController[id]);
    }

    private void SpawnPlayer()
    {

    }
}
