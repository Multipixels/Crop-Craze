using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RushManager : MonoBehaviour {

    [SerializeField]
    private AudioSource keyAudio;

    [SerializeField] 
    private AudioSource useKeyAudio;

    [SerializeField]
    private Tile tilePrefab;

    [SerializeField]
    private Transform cammy;

    public int level;

    [SerializeField]
    private Transform player;

    private readonly Map map = new Map();

    private Tile[,] mapTiles;

    private int tilesLeft;
    private int totalTiles;

    private float timeTaken;
    private int totalMoves;

    [SerializeField]
    private TMP_Text cropsCounter;
    [SerializeField]
    private TMP_Text movesCounter;
    [SerializeField]
    private TMP_Text timeCounter;

    [SerializeField]
    private GameObject pauseMenu;
    [SerializeField]
    private GameObject winMenu;

    private int totalKeys;

    private bool gameActive;

    // Start is called before the first frame update

    public void Start() {
        StartGame(0);
    }
    public void StartGame(int lvl) {

        GameObject[] tiles;

        tiles = GameObject.FindGameObjectsWithTag("tile");

        foreach (GameObject tile in tiles) {
            Destroy(tile);
        }

        level = lvl;
        GenerateField();
        gameActive = true;
        
        cropsCounter.text = $"Crops Left: {tilesLeft} / {totalTiles}";

        if (level == 0) {
            PlayerPrefs.SetInt("currentLevel", -1);
            timeTaken = 0f;
            movesCounter.text = "Moves Taken: 0";
            timeCounter.text = "Time: 00:00:00";
        }
        totalKeys = 0;
        Time.timeScale = 1;
    }

    private void Update() {
        if (gameActive == true) {
            timeTaken += Time.deltaTime;
            timeCounter.text = $"Time: {TimeSpan.FromSeconds(timeTaken).ToString("mm':'ss'.'ff")}";
        }
    }

    private void GenerateField() {

        mapTiles = new Tile[map.levels[level].GetLength(0), map.levels[level].GetLength(1)];

        for (int y = 0; y < map.levels[level].GetLength(0); y++) {
            for (int x = 0; x < map.levels[level].GetLength(1); x++) {
                Tile spawnedTile = Instantiate(tilePrefab, new Vector3(x, y, 1), Quaternion.identity);
                spawnedTile.name = $"Tile {x}, {y}";
                spawnedTile.GetComponent<Tile>().Initialize(map.levels[level][y, x]);

                if (map.levels[level][y, x] == 4) {
                    player.position = spawnedTile.transform.position;
                    player.position += new Vector3(0, 0, -1);
                }

                if (map.levels[level][y, x] == 2 || map.levels[level][y, x] == 6 || map.levels[level][y, x] == 7) {
                    tilesLeft += 1;
                    totalTiles += 1;
                } else if (map.levels[level][y, x] == 5 || map.levels[level][y, x] == 8) {
                    tilesLeft += 2;
                    totalTiles += 2;
                }

                mapTiles[y, x] = spawnedTile;
            }
        }

        cammy.transform.position = new Vector3((float)map.levels[level].GetLength(1) / 2 - 0.5f, (float)map.levels[level].GetLength(0) / 2 - 0.5f, -10);
        switch (level) {
            case 0: {
                    cammy.GetComponent<Camera>().orthographicSize = 4;
                    break;
                }
            case 1: {
                    cammy.GetComponent<Camera>().orthographicSize = 5;
                    break;
                }
            case 2: {
                    cammy.GetComponent<Camera>().orthographicSize = 5;
                    break;
                }
            case 3: {
                    cammy.GetComponent<Camera>().orthographicSize = 6;
                    break;
                }
            case 4: {
                    cammy.GetComponent<Camera>().orthographicSize = 6;
                    break;
                }
            case 5: {
                    cammy.GetComponent<Camera>().orthographicSize = 6;
                    break;
                }
            case 6: {
                    cammy.GetComponent<Camera>().orthographicSize = 10;
                    break;
                }
            case 7: {
                    cammy.GetComponent<Camera>().orthographicSize = 7;
                    break;
                }
            case 8: {
                    cammy.GetComponent<Camera>().orthographicSize = 10;
                    break;
                }
            case 9: {
                    cammy.GetComponent<Camera>().orthographicSize = 16;
                    break;
                }
        }
    }

    public bool PreMove(Vector2 desiredLocation) {
        int x = (int)desiredLocation.x;
        int y = (int)desiredLocation.y;
        int id = mapTiles[y, x].GetId();

        if (id == 1) {
            return false;
        }

        if (gameActive == false) {
            return false;
        }

        if ((id == 7 || id == 8) && totalKeys == 0) {
            return false;
        }

        return true;
    }
    public void PostMove(Transform newLocation) {
        int x = (int)newLocation.position.x;
        int y = (int)newLocation.position.y;

        totalMoves += 1;
        movesCounter.text = $"Moves Taken: {totalMoves}";

        if (mapTiles[y, x].GetId() == 2 || mapTiles[y, x].GetId() == 5) {
            tilesLeft -= 1;
        } else if (mapTiles[y, x].GetId() == 6) {
            tilesLeft -= 1;
            totalKeys += 1;

            keyAudio.volume = PlayerPrefs.GetFloat("audioVolume");
            keyAudio.Play();

        } else if (mapTiles[y, x].GetId() == 7 || mapTiles[y, x].GetId() == 8) {
            tilesLeft -= 1;
            totalKeys -= 1;

            useKeyAudio.volume = PlayerPrefs.GetFloat("audioVolume");
            useKeyAudio.Play();
        }

        cropsCounter.text = $"Crops Left: {tilesLeft} / {totalTiles}";
        mapTiles[y, x].Progress();

        if (tilesLeft == 0) {
            Win();
        }

    }

    private void Win() {

        if (level == 2) {
            gameActive = false;
            Time.timeScale = 0;
            winMenu.SetActive(true);

            winMenu.transform.Find("WinMoves").GetComponent<TMP_Text>().text = $"Moves Taken: {totalMoves}";
            winMenu.transform.Find("WinTime").GetComponent<TMP_Text>().text = timeCounter.text;

            timeCounter.gameObject.transform.parent.gameObject.SetActive(false);
            movesCounter.gameObject.transform.parent.gameObject.SetActive(false);
            cropsCounter.gameObject.transform.parent.gameObject.SetActive(false);

            PlayerPrefs.SetInt($"rushStars", Math.Max(PlayerPrefs.GetInt($"rushStars"), 1));

            if (totalTiles / (float)totalMoves >= 0.8f) {
                winMenu.transform.Find("Star2Full").gameObject.SetActive(true);
                PlayerPrefs.SetInt($"rushStars", Math.Max(PlayerPrefs.GetInt($"rushStars"), 2));
            }

            if (totalTiles == totalMoves) {
                winMenu.transform.Find("Star3Full").gameObject.SetActive(true);
                PlayerPrefs.SetInt($"rushStars", Math.Max(PlayerPrefs.GetInt($"rushStars"), 3));
            }
        } else {
            StartGame(level + 1);
        }

    }

    public void Pause(InputAction.CallbackContext context) {
        if (context.performed && tilesLeft != 0) {
            PauseGame();
        }
    }

    public void PauseGame() {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        gameActive = !gameActive;

        if (Time.timeScale == 1) Time.timeScale = 0;
        else Time.timeScale = 1;
    }
}
