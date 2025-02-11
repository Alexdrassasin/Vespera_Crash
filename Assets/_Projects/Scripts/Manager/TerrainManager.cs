
using NUnit.Framework;
using PurrNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class TerrainManager : NetworkBehaviour
{
    [System.Serializable]
    public class TerrainData
    {
        public string name;
        public AssetReferenceGameObject terrainModel;
        public Material skyboxMaterial;
        public GameObject SpawnPointSet;
        public GameObject Prefab;
    }

    public List<TerrainData> data = new List<TerrainData>();

    public TerrainData currentTerrainData;
    public bool isTerrainDoneSpawned;
    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }
    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<TerrainManager>();
    }

    private void Start()
    {
        //StartCoroutine(InitializeTerrain());
    
    }
    private IEnumerator InitializeTerrain()
    {
        while (!InstanceHandler.GetInstance<DataCarrier>())
        {
            yield return null;
        }
        //SpawnTerrain(InstanceHandler.GetInstance<DataCarrier>().selectedMap);
        SpawnTerrain_noAddressable(InstanceHandler.GetInstance<DataCarrier>().selectedMap);
    }



    public TerrainData GetTerrainDataByName(string terrainName)
    {
        // Use LINQ to find the TerrainData object with the matching name
        TerrainData result = data.FirstOrDefault(terrain => terrain.name == terrainName);

        if (result != null)
        {
            Debug.Log("Found TerrainData: " + result.name);
            return result;
        }
        else
        {
            Debug.LogWarning("TerrainData not found with name: " + terrainName);
            return null; // Or throw an exception if you prefer
        }
    }

    public void SpawnTerrain(string name)
    {
        AssetReferenceGameObject targetReference;

        currentTerrainData = GetTerrainDataByName(name);
        currentTerrainData.terrainModel.LoadAssetAsync().Completed += OnAddressableLoaded;
        RenderSettings.skybox = currentTerrainData.skyboxMaterial;
        currentTerrainData.SpawnPointSet.SetActive(true);
    }

    public void SpawnTerrain_noAddressable(string name)
    {
        currentTerrainData = GetTerrainDataByName(name);
        var terrainModel = Instantiate(currentTerrainData.Prefab);
        InstanceHandler.GetInstance<ObjectPoolManager>().destructibleObjects = terrainModel.GetComponent<TerrainController>().destructibleObjects;
        isTerrainDoneSpawned = true;
        RenderSettings.skybox = currentTerrainData.skyboxMaterial;
        currentTerrainData.SpawnPointSet.SetActive(true);
    }

    void OnAddressableLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            var terrainModel = Instantiate(handle.Result);
            InstanceHandler.GetInstance<ObjectPoolManager>().destructibleObjects = terrainModel.GetComponent<TerrainController>().destructibleObjects;
            isTerrainDoneSpawned = true;
        }
        else
        {
            Debug.LogError("Loading Asset Failed!", this);
        }
    }

    public List<Transform> RetriveSpawnPoint()
    {
        List<Transform> spawnPoints = new List<Transform>();
        foreach(Transform point in currentTerrainData.SpawnPointSet.transform)
        {
            spawnPoints.Add(point);
        }  
        return spawnPoints;
    }
}
