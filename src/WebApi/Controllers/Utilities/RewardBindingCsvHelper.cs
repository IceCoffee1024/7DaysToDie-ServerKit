using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.WebApi.Controllers.RewardBinding
{
    /// <summary>
    /// 奖品绑定
    /// </summary>
    public static class RewardBindingCsvHelper
    {
        public const string ColumnName = "绑定内容";

        public const string LegacyItemColumn = "绑定物品";
        public const string LegacyCmdColumn = "绑定命令";

        public const string TypeItem = "物品";
        public const string TypeCommand = "命令";
        public const string TypePoints = "积分";

        public class RewardEntry
        {
            public int RewardType { get; set; }
            public string DisplayName { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public int Count { get; set; }
            public int Weight { get; set; }
            public int Quality { get; set; } = 1;
            public int Durability { get; set; } = 100;
        }


        public static string Serialize(IEnumerable<RewardEntry> rewards, bool includeWeight)
        {
            var parts = new List<string>();
            foreach (var r in rewards)
            {
                var typeStr = r.RewardType switch
                {
                    1 => TypeCommand,
                    2 => TypePoints,
                    _ => TypeItem,
                };

                var subjectEscaped = EscapeField(r.Subject ?? string.Empty);
                var displayEscaped = EscapeField(r.DisplayName ?? string.Empty);

                if (includeWeight)
                {
                    parts.Add($"{typeStr}|{displayEscaped}|{subjectEscaped}|{r.Count}|{r.Weight}");
                }
                else
                {
                    parts.Add($"{typeStr}|{displayEscaped}|{subjectEscaped}|{r.Count}");
                }
            }
            return string.Join("; ", parts);
        }


        public static List<RewardEntry> Parse(string? value)
        {
            var list = new List<RewardEntry>();
            if (string.IsNullOrWhiteSpace(value)) return list;

            foreach (var raw in value.Split(';'))
            {
                var entry = raw.Trim();
                if (entry.Length == 0) continue;

                var parts = entry.Split('|');
                if (parts.Length < 3) continue;

                var typeStr = parts[0].Trim();
                var displayName = parts.Length > 1 ? UnescapeField(parts[1].Trim()) : string.Empty;
                var subject = parts.Length > 2 ? UnescapeField(parts[2].Trim()) : string.Empty;
                int count = 0;
                int weight = 0;
                if (parts.Length > 3) int.TryParse(parts[3].Trim(), out count);
                if (parts.Length > 4) int.TryParse(parts[4].Trim(), out weight);

                int rewardType = typeStr switch
                {
                    TypeCommand => 1,
                    TypePoints => 2,
                    _ => 0,
                };

                list.Add(new RewardEntry
                {
                    RewardType = rewardType,
                    DisplayName = displayName,
                    Subject = subject,
                    Count = count,
                    Weight = weight,
                    Quality = 1,
                    Durability = 100,
                });
            }

            return list;
        }

        public static List<RewardEntry> ParseLegacy(string? itemsText, string? commandsText)
        {
            var list = new List<RewardEntry>();
            if (!string.IsNullOrWhiteSpace(itemsText))
            {
                foreach (var raw in itemsText.Split(','))
                {
                    var entry = raw.Trim();
                    if (entry.Length == 0) continue;

                    string itemName = entry;
                    int itemCount = 1;
                    var starIdx = entry.LastIndexOf('*');
                    if (starIdx > 0)
                    {
                        var afterStar = entry.Substring(starIdx + 1);
                        if (int.TryParse(afterStar, out var c))
                        {
                            itemName = entry.Substring(0, starIdx).Trim();
                            itemCount = c;
                        }
                    }

                    list.Add(new RewardEntry
                    {
                        RewardType = 0,
                        DisplayName = string.Empty,
                        Subject = itemName,
                        Count = itemCount,
                        Weight = 1,
                        Quality = 1,
                        Durability = 100,
                    });
                }
            }
            if (!string.IsNullOrWhiteSpace(commandsText))
            {
                foreach (var raw in commandsText.Split(','))
                {
                    var entry = raw.Trim();
                    if (entry.Length == 0) continue;
                    list.Add(new RewardEntry
                    {
                        RewardType = 1,
                        DisplayName = string.Empty,
                        Subject = entry,
                        Count = 1,
                        Weight = 1,
                    });
                }
            }
            return list;
        }

        private static string EscapeField(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            return raw.Replace("\\", "\\\\").Replace("|", "\\p").Replace(";", "\\s");
        }

        private static string UnescapeField(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            return raw.Replace("\\s", ";").Replace("\\p", "|").Replace("\\\\", "\\");
        }

        public class WrittenReward
        {
            public int RewardType { get; set; }
            public int Id { get; set; }     
            public int Weight { get; set; } 
        }

        public static async Task<List<WrittenReward>> WriteRewardsToListsAsync(
            List<RewardEntry> rewards,
            IItemListRepository itemRepo,
            ICommandListRepository cmdRepo)
        {
            var written = new List<WrittenReward>();

            var allItems = (await itemRepo.GetAllAsync()).ToList();
            var allCmds = (await cmdRepo.GetAllAsync()).ToList();

            var existingItemIds = new HashSet<int>(allItems.Select(x => x.Id));
            var existingCmdIds = new HashSet<int>(allCmds.Select(x => x.Id));

            foreach (var r in rewards)
            {
                if (r.RewardType == 1)
                {
                    var inMainThread = r.Count == 0;
                    var subject = r.Subject ?? string.Empty;
                    var dup = allCmds.FirstOrDefault(c =>
                        string.Equals(c.Command, subject, StringComparison.Ordinal) &&
                        c.InMainThread == inMainThread);
                    int cmdId;
                    if (dup != null)
                    {
                        cmdId = dup.Id;
                        if (!string.IsNullOrEmpty(r.DisplayName) && r.DisplayName != dup.DisplayName)
                        {
                            dup.DisplayName = r.DisplayName;
                            await cmdRepo.UpdateAsync(dup);
                        }
                    }
                    else
                    {
                        int nextId = 1;
                        while (existingCmdIds.Contains(nextId)) nextId++;
                        var entity = new T_CommandList
                        {
                            Id = nextId,
                            Command = subject,
                            DisplayName = string.IsNullOrEmpty(r.DisplayName) ? null : r.DisplayName,
                            InMainThread = inMainThread,
                            Description = "CSV导入自动创建",
                            CreatedAt = DateTime.Now,
                        };
                        await cmdRepo.InsertWithIdAsync(entity);
                        existingCmdIds.Add(nextId);
                        allCmds.Add(entity);
                        cmdId = nextId;
                    }
                    written.Add(new WrittenReward { RewardType = 1, Id = cmdId, Weight = Math.Max(1, r.Weight) });
                }
                else if (r.RewardType == 2)
                {
                    var dup = allItems.FirstOrDefault(i =>
                        i.RewardType == 2 &&
                        i.Count == r.Count &&
                        string.Equals(i.DisplayName ?? string.Empty, r.DisplayName ?? string.Empty, StringComparison.Ordinal));
                    int itemId;
                    if (dup != null)
                    {
                        itemId = dup.Id;
                    }
                    else
                    {
                        int nextId = 1;
                        while (existingItemIds.Contains(nextId)) nextId++;
                        var entity = new T_ItemList
                        {
                            Id = nextId,
                            ItemName = "points",
                            DisplayName = string.IsNullOrEmpty(r.DisplayName) ? null : r.DisplayName,
                            RewardType = 2,
                            Count = r.Count,
                            Quality = 0,
                            Durability = 0,
                            Description = "CSV导入自动创建",
                            CreatedAt = DateTime.Now,
                        };
                        await itemRepo.InsertWithIdAsync(entity);
                        existingItemIds.Add(nextId);
                        allItems.Add(entity);
                        itemId = nextId;
                    }
                    written.Add(new WrittenReward { RewardType = 2, Id = itemId, Weight = Math.Max(1, r.Weight) });
                }
                else
                {
                    var subject = r.Subject ?? string.Empty;
                    var dup = allItems.FirstOrDefault(i =>
                        (i.RewardType == 0) &&
                        string.Equals(i.ItemName, subject, StringComparison.OrdinalIgnoreCase) &&
                        i.Count == r.Count &&
                        i.Quality == r.Quality &&
                        i.Durability == r.Durability);
                    int itemId;
                    if (dup != null)
                    {
                        itemId = dup.Id;
                        if (!string.IsNullOrEmpty(r.DisplayName) && r.DisplayName != dup.DisplayName)
                        {
                            dup.DisplayName = r.DisplayName;
                            await itemRepo.UpdateAsync(dup);
                        }
                    }
                    else
                    {
                        int nextId = 1;
                        while (existingItemIds.Contains(nextId)) nextId++;
                        var entity = new T_ItemList
                        {
                            Id = nextId,
                            ItemName = subject,
                            DisplayName = string.IsNullOrEmpty(r.DisplayName) ? null : r.DisplayName,
                            RewardType = 0,
                            Count = r.Count <= 0 ? 1 : r.Count,
                            Quality = r.Quality,
                            Durability = r.Durability,
                            Description = "CSV导入自动创建",
                            CreatedAt = DateTime.Now,
                        };
                        await itemRepo.InsertWithIdAsync(entity);
                        existingItemIds.Add(nextId);
                        allItems.Add(entity);
                        itemId = nextId;
                    }
                    written.Add(new WrittenReward { RewardType = 0, Id = itemId, Weight = Math.Max(1, r.Weight) });
                }
            }

            return written;
        }


        public static async Task<List<RewardEntry>> ReadFromBindingsAsync(
            IEnumerable<T_ItemList> items,
            IEnumerable<T_CommandList> commands)
        {
            return await Task.FromResult(BuildEntries(items, commands, null, null));
        }

        public static List<RewardEntry> BuildEntries(
            IEnumerable<T_ItemList> items,
            IEnumerable<T_CommandList> commands,
            IDictionary<int, int>? itemWeights,
            IDictionary<int, int>? commandWeights)
        {
            var list = new List<RewardEntry>();
            foreach (var i in items)
            {
                int weight = 1;
                if (itemWeights != null && itemWeights.TryGetValue(i.Id, out var w)) weight = w;

                list.Add(new RewardEntry
                {
                    RewardType = i.RewardType == 2 ? 2 : 0,
                    DisplayName = i.DisplayName ?? string.Empty,
                    Subject = i.RewardType == 2 ? "points" : i.ItemName,
                    Count = i.Count,
                    Weight = weight,
                    Quality = i.Quality,
                    Durability = i.Durability,
                });
            }
            foreach (var c in commands)
            {
                int weight = 1;
                if (commandWeights != null && commandWeights.TryGetValue(c.Id, out var w)) weight = w;

                list.Add(new RewardEntry
                {
                    RewardType = 1,
                    DisplayName = c.DisplayName ?? string.Empty,
                    Subject = c.Command ?? string.Empty,
                    Count = c.InMainThread ? 0 : 1,
                    Weight = weight,
                });
            }
            return list;
        }
    }
}
