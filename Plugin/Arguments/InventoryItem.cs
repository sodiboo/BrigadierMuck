using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using static Brigadier.StaticUtils;

namespace Brigadier.Arguments
{
    public class InventoryItemArgument : ArgumentType<InventoryItem>
    {
        public override InventoryItem Parse(StringScanner scanner)
        {
            scanner.ParseStack.Push(scanner.Cursor);
            if (scanner.CanRead() && scanner.Next == '#')
            {
                scanner.Skip();
                if (ItemManager.Instance.allItems.TryGetValue(scanner.MatchParse<int>(IntegerRegex), out var item))
                {
                    scanner.ParseStack.Pop();
                    return item;
                }
                else
                {
                    throw scanner.MakeException("Unknown item ID");
                }
            }
            InventoryItem result = null;
            int max = 0;
            foreach (var item in ItemManager.Instance.allItems.Values)
            {
                var name = ((Object)item).name;
                if (name.Length > max && scanner.CanRead(name.Length) && scanner.Read(name.Length) == name)
                {
                    result = item;
                    max = name.Length;
                }
                scanner.Cursor = scanner.ParseStack.Peek();
            }
            if (result != null)
            {
                scanner.ParseStack.Pop();
                scanner.Skip(max);
                return result;
            }
            throw scanner.MakeException("Unknown item");
        }

        public override Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder)
        {
            if (builder.Remaining.StartsWith("#"))
            {
                foreach (var item in ItemManager.Instance.allItems.Values)
                {
                    if ($"#{item.id}".StartsWith(builder.Remaining)) builder.Suggest(item.id, $"#{item.id}");
                }
                return builder.BuildTask();
            }
            if (builder.Remaining.IsNullOrWhiteSpace())
            {
                builder.Suggest("#");
            }
            foreach (var item in ItemManager.Instance.allItems.Values)
            {
                var name = ((Object)item).name;
                if (name.ToLower().StartsWith(builder.RemainingLowercase)) builder.Suggest(name);
            }
            return builder.BuildTask();
        }
    }
}