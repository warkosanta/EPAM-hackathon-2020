using System.Collections.Generic;
using System.Linq;

namespace TetrisClient.BotSolution
{
    public interface IFigureAdjuster
    {
        Element Element { get; }
        BestMoveDto GetBestMove(Board board);
    }
    public abstract class Figure : IFigureAdjuster
    {
        private const double a = -0.510066;
        private const double b = 0.760666;
        private const double c = -0.35663;
        private const double d = -0.18448;

        public abstract Element Element { get; }

        /// <summary>
        /// Get best move.
        /// </summary>
        /// <param name="board">Game board.</param>
        /// <returns>Best move coordinates.</returns>
        public BestMoveDto GetBestMove(Board board)
        {
            // clear board from current element
            ClearBoardFromCurrentFigure(board);

            // calculate best move
            return CalculateBestPosition(board);
        }
        private protected int FindColumnLandingPositionY(Board clearedBoard, int column)
        {
            int landingY = 0;
            for (int j = clearedBoard.Size - 1; j >= 0; j--)
            {
                if (!clearedBoard.IsFree(column, j))
                {
                    landingY = j + 1;
                    break;
                }
            }

            return landingY;
        }

        private protected List<BestMoveDto> GetPossibleMovements(Board board,
            Point[] predict, Point landingPoint, Rotation rotation)
        {
            List<BestMoveDto> snapshots = new List<BestMoveDto>();
            if (predict.Count(p => board.IsOutOfField(p.X, p.Y)) == 0
                && predict.Count(p => board.IsFree(p.X, p.Y)) == predict.Length
                && IsPathClear(board, predict))
            {
                foreach (var p in predict)
                {
                    if (!board.IsOutOfField(p.X, p.Y))
                        board.Set(p.X, p.Y, (char) Element.PURPLE);
                }

                var score = TetrisAI.GetMoveScore(board, a, b, c, d);
                score += board.Size - landingPoint.Y;

                foreach (var p in predict)
                {
                    if (!board.IsOutOfField(p.X, p.Y))
                        board.Set(p.X, p.Y, '.');
                }

                snapshots.Add(new BestMoveDto(score, landingPoint, rotation));

            }

            return snapshots;
        }

        private protected bool IsPathClear(Board board, Point[] futurePoints)
        {
            var xs = futurePoints.Select(p => p.X);
            var miny = futurePoints.Min(p => p.Y);
            for (int i = board.Size - 1; i > miny; i--)
            {
                foreach (var x in xs)
                {
                    if (!board.IsFree(x, i) && !futurePoints.Any(p=>p.X == i && p.Y == i)) return false;
                }
            }

            return true;
        }

        private protected abstract void ClearBoardFromCurrentFigure(Board board);

        private protected abstract BestMoveDto CalculateBestPosition(Board board);
    }

    public class OFigure : Figure
    {
        public override Element Element => Element.YELLOW;

        private protected override void ClearBoardFromCurrentFigure(Board board)
        {
            var currFigure = board.GetCurrentFigurePoint();

            board.Set(currFigure.X, currFigure.Y, '.');
            board.Set(currFigure.X + 1, currFigure.Y, '.');
            board.Set(currFigure.X, currFigure.Y - 1, '.');
            board.Set(currFigure.X + 1, currFigure.Y - 1, '.');
        }

