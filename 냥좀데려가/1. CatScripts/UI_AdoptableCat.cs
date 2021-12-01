using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_AdoptableCat : MonoBehaviour
{
    [Header("Selected Cat Data")]
    [SerializeField] private Image imgCat = null;
    [SerializeField] private TextMeshProUGUI txtName = null;
    [SerializeField] private TextMeshProUGUI txtType = null;
    [SerializeField] private TextMeshProUGUI[] txtStat = null;

    [Header("CatList")]
    [SerializeField] private GameObject adoptableCatPrefab = null;
    [SerializeField] private RectTransform contentAdoptableCatList = null;

    [SerializeField] private Button btnSelect = null;
    public int selectedCat = -1;
    public bool existCat;
    private int selectedCat_temp = -1;
    private List<ListItem_AdoptableCat> listAdoptableCats = new List<ListItem_AdoptableCat>();

    // 입양 가능 고양이 리스트 세팅
    public void Init(int adoptIndex)
    {
        ResetCatList();
        int cnt = 0;
        // 입양 가능한 고양이 리스트 생성
        for (int i = 0; i < Authentication.Inst.userData.cats.Count; i++)
        {
            if (CheckCondition(adoptIndex, i))
            {
                ListItem_AdoptableCat listItemGo = Instantiate(adoptableCatPrefab, contentAdoptableCatList).GetComponent<ListItem_AdoptableCat>();

                // 관리용 목록에 추가
                listAdoptableCats.Add(listItemGo);
                listItemGo.Init(i, Authentication.Inst.userData.cats[i]);

                // 고양이 버튼 클릭 연결
                int _cnt = cnt;
                listItemGo.btnSelectCat.onClick.AddListener(() =>
                {
                    OnClickCat(_cnt);
                    SoundManager.Instance.PlayClick();
                });
                cnt++;
            }
        }
        // 입양 가능한 고양이 유무에 따른 변수 설정
        if (listAdoptableCats.Count == 0)
        {
            existCat = false;
        }
        else
        {
            existCat = true;
        }
    }

    // 입양 가능 고양이 리스트 리셋
    public void ResetCatList()
    {
        btnSelect.interactable = false;
        selectedCat = -1;
        selectedCat_temp = -1;
        for (int i = 0; i < listAdoptableCats.Count; i++)
        {
            Destroy(listAdoptableCats[i].gameObject);
        }
        listAdoptableCats.Clear();
        imgCat.enabled = false;
        txtName.text = "";
        txtType.text = "";
        for (int i = 0; i < txtStat.Length; i++)
        {
            txtStat[i].text = "";
        }
        if (selectedCat != -1)
        {
            listAdoptableCats[selectedCat].UnCheck();
        }
    }

    // 입양 조건 달성 확인 함수
    private bool CheckCondition(int adoptIndex, int catIndex)
    {
        Adopt_List adopt = TableDataManager.Instance.table_Adopt_List[adoptIndex];
        CatData cat = Authentication.Inst.userData.cats[catIndex];
        if (cat.state == CatState.Free)
        {
            bool[] checkCondition = new bool[3] { false, false, false };
            for (int i = 0; i < 3; i++)
            {
                AdoptApplication application = adopt.AdoptApplications[i];
                if (application.Condition == AdoptCondition.None)
                {
                    checkCondition[i] = true;
                }
                else
                {
                    switch (application.Condition)
                    {
                        case AdoptCondition.MoreThanStat1:
                            if(cat.stat[0] >= application.Value)
                            {
                                checkCondition[i] = true;
                            }
                            break;
                        case AdoptCondition.MoreThanStat2:
                            if (cat.stat[1] >= application.Value)
                            {
                                checkCondition[i] = true;
                            }
                            break;
                        case AdoptCondition.MoreThanStat3:
                            if (cat.stat[2] >= application.Value)
                            {
                                checkCondition[i] = true;
                            }
                            break;
                        case AdoptCondition.CatType:
                            if (cat.TypeIndex == application.Value)
                            {
                                checkCondition[i] = true;
                            }
                            break;
                        case AdoptCondition.CatSubType:
                            if (cat.SubTypeIndex == application.Value)
                            {
                                checkCondition[i] = true;
                            }
                            break;
                    }
                }
            }
            return (checkCondition[0] && checkCondition[1] && checkCondition[2]);
        }
        else
        {
            return false;
        }
    }

    // 고양이 선택 적용
    public int Apply()
    {
        selectedCat = selectedCat_temp;
        return listAdoptableCats[selectedCat].catIndex;
    }

    // 고양이 선택 취소
    public void Cancel()
    {
        if (selectedCat != selectedCat_temp)
        {
            if (selectedCat != -1)
            {
                OnClickCat(selectedCat);
                selectedCat_temp = selectedCat;
            }
            else
            {
                btnSelect.interactable = false;
                imgCat.enabled = false;
                txtName.text = "";
                txtType.text = "";
                for (int i = 0; i < txtStat.Length; i++)
                {
                    txtStat[i].text = "";
                }
                listAdoptableCats[selectedCat_temp].UnCheck();
                selectedCat_temp = -1;
            }
        }
    }

    // 고양이 리스트 버튼 클릭 함수
    public void OnClickCat(int cnt)
    {
        imgCat.enabled = true;
        CatData catData = Authentication.Inst.userData.cats[listAdoptableCats[cnt].catIndex];
        if (catData.IconSprite == null)
        {
            imgCat.sprite = catData.SitSprite;
        }
        else
        {
            imgCat.sprite = catData.IconSprite;
        }
        txtName.text = catData.name;
        txtType.text = catData.TypeName;
        for (int i = 0; i < catData.stat.Length; i++)
        {
            txtStat[i].text = catData.stat[i].ToString();
        }
        listAdoptableCats[cnt].Check();
        if (selectedCat_temp != -1)
        {
            listAdoptableCats[selectedCat_temp].UnCheck();
        }
        selectedCat_temp = cnt;
        btnSelect.interactable = true;
    }
}