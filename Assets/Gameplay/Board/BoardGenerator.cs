using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Gameplay.Board
{
    public sealed class BoardGenerator
    {
        private const int MaxGenerateAttempts = 4096;
        
        private static readonly int[] FallbackBoard =
        {
            9, 5, 6, 3, 7, 6, 9, 5, 9,
            2, 1, 5, 6, 8, 5, 2, 3, 6,
            2, 3, 3, 1, 1, 7, 6, 1, 6,
        };

        private static readonly (int FirstNumber, int SecondNumber)[] PairNumberOptions =
        {
            (1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8), (9, 9),
            (1, 9), (9, 1),
            (2, 8), (8, 2),
            (3, 7), (7, 3),
            (4, 6), (6, 4),
        };

        private readonly System.Random _random;
        private readonly BoardMatchRules _matchRules;

        public BoardGenerator(int seed)
        {
            _random = new System.Random(seed);
            _matchRules = new BoardMatchRules();
        }

        public List<BoardCell> Generate(int columns, int rows, int targetPairCount)
        {
            int safeColumns = Mathf.Max(1, columns);
            int safeRows = Mathf.Max(1, rows);
            int cellCount = safeColumns * safeRows;
            int safeTargetPairCount = Mathf.Clamp(targetPairCount, 0, cellCount / 2);

            for (int attempt = 0; attempt < MaxGenerateAttempts; attempt++)
            {
                if (!TryGenerateNumbers(safeColumns, safeRows, safeTargetPairCount, out var numbers))
                {
                    continue;
                }

                return BuildCells(numbers, safeColumns);
            }

            // if we don't generate board in MaxGenerateAttempts we return predefined FallBackBoard
            return BuildCells(FallbackBoard, safeColumns);
        }

        private bool TryGenerateNumbers(int columns, int rows, int targetPairCount, out int[] numbers)
        {
            int cellCount = columns * rows;
            numbers = new int[cellCount];

            if (!TryPickPlannedPairs(cellCount, columns, targetPairCount, out var plannedPairs))
            {
                return false;
            }

            // We place the intended pair cells first and then fill every other cell
            // with values that do not create extra accidental pairs.
            if (!TryPlacePlannedPairs(numbers, columns, plannedPairs))
            {
                return false;
            }

            if (!TryFillSafeNumbers(numbers, columns))
            {
                return false;
            }

            return CountPairs(numbers, columns) == targetPairCount;
        }

        private bool TryPickPlannedPairs(
            int cellCount,
            int columns,
            int targetPairCount,
            out List<(int FirstIndex, int SecondIndex)> plannedPairs)
        {
            plannedPairs = new List<(int FirstIndex, int SecondIndex)>();

            if (targetPairCount == 0)
            {
                return true;
            }

            var freeIndices = new HashSet<int>(Enumerable.Range(0, cellCount));
            while (plannedPairs.Count < targetPairCount)
            {
                int anchorIndex = GetMostConstrainedPairIndex(freeIndices, columns, cellCount);
                if (anchorIndex < 0)
                {
                    return false;
                }

                List<int> candidateNeighbours = GetAvailablePairNeighbours(anchorIndex, freeIndices, columns, cellCount);
                if (candidateNeighbours.Count == 0)
                {
                    return false;
                }

                int neighbourIndex = candidateNeighbours[_random.Next(candidateNeighbours.Count)];
                plannedPairs.Add(OrderPair(anchorIndex, neighbourIndex));
                freeIndices.Remove(anchorIndex);
                freeIndices.Remove(neighbourIndex);
            }

            return true;
        }

        private bool TryPlacePlannedPairs(
            int[] numbers,
            int columns,
            IReadOnlyList<(int FirstIndex, int SecondIndex)> plannedPairs)
        {
            foreach (var plannedPair in plannedPairs)
            {
                var candidatePairs = PairNumberOptions.ToList();
                Shuffle(candidatePairs);

                bool placed = false;
                foreach (var candidatePair in candidatePairs)
                {
                    if (!CanPlaceNumber(numbers, plannedPair.FirstIndex, candidatePair.FirstNumber, plannedPair.SecondIndex, columns))
                    {
                        continue;
                    }

                    if (!CanPlaceNumber(numbers, plannedPair.SecondIndex, candidatePair.SecondNumber, plannedPair.FirstIndex, columns))
                    {
                        continue;
                    }

                    numbers[plannedPair.FirstIndex] = candidatePair.FirstNumber;
                    numbers[plannedPair.SecondIndex] = candidatePair.SecondNumber;
                    placed = true;
                    break;
                }

                if (!placed)
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryFillSafeNumbers(int[] numbers, int columns)
        {
            var remainingIndices = Enumerable.Range(0, numbers.Length)
                .Where(index => numbers[index] == 0)
                .ToList();

            return TryFillSafeNumbers(numbers, columns, remainingIndices);
        }

        private bool TryFillSafeNumbers(int[] numbers, int columns, List<int> remainingIndices)
        {
            if (remainingIndices.Count == 0)
            {
                return true;
            }

            int index = GetMostConstrainedIndex(remainingIndices, numbers, columns);
            remainingIndices.Remove(index);

            var candidates = GetSafeNumbers(index, numbers, columns);
            foreach (int candidate in candidates)
            {
                numbers[index] = candidate;

                if (TryFillSafeNumbers(numbers, columns, remainingIndices))
                {
                    return true;
                }
            }

            numbers[index] = 0;
            remainingIndices.Add(index);
            return false;
        }

        private bool CanPlaceNumber(
            IReadOnlyList<int> numbers,
            int index,
            int value,
            int allowedNeighbourIndex,
            int columns)
        {
            IReadOnlyList<int> neighbourIndices = GetOpeningNeighbourIndices(index, columns, numbers.Count);
            foreach (int neighbourIndex in neighbourIndices)
            {
                if (neighbourIndex == allowedNeighbourIndex)
                {
                    continue;
                }

                int neighbourValue = numbers[neighbourIndex];
                if (neighbourValue == 0)
                {
                    continue;
                }

                if (_matchRules.IsValueMatch(value, neighbourValue))
                {
                    return false;
                }
            }

            return true;
        }

        private int CountPairs(IReadOnlyList<int> numbers, int columns)
        {
            int pairCount = 0;

            for (int index = 0; index < numbers.Count; index++)
            {
                IReadOnlyList<int> neighbourIndices = GetOpeningNeighbourIndices(index, columns, numbers.Count);
                foreach (int neighbourIndex in neighbourIndices)
                {
                    if (neighbourIndex <= index)
                    {
                        continue;
                    }

                    if (_matchRules.IsValueMatch(numbers[index], numbers[neighbourIndex]))
                    {
                        pairCount++;
                    }
                }
            }

            return pairCount;
        }

        private List<int> GetSafeNumbers(int index, IReadOnlyList<int> numbers, int columns)
        {
            var candidates = Enumerable.Range(1, 9)
                .Where(candidate => CanPlaceNumber(numbers, index, candidate, -1, columns))
                .ToList();
            Shuffle(candidates);
            return candidates;
        }

        private int GetMostConstrainedIndex(IReadOnlyList<int> remainingIndices, IReadOnlyList<int> numbers, int columns)
        {
            int bestIndex = remainingIndices[0];
            int bestCandidateCount = int.MaxValue;
            int bestFilledNeighbours = -1;

            foreach (int index in remainingIndices)
            {
                int candidateCount = GetSafeNumbers(index, numbers, columns).Count;
                int filledNeighbours = 0;
                IReadOnlyList<int> neighbourIndices = GetOpeningNeighbourIndices(index, columns, numbers.Count);
                foreach (int neighbourIndex in neighbourIndices)
                {
                    if (numbers[neighbourIndex] != 0)
                    {
                        filledNeighbours++;
                    }
                }

                if (candidateCount < bestCandidateCount)
                {
                    bestCandidateCount = candidateCount;
                    bestFilledNeighbours = filledNeighbours;
                    bestIndex = index;
                    continue;
                }

                if (candidateCount == bestCandidateCount && filledNeighbours > bestFilledNeighbours)
                {
                    bestFilledNeighbours = filledNeighbours;
                    bestIndex = index;
                    continue;
                }

                if (candidateCount == bestCandidateCount &&
                    filledNeighbours == bestFilledNeighbours &&
                    _random.Next(2) == 0)
                {
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        private int GetMostConstrainedPairIndex(HashSet<int> freeIndices, int columns, int count)
        {
            var candidateIndices = freeIndices.ToList();
            Shuffle(candidateIndices);

            int bestIndex = -1;
            int smallestNeighbourCount = int.MaxValue;

            foreach (int index in candidateIndices)
            {
                int neighbourCount = GetAvailablePairNeighbours(index, freeIndices, columns, count).Count;
                if (neighbourCount == 0)
                {
                    continue;
                }

                if (neighbourCount < smallestNeighbourCount)
                {
                    smallestNeighbourCount = neighbourCount;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        private List<int> GetAvailablePairNeighbours(
            int index,
            IReadOnlyCollection<int> freeIndices,
            int columns,
            int count)
        {
            return GetOpeningNeighbourIndices(index, columns, count)
                .Where(freeIndices.Contains)
                .ToList();
        }

        private List<BoardCell> BuildCells(IReadOnlyList<int> numbers, int columns)
        {
            var cells = new List<BoardCell>(numbers.Count);

            for (int index = 0; index < numbers.Count; index++)
            {
                int row = index / columns;
                int column = index % columns;
                cells.Add(new BoardCell(index, row, column, numbers[index]));
            }

            return cells;
        }

        private IReadOnlyList<int> GetOpeningNeighbourIndices(int index, int columns, int count)
        {
            var neighbours = new List<int>();

            int row = index / columns;
            int column = index % columns;

            TryAddOpeningNeighbour(neighbours, row + 1, column, columns, count);
            TryAddOpeningNeighbour(neighbours, row - 1, column, columns, count);
            TryAddOpeningNeighbour(neighbours, row + 1, column - 1, columns, count);
            TryAddOpeningNeighbour(neighbours, row + 1, column + 1, columns, count);
            TryAddOpeningNeighbour(neighbours, row - 1, column - 1, columns, count);
            TryAddOpeningNeighbour(neighbours, row - 1, column + 1, columns, count);

            TryAddFlatNeighbour(neighbours, index - 1, count);
            TryAddFlatNeighbour(neighbours, index + 1, count);

            return neighbours;
        }

        private void TryAddOpeningNeighbour(List<int> neighbours, int row, int column, int columns, int count)
        {
            if (row < 0 || column < 0 || column >= columns)
            {
                return;
            }

            int index = column + (row * columns);
            TryAddFlatNeighbour(neighbours, index, count);
        }

        private void TryAddFlatNeighbour(List<int> neighbours, int index, int count)
        {
            if (index < 0 || index >= count || neighbours.Contains(index))
            {
                return;
            }

            neighbours.Add(index);
        }

        private void Shuffle<T>(IList<T> values)
        {
            for (int index = values.Count - 1; index > 0; index--)
            {
                int swapIndex = _random.Next(index + 1);
                (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
            }
        }

        private (int FirstIndex, int SecondIndex) OrderPair(int firstIndex, int secondIndex)
        {
            return firstIndex < secondIndex
                ? (firstIndex, secondIndex)
                : (secondIndex, firstIndex);
        }
    }
}
