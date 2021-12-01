using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_AdoptReward : MonoBehaviour
{
    [SerializeField] private RectTransform contentRewardList = null;
    [SerializeField] private Image imgPerson = null;
    [SerializeField] private Image imgCat = null;
    [SerializeField] private Image imgCatTutorial = null;
    [SerializeField] private CanvasGroup canvasCat = null;
    [SerializeField] private CanvasGroup canvasCatTutorial = null;
    [SerializeField] private TextMeshProUGUI txtRewardDesc = null;
    [SerializeField] private GameObject[] rewards = null;
    [SerializeField] private GameObject btnAdvertise = null;

    // 입양 보상 화면 세팅
    public void Init(int adoptIndex, int adoptedCat)
    {
        CatData catData = Authentication.Inst.userData.cats[adoptedCat];
        CatTable catTableData = TableDataManager.Instance.table_CatTable[catData.catTableIndex];
        Adopt_List adopt = TableDataManager.Instance.table_Adopt_List[adoptIndex];
        RewardData[] adoptReward = adopt.Rewards;

        if (adopt.Advertisement != 0 && Authentication.Inst.userData.todayAdvertise < TableDataManager.Instance.table_Setting["Advertisement_Watched_Count"].Value)
        {
            btnAdvertise.SetActive(true);
            string str = btnAdvertise.GetComponentInChildren<TextMeshProUGUI>().text;
            btnAdvertise.GetComponentInChildren<TextMeshProUGUI>().text = str.Replace("0", adopt.Advertisement.ToString());
        }
        else
        {
            btnAdvertise.SetActive(false);
        }
        // 입양 후원자 세팅
        imgPerson.sprite = adopt.NPC;
        if(adopt.NPC_Resource == 10)
        {
            canvasCat.alpha = 0;
            canvasCatTutorial.alpha = 1;
            imgCatTutorial.sprite = catTableData.SitSprite;
        }
        else
        {
            canvasCat.alpha = 1;
            canvasCatTutorial.alpha = 0;
            imgCat.sprite = catTableData.SitSprite;
        }

        // 입양 보상 멘트 세팅
        txtRewardDesc.text = adopt.RewardDesc;

        // 입양 보상 UI 전체 비활성화
        for (int i =0; i<rewards.Length; i++)
        {
            rewards[i].SetActive(false);
        }

        // 입양 보상 UI 리스트 생성
        for (int i = 0; i < adoptReward.Length; i++)
        {
            RewardData reward = adoptReward[i];

            if (reward.Type != RewardType.None)
            {
                int idx = (int)reward.Type - 1;
                if (reward.Type == RewardType.Furniture)
                {
                    rewards[idx].GetComponentInChildren<Image>().sprite = TableDataManager.Instance.table_Furniture[reward.Index].Sprite;
                }
                else if (reward.Type == RewardType.Item)
                {
                    rewards[idx].GetComponentInChildren<Image>().sprite = TableDataManager.Instance.table_Item[reward.Index].Sprite;
                }
                rewards[idx].SetActive(true);
                rewards[idx].GetComponentInChildren<TextMeshProUGUI>().text = reward.Value.ToString();
            }
        }
    }
}
