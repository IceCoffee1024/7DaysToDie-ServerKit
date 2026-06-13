using IceCoffee.SimpleCRUD.OptionalAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.Data.Entities
{
    [Table("T_VipGiftItem_v1")]
    public class T_VipGiftItem
    {
        [PrimaryKey]
        public string VipGiftId { get; set; }

        [PrimaryKey]
        public int ItemId { get; set; }
    }
}
