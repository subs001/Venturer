using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Thanks for downloading my custom projectiles script! :D
/// Feel free to use it in any project you like!
/// 
/// The code is fully commented but if you still have any questions
/// don't hesitate to write a yt comment
/// or use the #coding-problems channel of my discord server
/// 
/// Dave

public class CustomProjectiles : MonoBehaviour
{
    public bool activated;

    [Header("Please attatch components:")]
    public Rigidbody rb;
    public GameObject explosion;
    public LayerMask whatIsEnemies;

    [Header("Set the basic stats:")]
    [Range(0f,1f)]
    public float bounciness;
    public bool useGravity;

    [Header("Explosion:")]
    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;

    [Header("Lifetime:")]
    public int maxCollisions;
    public float maxLifetime;
    public bool explodeOnTouch = true;

    [Header("Second bullets (Spawn after explosion)")]
    public GameObject secondBullet;
    public int sb_amount;
    public float sb_forwardForce, sb_upwardForce, sb_randomForce;

    //Grow
    [Header("[Attribute] - Grow")]
    [Header("Special attributes, activating often overrides other properties!")]
    [Space(7)]
    public float scaleGrowingSpeed;
    public float damageGrowingSpeed;
    public float maxScale;
    [Tooltip("Needs to be bigger than explosion damage variable")]
    public float maxDamage;

    //Stick to objects
    [Header("[Attribute] - StickToObjects")]
    [Space(7)]
    public bool stickToObjects;
    public bool checkForEnemies;
    public float checkRange;
    bool sticking;
    Vector3 stickingPos;

    //Spawner
    [Header("[Attribute] - Spawner")] 
    [Space(5)]
    [Tooltip("Can be literally anything, feel free to make your bullet spawn houses if you want... :D")]
    public GameObject objectToSpawn;
    public float timeBetweenSpawns;
    [Tooltip("Normally at transform.position, if you attatch points it will choose one of the attatched randomly")]
    public Transform[] preferredSpawnPoints;
    [Tooltip("If you want infinite spawns just set to something like 9999")]
    public int maxSpawnAmount;
    float currentTimeBetweenSpawns;

    //Pearl
    [Header("[Attribute] - Pearl (Ender pearl)")]
    [Space(7)]
    [Tooltip("Could be everything, but normally the player, you need to type in the exact name of the object")]
    public string nameOfObjectToTp;
    Transform objectToTp;
    public bool tpOnFirstCollision;
    public bool tpOnEveryCollision;
    public bool tpOnExplosion;
    [Tooltip("switches places of your objectToTp and the bullet itself, commonly used together with sticky bullets")]
    public bool switchPlaces;

    //Vanish
    [Header("[Attribute] - Vanish--Appear")]
    [Space(7)]
    public float timeBeforeVanish;
    public float timeBetweenVanishAndAppear;
    public bool dontAppearAgain;
    public bool canStillExplodeWhenVanished;
    public bool canTravelThroughtWallsWhileVanished;
    public bool repeat;

    //Auto-Aim
    [Header("[Attribute] - Auto-Aim")]
    [Space(7)]
    public bool useSmoothAutoAim;
    public bool useInsaneAutoAim;
    public float autoAimIntensity;
    [Tooltip("The range in which enemies get detected and aimed at")]
    public float autoAimDetectRange;
    [Tooltip("The range in which the auto aim stops again (if you don't want that set it to 0)")]
    public float autoAimStopRange;

    //Drop down
    [Header("[Attribute] - Drop down")]
    [Space(7)]
    public float timeBeforeDropping;
    [Tooltip("Let's the bullet wait mid air before dropping")]
    public float waitMidAir;
    [Tooltip("By setting a negative value the bullets rises up instead")]
    public float dropForce;
    [Tooltip("Highly recommended :D")]
    public bool explodeDirectlyAfterDrop = true;

    //Smoke
    [Header("[Attribute] - Smoke")]
    [Space(7)]
    public GameObject smokeEffect;

    //Custom gravity
    [Header("[Attribute] - Custom gravity")]
    [Space(7)]
    public bool useCustomGravity;
    public Vector3 gravityDirection;
    public float gravityStrength;

    //Player dash
    [Header("[Attribute] - Player Dash")]
    public string playerObjectName;
    private GameObject player;
    public float dashForce;
    [Min(0.2f)]
    [Tooltip("Make it at least 0.2f")]
    public float dashDelay = 0.2f;

