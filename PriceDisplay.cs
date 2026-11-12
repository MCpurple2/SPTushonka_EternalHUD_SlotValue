using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EternalHudSlotValue
{
    internal sealed class PriceDisplay
    {
        // EternalHUD passes the same rich-text price row on successive redraws.
        private static readonly Regex Price = new Regex(
            @"^(?<labelColor><color=[^>]+>)(?<label>参考价格|Price indication): </color>(?<valueColor><color=[^>]+>)(?<value>[0-9]+)</color>(?<newline>\r?\n)$",
            RegexOptions.CultureInvariant);

        private string lastLine;
        private Match match;
        private decimal price;
        private long lastCells;
        private decimal? lastTotal;
        private int lastMissing;
        private string lastText;
        private string lastResult;
        private string addition;

        internal string Insert(string text, string line, int start, int width, int height, decimal? total, int missing)
        {
            if (width <= 0 || height <= 0)
                return text;

            if (!string.Equals(line, lastLine, StringComparison.Ordinal))
            {
                lastLine = line;
                match = Price.Match(line);
                decimal.TryParse(match.Groups["value"].Value, NumberStyles.None,
                    CultureInfo.InvariantCulture, out price);
                lastCells = 0;
            }
            if (price <= 0)
                return text;

            long cells = (long)width * height;
            if (cells != lastCells || total != lastTotal || missing != lastMissing)
            {
                lastCells = cells;
                lastTotal = total;
                lastMissing = missing;
                addition = FormatRow("单格价值: ", price / cells);
                if (total.HasValue)
                {
                    addition += FormatRow("总价值: ", total.Value);
                    if (missing > 0)
                        addition += "部分物品未计价" + match.Groups["newline"].Value;
                }
                lastText = null;
            }
            if (!string.Equals(text, lastText, StringComparison.Ordinal))
            {
                lastText = text;
                lastResult = text.Insert(start + line.Length, addition);
            }
            return lastResult;
        }

        private string FormatRow(string label, decimal amount)
        {
            string value = Math.Round(amount, 0, MidpointRounding.AwayFromZero)
                .ToString("0", CultureInfo.InvariantCulture);
            return match.Groups["labelColor"].Value + label + "</color>" +
                match.Groups["valueColor"].Value + value + "</color>" + match.Groups["newline"].Value;
        }
    }
}


