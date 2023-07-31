using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot_V4 : IChessBot
{
    // 0 = None, 1 = Pawn, 2 = Knight, 3 = Bishop, 4 = Rook, 5 = Queen, 6 = King
    int[] pieceValues = { 0, 10, 30, 30, 50, 90, 900 };

    public Move Think(Board board, Timer timer)
    {
        Move[] newGameMoves = board.GetLegalMoves();

        // Play a random move if nothing better is found
        Random rng = new();
        Move moveToPlay = newGameMoves[rng.Next(newGameMoves.Length)];
        
        // Depth of the minimax algorithm
        int depth = 2;
        int bestMove = -9999;

        int monkeyCounter = 0;

        foreach (Move newGameMove in newGameMoves)
        {   // Make the move 
            board.MakeMove(newGameMove);

            int moveValue = MiniMax(depth - 1, newGameMove, -10000, 10000, false);

            board.UndoMove(newGameMove);

            // Evaluate best move 
            if (moveValue >= bestMove) {
                bestMove = moveValue;
                moveToPlay = newGameMove;
            }
        }

        // Log time taken to make a move
        // Console.WriteLine($"Move time: {timer.MillisecondsElapsedThisTurn}ms");
        // Console.WriteLine($"Monkey counter: {monkeyCounter}");

        return moveToPlay;


        //--------------------------------------------------------------------------------
        // Functions
        //--------------------------------------------------------------------------------


        // MiniMax algorithm
        //--------------------------------------------------------------------------------
        int MiniMax(int depth, Move move, int alpha, int beta, bool isMaximisingPlayer)
        {
            if (depth == 0) 
                return isMaximisingPlayer ? EvaluateBoard() : -EvaluateBoard();
            

            Move[] newGameMoves = board.GetLegalMoves();

            int bestMove = isMaximisingPlayer ? -9999 : 9999;

            foreach (Move newGameMove in newGameMoves)
            {   // Make the move and evaluate it
                board.MakeMove(newGameMove);
                int moveValue = MiniMax(depth - 1, newGameMove, alpha, beta, !isMaximisingPlayer);

                monkeyCounter++;                

                bestMove = isMaximisingPlayer 
                ? Math.Max(bestMove, moveValue) 
                : Math.Min(bestMove, moveValue);

                board.UndoMove(newGameMove);

                // Alpha Beta pruning
                if (isMaximisingPlayer) 
                    alpha = Math.Max(alpha, bestMove);
                else beta = Math.Min(beta, bestMove);

                if (beta <= alpha) 
                    return bestMove;
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
                    Piece piece = board.GetPiece(new Square(file, rank));

                    // Get the absolute piece value
                    int pieceValue = pieceValues[(int)piece.PieceType];

                    // Add high weighting to checkmate
                    if (board.IsInCheckmate()) {
                        pieceValue *= 10;
                    }

                    // If piece is opponent, make it negative
                    if (piece.IsWhite != board.IsWhiteToMove) {
                        pieceValue = -pieceValue;
                        
                    }
                    totalEvaluation += pieceValue;
                }
            }

            // Calculate mobility and whether the game is a draw
            int myMobility = board.GetLegalMoves().Length;
            if (board.TrySkipTurn()) {
                int opponentMobility = board.GetLegalMoves().Length;
                int mobility = myMobility - opponentMobility;
                
                totalEvaluation += mobility;

                totalEvaluation -= board.IsDraw()
                ? 500
                : 0;

                board.UndoSkipTurn();
            }

            return totalEvaluation;
        }
        //--------------------------------------------------------------------------------
    }
}