// 배치가능 가구/고양이 리스트 데이터
public class AbleData
{
    public bool isFurniture;
    public FurnitureData furniture = null;
    public CatData cat = null;

    // 배치가능 가구 생성자
    public AbleData(FurnitureData furniture)
    {
        isFurniture = true;
        this.furniture = furniture;
        this.cat = null;
    }

    // 배치가능 고양이 생성자
    public AbleData(CatData cat)
    {
        isFurniture = false;
        this.cat = cat;
        this.furniture = null;
    }
}