        private protected override BestMoveDto CalculateBestPosition(Board board)
        {
            List<BestMoveDto> snapshots = new List<BestMoveDto>();

            for (int i = 0; i < board.Size; i++)
            {
                var landingY = FindColumnLandingPositionY(board, i);

                for (int j = 0; j < 4; j++)
                {
                    Rotation rotation;
                    Point[] predict;
                    Point newLandingPoint;
                    switch (j)
                    {
                        case 0:
                        {
                            rotation = Rotation.CLOCKWIZE_0;
                            newLandingPoint = new Point(i, landingY + 1);
                            break;
                        }
                        case 1:
                        {
                            rotation = Rotation.CLOCKWIZE_90;
                            newLandingPoint = new Point(i, landingY + 1);
                            break;
                        }
                        case 2:
                        {
                            rotation = Rotation.CLOCKWIZE_180;
                            newLandingPoint = new Point(i, landingY);
                            break;
                        }
                        default:
                        {
                            rotation = Rotation.CLOCKWIZE_270;
                            newLandingPoint = new Point(i, landingY);
                            break;
                        }

                    }

                    predict = FigureRotator.PredictCurrentFigurePoints(
                        rotation,
                        newLandingPoint,
                        board.GetCurrentFigureType());

                    snapshots.AddRange(
                        base.GetPossibleMovements(
                            board, predict, newLandingPoint, rotation));

                }
            }

            return snapshots.OrderByDescending(i => i.Score).First();
        }
    }

    public class IFigure : Figure
    {
        public override Element Element => Element.BLUE;

        private protected override BestMoveDto CalculateBestPosition(Board board)
        {
            List<BestMoveDto> snapshots = new List<BestMoveDto>();

            for (int i = 0; i < board.Size; i++)
            {
                var landingY = FindColumnLandingPositionY(board, i);

                for (int j = 0; j < 4; j++)
                {
                    Rotation rotation;
                    Point[] predict;
                    Point newLandingPoint;
                    switch (j)
                    {
                        case 0:
                            {
                                rotation = Rotation.CLOCKWIZE_0;
                                newLandingPoint = new Point(i, landingY + 2);
                                break;
                            }
                        case 1:
                            {
                                rotation = Rotation.CLOCKWIZE_90;
                                newLandingPoint = new Point(i, landingY);
                                break;
                            }
                        case 2:
                            {
                                rotation = Rotation.CLOCKWIZE_180;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        default:
                            {
                                rotation = Rotation.CLOCKWIZE_270;
                                newLandingPoint = new Point(i, landingY);
                                break;
                            }

                    }

                    predict = FigureRotator.PredictCurrentFigurePoints(
                        rotation,
                        newLandingPoint,
                        board.GetCurrentFigureType());

                    snapshots.AddRange(
                        base.GetPossibleMovements(
                            board, predict, newLandingPoint, rotation));

                }
            }

            return snapshots.OrderByDescending(i => i.Score).First();
        }

        private protected override void ClearBoardFromCurrentFigure(Board board)
        {
            var currFigure = board.GetCurrentFigurePoint();

            board.Set(currFigure.X, currFigure.Y - 1, '.');
            board.Set(currFigure.X, currFigure.Y - 2, '.');
        }
    }

    public class JFigure : Figure
    {
        public override Element Element => Element.CYAN;

