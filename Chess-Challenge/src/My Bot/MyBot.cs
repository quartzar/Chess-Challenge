using ChessChallenge.API;
using System;
using System.Numerics;
using System.Linq;

public class MyBot : IChessBot
{
    // 0 = None, 1 = Pawn, 2 = Knight, 3 = Bishop, 4 = Rook, 5 = Queen, 6 = King
    int[] pieceValues = { 0, 10, 30, 30, 50, 90, 900 };

    public Move Think(Board board, Timer timer)
    {
        Move[] newGameMoves = board.GetLegalMoves();

        // Stores the values of each move
        int[] moveValues = new int[newGameMoves.Length];

        // Checks what colour the bot is playing as
        bool amIWhite = board.IsWhiteToMove;

        // Play a random move if nothing better is found
        Random rng = new();
        Move moveToPlay = newGameMoves[rng.Next(newGameMoves.Length)];
        
        int bestMove = -9999;

        // Depth of the minimax algorithm
        int depth = 3;

        foreach (Move newGameMove in newGameMoves)
        {

            // Make the move 
            board.MakeMove(newGameMove);

            int moveValue = MiniMax(depth - 1, newGameMove, -10000, 10000, false);

            board.UndoMove(newGameMove);

            // Evaluate best move 
            if (moveValue >= bestMove) {
                bestMove = moveValue;
                moveToPlay = newGameMove;
            }
        }
        

        // Return the best move
        return moveToPlay;


        //--------------------------------------------------------------------------------
        // Functions
        //--------------------------------------------------------------------------------


        // MiniMax algorithm
        //--------------------------------------------------------------------------------
        int MiniMax(int depth, Move move, int alpha, int beta, bool isMaximisingPlayer)
        {
            if (depth == 0) 
            {
                // return -EvaluateBoard();
                return isMaximisingPlayer ? EvaluateBoard() : -EvaluateBoard();
            }

            Move[] newGameMoves = board.GetLegalMoves();

            // if (isMaximisingPlayer) 
            // {
            //     int bestMove = -9999;

            //     foreach (Move newGameMove in newGameMoves)
            //     {   // Make the move and evaluate it
            //         board.MakeMove(newGameMove);
            //         bestMove = Math.Max(bestMove, MiniMax(depth - 1, newGameMove, alpha, beta, !isMaximisingPlayer));
            //         board.UndoMove(newGameMove);

            //         // Alpha Beta pruning
            //         alpha = Math.Max(alpha, bestMove);
            //         if (beta <= alpha) {
            //             return bestMove;
            //         }
            //     }
            //     return bestMove;
            // }
            // else 
            // {
            //     int bestMove = 9999;

            //     foreach (Move newGameMove in newGameMoves)
            //     {   // Make the move and evaluate it
            //         board.MakeMove(newGameMove);
            //         bestMove = Math.Min(bestMove, MiniMax(depth - 1, newGameMove, alpha, beta, !isMaximisingPlayer));
            //         board.UndoMove(newGameMove);

            //         // Alpha Beta pruning
            //         beta = Math.Min(beta, bestMove);
            //         if (beta <= alpha) {
            //             return bestMove;
            //         }

            //     }
            //     return bestMove;
            // }

            int bestMove = isMaximisingPlayer ? -9999 : 9999;

            foreach (Move newGameMove in newGameMoves)
            {   // Make the move and evaluate it
                board.MakeMove(newGameMove);
                int moveValue = MiniMax(depth - 1, newGameMove, alpha, beta, !isMaximisingPlayer);
                
                bestMove = isMaximisingPlayer 
                ? Math.Max(bestMove, moveValue) 
                : Math.Min(bestMove, moveValue);

                board.UndoMove(newGameMove);

                // Alpha Beta pruning
                if (isMaximisingPlayer) 
                {
                    alpha = Math.Max(alpha, bestMove);
                }
                else 
                {
                    beta = Math.Min(beta, bestMove);
                }
                if (beta <= alpha) 
                {
                    return bestMove;
                }
            }
            return bestMove;
        }
        //--------------------------------------------------------------------------------



        // Evaluate the board from the perspective of the bot
        //--------------------------------------------------------------------------------
        int EvaluateBoard()
        {
            ulong bitboard = board.AllPiecesBitboard; 
            int totalEvaluation = 0;
            
            for (int i = 0; i < 64; ++i)
            {
                if ((bitboard & (1UL << i)) != 0) // If bit == 1, square is occupied.
                {
                    int file = i % 8; //File 'a' to 'h'
                    int rank = i / 8; //Rank '1' to '8'

                    // Get the piece on the square.
                    Square square = new Square(file, rank);
                    Piece piece = board.GetPiece(square);

                    // Get the absolute piece value
                    int pieceValue = pieceValues[(int)piece.PieceType];

                    // Add high weighting to checkmate
                    if (board.IsInCheckmate()) {
                        pieceValue *= 100;
                    }

                    // If piece is opponent, make it negative
                    if (piece.IsWhite != board.IsWhiteToMove) {
                        pieceValue = -pieceValue;
                        
                    }
                    totalEvaluation += pieceValue;
                }
            }
            return totalEvaluation;
        }
        //--------------------------------------------------------------------------------
    }
}