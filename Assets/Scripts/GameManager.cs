using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //GameManager is essentially the backbone of your game, acting as a centralized controller for managing the overall game state and various game-wide systems.Here are a few key roles it can play:

    //Scene Management: Transitioning between different game scenes (like moving from a main menu to a gameplay scene).
    //State Management: Keeping track of the game’s state, like whether the game is paused, in progress, or over.
    //Data Persistence: Storing data that should persist between scenes, such as player scores, settings, or achievements.
    //Global Access Point: Providing a single point of access for other scripts to call common game functions or get game-wide data.

    public Action<int> OpenComicViewEvent;
    public UIManager UIManager;
    public GameState CurrentState { get; private set; }
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize your game
        SetState(GameState.Playing);
    }

    void Update()
    {
        // Update game state
    }

    // Your additional methods here
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        // Broadcast the new state to other systems if needed
        Debug.Log($"Game state changed to: {newState}");
    }
}



public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}
