using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace CombineMachines.Helpers
{
    public static class Extensions
    {
        /// <summary>Returns true if the output stack has already been modified during the current processing cycle, and should not be changed again until the next processing cycle</summary>
        public static bool HasModifiedOutput(this SObject Item)
        {
            if (Item.modData != null && Item.modData.TryGetValue(ModEntry.ModDataOutputModifiedKey, out string HasModifiedString))
            {
                bool Result = bool.Parse(HasModifiedString);
                return Result;
            }
            else
                return false;
        }

        public static void SetHasModifiedOutput(this SObject Item, bool Value)
        {
            Item.modData[ModEntry.ModDataOutputModifiedKey] = Value.ToString();
        }

        public static bool IsCombinedMachine(this SObject Item)
        {
            return Item.modData != null && Item.modData.TryGetValue(ModEntry.ModDataQuantityKey, out string QuantityString);
        }

        public static bool TryGetCombinedQuantity(this SObject Item, out int Quantity)
        {
            Quantity = 0;

            if (Item.modData != null && Item.modData.TryGetValue(ModEntry.ModDataQuantityKey, out string QuantityString))
            {
                Quantity = int.Parse(QuantityString);
                return true;
            }
            else
                return false;
        }

        public static void SetCombinedQuantity(this SObject Item, int Quantity)
        {
            if (!IsMachine(Item))
                throw new InvalidOperationException("Only machines can be combined in mod: ." + nameof(CombineMachines));

            if (!TryGetCombinedQuantity(Item, out int PreviousValue))
                PreviousValue = 0;

            Item.modData[ModEntry.ModDataQuantityKey] = Quantity.ToString();
            int PreviousStack = Item.Stack;
            Item.Stack = 1;
            ModEntry.Logger.Log(string.Format("Set combined quantity on {0} (Stack={1}) from {2} to {3}", Item.DisplayName, PreviousStack, PreviousValue, Quantity), ModEntry.InfoLogLevel);
        }

        /// <summary>The item Ids of machines that are just regular objects, rather than BigCraftable item types.</summary>
        public static readonly List<int> NonBigCraftableMachineIds = new List<int>() {
            //710 // Crab Pot (currently disabled since I'm not sure how Crab Pots work. Doesn't seem like they're using the MinutesUntilReady field that other machines utilize)
        };

        public static bool IsMachine(this SObject Item)
        {
            return Item.bigCraftable.Value || NonBigCraftableMachineIds.Contains(Item.ParentSheetIndex);
        }

        //Taken from: https://stackoverflow.com/questions/521146/c-sharp-split-string-but-keep-split-chars-separators
        public static IEnumerable<string> SplitAndKeepDelimiter(this string s, char[] delims)
        {
            int start = 0, index;

            while ((index = s.IndexOfAny(delims, start)) != -1)
            {
                if (index - start > 0)
                    yield return s.Substring(start, index - start);
                yield return s.Substring(index, 1);
                start = index + 1;
            }

            if (start < s.Length)
            {
                yield return s.Substring(start);
            }
        }

        public static Rectangle GetOffseted(this Rectangle value, Point Offset)
        {
            return new Rectangle(value.X + Offset.X, value.Y + Offset.Y, value.Width, value.Height);
        }

        public static Point AsPoint(this Vector2 value)
        {
            return new Point((int)value.X, (int)value.Y);
        }
    }
}
