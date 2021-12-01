using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] Text displayText;
    [SerializeField] Canvas tutorialCanvas;
    [SerializeField] CanvasGroup canvasGroup;

    bool isCanvasVisible = true;

    int counter = 0;
    string[] tutorialText = { "Use WASD To Move Around", "Press SPACE To Jump",
        "Hold SHIFT While Running to Crouch-Slide"}; 

    // Variable to store inputs that have been entered
    bool[] flag = new bool[4];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
            flag[i] = false;

        StartCoroutine("EnableCanvas");
    }

    // Update is called once per frame
    void Update()
    {
        if (counter < 3 && isCanvasVisible)
        {       
            switch (counter)
            {
                case 0:
                    {                        
                        displayText.text = tutorialText[counter];
                        if (Input.GetKey(KeyCode.W)) flag[0] = true;
                        if (Input.GetKey(KeyCode.A)) flag[1] = true;
                        if (Input.GetKey(KeyCode.S)) flag[2] = true;
                        if (Input.GetKey(KeyCode.D)) flag[3] = true;
                        if (flag[0] && flag[1] && flag[2] && flag[3])
                        {
                            counter++;
                        }
                        break;
                    }

                case 1:
                    {
                        displayText.text = tutorialText[counter];
                        if (Input.GetKey(KeyCode.Space))
                        {
                                counter++;                           
                        }
                        break;
                    }

                case 2:
                    {
                        displayText.text = tutorialText[counter];
                        if (Input.GetKey(KeyCode.LeftShift))
                            counter++;
                        break;
                    }
            }
        } 
        else
        {
            displayText.text = "Tutorial Complete";
            StartCoroutine("DisableCanvas");
        }
    }

    IEnumerator EnableCanvas()
    {
        yield return new WaitForSeconds(5);
        tutorialCanvas.gameObject.SetActive(true);
        isCanvasVisible = true;
    }

    IEnumerator DisableCanvas()
    {
        yield return new WaitForSeconds(5);
        tutorialCanvas.gameObject.SetActive(false);
        isCanvasVisible = false;
    }






}
