using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        // generate random integer
        int random = new System.Random().Next(0, moves.Length);
        return moves[random];
    }
}