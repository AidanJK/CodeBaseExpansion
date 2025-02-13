using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    [SerializeField] private GameObject[] jars;

    [Header("JARS PARENT")]
    public Transform poolPatern;

    [Header("LISTS")]
    [HideInInspector] public List<GameObject> mugList;
    [HideInInspector] public List<GameObject> cupList;
    [HideInInspector] public List<GameObject> bootList;
    [HideInInspector] public List<GameObject> longList;

    [Header("PROBABILITYS AND POOL SIZE")]
    public int poolSizes;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        AddJarsToList(poolSizes);
    }
    private void AddJarsToList(int poolSize)
    {
        //MUGS
        for (int i = 0; i < poolSize; i++)
        {
            for (int j = 0; j < jars.Length; j++)
            {
                if (jars[j].GetComponent<Jar>().jarType == GameManager.JarType.Mug)
                {
                    GameObject go = Instantiate(jars[j]);
                    go.SetActive(false);
                    mugList.Add(go);
                    go.transform.SetParent(poolPatern, false);
                }
            }
        }

        //CUP
        for (int i = 0; i < poolSize; i++)
        {
            for (int j = 0; j < jars.Length; j++)
            {
                if (jars[j].GetComponent<Jar>().jarType == GameManager.JarType.Cup)
                {
                    GameObject go = Instantiate(jars[j]);
                    go.SetActive(false);
                    cupList.Add(go);
                    go.transform.SetParent(poolPatern, false);
                }
            }
        }

        //BOOT
        for (int i = 0; i < poolSize; i++)
        {
            for (int j = 0; j < jars.Length; j++)
            {
                if (jars[j].GetComponent<Jar>().jarType == GameManager.JarType.Boot)
                {
                    GameObject go = Instantiate(jars[j]);
                    go.SetActive(false);
                    bootList.Add(go);
                    go.transform.SetParent(poolPatern, false);
                }
            }
        }
        //LONG
        for (int i = 0; i < poolSize; i++)
        {
            for (int j = 0; j < jars.Length; j++)
            {
                if (jars[j].GetComponent<Jar>().jarType == GameManager.JarType.Long)
                {
                    GameObject go = Instantiate(jars[j]);
                    go.SetActive(false);
                    longList.Add(go);
                    go.transform.SetParent(poolPatern, false);
                }
            }
        }
    }
    public GameObject RequestJar()
    {
        GameManager.JarType jarType = ReturnJarType();
        if (jarType == GameManager.JarType.Mug)
        {
            for (int i = 0; i < mugList.Count; i++)
            {
                if (!mugList[i].activeSelf)
                {
                    mugList[i].SetActive(true);
                    return mugList[i];
                }
            }
            return null;
        }
        if (jarType == GameManager.JarType.Cup)
        {
            for (int i = 0; i < cupList.Count; i++)
            {
                if (!cupList[i].activeSelf)
                {
                    cupList[i].SetActive(true);
                    return cupList[i];
                }
            }
            return null;
        }
        if (jarType == GameManager.JarType.Boot)
        {
            for (int i = 0; i < bootList.Count; i++)
            {
                if (!bootList[i].activeSelf)
                {
                    bootList[i].SetActive(true);
                    return bootList[i];
                }
            }
            return null;
        }
        if (jarType == GameManager.JarType.Long)
        {
            for (int i = 0; i < longList.Count; i++)
            {
                if (!longList[i].activeSelf)
                {
                    longList[i].SetActive(true);
                    return longList[i];
                }
            }
            return null;
        }
        return null;
    }
    public void ChangeStartPosition(GameObject go)
    {
        go.transform.position = poolPatern.transform.position;
    }
    public GameManager.JarType ReturnJarType()
    {
        int random = Random.Range(0, 3);
        //cambiar el 0 a random para activar el randomizer
        switch (random)
        {
            case 0:
                {
                    return GameManager.JarType.Mug;
                }
            case 1:
                {
                    return GameManager.JarType.Boot;
                }
            case 2:
                {
                    return GameManager.JarType.Cup;
                }
            case 3:
                {
                    return GameManager.JarType.Long;
                }
            default:
                {
                    return GameManager.JarType.Mug;
                }
        }
    }
}
