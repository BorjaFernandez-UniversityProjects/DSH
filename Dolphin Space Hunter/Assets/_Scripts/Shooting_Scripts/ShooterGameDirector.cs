﻿/*
 * Copyright (c) Borja Fernández
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShooterGameDirector : MonoBehaviour
{

    #region Variables
    // Vamos a utilizar este gameDirector como lo que se conoce como "Singleton"
    // Para ello, tenemos una instancia de una clase, que apunta como referencia estática a sí misma.
    // De esta forma, podemos acceder a todos los atributos y métodos a través 
    private static ShooterGameDirector instance  = null;

    public GameObject player;
    public ShootingInterfaceManager shootingInterfaceManager;
    public AudioClip explosionSound;

    private GameObject activeFleets;
    private int numberOfCurrentFleets = 0;
    private int numberOfCurrentShips = 0;

    private int bonusModifier = 1;
    private int score = 0;
    private int remainingShield;
    private int ammunition, maxAmmunition;
    private int wave;
    private ShootingLevels_Container shootingLevel = new ShootingLevels_Container();
    private TextAsset easyDifficultyFile, mediumDifficultyFile, hardDifficultyFile;

    #endregion

    public static ShooterGameDirector Instance() { return instance; }

    #region UnityMethods

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        score = PlayerPrefs.GetInt("PlayerScore", 0);
        ammunition = PlayerPrefs.GetInt("Ammo", 0);
        loadDifficulty();
        PlayerPrefs.SetInt("RechargeAmmount", shootingLevel.rechargeAmmount);
        PlayerPrefs.SetInt("MaxAmmo", shootingLevel.maxAmmunition);
        Debug.Log("GameDirector Awake");
    }

    void Start()
    {

        Debug.Log("GameDirector Start");
    }

    void Update()
    {
        if (numberOfCurrentFleets < 3 || numberOfCurrentShips < 6)
        {

        }
    }

    #endregion

    public void missedShot()
    {
        addScore(-1);
    }

    public void shipHit()
    {
        addScore(5);
    }

    public void shipDestroyed(ShipController ship)
    {
        addScore(5 * ship.initialHealth);
        addBonusModifier(1);
        numberOfCurrentShips--;
        // Play animation on ship's position
    }

    public void fleetDestroyed(FleetController fleet)
    {
        addScore(50 * fleet.initialNumberOfShips);
        addBonusModifier(5);
        numberOfCurrentFleets--;
        // Briefly activate a message notifying the player that a whole
        // fleet's been wiped out
    
    }

    public void playerHit(ProjectileScript projectile)
    {
        addBonusModifier(-2);
        // Si el escudo YA estaba a 0, el jugador muere
        if (remainingShield == 0)
        {
            StartCoroutine(gameOver());
        }
        remainingShield -= projectile.damage;
        // Si, con el impacto, hemos perdido todos los escudos,
        // nos quedamos a 0
        if (remainingShield < 0)
        {
            remainingShield = 0;
        }

    }

    private void addScore(int scoreToAdd)
    {
        // Para no castigar excesivamente al jugador, cuando se aplique algún penalizador
        // a su puntuación, éste no se multiplicará por el bono
        if (scoreToAdd > 0)
        {
            score = score + (bonusModifier * (scoreToAdd));
        }
        else
        {
            score = score + scoreToAdd;
        }
        shootingInterfaceManager.updateScore(score);
    }

    private void addBonusModifier(int bonusToAdd)
    {
        bonusModifier += bonusToAdd;
        // El multiplicador nunca podrá ser negativo ni cero, ni estar por encima de 99
        bonusModifier = Mathf.Clamp(bonusModifier, 1, 100);
        shootingInterfaceManager.updateBonusModifier(bonusModifier);
    }


    IEnumerator gameOver()
    {
        // Dejamos de mostrar nada por pantalla
        Camera.main.enabled = false;
        // Paramos la música de la escena
        GetComponent<AudioSource>().Stop();
        // Reproducimos el sonido de explosión de la nave
        GetComponent<AudioSource>().PlayOneShot(explosionSound);
        yield return new WaitForSecondsRealtime(4f);
        PlayerPrefs.SetInt("Score", score);
        SceneManager.LoadScene("GameOverScene");
    }

    public void changeScene(string name)
    {
        addBonusModifier(-5);
        PlayerPrefs.SetInt("PlayerScore", score);
        PlayerPrefs.SetInt("Ammo", ammunition);
        PlayerPrefs.SetInt("Wave", wave);

        SceneManager.LoadScene(name);
    }

    private void loadDifficulty()
    {
        switch(PlayerPrefs.GetString("DifficultyMode", "Easy"))
        {
            case "Easy":
                JsonUtility.FromJsonOverwrite(easyDifficultyFile.text, shootingLevel);
                break;
            case "Medium":
                JsonUtility.FromJsonOverwrite(mediumDifficultyFile.text, shootingLevel);
                break;
            case "Hard":
                JsonUtility.FromJsonOverwrite(hardDifficultyFile.text, shootingLevel);
                break;

        }
    }
}
