using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPickCleaner : MonoBehaviour
{
    [SerializeField] private AutoPickManager manager = null;         // 자동 줍기 매니저

    public void HairBallDetected(int index)
    {
        manager.PickHairBall(index);
    }
}
