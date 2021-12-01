using UnityEngine;
using System.Collections;

public class HairBall : MonoBehaviour
{
    [SerializeField] private Canvas canvasHairBall = null;
    [SerializeField] private SpriteRenderer spriteRenderer = null;
    [SerializeField] private Rigidbody2D rigidBody = null;
    [SerializeField] private ParticleSystem particle = null;
    private GameObject parentObject = null;
    private GameObject groundObject = null;
    public float countTime = 0f;
    public int index_HairBall;
    private bool isClicked = false;
    private bool isPicked = false;
    private bool collisionDetect = false;
    private bool onGround = false;
    private float yGround;
    private float rnd_v = 0;
    private float rnd_a = 0;
    private bool afterSetting = false;
    private Vector2 curVelocity;
    private float curAngVelocity;
    private enum Direction
    {
        Left,
        Right
    }
    private Direction direction;

    private void OnEnable()
    {
        if (!afterSetting)
        {
            rigidBody.velocity = curVelocity;
            rigidBody.angularVelocity = curAngVelocity;
        }
        StartCoroutine(CoHairBall());
    }

    private void OnDisable()
    {
        afterSetting = false;
    }

    IEnumerator CoHairBall()
    {
        while (true)
        {
            if(rigidBody.velocity.x > 0.0000001f || rigidBody.velocity.x < -0.0000001f)
            {
                curVelocity = rigidBody.velocity;
                curAngVelocity = rigidBody.angularVelocity;
                if (!collisionDetect)
                {
                    if (rigidBody.velocity.y < 0f) collisionDetect = true;
                }
                else
                {
                    if (!onGround)
                    {
                        if (collisionDetect && transform.position.y <= yGround)
                        {
                            onGround = true;
                            rnd_v = Random.Range(0.1f, 0.2f);
                            rnd_a = Random.Range(0.05f, 0.1f);
                            rigidBody.constraints = RigidbodyConstraints2D.FreezePositionY;
                        }
                    }
                    else
                    {
                        rigidBody.velocity -= rigidBody.velocity * rnd_v;
                        rigidBody.angularVelocity -= rigidBody.angularVelocity * rnd_a;
                    }
                }
            }
            else
            {
                rigidBody.velocity = Vector2.zero;
                rigidBody.angularVelocity = 0;
            }

            if (isClicked)
            {
                spriteRenderer.enabled = false;
                particle.Play();
                SoundManager.Instance.PlayEffect(SoundType.Pickhairball);
                yield return YieldInstructionCache.WaitForSeconds(0.2f);
                StorageManager.Instance.AddItem(index_HairBall, 1, Authentication.Inst.userData.endTutorial);
                FurniturePlacement.Instance.RemoveHairBall(this);
                yield break;
            }
            else if (isPicked)
            {
                spriteRenderer.enabled = false;
                particle.Play();
                SoundManager.Instance.PlayEffect(SoundType.Pickhairball);
                yield return YieldInstructionCache.WaitForSeconds(0.2f);
                FurniturePlacement.Instance.RemoveHairBall(this);
                yield break;
            }
            if (!collisionDetect && rigidBody.velocity.y < 0f)
            {
                collisionDetect = true;
            }
            else
            {
                curVelocity = rigidBody.velocity;
                curAngVelocity = rigidBody.angularVelocity;
            }
            countTime += Time.deltaTime;
            if (countTime >= FurniturePlacement.Instance.lifeTimeHairBall)
            {
                FurniturePlacement.Instance.RemoveHairBall(this);
                yield break;
            }
            yield return null;
        }
    }

    public void SetHairBall(Item_Table hairBall, int renderOrder, int canvasOrder, GameObject _parentObject, GameObject _groundObject)
    {
        spriteRenderer.sprite = hairBall.Sprite;
        spriteRenderer.sortingOrder = renderOrder;
        spriteRenderer.enabled = true;
        canvasHairBall.sortingOrder = canvasOrder;
        rigidBody.constraints = RigidbodyConstraints2D.None;
        index_HairBall = hairBall.Index;
        parentObject = _parentObject;
        groundObject = _groundObject;
        yGround = groundObject.transform.position.y + 1;

        countTime = 0f;
        collisionDetect = false;
        onGround = false;
        isClicked = false;
        isPicked = false;
        afterSetting = true;

        gameObject.SetActive(true);
        direction = (Direction) Random.Range(0, 2);
        float rnd_X = Random.Range(80, 120);
        float rnd_Y = Random.Range(250, 300);
        float rnd_t = Random.Range(30, 50);
        if(direction == Direction.Left)
        {
            rigidBody.AddForce(new Vector2(-rnd_X, rnd_Y));
            rigidBody.AddTorque(rnd_t);
        }
        else
        {
            rigidBody.AddForce(new Vector2(rnd_X, rnd_Y));
            rigidBody.AddTorque(-rnd_t);
        }

        curVelocity = rigidBody.velocity;
        curAngVelocity = rigidBody.angularVelocity;

    }

    public void OnClickHairBall()
    {
        ItemData hairBall = Authentication.Inst.userData.items.Find(item => item.index == index_HairBall);
        if (hairBall == null || hairBall.count < TableDataManager.Instance.table_Setting["HairBall_Store_Max"].Value) isClicked = true;
        else UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String["Placement/Warning/CantGetHairBall"].Contents[(int)UI_Setting.language]);
    }

    public void AutoPickDetected()
    {
        ItemData hairBall = Authentication.Inst.userData.items.Find(item => item.index == index_HairBall);
        if (hairBall == null || hairBall.count < TableDataManager.Instance.table_Setting["HairBall_Store_Max"].Value) isPicked = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Cat") return;
        SpriteRenderer col_spriteRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
        float diffY = transform.position.y - collision.transform.position.y;
        if(onGround && collision.tag == "FloorFurniture")
        {
            if (diffY < -col_spriteRenderer.sprite.rect.height * 0.005 + 0.5f)
            {
                spriteRenderer.sortingOrder = col_spriteRenderer.sortingOrder + 4;
            }
            else
            {
                spriteRenderer.sortingOrder = col_spriteRenderer.sortingOrder - 2;
            }
        }
        if (collision.tag == "FloorFurniture" && collision.gameObject != parentObject && col_spriteRenderer.sortingOrder >= spriteRenderer.sortingOrder)
        {
            if (diffY < col_spriteRenderer.sprite.rect.height * 0.005)
            {
                float rnd = Random.Range(0.4f, 0.8f);
                rigidBody.velocity = new Vector2(rigidBody.velocity.x * -rnd, rigidBody.velocity.y);
            }
        }
        if (collision.tag == "CatBlockLeft" || collision.tag == "CatBlockRight")
        {
            float rnd = Random.Range(0.1f, 0.3f);
            rigidBody.velocity = new Vector2(rigidBody.velocity.x * -rnd, rigidBody.velocity.y);
        }
        if (collision.tag == "AutoPickCleaner")
        {
            collision.gameObject.GetComponent<AutoPickCleaner>().HairBallDetected(index_HairBall);
            AutoPickDetected();
        }
    }
}
