using FirebaseNetwork;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VisitManager : MonoBehaviour
{
    [Header("Common")]
    public int friendIndex = -1;
    public MyFriendData myFriend = null;
    //private UserData friendData = null;
    [SerializeField] private GameObject objectVisit = null;
    [SerializeField] private GameObject loadingUI = null;
    [SerializeField] private Button btnLike = null;

    [Header("Profile")]
    [SerializeField] private Sprite defaultProfileImg = null;
    [SerializeField] private TextMeshProUGUI txtLv = null;
    [SerializeField] private TextMeshProUGUI txtNick = null;
    [SerializeField] private TextMeshProUGUI txtStatus = null;
    [SerializeField] private RawImage imgProfile = null;
    [SerializeField] private TextMeshProUGUI txtCntFriend = null;
    [SerializeField] private TextMeshProUGUI txtCntLike = null;
    [SerializeField] private TextMeshProUGUI txtCntCat = null;

    [Header("Furniture")]
    [SerializeField] private GameObject objectWall = null;                              // 배치 벽지 오브젝트
    [SerializeField] private GameObject objectFloor = null;                             // 배치 바닥재 오브젝트
    [SerializeField] private Transform transformItemFurniture = null;                   // 가구 배치 부모 트랜스폼
    [SerializeField] private GameObject prefabItemFurniture = null;                     // 배치 가구 프리팹
    private List<Item_VisitFurniture> itemFurnitures = new List<Item_VisitFurniture>(); // 가구 리스트
    private int cntArrFurniture = 0;                                                    // 배치 가구수

    [Header("Cat")]
    [SerializeField] private Transform transformItemCat = null;                         // 고양이 배치 부모 트랜스폼
    [SerializeField] private GameObject prefabItemCat = null;                           // 배치 고양이 프리팹
    private List<Item_VisitCat> itemCats = new List<Item_VisitCat>();                   // 배치 고양이 리스트
    private int cntArrCat = 0;                                                          // 배치 고양이수

    [Header("Gift")]
    [SerializeField] private CanvasGroup canvasGift = null;
    [SerializeField] private TextMeshProUGUI txtGiftMessage = null;
    [SerializeField] private TextMeshProUGUI txtGiftCoin = null;
    [SerializeField] private TextMeshProUGUI txt_GiftOk = null;
    [SerializeField] private GameObject objectGiftBox = null;                           // 선물 상자 오브젝트
    [SerializeField] private GameObject particleGiftBox = null;                           // 선물 상자 오브젝트
    private Animator animGiftBox = null;                                                // 선물 상자 애니메이터
    private string strDefault_gift = "Advertise/UI/";

    #region Basic
    private void Awake()
    {
        for (int i = 0; i < TableDataManager.Instance.table_Setting["Furniture_Limited"].Value; i++)
        {
            Item_VisitFurniture item = Instantiate(prefabItemFurniture, transformItemFurniture).GetComponent<Item_VisitFurniture>();
            item.gameObject.SetActive(false);
            itemFurnitures.Add(item);
        }
        for (int i = 0; i < TableDataManager.Instance.table_Setting["Cat_Place_Max"].Value; i++)
        {
            Item_VisitCat item = Instantiate(prefabItemCat, transformItemCat).GetComponent<Item_VisitCat>();
            item.gameObject.SetActive(false);
            itemCats.Add(item);
        }
        txt_GiftOk.text = TableDataManager.Instance.table_String[strDefault_gift + "BtnOk"].Contents[(int)UI_Setting.language];
        animGiftBox = objectGiftBox.GetComponent<Animator>();
    }

    public void VisitSetting(int index, Texture2D profile)
    {
        objectVisit.SetActive(true);
        loadingUI.SetActive(true);
        UIManager.instance.isMain = true;
        friendIndex = index;
        myFriend = Authentication.Inst.userData.myFriends.Find(data => data.index == index);
        btnLike.enabled = true;
        btnLike.interactable = !myFriend.like;
        if (!myFriend.gift) MakeGiftBox();
        StartCoroutine(NetworkMethod.CoGetDataInfo(index, (result) =>
        {
            //friendData = result;
            ProfileVisit(result, profile);
            PlacementVisit(result);
            loadingUI.SetActive(false);
        }));
    }

    public void ExitVisit()
    {
        objectWall.GetComponent<SpriteRenderer>().sprite = null;
        objectFloor.GetComponent<SpriteRenderer>().sprite = null;

        for (int i = 0; i < cntArrFurniture; i++)
        {
            itemFurnitures[i].ResetData();
            itemFurnitures[i].gameObject.SetActive(false);
        }
        cntArrFurniture = 0;
        for (int i = 0; i < cntArrCat; i++)
        {
            itemCats[i].SetOrder(0);
            itemCats[i].gameObject.SetActive(false);
        }
        cntArrCat = 0;
        objectGiftBox.SetActive(false);
        particleGiftBox.SetActive(true);
        friendIndex = -1;
        myFriend = null;
        UIManager.instance.isMain = false;
        objectVisit.SetActive(false);
    }

    public void LikeFriend()
    {
        btnLike.interactable = false;
    }
    #endregion

    #region Profile
    private void ProfileVisit(UserData friend, Texture2D profile)
    {
        txtLv.text = friend.level.ToString();
        txtNick.text = friend.nickName;
        if (profile != null)
        {
            imgProfile.material.mainTexture = profile;
            imgProfile.texture = profile;
        }
        else
        {
            imgProfile.texture = defaultProfileImg.texture;
        }
        txtStatus.text = friend.status;
        txtCntFriend.text = (friend.myFriends == null) ? "0" : friend.myFriends.Count.ToString();
        txtCntLike.text = friend.like.ToString();
        txtCntCat.text = (friend.cats == null) ? "0" : friend.cats.Count.ToString();
    }
    #endregion

    #region Placement
    private void PlacementVisit(UserData friend)
    {
        FurnitureData curBuffBowl = friend.furnitures.Find((furniture) => furniture.idx == friend.curBuffBowl);
        FurnitureData curWallPaper = friend.furnitures.Find((furniture) => furniture.idx == friend.curWallPaper);
        FurnitureData curFloorMaterial = friend.furnitures.Find((furniture) => furniture.idx == friend.curFloorMaterial);

        if (curWallPaper != null)
        {
            objectWall.GetComponent<SpriteRenderer>().sprite = curWallPaper.SubSprite;
            objectWall.GetComponent<Transform>().position = curWallPaper.position;
        }
        if (curFloorMaterial != null)
        {
            objectFloor.GetComponent<SpriteRenderer>().sprite = curFloorMaterial.SubSprite;
            objectFloor.GetComponent<Transform>().position = curFloorMaterial.position;
        }


        foreach(FurnitureData furniture in friend.furnitures.FindAll(x => (x.SubType != FurnitureSubType.WallPaper && x.SubType != FurnitureSubType.FloorMaterial) && x.state != FurnitureState.Stored))
        {
            Item_VisitFurniture fur = itemFurnitures[cntArrFurniture];
            fur.gameObject.SetActive(true);
            fur.SetData(furniture);
            fur.transform.position = furniture.position;
            cntArrFurniture++;
            if (furniture.state == FurnitureState.UsingCat)
            {
                CatData cat = friend.cats.Find(temp => temp.idx == furniture.arrangeIndex);
                fur.ArrangeCat(cat, furniture.arrangeMarker);
            }
        }
        if(cntArrFurniture > 0) SortingFurniture();

        foreach(CatData cat in friend.cats.FindAll(x=> x.state == CatState.InInterior))
        {
            Item_VisitCat visitCat = itemCats[cntArrCat];
            visitCat.gameObject.SetActive(true);
            visitCat.transform.position = new Vector3(Random.Range(-14.0f, 14.0f), Random.Range(-8.0f, 2.5f), 1);
            visitCat.SetData(cat);
            cntArrCat++;
        }
    }

    private void SortingFurniture()
    {
        for (int i=0; i<cntArrFurniture-1; i++)
        {
            int temp_i = i;
            float temp_posy = itemFurnitures[i].transform.position.y - itemFurnitures[i].furniture.Sprite.rect.height * 0.005f;
            for (int j=i; j<cntArrFurniture;j++)
            {
                float j_posy = itemFurnitures[j].transform.position.y - itemFurnitures[j].furniture.Sprite.rect.height * 0.005f;
                if(j_posy > temp_posy)
                {
                    temp_i = j;
                    temp_posy = j_posy;
                }
            }
            Item_VisitFurniture temp = itemFurnitures[i];
            itemFurnitures[i] = itemFurnitures[temp_i];
            itemFurnitures[temp_i] = temp;
            itemFurnitures[i].ChangeOrderLayer(i * 6);
        }
        itemFurnitures[cntArrFurniture - 1].ChangeOrderLayer((cntArrFurniture - 1) * 6);
    }
    #endregion

    #region Gift
    private void MakeGiftBox()
    {
        objectGiftBox.transform.position = new Vector3(Random.Range(-14.0f, 14.0f), Random.Range(-8.0f, 2.5f), 1);
        objectGiftBox.SetActive(true);
    }

    public void OnClickGift()
    {
        animGiftBox.SetBool("isOpen",true);
        particleGiftBox.SetActive(false);
        StartCoroutine(WaitOpenBoxAnimation());
    }

    public void OnClickYesGiftUI()
    {
        canvasGift.blocksRaycasts = false;
        canvasGift.alpha = 0;
    }

    private void ShowGiftPopUP(int coin)
    {
        UserDataManager.Inst.AddCoin(5);
        myFriend.gift = true;
        NetworkMethod.SetFriendGift(Authentication.Inst.userData.myFriends.IndexOf(myFriend), myFriend.gift);
        objectGiftBox.SetActive(false);
        string msg = TableDataManager.Instance.table_String[strDefault_gift + "Coin"].Contents[(int)UI_Setting.language];
        txtGiftMessage.text = msg.Replace("0", coin.ToString());
        txtGiftCoin.text = coin.ToString();
        canvasGift.blocksRaycasts = true;
        canvasGift.alpha = 1;
    }

    private IEnumerator WaitOpenBoxAnimation()
    {
        while (true)
        {
            if (animGiftBox.GetCurrentAnimatorStateInfo(0).IsName("Base.GiftOpen") && animGiftBox.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99)
            {
                ShowGiftPopUP(5);
                yield break;
            }
            yield return null; ;
        }
    }
    #endregion
}
