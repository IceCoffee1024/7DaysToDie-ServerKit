using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    [Table("T_LevelGiftCommand_v1")]
    public class T_LevelGiftCommand
    {
        [PrimaryKey]
        public string LevelGiftId { get; set; }

        [PrimaryKey]
        public int CommandId { get; set; }
    }
}
