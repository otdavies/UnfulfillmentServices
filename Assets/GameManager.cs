using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class GameManager : MonoBehaviour, Observer<Player[]>
{
    private static GameManager gameManager;
    public static GameManager Instance
    {
        get
        {
            if (!gameManager)
            {
                gameManager = new GameObject("GameManager").AddComponent<GameManager>(); ;
            }
            return gameManager;
        }
    }

    public enum GameState { RESTARTING, PLAYING, WAITING }
    private Dictionary<XboxController, int> playerScores = new Dictionary<XboxController, int>(4);
    private GameState state = GameState.WAITING;

    private Player[] players;

    void Start()
    {
        Player[] players = new Player[0];

        playerScores.Add(XboxController.First, 0);
        playerScores.Add(XboxController.Second, 0);
        playerScores.Add(XboxController.Third, 0);
        playerScores.Add(XboxController.Fourth, 0);

        PlayerManager.Instance.AddObserver(this);
    }

    void Update()
    {
        if (players != null && players.Length > 1 && state == GameState.WAITING)
        {
            RestartGame();
        }
    }

    public void Notified(Player[] data)
    {
        players = data;
    }

    public void AddScore(Player p, int val)
    {
        playerScores[p.controller] += val;
    }

    private void ResetScores()
    {
        playerScores[XboxController.First] = 0;
        playerScores[XboxController.Second] = 0;
        playerScores[XboxController.Third] = 0;
        playerScores[XboxController.Fourth] = 0;
    }

    private void DespawnPlayers()
    {
        if (players != null)
        foreach (Player p in players)
        {
            PlayerManager.Instance.DespawnPlayer(p.controller);
        }
    }   

    private void SpawnPlayers()
    {
        if (players != null)
        foreach (Player p in players)
        {
            PlayerManager.Instance.SpawnPlayer(p.controller);
        }
    }

    private void PlayGame()
    {
        WaitThenTransition(3, GameState.PLAYING, SpawnPlayers);
    }

    private void RestartGame()
    {
        TransitionNow(GameState.RESTARTING, NotifyPlayers, "Restarting");
        WaitThenTransition(1, GameState.RESTARTING, DespawnPlayers);
        WaitThenTransition(3, GameState.PLAYING, SpawnPlayers);

    }

    private void NotifyPlayers(string msg)
    {
        Debug.Log(msg);
    }

    private void TransitionNow(GameState state, Action f)
    {
        this.state = state;
        f();
    }

    private void TransitionNow(GameState state, Action<string> f, string msg)
    {
        this.state = state;
        f(msg);
    }

    private void WaitThenTransition(float time, GameState state, Action f)
    {
        StartCoroutine(WaitTransition(time, state, f));
    }

    private void WaitThenTransition(float time, GameState state, Action<string> f, string msg)
    {
        StartCoroutine(WaitTransition(time, state, f, msg));
    }

    private IEnumerator WaitTransition(float time, GameState state, Action f)
    {
        yield return new WaitForSeconds(time);
        this.state = state;
        f();
    }
    private IEnumerator WaitTransition(float time, GameState state, Action<string> f, string msg)
    {
        yield return new WaitForSeconds(time);
        this.state = state;
        f(msg);
    }

}
