using IceCoffee.SimpleCRUD.OptionalAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// Colored Chat
    /// </summary>
    public class T_ColoredChat
    {
        /// <summary>
        /// Player Id
        /// </summary>
        [PrimaryKey]
        public required string Id { get; set; }

        /// <summary>
        /// Created At
        /// </summary>
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Custom Name
        /// </summary>
        public string? CustomName { get; set; }

        /// <summary>
        /// Name Color
        /// </summary>
        public string? NameColor { get; set; }

        /// <summary>
        /// Text Color
        /// </summary>
        public string? TextColor { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }
    }
}
