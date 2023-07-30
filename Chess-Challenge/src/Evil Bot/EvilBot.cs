using ChessChallenge.API;
using System;

// MyBot-v3 (with alpha-beta pruning)
namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
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
            int depth = 2;

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

                if (isMaximisingPlayer) 
                {
                    int bestMove = -9999;

                    foreach (Move newGameMove in newGameMoves)
                    {   // Make the move and evaluate it
                        board.MakeMove(newGameMove);
                        bestMove = Math.Max(bestMove, MiniMax(depth - 1, newGameMove, alpha, beta, !isMaximisingPlayer));
                        board.UndoMove(newGameMove);

                        // Alpha Beta pruning
                        alpha = Math.Max(alpha, bestMove);
                        if (beta <= alpha) {
                            return bestMove;
                        }
                    }
                    return bestMove;
                }

                else 
                {
                    int bestMove = 9999;

                    foreach (Move newGameMove in newGameMoves)
                    {   // Make the move and evaluate it
                        board.MakeMove(newGameMove);
                        bestMove = Math.Min(bestMove, MiniMax(depth - 1, newGameMove, alpha, beta, !isMaximisingPlayer));
                        board.UndoMove(newGameMove);

                        // Alpha Beta pruning
                        beta = Math.Min(beta, bestMove);
                        if (beta <= alpha) {
                            return bestMove;
                        }

                    }
                    return bestMove;
                }
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

                        // Add high weighting to checkmate
                        if (board.IsInCheckmate()) {
                            pieceValue *= 100;
                        }

                        // Console.WriteLine("isWhite: "+ piece.IsWhite + " isWhiteToMove: " + board.IsWhiteToMove);

                        // If piece is opponent, make it negative
                        if (piece.IsWhite != board.IsWhiteToMove) {
                            pieceValue = -pieceValue;
                            
                        }

                        // Add the piece value to the total evaluation
                        totalEvaluation += pieceValue;

                        // Console.WriteLine(piece);
                    }
                }
                // Console.WriteLine(totalEvaluation);
                return totalEvaluation;
            }
            //--------------------------------------------------------------------------------
        }
    }
}


// // MyBot-v2
// namespace ChessChallenge.Example
// {
//     // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
//     // Plays randomly otherwise.
//     public class EvilBot : IChessBot
//     {
//         public Move Think(Board board, Timer timer)
//         {
//             Move[] newGameMoves = board.GetLegalMoves();

//             // Stores the values of each move
//             // 0 = None, 1 = Pawn, 2 = Knight, 3 = Bishop, 4 = Rook, 5 = Queen, 6 = King
//             int[] moveValues = new int[newGameMoves.Length];
//             int[] pieceValues = { 0, 10, 30, 30, 50, 90, 1000 };

//             // Play a random move if nothing better is found
//             Random rng = new();
//             Move moveToPlay = newGameMoves[rng.Next(newGameMoves.Length)];
            
//             int bestMove = -999;

//             // Depth of the minimax algorithm
//             int depth = 1;

//             foreach (Move newGameMove in newGameMoves)
//             {

//                 // Make the move 
//                 board.MakeMove(newGameMove);

//                 int moveValue = MiniMax(depth, newGameMove, false);

//                 board.UndoMove(newGameMove);



//                 // Evaluate best move 
//                 if (moveValue >= bestMove) {
//                     bestMove = moveValue;
//                     moveToPlay = newGameMove;
//                 }
//             }
            

//             // Return the best move
//             return moveToPlay;


//             //--------------------------------------------------------------------------------
//             // Functions
//             //--------------------------------------------------------------------------------


//             // MiniMax algorithm
//             //--------------------------------------------------------------------------------
//             int MiniMax(int depth, Move move, bool isMaximisingPlayer)
//             {
//                 if (depth == 0) {
//                     return -EvaluateBoard();
//                 }

//                 Move[] newGameMoves = board.GetLegalMoves();

//                 if (isMaximisingPlayer) 
//                 {
//                     int bestMove = -999;

