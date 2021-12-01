using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_CatDataList : MonoBehaviour
{
    [SerializeField] private UI_CatDataDetail catDataDetailUI = null;
    [SerializeField] private RectTransform contents = null;
    [SerializeField] private GameObject catDataListPrefab = null;
    [SerializeField] private List<ListItem_CatData> listCatItem = new List<ListItem_CatData>();
    [SerializeField] private List<ListItem_CatData> listCatItemReal = new List<ListItem_CatData>();

    public void OnClickCat(CatData catData)
    {
        catDataDetailUI.SetCatDetail(catData);
        UIManager.instance.OpenPopUpChildControl(-1);
    }

    // 보유 고양이 패널 생성
    public void CreateCatDataItem()
    {
        int countCats = Authentication.Inst.userData.cats.Count;
        // 생성할 빈 패널 수 계산
        int blankPanel = (countCats > 12) ? ((countCats - 1) / 3 + 1) * 3 : 12;

        // 빈 패널 생성
        for (int i = 0; i < blankPanel; i++)
        {
            ListItem_CatData catDataListItemGo = Instantiate(catDataListPrefab, contents).GetComponent<ListItem_CatData>();

            // 관리용 목록에 추가
            listCatItem.Add(catDataListItemGo);
        }

        // 패널에 고양이 데이터 삽입
        for (int i = 0; i < countCats; i++)
        {
            CatData catData = Authentication.Inst.userData.cats[i];
            listCatItemReal.Add(listCatItem[i]);
            listCatItem[i].Init(catData);
            int _i = i;
            // 고양이 버튼을 눌렀을때 실행되는 함수 추가 
            listCatItem[i].btnCatData.onClick.AddListener(() => {
                OnClickCat(catData);
                SoundManager.Instance.PlayClick();
            });
        }
        ReArrangeCatList();
    }

    public void OnClickCloseCatDetail()
    {
        UIManager.instance.OpenPopUpChildControl(0);
    }

    // 고양이 추가 함수
    public void AddCatDataItem()
    {
        int countCats = Authentication.Inst.userData.cats.Count;
        // 빈 패널이 부족할 경우 신규 빈 패널 생성
        if (listCatItem.Count < countCats)
        {
            for (int i = 0; i < 3; i++)
            {
                ListItem_CatData catDataListItemGo = Instantiate(catDataListPrefab, contents).GetComponent<ListItem_CatData>();

                // 관리용 목록에 추가
                listCatItem.Add(catDataListItemGo);
            }
        }

        // 패널에 고양이 데이터 삽입
        int index = countCats - 1;
        CatData catData = Authentication.Inst.userData.cats[index];
        listCatItemReal.Add(listCatItem[index]);
        listCatItem[index].Init(catData);
        // 고양이 버튼을 눌렀을때 실행되는 함수 추가 
        listCatItem[index].btnCatData.onClick.AddListener(() => {
            catDataDetailUI.SetCatDetail(catData);
            UIManager.instance.OpenPopUpChildControl(-1);
            SoundManager.Instance.PlayClick();
        });
        ReArrangeCatList();
    }

    // 고양이 제거 함수
    public void RemoveCatItem(int index)
    {
        // 선택된 고양이 오프젝트 제거 및 리스트에서 삭제
        int _index = listCatItemReal.IndexOf(listCatItem[index]);
        Destroy(listCatItem[index].gameObject);
        listCatItem.RemoveAt(index);
        listCatItemReal.RemoveAt(_index);

        //  빈패널이 2개 남는 경우 제거 및 남지 않을 경우 맨 뒤에 빈패널 추가 생성
        if (listCatItem.Count - 2 == Authentication.Inst.userData.cats.Count && listCatItem.Count > 12)
        {
            Destroy(listCatItem[listCatItem.Count - 1].gameObject);
            listCatItem.RemoveAt(listCatItem.Count - 1);
            Destroy(listCatItem[listCatItem.Count - 1].gameObject);
            listCatItem.RemoveAt(listCatItem.Count - 1);
        }
        else
        {
            ListItem_CatData catDataListItemGo = Instantiate(catDataListPrefab, contents).GetComponent<ListItem_CatData>();
            listCatItem.Add(catDataListItemGo);
        }
    }

    // 고양이 이름 변경
    public void ChangeName(int index, string name)
    {
        listCatItem[index].ChangeName(name);
    }

    // 고양이 레벨 변경
    public void ChangeLevel(int index, int level)
    {
        listCatItem[index].ChangeLevel(level);
        ReArrangeCatList();
    }

    // 고양이 가구 배치 유무에 따른 패널 마스크 생성
    public void ChangeState(int index, CatState state)
    {
        listCatItem[index].ChangeInOutFuniture(state);
        ReArrangeCatList();
    }

    // 보유 고양이 리스트 재배열 (상태 자유, 커마 고양이, 레벨 높은 순, 고양이 인덱스가 빠른 순으로 나열)
    private void ReArrangeCatList()
    {
        for (int i = 0; i < listCatItemReal.Count - 1; i++)
        {
            int tmp = i;
            for (int j = i + 1; j < listCatItemReal.Count; j++)
            {
                CatData cat_tmp = listCatItemReal[tmp].catData;
                CatData cat_j = listCatItemReal[j].catData;
                CatTable catTable_tmp = TableDataManager.Instance.table_CatTable[cat_tmp.catTableIndex];
                CatTable catTable_j = TableDataManager.Instance.table_CatTable[cat_j.catTableIndex];
                int index_tmp = listCatItem.IndexOf(listCatItemReal[tmp]);
                int index_j = listCatItem.IndexOf(listCatItemReal[j]);
                if (cat_tmp.state > cat_j.state && cat_j.state == 0)
                {
                    tmp = j;
                }
                else if ((cat_tmp.state > 0 && cat_j.state > 0) || cat_tmp.state == cat_j.state)
                {
                    if (catTable_tmp.Cat_Level_Index < catTable_j.Cat_Level_Index)
                    {
                        tmp = j;
                    }
                    else if (catTable_tmp.Cat_Level_Index == catTable_j.Cat_Level_Index)
                    {
                        if (cat_tmp.level < cat_j.level)
                        {
                            tmp = j;
                        }
                        else if (cat_tmp.level == cat_j.level)
                        {
                            if (index_tmp > index_j)
                            {
                                tmp = j;
                            }
                        }
                    }
                }
            }
            listCatItemReal[tmp].gameObject.transform.SetSiblingIndex(i);
            ListItem_CatData temp = listCatItemReal[i];
            listCatItemReal[i] = listCatItemReal[tmp];
            listCatItemReal[tmp] = temp;
        }
    }
}
