using System;

namespace GTweak.Core.Services
{
    internal class FuzzySearchService
    {
        private const string CyrillicLayoutMap = "йцукенгшщзхъфывапролджэячсмитьбюёієїўґђјљњћџѕ";
        private const string QwertyLayoutMap = "qwertyuiop[]asdfghjkl;'zxcvbnm,.`s]['\\][;'/s";

        public bool IsMatch(string sourceText, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(sourceText) || string.IsNullOrWhiteSpace(searchQuery))
            {
                return false;
            }

            string normalizedQuery = searchQuery.Trim().ToLowerInvariant();

            if (CheckExactOrFuzzyMatch(sourceText, normalizedQuery))
            {
                return true;
            }

            string correctedQuery = MapCyrillicToQwerty(normalizedQuery);

            if (!ReferenceEquals(normalizedQuery, correctedQuery))
            {
                if (CheckExactOrFuzzyMatch(sourceText, correctedQuery))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckExactOrFuzzyMatch(string sourceText, string normalizedQuery)
        {
            if (sourceText.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return CheckWordsFuzzyMatch(sourceText, normalizedQuery);
        }

        private bool CheckWordsFuzzyMatch(string sourceText, string normalizedQuery)
        {
            int maxAllowedErrors = normalizedQuery.Length > 4 ? 2 : 1;
            int currentWordStartIndex = 0;
            int sourceLength = sourceText.Length;

            for (int i = 0; i <= sourceLength; i++)
            {
                if (i == sourceLength || char.IsWhiteSpace(sourceText[i]) || char.IsPunctuation(sourceText[i]))
                {
                    int wordLength = i - currentWordStartIndex;

                    if (wordLength > 0 && Math.Abs(wordLength - normalizedQuery.Length) <= maxAllowedErrors)
                    {
                        if (CalculateLevenshteinDistance(sourceText, currentWordStartIndex, wordLength, normalizedQuery) <= maxAllowedErrors)
                        {
                            return true;
                        }
                    }

                    currentWordStartIndex = i + 1;
                }
            }
            return false;
        }

        private int CalculateLevenshteinDistance(string sourceText, int wordStart, int wordLength, string normalizedQuery)
        {
            int queryLength = normalizedQuery.Length;

            if (queryLength > 128)
            {
                return int.MaxValue;
            }

            Span<int> previousRowCosts = stackalloc int[queryLength + 1];
            Span<int> currentRowCosts = stackalloc int[queryLength + 1];

            for (int i = 0; i <= queryLength; i++)
            {
                previousRowCosts[i] = i;
            }

            for (int sourceIndex = 0; sourceIndex < wordLength; sourceIndex++)
            {
                currentRowCosts[0] = sourceIndex + 1;

                char sourceChar = char.ToLowerInvariant(sourceText[wordStart + sourceIndex]);

                for (int queryIndex = 0; queryIndex < queryLength; queryIndex++)
                {
                    int substitutionCost = (sourceChar == normalizedQuery[queryIndex]) ? 0 : 1;

                    int deletionCost = previousRowCosts[queryIndex + 1] + 1;
                    int insertionCost = currentRowCosts[queryIndex] + 1;
                    int matchOrReplaceCost = previousRowCosts[queryIndex] + substitutionCost;

                    currentRowCosts[queryIndex + 1] = Math.Min(Math.Min(insertionCost, deletionCost), matchOrReplaceCost);
                }

                currentRowCosts.CopyTo(previousRowCosts);
            }

            return currentRowCosts[queryLength];
        }

        private string MapCyrillicToQwerty(string normalizedQuery)
        {
            int length = normalizedQuery.Length;

            Span<char> correctedBuffer = length <= 128 ? stackalloc char[length] : new char[length];

            bool isLayoutChanged = false;

            for (int i = 0; i < length; i++)
            {
                char originalChar = normalizedQuery[i];

                int cyrillicIndex = CyrillicLayoutMap.IndexOf(originalChar);

                if (cyrillicIndex >= 0 && cyrillicIndex < QwertyLayoutMap.Length)
                {
                    correctedBuffer[i] = QwertyLayoutMap[cyrillicIndex];
                    isLayoutChanged = true;
                }
                else
                {
                    correctedBuffer[i] = originalChar;
                }
            }

            return isLayoutChanged ? new string(correctedBuffer.ToArray()) : normalizedQuery;
        }
    }
}