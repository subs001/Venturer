using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    //Assingables
    public Transform playerCam;
    public Transform orientation;

    //Other
    private Rigidbody rb;

    //Rotation and look
    public bool lockLook;
    private float xRotation;
    public float sensitivity = 50f;
    private float sensMultiplier = 1f;

    // public float energy, energyRegen;
    // public int health, regen;
    // int maxHealth;
    // bool fighting;
    

    //Movement
    public Vector3 inputVector;
    public float moveSpeed = 4500;
    public float startBaseSpeed = 15f, baseSpeed;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;
    public float crouchGravityMultiplier;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;

    public int startDoubleJumps = 1;
    int doubleJumpsLeft;

    //Input
    public float x, y;
    bool jumping, sprinting, crouching;

    //Air contorl
    public float airForwardForce;

    //AirDash
    public float dashForce, dashTime;
    bool readyToDash;
    int wTapTimes = 0;

    //RocketBoost
    public float maxRocketTime;
    public float rocketForce;
    bool rocketActive, readyToRocket;
    bool alreadyInvokedRockedStop;
    float rocketTimer;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    //Wallrunning
    public LayerMask whatIsWall;
    RaycastHit wallHitR, wallHitL;
    public bool isWallRight, isWallLeft;
    public float maxWallrunTime;
    public float wallrunForce, wallrunUpwardForce, maxWallSpeed;
    public int wallJumps, wallJumpsLeft;
    public bool readyToWallrun, isWallRunning;
    public bool resetDoubleJumpsOnWall;
    //CamTilt
    public float maxWallRunCameraTilt;
    public float wallRunCameraTilt = 0;

    //Climbing
    public float climbForce, maxClimbSpeed;
    public LayerMask whatIsLadder;
    bool alreadyStoppedAtLadder;

    //Speed Display
    public float speedRecord;
    public TextMeshProUGUI baseVelocityDisplay, currentVelocityDisplay, recordVelocityDisplay;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // maxHealth = health;
        baseSpeed = startBaseSpeed;
    }
    void Start()
    {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Movement();
    }
    private void Update()
    {
        MyInput();
        if (!lockLook) Look();
        CheckForWall();

        // EnergyManager();

        //Stuff
        if (Input.GetKeyDown(KeyCode.L)) SceneManager.LoadScene(1);

        //Trail renderer
        //if (rb.velocity.magnitude <= 25) GetComponent<TrailRenderer>().startWidth 

        //Set speed display
        if (currentVelocityDisplay != null)
            currentVelocityDisplay.SetText("currVel: " + Mathf.RoundToInt(rb.velocity.magnitude));
        if (baseVelocityDisplay != null)
            baseVelocityDisplay.SetText("baseVel: " + baseSpeed);
        if (recordVelocityDisplay != null)
            recordVelocityDisplay.SetText("recordVel: " + speedRecord);

        //new speed record
        if (speedRecord < rb.velocity.magnitude)
            speedRecord = Mathf.RoundToInt(rb.velocity.magnitude);
        //reset Record
        if (Input.GetKeyDown(KeyCode.R)) speedRecord = 0;

    }

    
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftShift);

        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftShift))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftShift))
            StopCrouch();

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping && grounded) Jump();
        //Double Jumping
        if (Input.GetButtonDown("Jump") && !grounded && doubleJumpsLeft >= 1)
        {
            Jump();
        }

        //Dashing
        if (Input.GetKeyDown(KeyCode.W) && wTapTimes<=1){
            wTapTimes++;
            Invoke("ResetTapTimes", 0.3f);
        }
        if (wTapTimes == 2 && readyToDash) Dash();

        //Wallrun
        if (isWallRight && !grounded && readyToWallrun) StartWallrun();
        if (isWallLeft && !grounded && readyToWallrun) StartWallrun();
        //reset wallrun
        if (!isWallRight && !isWallLeft && !readyToWallrun) readyToWallrun = true;

        //Climbing
        if (Physics.Raycast(transform.position, orientation.forward, 1, whatIsLadder) && y > .9f)
            Climb();
        else alreadyStoppedAtLadder = false;
    }

    private void ResetTapTimes()
    {
        wTapTimes = 0;
    }

    private void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement()
    {
        //Extra gravity
        //Needed that the Ground Check works better!
        float gravityMultiplier = 10f;

        if (crouching) gravityMultiplier = crouchGravityMultiplier;

        rb.AddForce(Vector3.down * Time.deltaTime * gravityMultiplier);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //Air Control
        //if (!grounded) rb.AddForce(orientation.forward * Time.deltaTime * airForwardForce);

        //ResetStuff when touching ground
        if (grounded){
            readyToDash = true;
            doubleJumpsLeft = startDoubleJumps;
            wallJumpsLeft = wallJumps;
        }

        //Set max speed
        float maxSpeed = this.baseSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxSpeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Slow down, don't allow going to fast
        ///if (rb.velocity.magnitude > baseSpeed) SlowDown();

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (grounded && crouching) multiplierV = 0f;

        //Apply forces to move player
        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);

        //Base speed handling
        if (rb.velocity.magnitude > baseSpeed - 3) IncreaseBaseSpeed();
        else if (baseSpeed > startBaseSpeed) DecreaseBaseSpeed();
    }

    private void Jump()
    {
        if (grounded) { 
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector3.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (!grounded && !isWallRunning){
            readyToJump = false;
            doubleJumpsLeft--;

            //Add jump forces
            rb.AddForce(orientation.forward * jumpForce * 1.2f);
            rb.AddForce(Vector2.up * jumpForce * 1.7f);
            rb.AddForce(normalVector * jumpForce * 0.7f);

            //Dampen y velocity
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.4f, rb.velocity.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Walljump
        if (isWallRunning){
            //Return if no walljumps left
            if (wallJumpsLeft <= 0) return;

            readyToJump = false;
            wallJumpsLeft--;

            //normal jump
            if (isWallLeft && !Input.GetKey(KeyCode.D) || isWallRight && !Input.GetKey(KeyCode.A))
            {
                rb.AddForce(Vector2.up * jumpForce * 1.5f);
                rb.AddForce(normalVector * jumpForce * 0.75f);
            }

            //sidwards wallhop
            if (isWallRight||isWallLeft && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) rb.AddForce(-orientation.up * jumpForce * 1f);
            if (isWallRight && Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * jumpForce * 3.2f);
            if (isWallLeft && Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * jumpForce * 3.2f);

            //Always add forward force
            rb.AddForce(orientation.forward * jumpForce * 1f);

            //Reset Velocity
            rb.velocity = Vector3.zero;

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Dash()
    {
        readyToDash = false;
        wTapTimes = 0;

        //Add force
        rb.AddForce(orientation.forward * dashForce);
    }

    float elapsedWallTime;
    private void StartWallrun()
    {
        Debug.Log("StartWallrun");

        //When to stop
        if (grounded) StopWallRun();

        //Count up timer
        elapsedWallTime += Time.deltaTime;

        //Leave wall run when timer reaches maximum
        if (elapsedWallTime > maxWallrunTime)
        {
            //StopWallRun();
        }

        //rb.useGravity = false;
        isWallRunning = true;

        //Add upward force
        rb.AddForce(orientation.up * wallrunUpwardForce * Time.deltaTime);

        if (rb.velocity.magnitude <= maxWallSpeed)
        {
            rb.AddForce(orientation.forward * wallrunForce * Time.deltaTime);

            //Make sure char sticks to wall
            if (isWallRight)
            rb.AddForce(orientation.right * wallrunForce / 5 * Time.deltaTime);
            else
                rb.AddForce(-orientation.right * wallrunForce / 5* Time.deltaTime);
        }
    }
    private void StopWallRun()
    {
        Debug.Log("StopWallrun");

        isWallRunning = false;
        readyToWallrun = false;

        //Reset timer
        elapsedWallTime = 0;
    }
    private void CheckForWall()
    {
       isWallRight = Physics.Raycast(transform.position, orientation.right, out wallHitR, 1f, whatIsWall);
       isWallLeft = Physics.Raycast(transform.position, -orientation.right, out wallHitL, 1f, whatIsWall);

        if (!isWallLeft && !isWallRight && isWallRunning) StopWallRun();
        if (isWallLeft || isWallRight && resetDoubleJumpsOnWall) doubleJumpsLeft = startDoubleJumps;
    }
    private void Climb()
    {
        //Makes possible to climb even when falling down fast
        Vector3 vel = rb.velocity;
        if (rb.velocity.y < 0.5f && !alreadyStoppedAtLadder){
            rb.velocity = new Vector3(vel.x, 0, vel.z);
            //Make sure char get's at wall
            alreadyStoppedAtLadder = true;
            rb.AddForce(orientation.forward * 500 * Time.deltaTime);
        }

        //Push character up
        if (rb.velocity.magnitude < maxClimbSpeed)
        rb.AddForce(orientation.up * climbForce * Time.deltaTime);

        //Doesn't Push into the wall
        if (!Input.GetKey(KeyCode.S)) y = 0;
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunCameraTilt);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);

        //While Wallrunning
        //Tilts camera in .5 second
        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallRight)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;

        //Tilts camera back again
        if (wallRunCameraTilt > 0 && !isWallRight && !isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        if (wallRunCameraTilt < 0 && !isWallRight && !isWallLeft)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
    }

    float timer1, timer2;
    private void IncreaseBaseSpeed()
    {
        //Only increase in .1 ticks
        timer1 += Time.deltaTime * 1f;

        if (timer1 > 2f)
        {
            baseSpeed += 0.1f;
            timer1 = 0;
        }
    }
    private void DecreaseBaseSpeed()
    {
        //Only decrease in .1 ticks
        timer2 += Time.deltaTime * 0.6f;

        if (timer2 > 2f)
        {
            baseSpeed -= 0.1f;
            timer2 = 0;
        }
    }
    private void SlowDown()
    {
        Debug.Log("SlowingDown");

        ///Vector3 baseVelVector = rb.velocity.normalized * baseSpeed;

        ///rb.AddForce(-rb.velocity * 1f * Time.deltaTime, ForceMode.Impulse);

        Debug.Log("Drag: " + rb.drag);
        rb.drag = 2;
    }
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        /*Limit diagonal running. Only when holding down W and D or W and A
        if (x != 0 && y != 0)
        {
            if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > baseSpeed)
            {
                float fallspeed = rb.velocity.y;
                Vector3 n = rb.velocity.normalized * baseSpeed;
                rb.velocity = new Vector3(n.x, fallspeed, n.z);
            }
        } */

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > baseSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * baseSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }

        //if (!grounded || jumping) return; (Dani's settings)
        if (!grounded || jumping || isWallRunning) return;

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement:
        ///Counters when no Input and still moving || Input in opposite direction then velocity
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
    }

    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// Handle ground detection
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    #region abilityFunctions

    public void DashInDirection(Vector3 dir, float force)
    {
        rb.AddForce(dir * force, ForceMode.Impulse);
    }

    #endregion
}
