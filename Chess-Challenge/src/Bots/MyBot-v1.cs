using ChessChallenge.API;
using System;
using System.Numerics;
using System.Linq;

public class MyBot_V1 : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();

        // Stores the values of each move
        // 0 = None, 1 = Pawn, 2 = Knight, 3 = Bishop, 4 = Rook, 5 = Queen, 6 = King
        int[] moveValues = new int[allMoves.Length];
        int[] pieceValues = { 0, 10, 30, 30, 50, 90, 1000 }; // Monkeys?

        // Play a random move if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        int bestValue = -999;

        foreach (Move move in allMoves)
        {
            // Checks if the move is an en-passant, and take the bait
            if (move.IsEnPassant) {
                Console.WriteLine("En-passant detected");
                moveToPlay = move;
                break;
            }

            // Make the move and evaluate the board, then undo the move
            board.MakeMove(move);
            
            // Check if the move is a checkmate, and take the bait
            if (board.IsInCheckmate()) {
                Console.WriteLine("Checkmate detected");
                moveToPlay = move;
                break;
            }

            int boardValue = -EvaluateBoard();
            board.UndoMove(move);
            // Console.WriteLine($"Move: {move} | Value: {boardValue}");
            

            // Check if move square is attacked
            boardValue -= MoveIsAttacked(move) / 2; 

            // If the board value is better than the current best value, make it the new best value
            if (boardValue > bestValue) {
                bestValue = boardValue;
                moveToPlay = move;
            }
        }

        // board.MakeMove(moveToPlay);
        // allMoves = board.GetLegalMoves();

        // foreach(Move move in allMoves) {
        //     Console.WriteLine($"Move: {move} | Value: {moveValues[Array.IndexOf(allMoves, move)]}");
        // }
        // board.UndoMove(moveToPlay);

        

        // Return the best move
        return moveToPlay;


        //--------------------------------------------------------------------------------
        // Functions
        //--------------------------------------------------------------------------------

        // Evalutes if the move square is under attack and returns piece value if it is
        int MoveIsAttacked(Move move)
        {  
            if (board.SquareIsAttackedByOpponent(new Square
            (move.TargetSquare.File, move.TargetSquare.Rank))) 
            {
                Console.WriteLine("Move" + move + "is attacked, Piece value: " + pieceValues[(int)move.MovePieceType]);
                return pieceValues[(int)move.MovePieceType];
            }
            return 0;
        }


        // Evaluate the board from the perspective of the bot
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

                    // Convert file and rank to algebraic notation.
                    // char fileChar = (char)(file + 'a');
                    // int rankInt = rank + 1;
                    // Console.WriteLine($"{fileChar}{rankInt}");

                    // Create new Square object.
                    Square square = new Square(file, rank);

                    // Get the piece on the square.
                    Piece piece = board.GetPiece(square);

                    // Get the absolute piece value
                    int pieceValue = pieceValues[(int)piece.PieceType];

                    // If piece is not my colour, make it negative
                    if (piece.IsWhite != board.IsWhiteToMove) {
                        pieceValue = -pieceValue;
                    }

                    // Add the piece value to the total evaluation
                    totalEvaluation += pieceValue;

                    // Console.WriteLine(piece);
                }
            }
            return totalEvaluation;
        }
    }
}


/////////////////////////
// Old code snippets