
using UnityEngine;
using TMPro;


public class ProjectileGun : MonoBehaviour
{
    public bool shootingEnabled = true;

    [Header("Attatch your bullet prefab")]
    public GameObject bullet;

    //Gun stats
    public float shootForce, upwardForce;
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    //recoil
    public Rigidbody playerRb;
    public float recoilForce;

    //some bools
    bool shooting, readyToShoot, reloading;

    public Camera fpsCam;
    public GameObject muzzleFlash;
    public Transform attackPoint;

    //Show bullet amount
    //public CamShake camShake;
    //public float camShakeMagnitude, camShakeDuration;
    public TextMeshProUGUI text;

    public bool allowInvoke = true;

    public CharacterController player;

    private void Start()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }
    void Update()
    {
        MyInput();

        // Set Text
        // text.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
    }
    private void MyInput()
    {
        //Input
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();

        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0){
            bulletsShot = bulletsPerTap;
            Shoot(); //Function has to be after bulletsShot = bulletsPerTap
        }
    }
    private void Shoot()
    {
        if (!shootingEnabled) return;

        readyToShoot = false;

        //Find the hit position using a raycast
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        //Check if the ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        //Calculate direction
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        //Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);

        //Calc Direction with Spread
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, z);

        //Instantiate bullet/projectile
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
        currentBullet.transform.forward = directionWithSpread.normalized;

        //AddForce
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * upwardForce, ForceMode.Impulse);
        //Activate bullet
        if (currentBullet.GetComponent<CustomProjectiles>()) currentBullet.GetComponent<CustomProjectiles>().activated = true;

        Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        //Shake Camera
        //camShake.StartCoroutine(camShake.Shake(camShakeDuration, camShakeMagnitude));

        bulletsLeft--;
        bulletsShot--;

        if (allowInvoke)
        {
            Invoke("ShotReset", timeBetweenShooting);
            allowInvoke = false;

            //Add recoil force to player (should only be called once)
            playerRb.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);
            player.transform.Translate(playerRb.transform.position);

        }

        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }
    private void ShotReset()
    {
        readyToShoot = true;
        allowInvoke = true;
    }
    private void Reload()
    {
        reloading = true;

        Invoke("ReloadingFinished", reloadTime);
    }
    private void ReloadingFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    #region Setters

    public void SetShootForce(float v)
    {
        shootForce = v;
    }
    public void SetUpwardForce(float v)
    {
        upwardForce = v;
    }
    public void SetFireRate(float v)
    {
        float _v = 2 / v;
        timeBetweenShooting = _v;
    }
    public void SetSpread(float v)
    {
        spread = v;
    }
    public void SetMagazinSize (float v)
    {
        int _v = Mathf.RoundToInt(v);
        magazineSize = _v;
    }
    public void SetBulletsPerTap(float v)
    {
        int _v = Mathf.RoundToInt(v);
        bulletsPerTap = _v;
    }

    #endregion
}