    //Charge
    [Header("[Attribute] - Charge")]
    [Min(0.2f)]
    [Tooltip("Needs to be over 0.2f")]
    public float chargeTime;
    [Tooltip("How much the velocity get's boosted after charging")]
    public float chargedVelocityMultiplier;
    Vector3 savedVelocity;


    private int collisions;

    private PhysicMaterial physic_mat;
    public bool alreadyExploded;

    /// Call the setup and attribute functions that need to be called directly at the start
    /// as well as set some variables
    void Start()
    {
        Setup();

        currentTimeBetweenSpawns = timeBetweenSpawns;

        //Find player and call dash function (if activated)
        if (playerObjectName.Length > 0) player = GameObject.Find("PlayerObj");
        if (player != null) Invoke("PlayerDash", dashDelay);

        //Charge
        if (chargeTime > 0) StartCoroutine(SaveVelocity());

        //Set object to tp
        if (nameOfObjectToTp.Length != 0)
        objectToTp = GameObject.Find(nameOfObjectToTp).transform;

        //fixing the bug of sticky bullets instant exploding when switching places
        if (checkForEnemies && switchPlaces && tpOnFirstCollision || tpOnEveryCollision)
        {
            checkForEnemies = false;
            turnCheckForEnemyOnAgain = true;
        }

        //Set timers
        if (timeBeforeVanish > 0)
            currentTimeBeforeVanish = timeBeforeVanish;
        if (timeBetweenVanishAndAppear > 0)
            currentTimeBetweenVanishAndAppear = timeBetweenVanishAndAppear;
    }

    /// Here are all functions called (except Setup), it works always the same,
    /// check if a specific requirement is fullfilled and if so, call the function
    void Update()
    {
        if (!activated) return;

        if (collisions >= maxCollisions && activated) Explode();

        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0 && activated) Explode();

        if (sticking) StickToObject();

        if (objectToSpawn != null) Spawner();

        if (scaleGrowingSpeed > 0 || damageGrowingSpeed > 0) Grow();

        if (timeBeforeVanish != 0) Vanish();

        if (useSmoothAutoAim || useInsaneAutoAim) AutoAim();

        if (timeBeforeDropping != 0) DropDown();

