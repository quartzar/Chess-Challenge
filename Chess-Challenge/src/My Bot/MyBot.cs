using ChessChallenge.API;
using System;
using System.Numerics;
using System.Linq;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        // Stores the values of each move
        // 0 = None, 1 = Pawn, 2 = Knight, 3 = Bishop, 4 = Rook, 5 = Queen, 6 = King
        int[] moveValues = new int[moves.Length];
        int[] piecePoints = {0, 1, 3, 3, 5, 9, 90};

        

        
        Console.WriteLine("Total evaluation: " + EvaluateBoard());




        Move bestMove = moves[0];
        int bestValue = -9999;

        for(int i = 0; i < moves.Length; i++)
        {
            // Checks to see if the move is an en-passant, and take the bait
            if(moves[i].IsEnPassant) {
                Console.WriteLine("En-passant detected");
                return moves[i];
            }

            // Make the move and evaluate the board, then undo the move
            Move newMove = moves[i];
            board.MakeMove(newMove);
            
            int boardValue = -EvaluateBoard();
            board.UndoMove(newMove);

            // If the board value is better than the current best value, make it the new best value
            if(boardValue > bestValue) {
                bestValue = boardValue;
                bestMove = newMove;
            }
        }

        // Return the best move
        return bestMove;

        //--------------------------------------------------------------------------------
        // Functions
        //--------------------------------------------------------------------------------

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
                    int pieceValue = piecePoints[(int)piece.PieceType];

                    // If piece is not my colour, make it negative
                    if(piece.IsWhite != board.IsWhiteToMove) {
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


    // Returns the value of a piece
    // public int GetPieceValue(int piece, int[] piecePoints, bool myPiece) {
    //     if(piece == 0 ) {
    //         return 0;
    //     }

    //     return myPiece ? piecePoints[piece] : -piecePoints[piece];
    // }

    // Evaluates the board
    // public int EvaluateBoard(Board board, int[] piecePoints, ulong bitboard) {
    //     int totalEvaluation = 0;

    //     for (int i = 0; i < 64; ++i)
    //     {
    //         if ((bitboard & (1UL << i)) != 0) // If bit == 1, square is occupied.
    //         {
    //             int file = i % 8; //File 'a' to 'h'
    //             int rank = i / 8; //Rank '1' to '8'

    //             // Convert file and rank to algebraic notation.
    //             // char fileChar = (char)(file + 'a');
    //             // int rankInt = rank + 1;
    //             // Console.WriteLine($"{fileChar}{rankInt}");

    //             // Create new Square object.
    //             Square square = new Square(file, rank);

    //             // Get the piece on the square.
    //             int piece = (int)board.GetPiece(square).PieceType;
                
                

    //             Console.WriteLine(piece);
    //         }
    //     }

    //     return totalEvaluation;
    // }

}





/////////////////////////
// Old code snippets

// Conditional move values
// Move currentMove = moves[i];
// moveValues[i] = 
// currentMove.IsEnPassant ? (int)currentMove.MovePieceType :
// currentMove.IsCastles ? (int)currentMove.MovePieceType :
// currentMove.IsCapture ? (int)currentMove.MovePieceType:
// currentMove.IsPromotion ? (int)currentMove.MovePieceType :
// (int)currentMove.MovePieceType;



// Checks to see if the move is a capture
// if(moves[i].IsCapture) {
//     moveValues[i] = piecePoints[(int)moves[i].MovePieceType] + (2 * piecePoints[(int)moves[i].CapturePieceType]);
//     Console.WriteLine("Capture piece:" + moveValues[i]);
//     continue;
// }

// // Checks to see if the move is a promotion
// if(moves[i].IsPromotion) {
//     moveValues[i] = piecePoints[(int)moves[i].PromotionPieceType];
//     Console.WriteLine("Promotion piece:" + moveValues[i] + " " + moves[i].PromotionPieceType);
//     continue;
// }

// moveValues[i] = (int)moves[i].MovePieceType;
// // Console.WriteLine(moveValues[i]);