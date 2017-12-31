using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;
using UnityEngine.UI;

public class GameManager : MonoBehaviour, Observer<Player[]>
{
    public int winCondiitonAmount = 1;
    public AnnounceText announceText;
    public ScoreText scoreText;
    public GameObject boxSpawner;

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

    public enum GameState { RESTARTING, PLAYING, WAITING, WINSCREEN }
    private GameState state = GameState.WAITING;

    private Player[] players;
    private int[] scores = new int[4];

    void Start()
    {
        Player[] players = new Player[0];
        PlayerManager.Instance.AddObserver(this);
        gameManager = this;
    }

    void Update()
    {
        if (players == null || players.Length < 1)
        {
            return;
        }

        if (state == GameState.WAITING)
        {
            RestartGame("Looks like wehave enough players");
        }
        else if(state == GameState.PLAYING)
        {
            foreach (Player p in players)
            {
                if(scores[PlayerToXboxInt(p)] >= winCondiitonAmount)
                {
                    TransitionNow(GameState.WINSCREEN, NotifyPlayers, "Looks like we have a winner");
                    WaitThenTransition(3, GameState.WINSCREEN, NotifyPlayers, "The " + p.controller + " player wins");
                    WaitThenTransition(6, GameState.WINSCREEN, NotifyPlayers, "Full rip everyone else");
                    WaitThenTransition(9, GameState.WINSCREEN, RestartGame, "Next round incoming");
                }
            }
        }
    }

    public void Notified(Player[] data)
    {
        players = data;
    }

    public void AddScore(Player p, int val)
    {
        if (p == null) return;

        scores[PlayerToXboxInt(p)]++;

        scoreText.SetScore(PlayerToXboxInt(p), scores[PlayerToXboxInt(p)]);
    }

    public void SetScore(Player p, int val)
    {
        if (p == null) return;

        scores[PlayerToXboxInt(p)] = val;

        scoreText.SetScore(PlayerToXboxInt(p), scores[PlayerToXboxInt(p)]);
    }

    private void ResetScores()
    {
        if (players != null)
        foreach (Player p in players)
        {
            SetScore(p, 0);
        }
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
        PlayerManager.Instance.SpawnPlayer(XboxController.First);
        PlayerManager.Instance.SpawnPlayer(XboxController.Second);
        PlayerManager.Instance.SpawnPlayer(XboxController.Third);
        PlayerManager.Instance.SpawnPlayer(XboxController.Fourth);
    }

    private void PlayGame()
    {
        WaitThenTransition(3, GameState.PLAYING, SpawnPlayers);
    }

    private void RestartGame(string startText)
    {
        TransitionNow(GameState.RESTARTING, NotifyPlayers, startText);
        WaitThenTransition(2, GameState.RESTARTING, ResetScores);
        WaitThenTransition(3, GameState.RESTARTING, NotifyPlayers, "Get ready, first to " + winCondiitonAmount + " boxes!");
        WaitThenTransition(3, GameState.RESTARTING, DespawnPlayers);
        WaitThenTransition(6, GameState.PLAYING, SpawnPlayers);
        IPoolable[] poolable = boxSpawner.GetComponentsInChildren<IPoolable>();

        foreach(IPoolable p in poolable)
        {
            p.Destroy();
        }
    }

    private void NotifyPlayers(string msg)
    {
        announceText.SetText(msg);
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

    private int PlayerToXboxInt(Player p)
    {
        if (p.controller == XboxController.First) return 0;
        else if (p.controller == XboxController.Second) return 1;
        else if (p.controller == XboxController.Third) return 2;
        else if (p.controller == XboxController.Fourth) return 3;
        return -1;
    }

}