        if (useCustomGravity) CustomGravity();
    }

    ///Just to set the basic variables of the bullet/projectile
    private void Setup()
    {
        //Setup physics material
        physic_mat = new PhysicMaterial();
        physic_mat.bounciness = bounciness;
        physic_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physic_mat.bounceCombine = PhysicMaterialCombine.Maximum;
        //Apply the physics material to the collider
        GetComponent<SphereCollider>().material = physic_mat;

        //Don't use unity's gravity, we made our own (to have more control)
        rb.useGravity = useGravity;
    }

    public void Explode()
    {
        //Bug fixing
        if (alreadyExploded) return;
        alreadyExploded = true;

        Debug.Log("Explode");

        //Instantiate explosion if attatched
        if (explosion != null)
            Instantiate(explosion, transform.position, Quaternion.identity);

        //Check for enemies and damage them
       /* Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);
        for (int i = 0; i < enemies.Length; i++)
        {
            //Damage enemies
            *//*if (enemies[i].GetComponent<ShootingAi>())
            enemies[i].GetComponent<ShootingAi>().TakeDamage(explosionDamage);*//*

            //Add explosion force to enemies
            if (enemies[i].GetComponent<Rigidbody>())
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange, 2f);
        }*/

        //Pearl(Teleport) to position
        if (objectToTp != null && tpOnExplosion) Pearl(transform.position);

        //Instantiate smoke if attatched
        if (smokeEffect != null)
            Instantiate(smokeEffect, transform.position, Quaternion.identity);

        //Invoke destruction
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<TrailRenderer>().emitting = false;
        Invoke("Delay", 0.08f);

        //Spawn second bullets and add forces (if second bullet attatched
        if (secondBullet == null) return;

        for (int i = 0; i < sb_amount; i++)
        {
            float x = Random.Range(-1, 1f); float y = Random.Range(-1, 1f); float z = Random.Range(-1, 1f);
            Vector3 random = new Vector3(x, y, z);
            Rigidbody sb_rigidbody = Instantiate(secondBullet, transform.position + random, Quaternion.identity).GetComponent<Rigidbody>();

            //Add forces
            if (sb_forwardForce > 0)
                sb_rigidbody.AddForce(transform.forward * sb_forwardForce, ForceMode.Impulse);
            if (sb_upwardForce > 0)
                sb_rigidbody.AddForce(sb_rigidbody.transform.up * sb_upwardForce, ForceMode.Impulse);
            if (sb_randomForce > 0)
                sb_rigidbody.AddForce(random * sb_randomForce, ForceMode.Impulse);
            
            sb_amount--;
        }
    }
    private void Delay()
    {
        Destroy(gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!activated) return;

        //If isVanished && !canStillExplodeWhenVanished stop the function
        if (isVanished && !canStillExplodeWhenVanished) return;

        //Colliding with other bullet doesn't count
        ///if (collision.collider.CompareTag("Bullet")) return;

        //Stick to objects, only if not already sticking and switchPlaces is turned off (bug fixing)
        if (stickToObjects && !sticking && !switchPlaces) 
        {
            sticking = true;
            stickingPos = transform.position;
            return;
        }

        //Pearl(Teleport) to position
        if (objectToTp != null && tpOnFirstCollision)
        {
            Pearl(transform.position);
            tpOnFirstCollision = false;
            tpOnEveryCollision = false;
        }
        if (objectToTp != null && tpOnEveryCollision) Pearl(transform.position);

        //Explode on touch
        if (explodeOnTouch && collision.collider.CompareTag("Enemy")) Explode();

        //Count up collisions
        collisions++;
    }

    #region Attribute functions

    float growingTime = 0;
    private void Grow()
    {
        //grow in size 
        if (transform.localScale.x < maxScale)
        transform.localScale += Vector3.one * Time.deltaTime * scaleGrowingSpeed;

        //Damage requires an integer, so we need to increase it a bit differently
        if (explosionDamage < maxDamage)
        {
            growingTime += Time.deltaTime * damageGrowingSpeed;
            if (growingTime >= 1)
            {
                //increase damage by one
                explosionDamage++;
                growingTime = 0;
            }
        }

        //If negative scaleGrowningSpeed (shrinking), destroy at scale 0
        if (transform.localScale.x <= 0) Explode();
    }

    private void StickToObject()
    {
        if (rb.useGravity == true) rb.useGravity = false;

        transform.position = stickingPos;

        //Check for enemies
        if (checkForEnemies && Physics.CheckSphere(transform.position, checkRange, whatIsEnemies))
        {
            Explode();
        }
    }

    private void Spawner()
    {
        //Count down timer
        currentTimeBetweenSpawns -= Time.deltaTime;
        //Spawn objects
        if (currentTimeBetweenSpawns <= 0 && maxSpawnAmount > 0)
        {
            if (preferredSpawnPoints.Length == 0) Instantiate(objectToSpawn, transform.position, Quaternion.identity);

            //If spawnpoints attatched choose one random
            if (preferredSpawnPoints.Length > 0)
                Instantiate(objectToSpawn, preferredSpawnPoints[Random.Range(0,preferredSpawnPoints.Length)].position, Quaternion.identity);

            //count down spawnAmount
            maxSpawnAmount--;
            //reset timer
            currentTimeBetweenSpawns = timeBetweenSpawns;
        }
    }

    bool turnCheckForEnemyOnAgain;
    private void Pearl(Vector3 teleportPosition)
    {
        if (switchPlaces) transform.position = objectToTp.position;
        if (stickToObjects)
        {
            stickingPos = objectToTp.position;
            sticking = true;

            //bug fixing
            if (turnCheckForEnemyOnAgain) Invoke("TurnCheckForEnemiesOnAgain", 0.1f);
        }

        objectToTp.position = teleportPosition;
    }
    private void TurnCheckForEnemiesOnAgain()
    {
        checkForEnemies = true;
    }

    bool isVanished;
    float currentTimeBeforeVanish, currentTimeBetweenVanishAndAppear;
    private void Vanish()
    {
        //Count down timer and start vanishing when timer at 0
        if (!isVanished)
            currentTimeBeforeVanish -= Time.deltaTime;

        if (currentTimeBeforeVanish <= 0)
        {
            //Make invisible
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<TrailRenderer>().enabled = false;

            //Disable collider to travel through walls (if activated)
            if (canTravelThroughtWallsWhileVanished) GetComponent<SphereCollider>().enabled = false;

            isVanished = true;

            //Reset timer
            currentTimeBeforeVanish = timeBeforeVanish;
        }

        if (isVanished && !dontAppearAgain)
        {
            currentTimeBetweenVanishAndAppear -= Time.deltaTime;
        }

        //Appear again
        if (currentTimeBetweenVanishAndAppear <= 0)
        {
            //Make visible
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<TrailRenderer>().enabled = true;

            //Enable collider to travel through walls (if activated)
            if (canTravelThroughtWallsWhileVanished) GetComponent<SphereCollider>().enabled = true;

            isVanished = false;

            //Reset timer
            currentTimeBetweenVanishAndAppear = timeBetweenVanishAndAppear;

            //Stop function (Don't repeat again)
            if (!repeat) timeBeforeVanish = 0;
        }
    }

    private void AutoAim()
    {
        //Check for enemies in range
        Collider[] enemies = Physics.OverlapSphere(transform.position, autoAimDetectRange, whatIsEnemies);

        //Add force towards the first enemy
        if (enemies.Length > 0)
        {
            //Calculate direction vector
            Vector3 dirToEnemy = enemies[0].transform.position - transform.position;

            if (useSmoothAutoAim)
            rb.AddForce(dirToEnemy.normalized * Time.deltaTime * autoAimIntensity * 20);

            if (useInsaneAutoAim)
            rb.velocity = dirToEnemy.normalized * Time.deltaTime * autoAimIntensity * 30;

            ///other tries
            ///rb.AddForce(dirToEnemy.normalized * Time.deltaTime * autoAimIntensity, ForceMode.VelocityChange);
            ///rb.velocity += dirToEnemy.normalized * Time.deltaTime * autoAimIntensity;
        }

        //Stop when needed
        if (Physics.CheckSphere(transform.position, autoAimStopRange, whatIsEnemies))
        {
            useInsaneAutoAim = false;
            useSmoothAutoAim = false;
        }
    }

    private void DropDown()
    {
        //start timer
        timeBeforeDropping -= Time.deltaTime;

        //Add downward force
        if (timeBeforeDropping <= 0)
        {
            //Just that the if statement only runs once
            timeBeforeDropping = 100;

            //Set velocity to 0 first and deactivate gravity (only if the drop force is positive)
            if (dropForce > 0)
            rb.useGravity = false;
            rb.velocity = Vector3.zero;

            //Whait mid air if needed and add force
            if (waitMidAir == 0)
                rb.AddForce(Vector3.down * dropForce, ForceMode.Impulse);
            else
                Invoke("LateDropDown", waitMidAir);

            //Make sure bullet explodes on next ground touch
            if (explodeDirectlyAfterDrop) Invoke("WaitABitLol", 0.2f + waitMidAir);
        }
    }
    private void LateDropDown() { rb.AddForce(Vector3.down * dropForce, ForceMode.Impulse); }
    private void WaitABitLol() { collisions = maxCollisions - 1; }

    private void PlayerDash()
    {
        Vector3 dashDirection = transform.position - player.transform.position;

        player.GetComponent<PlayerMovement>().DashInDirection(dashDirection.normalized, dashForce);
    }

    private IEnumerator SaveVelocity()
    {
        yield return new WaitForSeconds(0.025f);
        savedVelocity = rb.velocity;
        yield return new WaitForEndOfFrame();
        rb.velocity = Vector3.zero;
        rb.useGravity = false;
        Invoke("ChargeFinished", chargeTime - 0.03f);
    }
    private void ChargeFinished()
    {
        rb.useGravity = useGravity;
        rb.velocity = savedVelocity * chargedVelocityMultiplier;
    }

    private void CustomGravity()
    {
        //Deactivate normal gravity
        if (rb.useGravity) rb.useGravity = false;

        //Add gravity force
        rb.AddForce(gravityDirection.normalized * gravityStrength * Time.deltaTime * 20);
    }

    #endregion

    ///Just for visualizing a few variables
    #region Debugging
    private void OnDrawGizmosSelected()
    {
        //visualize the explosion range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }

    #endregion

    /// The setters need to be here that the slider Ui works, 
    /// if you don't need the ingame sliders anyway, just delete them :D
    #region Setters

    public void SetBounciness(float v)
    {
        bounciness = v;
    }
    public void SetGravity(float v)
    {
        if (v == 1) useGravity = true;
        else useGravity = false;
    }
    public void SetMaxCollisions(float v)
    {
        int _v = Mathf.RoundToInt(v);
        maxCollisions = _v;
    }
    public void SetMaxLifetime(float v)
    {
        int _v = Mathf.RoundToInt(v);
        maxLifetime = _v;
    }
    public void SetExplosionRange(float v)
    {
        explosionRange = v;
    }
    public void SetExplosionDamage(float v)
    {
        int _v = Mathf.RoundToInt(v);
        explosionDamage = _v;
    }

    #endregion
}
