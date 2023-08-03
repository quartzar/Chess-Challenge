using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot_V5_1 : IChessBot
{
    // 0 = None, 1 = Pawn, 2 = Knight, 3 = Bishop, 4 = Rook, 5 = Queen, 6 = King
    int[] pieceValues = { 0, 10, 31, 33, 50, 90, 900};

    //--------------------------------------------------------------------------------
    // Transposition table                                            CREDIT: Selenaut
    
    // private const byte INVALID = 0, EXACT = 1, LOWERBOUND = 2, UPPERBOUND = 3;

    // 14 bytes per entry, likely will align to 16 bytes due to padding (if it aligns to 32, recalculate max TP table size)
    struct Transposition
    {
        public ulong zobristHash;
        public Move move;
        public int evaluation;
        public sbyte depth;
        public byte flag;
    }; 

    Transposition[] m_TPTable;
    // ulong k_TpMask = 0x7FFFFF; //4.7 million entries, likely consuming about 151 MB of memory
    //To access => ref Transposition transposition = ref m_TPTable[board.ZobristKey & k_TpMask];
    //you also need to check to make sure that the stored hash is equal to the board state you're interested in, alongside your normal checks w.r.t. depth etc.

    public MyBot_V5_1()
    {
        m_TPTable = new Transposition[0x800000];
    }

    // Board m_board;

    //--------------------------------------------------------------------------------

    public Move Think(Board board, Timer timer)
    {
        Move[] newGameMoves = OrderMoves(board.GetLegalMoves());
        
        // m_board = board;
        
        // Play a random move if nothing better is found
        Random rng = new();
        Move moveToPlay = newGameMoves[rng.Next(newGameMoves.Length)];
        
        // Depth of the minimax algorithm
        int depth = 3;
        int bestEvaluation = -9999;

        int monkeyCounter = 0; //#DEBUG
        int ttCounter = 0; //#DEBUG
        int startTime = timer.MillisecondsElapsedThisTurn; //#DEBUG

        // int maxTime = timer.MillisecondsRemaining/30;

        // for (sbyte depth = 1;; depth++) // this is going to break // it did break
        // {
        foreach (Move newGameMove in newGameMoves)
        {   // Make the move 
            board.MakeMove(newGameMove);

            int moveValue = -Negamax(depth - 1, newGameMove, -10000, 10000);

            board.UndoMove(newGameMove);

            // Evaluate best move 
            if (moveValue >= bestEvaluation) {
                bestEvaluation = moveValue;
                moveToPlay = newGameMove;
            }
        }
        //     Console.WriteLine($"Depth: {depth}"); //#DEBUG
        //     if (!ShouldExecuteNextDepth(timer, maxTime)) break;
        // }
        

        // Log time taken to make a move
        // Console.WriteLine($"Move time: {timer.MillisecondsElapsedThisTurn}ms");
        Console.WriteLine($"Monkey counter: {monkeyCounter}");

        // Log positions per second
        int moveTime = timer.MillisecondsElapsedThisTurn - startTime; //#DEBUG
        int positionsPerSecond 
        = monkeyCounter > 0 & moveTime > 0 ? (int)(monkeyCounter * 1000 / moveTime) : 0; //#DEBUG
        string formattedPositionsPerSecond = string.Format("{0:n0}", positionsPerSecond); //#DEBUG
        Console.WriteLine($"Positions/s => {formattedPositionsPerSecond}"); //#DEBUG
        // Console.WriteLine($"TTable hits => {ttCounter}"); //#DEBUG

        return moveToPlay;


//------//--------------------------------------------------------------------------------
//------// Functions
//------//--------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------


        // Negamax algorithm
        //--------------------------------------------------------------------------------
        int Negamax(int depth, Move move, int alpha, int beta)
        {

            Move[] newGameMoves = board.GetLegalMoves();

            // Do not evaluate if move leads to draw or checkmate
            if (board.IsDraw()) return 0; // ~75+/-20 Elo increase over V4
            if (newGameMoves.Length == 0) // Checkmate
            {
                return board.PlyCount - 99999;
            }

            // Transposition table lookup
            // ref Transposition tp = ref m_TPTable[m_board.ZobristKey & 0x7FFFFF];
            // if (tp.zobristHash == m_board.ZobristKey && tp.depth >= depth)
            // {
            //     ttCounter++; //#DEBUG
            //     switch (tp.flag)
            //     {
            //         case 1: // EXACT
            //             return tp.evaluation;
            //         case 2: // LOWERBOUND
            //             alpha = Math.Max(alpha, tp.evaluation);
            //             break;
            //         case 3: // UPPERBOUND
            //             beta = Math.Min(beta, tp.evaluation);
            //             break;
            //     }
            //     if (alpha >= beta) 
            //         return tp.evaluation;
            // }

            // If depth is 0, evaluate the board
            if (depth == 0) 
                return EvaluateBoard();


            // Order the moves for efficiency TODO: Improve ordering, reduce token count
            Move[] orderedMoves = OrderMoves(newGameMoves);

            int bestEvaluation = -9999;          

            foreach (Move orderedMove in orderedMoves)
            {   
                board.MakeMove(orderedMove);
                
                int moveValue = -Negamax(depth - 1, orderedMove, -beta, -alpha);  
                bestEvaluation = Math.Max(bestEvaluation, moveValue);

                board.UndoMove(orderedMove); 

                // Alpha Beta pruning
                alpha = Math.Max(alpha, bestEvaluation);
                if (alpha >= beta) 
                    break;
            }

            // Store the evaluation of the current position into the transposition table
            // tp.zobristHash = m_board.ZobristKey;
            // tp.evaluation = bestEvaluation;
            // tp.depth = (sbyte)depth;

            // if (bestEvaluation <= alpha)
            //     tp.flag = 3; // UPPERBOUND
            // else if (bestEvaluation >= beta)
            //     tp.flag = 2; // LOWERBOUND
            // else
            //     tp.flag = 1; // EXACT


            return bestEvaluation;
        }
        //--------------------------------------------------------------------------------



        // Evaluate the board from the perspective of the bot
        //--------------------------------------------------------------------------------
        int EvaluateBoard()
        {   
            monkeyCounter++; //#DEBUG

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

                    // If piece is opponent, make it negative
                    if (piece.IsWhite != board.IsWhiteToMove) {
                        pieceValue = -pieceValue;
                        
                    }
                    totalEvaluation += pieceValue;
                }
            }
            // Calculate mobility --> ELO diff increase of 100-130
            int myMobility = board.GetLegalMoves().Length;
            if (board.TrySkipTurn()) {
                int opponentMobility = board.GetLegalMoves().Length;
                int mobility = myMobility - opponentMobility;
                
                totalEvaluation += mobility;
                board.UndoSkipTurn();
            }

            return totalEvaluation;
        }
        //--------------------------------------------------------------------------------


        // Move ordering function
        //--------------------------------------------------------------------------------
        Move[] OrderMoves(Move[] moves)
        {
            List<Move> captureMoves = new List<Move>();
            List<Move> promotionMoves = new List<Move>();
            List<Move> otherMoves = new List<Move>();

            foreach (Move m in moves)
            {
                if (m.IsCapture) { 
                    captureMoves.Add(m);
                }
                else if (m.IsPromotion) {
                    promotionMoves.Add(m);
                }
                else otherMoves.Add(m);
            }
        
            return captureMoves.Concat(promotionMoves).Concat(otherMoves).ToArray();
        }
        //--------------------------------------------------------------------------------


        // Depth timer
        //--------------------------------------------------------------------------------
        bool ShouldExecuteNextDepth(Timer timer, int maxThinkTime)
        {
            int currentThinkTime = timer.MillisecondsElapsedThisTurn;
            return ((maxThinkTime - currentThinkTime) > currentThinkTime * 2);
        }
        //--------------------------------------------------------------------------------
    }
}