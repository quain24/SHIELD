using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ShieldTests.RawData
{
    public class Class1
    {
        [Fact()]
        public int Acalc()
        {
            var source = new List<string>() { "1 1", "1 2", "3 5", "4 5", "1 2", "1 3", "1 2" };

            if (IsOneOrMorePairsInvalid(source))
                throw new ArgumentOutOfRangeException(nameof(source));

            var gridMaxRow = source.Max(c => int.Parse(c[0].ToString()));
            var gridMaxCol = source.Max(c => int.Parse(c[2].ToString()));

            var grid = new Dictionary<int, List<int>>(gridMaxRow);

            for (var i = 0; i < gridMaxRow; i++)
            {
                grid.Add(i, new List<int>(gridMaxCol));
                for (var j = 0; j < gridMaxCol; j++)
                    grid[i].Add(0);
            }

            foreach (var entry in source)
            {
                var maxRow = int.Parse(entry[0].ToString());
                var maxCol = int.Parse(entry[2].ToString());
                for (var i = 0; i < maxRow; i++)
                {
                    for (var j = 0; j < maxCol; j++)
                    {
                        grid[i][j]++;
                    }
                }
            }

            // ReSharper disable once ComplexConditionExpression
            // Need only values for each row
            return grid.Select((kvp, _) => kvp.Value)
                // Count every 3 or more in every row
                .Select(list => list.Count(n => n >= 3))
                // Sum them up
                .Aggregate(0, (sum, next) => sum + next);
        }

        private static bool IsOneOrMorePairsInvalid(IEnumerable<string> source)
        {
            return source.Any(pair => pair.Length != 3 ||
                                      (!char.IsDigit(pair[0]) && int.Parse(pair.Substring(0, 1)) > 0) ||
                                      (!char.IsDigit(pair[2]) && int.Parse(pair.Substring(2, 1)) > 0));
        }

        [Fact()]
        public void MinimumSwaps()
        {
            var status = "SSSRSR";
            //111010

            status = status.ToLowerInvariant();
            var output = 0;

            // ReSharper disable once ComplexConditionExpression
            var inputNumber = status.Select(c =>
            {
                if (c == 'r') return 0;
                if (c == 's') return 1;
                throw new ArgumentOutOfRangeException(nameof(status));
            }).ToList();

            var inputNumberRight = new List<int>(inputNumber);

            var swapFromLeft = 0;
            var swapFromRight = 0;

            var prevLeft = inputNumber[0];
            var prevRigt = inputNumber.Last();

            for (int i = 1, j = inputNumber.Count - 2; i < inputNumber.Count - 1 || j == 0; i++, j--)
            {
                // left
                if (prevLeft == inputNumber[i])
                {
                    swapFromLeft++;
                    inputNumber[i] = inputNumber[i] == 0 ? 1 : 0;
                }
                prevLeft = inputNumber[i];

                // right
                if (prevRigt == inputNumberRight[j])
                {
                    swapFromRight++;
                    inputNumberRight[j] = inputNumberRight[j] == 0 ? 1 : 0;
                }
                prevRigt = inputNumberRight[j];
            }

            output = Math.Min(swapFromRight, swapFromLeft);

            Assert.True(output == 1);
        }

        [Fact()]
        public void Mathe()
        {
            var a = new List<int>() { 9, 5, 8 };
            var output = 0;

            var sum = 0;

            for (int i = 0; i < a.Count; i++)
            {
                var sublist = a.GetRange(0, i + 1);

                sublist.Sort();

                var tmpSum = 0;
                int indexer = 1;
                foreach (var number in sublist)
                {
                    tmpSum += indexer * number;
                    indexer++;
                }

                sum += tmpSum;
            }

            long modulo = (long)Math.Pow(10, 9) + 7;

            output = (int)(sum % modulo);

            Assert.True(output == 80);
        }

    }
}