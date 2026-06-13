namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Item and Blocks
    /// </summary>
    [Authorize]
    [RoutePrefix("api/ItemBlocks")]
    public class ItemBlocksController : ApiController
    {
        /// <summary>
        /// Get item and blocks.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public Paged<ItemBlock> GetItemBlocks([FromUri] ItemBlockQuery model)
        {
            List<ItemBlock> itemBlocks;
            switch (model.ItemBlockKind)
            {
                case ItemBlockKind.All:
                    itemBlocks = GetAllItemsAndBlocks(model.Language, model.Keyword, model.ShowUserHidden);
                    break;

                case ItemBlockKind.Item:
                    itemBlocks = GetAllItems(model.Language, model.Keyword, model.ShowUserHidden);
                    break;

                case ItemBlockKind.Block:
                    itemBlocks = GetAllBlocks(model.Language, model.Keyword, model.ShowUserHidden);
                    break;
                default:
                    itemBlocks = new List<ItemBlock>();
                    break;
            }

            int pageSize = model.PageSize;
            var items = pageSize < 0 ? itemBlocks : itemBlocks.Skip((model.PageNumber - 1) * pageSize).Take(pageSize);
            var result = new Paged<ItemBlock>(items, itemBlocks.Count);

            return result;
        }

        private static List<Models.ItemBlock> GetAllBlocks(Language language, string? keyword, bool showUserHidden)
        {
            return GetItemsAndBlocks(0, Block.ItemsStartHere, item => item.IsBlock() == true, language, keyword, showUserHidden);
        }

        private static List<Models.ItemBlock> GetAllItems(Language language, string? keyword, bool showUserHidden)
        {
            return GetItemsAndBlocks(Block.ItemsStartHere, ItemClass.list.Length, item => item.IsBlock() == false, language, keyword, showUserHidden);
        }

        private static List<Models.ItemBlock> GetAllItemsAndBlocks(Language language, string? keyword, bool showUserHidden)
        {
            return GetItemsAndBlocks(0, ItemClass.list.Length, null, language, keyword, showUserHidden);
        }

        private static List<Models.ItemBlock> GetItemsAndBlocks(
            int startId,
            int endId,
            Func<ItemClass, bool>? filter,
            Language language,
            string? keyword,
            bool showUserHidden)
        {
            var result = new List<Models.ItemBlock>();
            for (int id = startId; id < endId; id++)
            {
                ItemClass itemClass = ItemClass.GetForId(id);
                if (itemClass != null)
                {
                    EnumCreativeMode creativeMode = itemClass.CreativeMode;
                    if ((creativeMode == EnumCreativeMode.All || showUserHidden) && (filter == null || filter.Invoke(itemClass)))
                    {
                        string itemName = itemClass.GetItemName();

                        if (string.IsNullOrEmpty(itemName))
                        {
                            continue;
                        }

                        string localizationName = Utilities.Utils.GetLocalization(itemName, language);
                        if (string.IsNullOrEmpty(keyword) == false)
                        {
                            if (itemName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) == -1
                                && localizationName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) == -1
                                && id.ToString().IndexOf(keyword, StringComparison.OrdinalIgnoreCase) == -1)
                            {
                                continue;
                            }
                        }

                        var itemBlock = new Models.ItemBlock()
                        {
                            Id = id,
                            ItemName = itemName,
                            IconColor = itemClass.GetIconTint().ToHex(),
                            MaxStackAllowed = itemClass.Stacknumber.Value,
                            IconName = itemClass.GetIconName(),
                            IsBlock = itemClass.IsBlock(),
                            LocalizationName = localizationName
                        };

                        result.Add(itemBlock);
                    }
                }
            }

            return result;
        }
    }
}