        private protected override BestMoveDto CalculateBestPosition(Board board)
        {
            List<BestMoveDto> snapshots = new List<BestMoveDto>();

            for (int i = 0; i < board.Size; i++)
            {
                var landingY = FindColumnLandingPositionY(board, i);

                for (int j = 0; j < 4; j++)
                {
                    Rotation rotation;
                    Point[] predict;
                    Point newLandingPoint;
                    switch (j)
                    {
                        case 0:
                            {
                                rotation = Rotation.CLOCKWIZE_0;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        case 1:
                            {
                                rotation = Rotation.CLOCKWIZE_90;
                                newLandingPoint = new Point(i, landingY);
                                break;
                            }
                        case 2:
                            {
                                rotation = Rotation.CLOCKWIZE_180;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        default:
                            {
                                rotation = Rotation.CLOCKWIZE_270;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }

                    }

                    predict = FigureRotator.PredictCurrentFigurePoints(
                        rotation,
                        newLandingPoint,
                        board.GetCurrentFigureType());

                    snapshots.AddRange(
                        base.GetPossibleMovements(
                            board, predict, newLandingPoint, rotation));

                }
            }

            return snapshots.OrderByDescending(i => i.Score).First();
        }

        private protected override void ClearBoardFromCurrentFigure(Board board)
        {
            var currFigure = board.GetCurrentFigurePoint();

            board.Set(currFigure.X, currFigure.Y, '.');
            board.Set(currFigure.X, currFigure.Y - 1, '.');
            board.Set(currFigure.X - 1, currFigure.Y - 1, '.');
        }
    }

    public class LFigure : Figure
    {
        public override Element Element => Element.ORANGE;

        private protected override BestMoveDto CalculateBestPosition(Board board)
        {
            List<BestMoveDto> snapshots = new List<BestMoveDto>();

            for (int i = 0; i < board.Size; i++)
            {
                var landingY = FindColumnLandingPositionY(board, i);

                for (int j = 0; j < 4; j++)
                {
                    Rotation rotation;
                    Point[] predict;
                    Point newLandingPoint;
                    switch (j)
                    {
                        case 0:
                            {
                                rotation = Rotation.CLOCKWIZE_0;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        case 1:
                            {
                                rotation = Rotation.CLOCKWIZE_90;
                                newLandingPoint = new Point(i, landingY);
                                break;
                            }
                        case 2:
                            {
                                rotation = Rotation.CLOCKWIZE_180;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        default:
                            {
                                rotation = Rotation.CLOCKWIZE_270;
                                newLandingPoint = new Point(i, landingY);
                                break;
                            }

                    }

                    predict = FigureRotator.PredictCurrentFigurePoints(
                        rotation,
                        newLandingPoint,
                        board.GetCurrentFigureType());

                    snapshots.AddRange(
                        base.GetPossibleMovements(
                            board, predict, newLandingPoint, rotation));

                }
            }

            return snapshots.OrderByDescending(i => i.Score).First();
        }

        private protected override void ClearBoardFromCurrentFigure(Board board)
        {
            var currFigure = board.GetCurrentFigurePoint();

            board.Set(currFigure.X, currFigure.Y, '.');
            board.Set(currFigure.X + 1, currFigure.Y - 1, '.');
            board.Set(currFigure.X, currFigure.Y - 1, '.');
        }
    }

    public class SFigure : Figure
    {
        public override Element Element => Element.GREEN;

        private protected override BestMoveDto CalculateBestPosition(Board board)
        {
            List<BestMoveDto> snapshots = new List<BestMoveDto>();

            for (int i = 0; i < board.Size; i++)
            {
                var landingY = FindColumnLandingPositionY(board, i);

                for (int j = 0; j < 4; j++)
                {
                    Rotation rotation;
                    Point[] predict;
                    Point newLandingPoint;
                    switch (j)
                    {
                        case 0:
                            {
                                rotation = Rotation.CLOCKWIZE_0;
                                newLandingPoint = new Point(i, landingY);
                                break;
                            }
                        case 1:
                            {
                                rotation = Rotation.CLOCKWIZE_90;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        case 2:
                            {
                                rotation = Rotation.CLOCKWIZE_180;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        default:
                            {
                                rotation = Rotation.CLOCKWIZE_270;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }

                    }

                    predict = FigureRotator.PredictCurrentFigurePoints(
                        rotation,
                        newLandingPoint,
                        board.GetCurrentFigureType());

                    snapshots.AddRange(
                        base.GetPossibleMovements(
                            board, predict, newLandingPoint, rotation));

                }
            }

            return snapshots.OrderByDescending(i => i.Score).First();
        }

        private protected override void ClearBoardFromCurrentFigure(Board board)
        {
            var currFigure = board.GetCurrentFigurePoint();

            board.Set(currFigure.X + 1, currFigure.Y + 1, '.');
            board.Set(currFigure.X - 1, currFigure.Y, '.');
            board.Set(currFigure.X, currFigure.Y + 1, '.');
            board.Set(currFigure.X, currFigure.Y, '.');
        }
    }

    public class ZFigure : Figure
    {
        public override Element Element => Element.RED;

        private protected override BestMoveDto CalculateBestPosition(Board board)
        {
            List<BestMoveDto> snapshots = new List<BestMoveDto>();

            for (int i = 0; i < board.Size; i++)
            {
                var landingY = FindColumnLandingPositionY(board, i);

                for (int j = 0; j < 4; j++)
                {
                    Rotation rotation;
                    Point[] predict;
                    Point newLandingPoint;
                    switch (j)
                    {
                        case 0:
                            {
                                rotation = Rotation.CLOCKWIZE_0;
                                newLandingPoint = new Point(i, landingY);
                                break;
                            }
                        case 1:
                            {
                                rotation = Rotation.CLOCKWIZE_90;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        case 2:
                            {
                                rotation = Rotation.CLOCKWIZE_180;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        default:
                            {
                                rotation = Rotation.CLOCKWIZE_270;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }

                    }

                    predict = FigureRotator.PredictCurrentFigurePoints(
                        rotation,
                        newLandingPoint,
                        board.GetCurrentFigureType());

                    snapshots.AddRange(
                        base.GetPossibleMovements(
                            board, predict, newLandingPoint, rotation));

                }
            }

            return snapshots.OrderByDescending(i => i.Score).First();
        }

        private protected override void ClearBoardFromCurrentFigure(Board board)
        {
            var currFigure = board.GetCurrentFigurePoint();

            board.Set(currFigure.X - 1, currFigure.Y + 1, '.');
            board.Set(currFigure.X, currFigure.Y + 1, '.');
            board.Set(currFigure.X, currFigure.Y, '.');
            board.Set(currFigure.X + 1, currFigure.Y, '.');
        }
    }

    public class TFigure : Figure
    {
        public override Element Element => Element.PURPLE;

        private protected override BestMoveDto CalculateBestPosition(Board board)
        {
            List<BestMoveDto> snapshots = new List<BestMoveDto>();

            for (int i = 0; i < board.Size; i++)
            {
                var landingY = FindColumnLandingPositionY(board, i);

                for (int j = 0; j < 4; j++)
                {
                    Rotation rotation;
                    Point[] predict;
                    Point newLandingPoint;
                    switch (j)
                    {
                        case 0:
                            {
                                rotation = Rotation.CLOCKWIZE_0;
                                newLandingPoint = new Point(i, landingY);
                                break;
                            }
                        case 1:
                            {
                                rotation = Rotation.CLOCKWIZE_90;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        case 2:
                            {
                                rotation = Rotation.CLOCKWIZE_180;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }
                        default:
                            {
                                rotation = Rotation.CLOCKWIZE_270;
                                newLandingPoint = new Point(i, landingY + 1);
                                break;
                            }

                    }

                    predict = FigureRotator.PredictCurrentFigurePoints(
                        rotation,
                        newLandingPoint,
                        board.GetCurrentFigureType());

                    snapshots.AddRange(
                        base.GetPossibleMovements(
                            board, predict, newLandingPoint, rotation));

                }
            }

            return snapshots.OrderByDescending(i => i.Score).First();
        }

        private protected override void ClearBoardFromCurrentFigure(Board board)
        {
            var currFigure = board.GetCurrentFigurePoint();

            board.Set(currFigure.X, currFigure.Y + 1, '.');
            board.Set(currFigure.X - 1, currFigure.Y, '.');
            board.Set(currFigure.X, currFigure.Y, '.');
            board.Set(currFigure.X + 1, currFigure.Y, '.');
        }
    }

    public class BestMoveDto
    {
        public double Score;
        public Point LandingPoint;
        public Rotation Rotation;
        public BestMoveDto(double score, Point landingPoint, Rotation rotation)
        {
            Score = score;
            LandingPoint = landingPoint;
            Rotation = rotation;
        }
    }
}