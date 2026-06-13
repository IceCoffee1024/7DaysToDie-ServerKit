using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    [Table("T_LevelGiftItem_v1")]
    public class T_LevelGiftItem
    {
        [PrimaryKey]
        public string LevelGiftId { get; set; }

        [PrimaryKey]
        public int ItemId { get; set; }
    }
}
