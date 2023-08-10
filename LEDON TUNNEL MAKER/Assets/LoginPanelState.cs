using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginPanelState : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject loginPanel;
    
    public GameObject accountPanel;
    public bool accountPannelOn;
    public bool loginPanelOn;
    public UIManager uiManager;
    public GameObject adminButton;
    
    void Start()
    {
        loginPanelOn= false;
        accountPannelOn= false;
    }
    private void Update()
    {
        if (uiManager.accountType == "admin")
        {
            adminButton.SetActive(true);
        }
        else
        {
            adminButton.SetActive(false);
        }
    }

    public void LoginPanelButtonPres()
    {
       

        if (uiManager.accountUsername == "")
        {
            if (!loginPanelOn)
            {
                loginPanel.SetActive(true);
                loginPanelOn = true;
            }
            else
            {
                loginPanel.SetActive(false);
                loginPanelOn = false;
            }
        }
        else if(uiManager.accountUsername != "")
        {
            if (!accountPannelOn)
            {
                accountPanel.SetActive(true);
                loginPanel.SetActive(false) ;
                accountPannelOn = true;
            }
            else
            {
                accountPanel.SetActive(false);
                accountPannelOn = false;
            }
        }
    }

}