//                     foreach (Move newGameMove in newGameMoves)
//                     {
//                         board.MakeMove(newGameMove);
//                         bestMove = Math.Max(bestMove, MiniMax(depth - 1, newGameMove, !isMaximisingPlayer));
//                         board.UndoMove(newGameMove);
//                     }
//                     return bestMove;
//                 }

//                 else 
//                 {
//                     int bestMove = 999;

//                     foreach (Move newGameMove in newGameMoves)
//                     {
//                         board.MakeMove(newGameMove);
//                         bestMove = Math.Min(bestMove, MiniMax(depth - 1, newGameMove, !isMaximisingPlayer));
//                         board.UndoMove(newGameMove);
//                     }
//                     return bestMove;
//                 }
//             }
//             //--------------------------------------------------------------------------------


//             // Evalutes if the move square is under attack and returns piece value if it is
//             //--------------------------------------------------------------------------------
//             int MoveIsAttacked(Move move)
//             {  
//                 if (board.SquareIsAttackedByOpponent(new Square
//                 (move.TargetSquare.File, move.TargetSquare.Rank))) 
//                 {
//                     Console.WriteLine("Move" + move + "is attacked, Piece value: " + pieceValues[(int)move.MovePieceType]);
//                     return pieceValues[(int)move.MovePieceType];
//                 }
//                 return 0;
//             }
//             //--------------------------------------------------------------------------------


//             // Evaluate the board from the perspective of the bot
//             //--------------------------------------------------------------------------------
//             int EvaluateBoard()
//             {
//                 ulong bitboard = board.AllPiecesBitboard; 
//                 int totalEvaluation = 0;
                
//                 for (int i = 0; i < 64; ++i)
//                 {
//                     if ((bitboard & (1UL << i)) != 0) // If bit == 1, square is occupied.
//                     {
//                         int file = i % 8; //File 'a' to 'h'
//                         int rank = i / 8; //Rank '1' to '8'

//                         // Convert file and rank to algebraic notation.
//                         // char fileChar = (char)(file + 'a');
//                         // int rankInt = rank + 1;
//                         // Console.WriteLine($"{fileChar}{rankInt}");

//                         // Create new Square object.
//                         Square square = new Square(file, rank);

//                         // Get the piece on the square.
//                         Piece piece = board.GetPiece(square);

//                         // Get the absolute piece value
//                         int pieceValue = pieceValues[(int)piece.PieceType];

//                         // Add high weighting to checkmate
//                         if (board.IsInCheckmate()) {
//                             pieceValue *= 100;
//                         }

//                         // If piece is not my colour, make it negative
//                         if (piece.IsWhite != board.IsWhiteToMove) {
//                             pieceValue = -pieceValue;
//                         }

//                         // Add the piece value to the total evaluation
//                         totalEvaluation += pieceValue;

//                         // Console.WriteLine(piece);
//                     }
//                 }
//                 return totalEvaluation;
//             }
//             //--------------------------------------------------------------------------------
//         }
//     }
// }




// MyBot-v1
// namespace ChessChallenge.Example
// {
//     // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
//     // Plays randomly otherwise.
//     public class EvilBot : IChessBot
//     {
//         public Move Think(Board board, Timer timer)
//         {
//             Move[] allMoves = board.GetLegalMoves();

//             // Stores the values of each move
//             // 0 = None, 1 = Pawn, 2 = Knight, 3 = Bishop, 4 = Rook, 5 = Queen, 6 = King
//             int[] moveValues = new int[allMoves.Length];
//             int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

//             // Play a random move if nothing better is found
//             Random rng = new();
//             Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
//             int bestValue = -999;

//             foreach (Move move in allMoves)
//             {
//                 // Checks if the move is an en-passant, and take the bait
//                 if (move.IsEnPassant) {
//                     Console.WriteLine("En-passant detected");
//                     moveToPlay = move;
//                     break;
//                 }

//                 // Make the move and evaluate the board, then undo the move
//                 board.MakeMove(move);
                
//                 // Check if the move is a checkmate, and take the bait
//                 if (board.IsInCheckmate()) {
//                     Console.WriteLine("Checkmate detected");
//                     moveToPlay = move;
//                     break;
//                 }

