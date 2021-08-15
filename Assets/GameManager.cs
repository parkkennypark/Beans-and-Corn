﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public delegate void GameAction();
    public event GameAction OnGameStart;

    public static GameManager instance;
    public Animator animator;
    public List<Grabbable> crops = new List<Grabbable>();

    public TextMeshProUGUI lossText, resetText, dayText;
    public float cameraDamper = 0.4f;
    public float cameraSmoothSpeed = 4f;

    public bool gameStarted;

    private bool won;
    private bool lost;
    private int currentDay;
    private Transform lossTarget;

    public Vector3 cameraBaseRot;
    public Transform cameraGameTransform;

    [TextArea(3, 10)]
    public string[] failMessages;
    [TextArea(3, 10)]
    public string winText;

    void Awake()
    {
        instance = this;
        // cameraBaseRot = Camera.main.transform.rotation;
    }

    void Update()
    {
        Transform camera = Camera.main.transform;

        if (!gameStarted)
        {
            return;
        }

        if (!lost && !won)
        {
            camera.position = Vector3.Lerp(camera.position, cameraGameTransform.position, Time.deltaTime * 3f);
        }


        if (!lost)
        {
            Quaternion toPlayerRot = Quaternion.LookRotation(Player.instance.transform.position - camera.position, Vector3.up);
            Quaternion dampedRot = Quaternion.Lerp(cameraGameTransform.rotation, toPlayerRot, cameraDamper);
            camera.rotation = Quaternion.Lerp(camera.rotation, dampedRot, Time.deltaTime * cameraSmoothSpeed);
        }

        if (lost)
        {
            Quaternion targetRot = Quaternion.LookRotation(lossTarget.position - camera.position, Vector3.up);
            camera.rotation = Quaternion.Lerp(camera.rotation, targetRot, Time.deltaTime * 2f);
            camera.Translate(camera.forward * Time.deltaTime * 1);
        }


    }

    public void StartGame()
    {
        gameStarted = true;
        if (OnGameStart != null)
        {
            OnGameStart();
        }
    }

    public void LoseGame(int loseCode, Transform lossTarget)
    {
        if (lost || won)
        {
            return;
        }

        lost = true;
        this.lossTarget = lossTarget;
        lossText.text = failMessages[loseCode];
        animator.SetTrigger("lost");
    }

    public void WinGame()
    {
        if (lost || won)
        {
            return;
        }
        won = true;
        lossText.text = winText;
        animator.SetTrigger("lost");
        resetText.text = "Click here to return to the menu.";
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddToCrops(Grabbable crop)
    {
        print("CROP ADDED");
        crops.Add(crop);
        foreach (Truck.Request request in Truck.instance.requests)
        {
            if (request.itemName == crop.itemName)
            {
                print(request.itemName);
                request.amount++;
            }
        }
    }

    public void NextDay()
    {
        currentDay++;
        if (currentDay > 5)
        {
            currentDay = 1;
        }
        UpdateDayText();
    }

    public void PreviousDay()
    {
        currentDay--;
        if (currentDay < 1)
        {
            currentDay = 5;
        }
        UpdateDayText();
    }

    private void UpdateDayText()
    {
        dayText.text = "Day " + currentDay;
    }
}
