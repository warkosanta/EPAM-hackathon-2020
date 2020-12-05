using System;

namespace TetrisClient.BotSolution
{
    public static class TetrisAI
    {
        public static double GetMoveScore(Board board, double a,
            double b, double c, double d)
        {
            var grid = Transpose(board.GetField());
            return a * CalculateAggregateHeight(grid) +
                   b * CalculateCompleteLines(grid) +
                   c * CalculateHoles(grid) +
                   d * CalculateBumpiness(grid);
        }

        private static int CalculateBumpiness(char[,] grid)
        {
            var columnsHeight = new int[grid.GetLength(1)];

            for (int i = 0; i < grid.GetLength(1); i++)
            {
                for (int j = 0; j < grid.GetLength(0); j++)
                {
                    if (grid[j, i] != '.')
                    {
                        columnsHeight[i] = grid.GetLength(0) - j;
                        break;
                    }
                }
            }

            var absoluteBumpiness = CountAbsoluteHeightDifferences(columnsHeight);
            return absoluteBumpiness;
        }
        private static int CalculateAggregateHeight(char[,] grid)
        {
            var aggregateHeight = 0;
            for (int i = 0; i < grid.GetLength(1); i++)
            {
                var columnHeight = 0;
                for (int j = 0; j < grid.GetLength(0); j++)
                {
                    if (grid[j, i] != '.')
                    {
                        columnHeight = grid.GetLength(0) - j;
                        break;
                    }
                }

                aggregateHeight += columnHeight;
            }

            return aggregateHeight;
        }
        private static int CalculateCompleteLines(char[,] grid)
        {
            var fullLineCounter = 0;
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                var isLineFull = true;
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] == '.')
                    {
                        isLineFull = false;
                        break;

                    }
                }

                fullLineCounter += isLineFull ? 1 : 0;
            }

            return fullLineCounter;
        }
        private static int CalculateHoles(char[,] grid)
        {
            var holesCounter = 0;
            for (int i = grid.GetLength(0) - 1; i > 0; i--)
            {
                for (int j = grid.GetLength(1) - 1; j >= 0; j--)
                {
                    if (grid[i, j] == '.' && grid[i - 1, j] != '.')
                    {
                        holesCounter++;
                    }
                }
            }

            return holesCounter;
        }
        private static int CountAbsoluteHeightDifferences(int[] heights)
        {
            int counter = 0;

            for (int i = 0; i < heights.Length - 1; i++)
            {
                counter += Math.Abs(heights[i] - heights[i + 1]);
            }

            return counter;
        }
        private static char[,] Transpose(char[,] matrix)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);

            char[,] result = new char[h, w];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    result[17 - j, 17 - i] = matrix[i, j];
                }
            }

            return result;
        }
    }
}