//                 int boardValue = -EvaluateBoard();
//                 board.UndoMove(move);
//                 // Console.WriteLine($"Move: {move} | Value: {boardValue}");
                

//                 // Check if move square is attacked
//                 boardValue -= MoveIsAttacked(move) / 2; 

//                 // If the board value is better than the current best value, make it the new best value
//                 if (boardValue > bestValue) {
//                     bestValue = boardValue;
//                     moveToPlay = move;
//                 }
//             }

//             // board.MakeMove(moveToPlay);
//             // allMoves = board.GetLegalMoves();

//             // foreach(Move move in allMoves) {
//             //     Console.WriteLine($"Move: {move} | Value: {moveValues[Array.IndexOf(allMoves, move)]}");
//             // }
//             // board.UndoMove(moveToPlay);

            

//             // Return the best move
//             return moveToPlay;


//             //--------------------------------------------------------------------------------
//             // Functions
//             //--------------------------------------------------------------------------------

//             // Evalutes if the move square is under attack and returns piece value if it is
//             int MoveIsAttacked(Move move)
//             {  
//                 if (board.SquareIsAttackedByOpponent(new Square
//                 (move.TargetSquare.File, move.TargetSquare.Rank))) 
//                 {
//                     Console.WriteLine("Move" + move + "is attacked, Piece value: " + pieceValues[(int)move.MovePieceType]);
//                     return pieceValues[(int)move.MovePieceType];
//                 }
//                 return 0;
//             }


//             // Evaluate the board from the perspective of the bot
//             int EvaluateBoard()
//             {
//                 ulong bitboard = board.AllPiecesBitboard; 
//                 int totalEvaluation = 0;
                
//                 for (int i = 0; i < 64; ++i)
//                 {
//                     if ((bitboard & (1UL << i)) != 0) // If bit == 1, square is occupied.
//                     {
//                         int file = i % 8; //File 'a' to 'h'
//                         int rank = i / 8; //Rank '1' to '8'

//                         // Convert file and rank to algebraic notation.
//                         // char fileChar = (char)(file + 'a');
//                         // int rankInt = rank + 1;
//                         // Console.WriteLine($"{fileChar}{rankInt}");

//                         // Create new Square object.
//                         Square square = new Square(file, rank);

//                         // Get the piece on the square.
//                         Piece piece = board.GetPiece(square);

//                         // Get the absolute piece value
//                         int pieceValue = pieceValues[(int)piece.PieceType];

//                         // If piece is not my colour, make it negative
//                         if (piece.IsWhite != board.IsWhiteToMove) {
//                             pieceValue = -pieceValue;
//                         }

//                         // Add the piece value to the total evaluation
//                         totalEvaluation += pieceValue;

//                         // Console.WriteLine(piece);
//                     }
//                 }
//                 return totalEvaluation;
//             }
//         }
//     }
// }



// OG EvilBot
// namespace ChessChallenge.Example
// {
//     // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
//     // Plays randomly otherwise.
//     public class EvilBot : IChessBot
//     {
//         // Piece values: null, pawn, knight, bishop, rook, queen, king
//         int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

//         public Move Think(Board board, Timer timer)
//         {
//             Move[] allMoves = board.GetLegalMoves();

//             // Pick a random move to play if nothing better is found
//             Random rng = new();
//             Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
//             int highestValueCapture = 0;

//             foreach (Move move in allMoves)
//             {
//                 // Always play checkmate in one
//                 if (MoveIsCheckmate(board, move))
//                 {
//                     moveToPlay = move;
//                     break;
//                 }

//                 // Find highest value capture
//                 Piece capturedPiece = board.GetPiece(move.TargetSquare);
//                 int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

//                 if (capturedPieceValue > highestValueCapture)
//                 {
//                     moveToPlay = move;
//                     highestValueCapture = capturedPieceValue;
//                 }
//             }

//             return moveToPlay;
//         }

//         // Test if this move gives checkmate
//         bool MoveIsCheckmate(Board board, Move move)
//         {
//             board.MakeMove(move);
//             bool isMate = board.IsInCheckmate();
//             board.UndoMove(move);
//             return isMate;
//         }
//     }
// }

