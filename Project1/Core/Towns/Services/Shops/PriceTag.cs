using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;

namespace Project1.Core.Towns.Services.Shops;

internal class PriceTag : ISaveableNewNew<PriceTag>, ISerializableNew<PriceTag>
{
    public EntityRefId Item;
    public int Price;

    public PriceTag(EntityRefId item, int price)
    {
        Item = item;
        Price = price;
    }

    private PriceTag()
    {
        
    }
    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Item", this.Item);
        tag.Save("Price", this.Price);
        return tag;
    }
    public static PriceTag Create(SaveTag tag)
    {
        var itemid = tag.LoadEntityRefId("Item");
        var price = tag.LoadInt("Price");
        return new(itemid, price);
    }
    public void Write(IDataWriter w)
    {
        w.Write(this.Item);
        w.Write(this.Price);
    }

    public PriceTag Read(IDataReader r)
    {
        this.Item = r.ReadEntityRefId();
        this.Price = r.ReadInt32();
        return this;
    }

    public static PriceTag Create(IDataReader r)
        => new PriceTag().Read(r);
}
