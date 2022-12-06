using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectileSpawn : MonoBehaviour
{
    [SerializeField] GameObject objectPrefab;
    [SerializeField] GameObject launcher;
    [SerializeField] List<GameObject> poolObjects;
    [SerializeField] int noOfActiveObjects = 0;
    [SerializeField] int initialPoolSize = 10; 
    public float initializedShootCooldownTime = 0.5f;
    public float launchVelocity = 2000f; 

    public int initialMaxAmmo = 10;
    public float reloadTime = 3.0f;
    public int currentAmmo = 10;
    private bool isReloading = false;

    private int createProjectileIndex = 0;
    private int destroyProjectileIndex = 0;

    [SerializeField] float timeToNextDespawn;
    [SerializeField] float despawnCooldownTime = 3.0f;
    public float ShootCooldwonTime = 0.5f;

    [SerializeField] public float lastDeltaTime;

    
    
    void Start() {
        //Initialisieren des Projektil Pools
        poolObjects = new List<GameObject>(initialPoolSize);
        for (int i = 0; i < initialPoolSize; i++) {
            poolObjects.Add(createNewObject());
        }
        
    }

    void Update() {
        lastDeltaTime = Time.deltaTime;
        timeToNextDespawn -= Time.deltaTime;

        //Führt Update nur aus wenn nicht grade nachgeladen wird
        if (isReloading) {
            return;
        }

        //Nachladen wenn man keine Munition mehr hat
        if(currentAmmo <= 0) {
            StartCoroutine(Reload());
            return;
        }

        //Zerstört ältestes Projektil nach despawnCooldownTime
        if (timeToNextDespawn < 0 && noOfActiveObjects > 0) {
            DestroyObject();
            timeToNextDespawn = despawnCooldownTime;
        }
        
        //Nachladen wenn man 'R' drückt
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < initialMaxAmmo) {
            StartCoroutine(Reload());
            return;
        }

        //Feuert wenn wenn der Cooldown für das schießen um ist
        //Deaktiviert ältestes Pool Objekt, falls das Pool Limit erreicht ist
        if (Input.GetButtonDown("Fire1")) {
            if (noOfActiveObjects >= initialPoolSize) {
                DestroyObject();
            } if (ShootCooldwonTime < 0) {
                spawnProjectile();
                timeToNextDespawn = despawnCooldownTime;
                ShootCooldwonTime = initializedShootCooldownTime;
            }
        }
        ShootCooldwonTime -= Time.deltaTime;
    }

    //Reloading...
    IEnumerator Reload() {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = initialMaxAmmo;
        isReloading = false;
    }

    //Setzt Projektil aus Pool activ und feuert es ab
    void spawnProjectile() {
        var currentGo = poolObjects[createProjectileIndex];
        nextSpawnIndex();
        
        //Setzt Projektil an die Stelle des Launchers
        currentGo.transform.position = transform.position;
        currentGo.transform.rotation = launcher.transform.rotation;
        currentGo.SetActive(true);
        currentGo.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, launchVelocity));

        currentAmmo--;
        noOfActiveObjects += 1;
    }

    void nextSpawnIndex () {
        if (createProjectileIndex < initialPoolSize - 1) {
            createProjectileIndex++;
        } else {
            createProjectileIndex = 0;
        }
    }

    void nextDestroyIndex() {
        if (destroyProjectileIndex < initialPoolSize - 1) {
            destroyProjectileIndex++;
        } else {
            destroyProjectileIndex = 0;
        }
    }

    //Zum initialisieren von Pool Objekten
    GameObject createNewObject() {
        var go = Instantiate<GameObject>(objectPrefab);
        go.SetActive(false);
        return go;
    }

    //Deaktiviert Pool Objekte
    void DestroyObject() {
        poolObjects[destroyProjectileIndex].SetActive(false);
        nextDestroyIndex();
        noOfActiveObjects -= 1;
    }
}
