using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private List<ObjectPoolInfo> objectPools = new List<ObjectPoolInfo>();
    private GameObject objectPoolContainer;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupObjectPoolContainer();
    }

    #region OBJECT POOLING
    public GameObject GetPoolObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        // Check if a pool for this object already exists
        ObjectPoolInfo objectPool = objectPools.Find(x => x.objectName == objectToSpawn.name);

        // If no pool exists for this object, create one
        if (objectPool == null) {
            objectPool = new ObjectPoolInfo() {
                objectName = objectToSpawn.name,
                objectToSpawn = objectToSpawn,
                parentTransform = CreatePoolContainer(objectToSpawn)
            };

            objectPools.Add(objectPool);
        }

        // Check if any inactive objects are available in the selected pool
        GameObject spawnableObject = objectPool.inactiveObjectPool.FirstOrDefault();

        if (spawnableObject == null) {
            // You do not need to add this object to the inactive pool
            // It will be added to the pool when it is returned
            spawnableObject = Instantiate(objectToSpawn, spawnPosition, spawnRotation, objectPool.parentTransform);
        } else {
            spawnableObject.transform.position = spawnPosition;
            spawnableObject.transform.rotation = spawnRotation;
            spawnableObject.SetActive(true);
            objectPool.inactiveObjectPool.Remove(spawnableObject);
        }

        return spawnableObject;
    }

    public void ReturnPoolObject(GameObject objectToPool)
    {
        ObjectPoolInfo objectPool = objectPools.Find(x => x.objectName == RemoveCloneString(objectToPool.name));

        if (objectPool != null) {
            objectToPool.SetActive(false);
            objectPool.inactiveObjectPool.Add(objectToPool);
        } else {
            Debug.Log("No Pool Found for " + objectToPool.name);
        }
    }
    #endregion

    #region HELPER FUNCTIONS
    private void SetupObjectPoolContainer()
    {
        objectPoolContainer = new GameObject("Object Pools");
    }

    private Transform CreatePoolContainer(GameObject objectToSpawn)
    {
        GameObject newObjectPoolContainer = new GameObject(RemoveCloneString(objectToSpawn.name) + " Pool");
        newObjectPoolContainer.transform.SetParent(objectPoolContainer.transform);
        return newObjectPoolContainer.transform;
    }

    private string RemoveCloneString(string objectName) => objectName.Substring(0, objectName.Length - 7);
    #endregion
}

[System.Serializable]
public class ObjectPoolInfo
{
    public string objectName;
    public GameObject objectToSpawn;
    public Transform parentTransform;
    public List<GameObject> inactiveObjectPool = new List<GameObject>();

}
