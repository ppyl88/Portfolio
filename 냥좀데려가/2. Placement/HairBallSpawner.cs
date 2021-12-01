using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HairBallSpawner : MonoBehaviour
{
    [SerializeField] private SortingGroup parentRenderer = null;
    [SerializeField] private GameObject prefabHairBall = null;
    [SerializeField] private GameObject parentObject = null;
    [SerializeField] private GameObject groundObject = null;
    private float[] countTimes;
    public bool making = true;

    private void Awake()
    {
        countTimes = new float[FurniturePlacement.Instance.itemHairBalls.Count];
        making = true;
    }

    private void Start()
    {
        for(int i=0; i<countTimes.Length; i++)
        {
            countTimes[i] = Random.Range(-1.5f, 1.5f);
        }
    }

    private void Update()
    {
        if(making && UserDataManager.Inst.cntHairBall < FurniturePlacement.Instance.maxHairBall)
        {
            for (int i = 0; i < FurniturePlacement.Instance.itemHairBalls.Count; i++)
            {
                countTimes[i] += Time.deltaTime;
                if (countTimes[i] >= FurniturePlacement.Instance.itemHairBalls[i].HairBall_Spawn_Time)
                {
                    int rnd = Random.Range(0, 100);
                    if (rnd < (int)FurniturePlacement.Instance.itemHairBalls[i].HairBall_Spawn_Rate)
                    {
                        SpawnHairBall(i);
                    }
                    countTimes[i] = 0;
                    if (UserDataManager.Inst.cntHairBall >= FurniturePlacement.Instance.maxHairBall) break;
                }
            }
        }
    }

    public void SpawnHairBall(int index)
    {
        FurniturePlacement.Instance.AddHairBall(index, parentRenderer.sortingOrder, transform.position, parentObject, groundObject);
    }
}
