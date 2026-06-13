using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    [Table("T_VipGiftCommand_v1")]
    public class T_VipGiftCommand
    {
        [PrimaryKey]
        public required string VipGiftId { get; set; }

        [PrimaryKey]
        public int CommandId { get; set; }
    }
}
