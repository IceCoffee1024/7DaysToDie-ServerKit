using IceCoffee.SimpleCRUD.OptionalAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.Data.Entities
{
    public class T_GoodsCommand
    {
        [PrimaryKey]
        public int GoodsId { get; set; }

        [PrimaryKey]
        public int CommandId { get; set; }
    }
}
