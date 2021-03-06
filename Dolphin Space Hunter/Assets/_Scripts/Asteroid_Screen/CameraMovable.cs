/*
 * Copyright (c) Sergio Ruiz
 * 
 * 
 * 
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;



public class CameraMovable : MonoBehaviour
{
    public TextMeshProUGUI scorePanel, ammunitionCounter;

    public Camera ArCamera;
    public float startDuration,shakeDuration, startAmount, shakeAmount, smoothAmount; //shake
    public int remainingShield;
    public AudioClip explosionSound;
    public GameObject gameDirector;
    public static int currentScore;

    private GUISteroid asteroidUI;
    private bool noShield, isShaking; //Global para comprobar si tenemois escudo
    private int currentAmmunition, maxAmmunition,asteroidDamage;//Escudo actual
    AudioSource collisionSound;
    private bool playerDead = false;


    void Start()
    {
        asteroidUI = this.gameObject.GetComponent<GUISteroid>();
        loadParameters();
        
        if (remainingShield <= 0)
        {
            noShield = true;
        }
        else
        {
            noShield = false;
        }

        
        isShaking = false;
        asteroidUI.asteroidHit(remainingShield);
        ammunitionCounter.text = currentAmmunition.ToString() + " / " + maxAmmunition.ToString();
        collisionSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        actualizaCamara();
        scorePanel.text = "Score: " + currentScore.ToString();
    }



    private void actualizaCamara()
    {
        float speed = 350.0f;
        Vector3 prueba  = Vector3.zero;

        if (Mathf.Abs(Input.acceleration.z) >=0.1f)
        {
            prueba.x = -Input.acceleration.z;
        }

        if (Mathf.Abs(Input.acceleration.x) >= 0.1f)
        { prueba.z = Input.acceleration.x; }

        // clamp acceleration vector to the unit sphere
        if (prueba.sqrMagnitude > 1)
            prueba.Normalize();

        prueba *= Time.deltaTime;
        prueba *= speed;
        
        Vector3 newPosition = new Vector3(ArCamera.transform.position.x + prueba.z, ArCamera.transform.position.y + prueba.x, 0);

        ArCamera.transform.position= newPosition;

    }


    void ShakeCamera()
    {

        startAmount = 40;//Set default (start) values
        startDuration = 40;//Set default (start) values
        shakeDuration = 20;
        if (!isShaking) { StartCoroutine(Shake());}//Only call the coroutine if it isn't currently running. Otherwise, just set the variables.
    }

	IEnumerator Shake() {
		isShaking = true;
        float shakePercentage;
        bool smooth = true;
		while (shakeDuration > 0.01f) {
			Vector3 rotationAmount = Random.insideUnitSphere * shakeAmount;//A Vector3 to add to the Local Rotation
			rotationAmount.z = 0;//Don't change the Z; it looks funny.
 
			shakePercentage = shakeDuration / startDuration;//Used to set the amount of shake (% * startAmount).
 
			shakeAmount = startAmount * shakePercentage;//Set the amount of shake (% * startAmount).
			shakeDuration = Mathf.Lerp(shakeDuration, 0, Time.deltaTime);//Lerp the time, so it is less and tapers off towards the end.
 
 
			if(smooth)
				 transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotationAmount), Time.deltaTime * smoothAmount);
			else
				transform.localRotation = Quaternion.Euler (rotationAmount);//Set the local rotation the be the rotation amount.
 
			yield return null;
		}
        transform.localRotation = Quaternion.identity;//Set the local rotation to 0 when done, just to get rid of any fudging stuff.
		isShaking = false;
	}
 
    public void saveInformation()
    {
        PlayerPrefs.SetInt("Shield", remainingShield);
        PlayerPrefs.SetInt("PlayerScore", currentScore);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Asteroid")
        {
            if (playerDead == false)
            {
                collisionSound.Play();
            }
            ShakeCamera();

            remainingShield -= asteroidDamage;
            asteroidUI.asteroidHit(remainingShield);
            if (remainingShield <= 0)
            {
                if (noShield)
                {
                    StartCoroutine(gameOver());
                }
                else
                {
                    noShield = true;
                }
            }
        }

    }

    private void loadParameters()
    {
        asteroidDamage = PlayerPrefs.GetInt("AsteroidDamage", 10);
        remainingShield = PlayerPrefs.GetInt("Shield", 50);
        currentScore = PlayerPrefs.GetInt("PlayerScore", 0);
        currentAmmunition = PlayerPrefs.GetInt("Ammo", 0);
        maxAmmunition = PlayerPrefs.GetInt("MaxAmmo", 50);
       

        scorePanel.text = "Score: " + currentScore.ToString();
    }

    IEnumerator gameOver()
    {
        playerDead = true;
        // Dejamos de mostrar nada por pantalla
        Camera.main.enabled = false;
        PlayerPrefs.SetInt("PlayerScore", currentScore);
        asteroidUI.shieldBar.gameObject.SetActive(false);
        // Paramos la música de la escena
        gameDirector.GetComponent<AudioSource>().Stop();
        gameDirector.GetComponent<AudioSource>().enabled = false;
        // Reproducimos el sonido de explosión de la nave
        GetComponent<AudioSource>().PlayOneShot(explosionSound, 1f);
        yield return new WaitForSecondsRealtime(4f);
        SceneManager.LoadScene("GameOverScene");
    }
}

