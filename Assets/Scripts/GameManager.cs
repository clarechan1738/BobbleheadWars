using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //GameObjects For Player, Alien & Spawn Point Info
    public GameObject player;
    public GameObject[] spawnPoints;
    public GameObject alien;

    //Variables To Hold Alien Spawn Information (Accessible Within Unity)
    public int maxAliensOnScreen;
    public int totalAliens;
    public float minSpawnTime;
    public float maxSpawnTime;
    public int aliensPerSpawn;

    //Variables To Hold Amount On Screen & Alien Spawn Times (Not Accessible Within Unity)
    private int aliensOnScreen = 0;
    private float generatedSpawnTime = 0;
    private float currentSpawnTime = 0;

    public GameObject upgradePrefab;
    public Gun gun;
    public float upgradeMaxTimeSpawn = 7.5f;
    private bool spawnedUpgrade = false;
    private float actualUpgradeTime = 0;
    private float currentUpgradeTime = 0;

    public GameObject deathFloor;

    public Animator arenaAnimator;

    // Start is called before the first frame update
    void Start()
    {
        actualUpgradeTime = Random.Range(upgradeMaxTimeSpawn - 3.0f, upgradeMaxTimeSpawn);
        actualUpgradeTime = Mathf.Abs(actualUpgradeTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            return;
        }
        currentUpgradeTime += Time.deltaTime;

        if (currentUpgradeTime > actualUpgradeTime)
        {
            // 1
            if (!spawnedUpgrade)
            {
                // 2
                int randomNumber = Random.Range(0, spawnPoints.Length - 1);
                GameObject spawnLocation = spawnPoints[randomNumber];
                // 3
                GameObject upgrade = Instantiate(upgradePrefab) as GameObject;
                Upgrade upgradeScript = upgrade.GetComponent<Upgrade>();
                upgradeScript.gun = gun;
                upgrade.transform.position = spawnLocation.transform.position;
                // 4
                spawnedUpgrade = true;
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.powerUpAppear);
            }
        }

        //Time Passed Since Last Spawn Call
        currentSpawnTime += Time.deltaTime;

        //Condition To Generate New Alien Wave
        if (currentSpawnTime > generatedSpawnTime)
        {
            //Resets Timer After Spawn Occurs
            currentSpawnTime = 0;

            //Spawn-Time Randomizer
            generatedSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
            
            //Prevents Further Spawns If Maximum Is Reached 
            if (aliensPerSpawn > 0 && aliensOnScreen < totalAliens)
            {
                //Holds Previous Spawn Locations Of Aliens
                List<int> previousSpawnLocations = new List<int>();

                //Limits Amount Of Aliens To Number Of Spawn Points
                if (aliensPerSpawn > spawnPoints.Length)
                {
                    aliensPerSpawn = spawnPoints.Length - 1;
                }

                //Preventative Code To Avoid More Alien Spawns Than Are Spawnpoints Configured
                aliensPerSpawn = (aliensPerSpawn > totalAliens) ? aliensPerSpawn - totalAliens : aliensPerSpawn;

                //Loops Once For Each Spawned Alien
                for (int i = 0; i < aliensPerSpawn; i++)
                {
                    //Condition To Check If Aliens Exceed Maximum
                    if (aliensOnScreen < maxAliensOnScreen)
                    {
                        //Keeps Track Of Aliens Spawned
                        aliensOnScreen += 1;

                        // 1 -- Inititalize To -1 To Indicate Number Has Not Been Chosen/Found For Spawnpoint
                        int spawnPoint = -1;

                        // 2 -- Keeps Looking For Spawn Point (Index) That Has Not Been Used
                        while (spawnPoint == -1)
                        {

                            // 3 -- Creates Random Index Of 'List' Between 0 & Number Of Spawnpoints
                            int randomNumber = Random.Range(0, spawnPoints.Length - 1);


                            // 4 -- Checks To See If Random Spawnpoint Has Not Been Used
                            if (!previousSpawnLocations.Contains(randomNumber))
                            {
                                //Add Selected Random Number To 'List' 
                                previousSpawnLocations.Add(randomNumber);

                                //Uses Selected Random Number For Spawn Location Index
                                spawnPoint = randomNumber;
                            }
                        }

                        //Spot In Arena To Spawn Next Alien
                        GameObject spawnLocation = spawnPoints[spawnPoint];

                        //Creates New Alien Instance From 'Alien' Prefab
                        GameObject newAlien = Instantiate(alien) as GameObject;

                        //Position New Alien In Randomly Chosen Unused Spawnpoint
                        newAlien.transform.position = spawnLocation.transform.position;

                        //Gets Reference To Alien Script & New Alien Spawned
                        Alien alienScript = newAlien.GetComponent<Alien>();

                        //Set New Alien Target To Current Player Location
                        //NOTE: GameManager Script Affecting Alien Code
                        alienScript.target = player.transform;

                        //Rotates Alien Towards The Player
                        Vector3 targetRotation = new Vector3(player.transform.position.x, newAlien.transform.position.y, player.transform.position.z);
                        newAlien.transform.LookAt(targetRotation);
                        alienScript.OnDestroy.AddListener(AlienDestroyed);

                        alienScript.GetDeathParticles().SetDeathFloor(deathFloor);

                    }
                }
            }
        }
    }

    public void AlienDestroyed()
    {
        aliensOnScreen -= 1;
        totalAliens -= 1;
        if (totalAliens == 0)
        {
            Invoke("endGame", 2.0f);
        }
    }

    private void endGame()
    {
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.
        elevatorArrived);
        arenaAnimator.SetTrigger("PlayerWon");
    }

}