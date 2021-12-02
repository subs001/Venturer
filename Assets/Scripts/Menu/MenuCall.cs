using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuCall : MonoBehaviour
{
    public GameObject menu;
    public PlayerMovement player;
    public ProjectileGun projectileGun;
    private bool isShowing = false;
    public Slider sens;

    void Start()
    {
        menu.SetActive(false);
        sens.value = player.sensitivity;
    }

    void Update()
    {
        player.sensitivity = 50 * sens.value;
        if (Input.GetKeyDown("tab"))
        {
            StartCoroutine(detectPush());
            if (Input.GetKeyDown("tab"))
            {
                handleButton();
            }
        }
    }

    IEnumerator detectPush()
    {
        yield return new WaitForSeconds(0.1f);
    }

    void handleButton()
    {
        if (isShowing)
        {
            resume();
        }
        else
        {
            pauseAndMenu();
        }
    }

    public void pauseAndMenu()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        menu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        isShowing = true;
        projectileGun.shootingEnabled = false;
        player.lockLook = true;
    }

    public void resume()
    {
        Time.timeScale = 1;
        menu.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isShowing = false;
        projectileGun.shootingEnabled = true;
        player.lockLook = false;
    }
}
