using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XboxCtrlrInput;

public class PlayerManager : Observable<Player[]>
{
    private static PlayerManager playerManager;
    public static PlayerManager Instance
    {
        get
        {
            if (!playerManager)
            {
                playerManager = new GameObject("PlayerManager").AddComponent<PlayerManager>(); ;
            }
            return playerManager;
        }
    }

    public class ControllerContainer
    {
        public ControllerContainer(int intId, XboxController controllerId, bool spawned)
        {
            this.intId = intId;
            this.controllerId = controllerId;
            this.spawned = spawned;
        }

        public int intId;
        public XboxController controllerId;
        public bool spawned;
    }

    public Transform[] playerSpawnPoint;
    public Material[] playerMaterials;

    public string playerResourceId = "Player";

    private bool[] controllersConnected;
    private ControllerContainer[] connected = new ControllerContainer[4];
    private Dictionary<int, Player> players = new Dictionary<int, Player>(4);

    private Dictionary<int, XboxController> intToXboxController = new Dictionary<int, XboxController>(4);
    private Dictionary<XboxController, int> xboxControllerToInt = new Dictionary<XboxController, int>(4);

    private void Awake()
    {
        playerManager = this;

        controllersConnected = new bool[4];

        // needed for conversions, this is cancer
        intToXboxController.Add(0, XboxController.First);
        intToXboxController.Add(1, XboxController.Second);
        intToXboxController.Add(2, XboxController.Third);
        intToXboxController.Add(3, XboxController.Fourth);
        xboxControllerToInt.Add(XboxController.First, 0);
        xboxControllerToInt.Add(XboxController.Second, 1);
        xboxControllerToInt.Add(XboxController.Third, 2);
        xboxControllerToInt.Add(XboxController.Fourth, 3);


        StartCoroutine(UpdateControllerList());
    }

    private void Update()
    {
        for (int i = 0; i < connected.Length; i++)
        {
            ControllerContainer controller = connected[i];
            if (controller == null) continue;

            if (XCI.GetButtonDown(XboxButton.Start, controller.controllerId))
            {
                if (controller.spawned)
                {
                    if (players.ContainsKey(i))
                        RemovePlayer(controller.intId);
                    controller.spawned = false;
                }
                else
                if (!controller.spawned)
                //{
                //    if (players.ContainsKey(i))
                //        RemovePlayer(controller.intId);
                //    controller.spawned = false;
                //}
                //else
                {
                    SpawnPlayer(controller.intId);
                    controller.spawned = true;
                }
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
        if (state && connected[id] == null) connected[id] = new ControllerContainer(id, intToXboxController[id], false);
    }

    private void SpawnPlayer(int i)
    {
        if (connected[i] == null) return;

        Transform spawnPoint = playerSpawnPoint[i];
        IPoolable o = PoolableFactory.Instance.Create<PlayerPoolable>(playerResourceId, spawnPoint.position, spawnPoint.rotation, null);
        Player p = (o as MonoBehaviour).gameObject.GetComponentInChildren<Player>();
        p.PlayerMaterial = playerMaterials[i];
        p.controller = intToXboxController[i];

        if (!players.ContainsKey(i)) players.Add(i, p);
        else players[i] = p;

        NotifyObservers(players.Values.ToArray());
    }

    private void RemovePlayer(int i)
    {
        if (players.ContainsKey(i))
        {
            players[i].transform.parent.GetComponent<IPoolable>().Destroy();
            players.Remove(i);

            NotifyObservers(players.Values.ToArray());
        }
    }

    public void SpawnPlayer(XboxController xboxController)
    {
        SpawnPlayer(xboxControllerToInt[xboxController]);
    }

    public void DespawnPlayer(XboxController xboxController)
    {
        RemovePlayer(xboxControllerToInt[xboxController]);
    }

    public Player GetPlayerByController(XboxController xboxController)
    {
        return players[xboxControllerToInt[xboxController]];
    }
}
