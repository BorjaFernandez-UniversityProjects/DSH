using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; //No reconoce el paquete

public class AmmunitionReloadScript : MonoBehaviour
{
    public int indicatorSpeed;
    
    public TextMeshProUGUI ammunitionCounter, scorePanel;
    public ProgressBar shieldBar; //Funciona aunque sala este error, no se porque sale

    private int absMax = 49;
    private int currentAmmunition, maxAmmunition, currentScore, currentShield;
    //Color Ranges:
    //-49[RED]-45[YELLOW]-6[GREEN]5[YELLOW]41[RED]49
    private int[] colorBarRanges = {-45, -6, 5, 41};



    void Start()
    {
        currentAmmunition = PlayerPrefs.GetInt("Ammo", 0);
        maxAmmunition = PlayerPrefs.GetInt("MaxAmmo", 100);
        currentShield = PlayerPrefs.GetInt("Shield", 100);
        currentScore = PlayerPrefs.GetInt("PlayerScore", 0);
        indicatorSpeed = PlayerPrefs.GetInt("reloadBarSpeed", 500);
        shieldBar.BarValue = currentShield;
        ammunitionCounter.text = currentAmmunition.ToString() + "/" + maxAmmunition.ToString();
        setAmmounitionCounterColor();
        scorePanel.text = "Score: " + currentScore.ToString();

        StartCoroutine(reloadAction());
    }


    void Update()
    {
        if(Input.touchCount==1 && Input.GetTouch(0).phase == TouchPhase.Began && currentAmmunition<maxAmmunition){
            if(transform.localPosition.x <= colorBarRanges[0] || transform.localPosition.x >= colorBarRanges[3]){
                currentAmmunition += (maxAmmunition/10);    //Red reload => +10% of ammo
            }else if(transform.localPosition.x <=colorBarRanges[1] || transform.localPosition.x>=colorBarRanges[2]){
                currentAmmunition += (maxAmmunition/5);     //Yellow reload => +20% of ammo
            }else{
                currentAmmunition += (maxAmmunition/2);     //Green reload => +50% of ammo
            }
            if(currentAmmunition > maxAmmunition){
                currentAmmunition = maxAmmunition;
            }
            ammunitionCounter.text = currentAmmunition.ToString() + "/" + maxAmmunition.ToString();
            setAmmounitionCounterColor();
        }
    }

    IEnumerator reloadAction(){
        bool right = true;
        while(true){
            if(transform.localPosition.x <= -absMax){
                right = true;
            }else if(transform.localPosition.x >= absMax){
                right = false;
            }

            if(right){
                transform.Translate(Vector3.right * indicatorSpeed * Time.deltaTime);
            }else{
                transform.Translate(Vector3.left * indicatorSpeed * Time.deltaTime);
            }
            
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }

    void setAmmounitionCounterColor(){
        if(currentAmmunition <= (maxAmmunition*0.1)){
            ammunitionCounter.color = Color.red;
        }else if(currentAmmunition <= (maxAmmunition*0.75)){
            ammunitionCounter.color = Color.yellow;
        }else{
            ammunitionCounter.color = Color.green;
        }
    }

    public void saveInformation(){
        PlayerPrefs.SetInt("Ammo", currentAmmunition);
        PlayerPrefs.SetInt("Shield", currentShield);
    }
}
