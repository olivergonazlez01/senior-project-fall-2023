using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Spitter : Pathfinding_entity
{
    // Sets the speed and velocity of the zombies
    private const float SPEED = 3.0f;
    
    // Sets the health and multiplier (will change per round) of the zombies
    public float healthMultiplier = 1;

    // References to the player, zombie sprite and animator, game controller, and spawner
    public GameObject Player;
    [SerializeField] AudioSource spitSound;
    public SpriteRenderer zombie;
    public Animator animator;
    public GameObject gameController;
    MainController controller;
    public GameObject spawner;
    private Transform spitterSpawner;

    // Variables for powerup drops
    public Transform PUTemp;
    Vector3 dropPosition;

    // Variables for attacking behavior
    bool isAttacking = false;
    public float ATTACK_INTERVAL = 3.0f;
    private float attackTimer = 0.0f;

    public void attack() {
        isAttacking = true;
        setTarget(null);
    }

    public void chase() {
        isAttacking = false;
        GameObject playerTag = GameObject.FindGameObjectWithTag("Player");
        Player = playerTag;
        setTarget(Player.transform);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        // call Pathfinding-entity start method
        base.Start();

        // Set the variables
        Player = GameObject.Find("pawl");
        setTarget(Player.transform);
        setEntitySpeed(SPEED);
        health = specialZombie ? (short)500 : (short) Math.Ceiling((double) (100 * healthMultiplier));
        gameController = GameObject.Find("Game Controller");
        controller = gameController.GetComponentInChildren<MainController>();
    }

    void Update()
    {
        // If a zombie's health reaches 0, decide if the zombie will drop a powerup, and return it to the  spawner
        if (health <= 0)
        {            
            // Grabs position of the zombie
            dropPosition = transform.position;

            // Remove it from the list of active zombies and update the game controller, play zombie death sound
            controller.activeZombies.Remove(transform.gameObject);
            if(!specialZombie) controller.zombiesLeft--;
            soundController.playDeath();

            // Set the location and parent of the zombie to the spawner and turn off zombie
            spawner = GameObject.Find("Zombie Spawner");
            spitterSpawner = spawner.transform.GetChild(2);
            transform.SetParent(spitterSpawner.transform);
            transform.localPosition = new Vector2(0, 0);
            transform.gameObject.SetActive(false);
            
            // Random chance for dropping powerup
            int chance = UnityEngine.Random.Range(0, 50);

            if (chance <= 9)
            {
                // Chooses a random powerup to drop
                int powerup = UnityEngine.Random.Range(0, 4);
                // These values allow the game to properly choose a powerup that has not already spawned
                bool chosen = false;
                bool opt0 = false;
                bool opt1 = false;
                bool opt2 = false;
                bool opt3 = false;
                Powerup temp;
                // Will keep running until a powerup has been chosen, or no powerups are available at the moment
                while (!chosen)
                {
                    switch(powerup)
                    {
                        case 0:
                            PUTemp = GameObject.Find("Insta-Kill").transform;
                            temp = PUTemp.GetComponent<Powerup>();
                            // if the loop has already run through this case
                            // stop the loop and make the chosen powerup null
                            if (opt0)   { PUTemp = null; chosen = true; }
                            else 
                            {
                                // else if the chosen powerup is not spawned in the world, 
                                // choose it
                                if (!temp.active)   chosen = true;
                                // else, move to the next case and make sure that this 
                                // Case cannot be run by the loop again
                                else    { powerup = 1; opt0 = true; }
                            }
                            break;

                        case 1:
                            PUTemp = GameObject.Find("Max Ammo").transform;
                            temp = PUTemp.GetComponent<Powerup>();
                            if (opt1)   { PUTemp = null; chosen = true; }
                            else
                            {                        
                                if (!temp.active)   chosen = true;
                                else    { powerup = 2; opt1 = true; }
                            }
                            break;

                        case 2:
                            PUTemp = GameObject.Find("Nuke").transform;
                            temp = PUTemp.GetComponent<Powerup>();
                            if (opt2)   { PUTemp = null; chosen = true; }
                            else
                            {
                                if (!temp.active)   chosen = true;
                                else    { powerup = 3; opt2 = true; }
                            }
                            break;

                        case 3:
                            PUTemp = GameObject.Find("Double Points").transform;
                            temp = PUTemp.GetComponent<Powerup>();
                            if (opt3)   { PUTemp = null; chosen = true; }
                            else
                            {
                                if (!temp.active)   chosen = true;
                                else    { powerup = 0; opt3 = true; }
                            }
                            break;
                    }
                }
                
                // If PUTemp is not null, then a powerup has been chosen
                if (PUTemp != null)
                {
                    // Make its position the drop position of the zombie, and activate its script
                    PUTemp.position = new Vector3(dropPosition.x, dropPosition.y, -1f);
                    temp = PUTemp.GetComponent<Powerup>();
                    temp.active = true;
                }
            }
        }

        // Behavior Tree 
        if (isAttacking) {
            if (attackTimer <= 0.0f) {
                // start new attack
                spitSound.PlayOneShot(spitSound.clip);
                Spit s = transform.GetChild(0).GetComponent<Spit>();
                s.shoot(Player.transform);
                attackTimer = ATTACK_INTERVAL;
            }
            attackTimer -= Time.deltaTime;
        } else if (isMoving()) {
            // Follow player around the map no matter the distance
            // Make sure the zombie follows the game object with the player tag (NEEDED FOR THE YARN BOMBS)
            GameObject playerTag = GameObject.FindGameObjectWithTag("Player");
            Player = playerTag;
            setTarget(Player.transform);
            setEntitySpeed(SPEED);

            animator.SetFloat("speed", getDirection().magnitude);
        } else {
            //Debug.Log("not moving lol");
        }

        // make sure zombie is always facing player
        if ((Player.transform.position - transform.position).x > 0) {
            zombie.flipX = true;
        } else {
            zombie.flipX = false;
        }
    
    }
}