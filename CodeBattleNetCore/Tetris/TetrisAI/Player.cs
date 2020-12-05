using System.Collections.Generic;
using System.Linq;

namespace TetrisClient.BotSolution
{
    public static class Player
    {
        private static readonly IEnumerable<IFigureAdjuster> figureOperators
            = new IFigureAdjuster[]
            {
                new OFigure(),
                new IFigure(),
                new JFigure(),
                new LFigure(),
                new ZFigure(),
                new SFigure(),
                new TFigure(),
            };

        /// <summary>
        /// Get command for the best move.
        /// </summary>
        /// <param name="board">Game board.</param>
        public static Command MakeMove(Board board)
        {
            Command command = Command.ROTATE_CLOCKWISE_180.Then(Command.ROTATE_CLOCKWISE_180);

            BestMoveDto bestMove = figureOperators
                .First(f => f.Element 
                            == board.GetCurrentFigureType())
                .GetBestMove(board);
       
            command = AdjustRotation(bestMove.Rotation, command);
            command = AdjustX(board.GetCurrentFigurePoint().X,
                bestMove.LandingPoint.X, command);

            return command.Then(Command.DOWN);
        }

        private static Command AdjustRotation(Rotation rotation, Command command)
        {
            switch (rotation)
            {
                case Rotation.CLOCKWIZE_90: return command.Then(Command.ROTATE_CLOCKWISE_90);

                case Rotation.CLOCKWIZE_180: return command.Then(Command.ROTATE_CLOCKWISE_180);

                case Rotation.CLOCKWIZE_270: return command.Then(Command.ROTATE_CLOCKWISE_270);

                default: return command;
            }
        }

        private static Command AdjustX(int currentX, int futureX, Command command)
        {
            while (currentX != futureX)
            {
                if (currentX > futureX)
                {
                    command = command.Then(Command.LEFT);
                    currentX--;
                }

                if (currentX < futureX)
                {
                    command = command.Then(Command.RIGHT);
                    currentX++;
                }
            }

            return command;
        }
    }
}