using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
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

    Move[] m_killerMoves;

    Board m_board;

    public MyBot()
    {
        m_killerMoves = new Move[1024];
        m_TPTable = new Transposition[0x800000];
    }

    //--------------------------------------------------------------------------------

    public Move Think(Board board, Timer timer)
    {
        m_board = board;

        int monkeyCounter = 0; //#DEBUG
        int ttCounter = 0; //#DEBUG
        int startTime = timer.MillisecondsElapsedThisTurn; //#DEBUG

        Transposition moveToPlay = m_TPTable[board.ZobristKey & 0x7FFFFF];
        int maxTime = timer.MillisecondsRemaining/50;

        for (sbyte depth = 1;; depth++) // this is going to break // it did break
        {
            Negamax(depth, -100000000, 100000000);
            moveToPlay = m_TPTable[board.ZobristKey & 0x7FFFFF];
            Console.WriteLine($"Depth: {depth}"); //#DEBUG
            if (!ShouldExecuteNextDepth(timer, maxTime)) break;
        }

        

        // Log time taken to make a move
        // Console.WriteLine($"Move time: {timer.MillisecondsElapsedThisTurn}ms");
        string formattedMonkeyCounter = string.Format("{0:n0}", monkeyCounter); //#DEBUG
        Console.WriteLine($"Monkey counter: {formattedMonkeyCounter}"); //#DEBUG

        // Log positions per second
        int moveTime = timer.MillisecondsElapsedThisTurn - startTime; //#DEBUG
        int positionsPerSecond 
        = monkeyCounter > 0 & moveTime > 0 ? (int)(monkeyCounter * 1000 / moveTime) : 0; //#DEBUG
        string formattedPositionsPerSecond = string.Format("{0:n0}", positionsPerSecond); //#DEBUG
        Console.WriteLine($"Positions/s => {formattedPositionsPerSecond}"); //#DEBUG
        Console.WriteLine($"TTable hits => {ttCounter}"); //#DEBUG

        return moveToPlay.move;


//------//--------------------------------------------------------------------------------
//------// Functions
//------//--------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------


        // Negamax algorithm
        //--------------------------------------------------------------------------------
        int Negamax(int depth, int alpha, int beta)
        {
            bool pvNode = alpha + 1 < beta;
            bool inQSearch = depth <= 0;
            int startingAlpha = alpha;
            int bestEvaluation = -2147483647;              

            // Transposition table lookup
            ref Transposition TT = ref m_TPTable[m_board.ZobristKey & 0x7FFFFF];
            if (TT.zobristHash == m_board.ZobristKey && TT.depth >= depth)
            {
                ttCounter++; //#DEBUG
                // If score is exact (alpha < score < beta), return it
                if (TT.flag == 1) return TT.evaluation;
                // If lowerbound (score >= beta), return it
                else if (TT.flag == 2 && TT.evaluation >= beta) return TT.evaluation;
                // If upperbound (score <= alpha), return it
                else if (TT.flag == 3 && TT.evaluation <= alpha) return TT.evaluation;
            }

            // Move[] newGameMoves = board.GetLegalMoves();

            // Do not evaluate if move leads to draw or checkmate
            if (m_board.IsDraw()) return -10; // ~75+/-20 Elo increase over V4
            if (m_board.IsInCheckmate()) return m_board.PlyCount - 1000000; // checkmate

            int standingPat = EvaluateBoard();


            // Retrieve captures if in quiescence search AND not in check, 
            // otherwise retrieve all legal moves
            Move[] newMoves = board.GetLegalMoves(inQSearch && !m_board.IsInCheck());
            if (newMoves.Length == 0) return standingPat; // stalemate (cannot reach in checkmate)
            if (inQSearch)
            {
                if (standingPat >= beta) return standingPat;
                if (standingPat > alpha) alpha = standingPat;
            }


            // Order the moves for efficiency TODO: Improve ordering, reduce token count
            OrderMoves(ref newMoves, depth);
            Move bestMove = newMoves[0]; // assuming first move is best move

                

            for (int m = 0; m < newMoves.Length; m++)
            {   
                Move move = newMoves[m];
                m_board.MakeMove(move);
                
                int moveValue = -Negamax(depth - 1, -beta, -alpha);
                // int moveValue = -Negamax(depth - 1, (inQSearch || m == 0) ? -beta : -alpha - 1, -alpha);  
                // if (!inQSearch && m != 0 && moveValue > alpha && moveValue < beta)
                //     moveValue = -Negamax(depth - 1, -beta, -alpha); // re-search if null window was good
                // bestEvaluation = Math.Max(bestEvaluation, moveValue);
                m_board.UndoMove(move); 

                if (bestEvaluation < moveValue)
                {
                    bestEvaluation = moveValue;
                    bestMove = move;
                }

                // Alpha Beta pruning
                alpha = Math.Max(alpha, bestEvaluation);
                if (alpha >= beta) break;
            }

            // Store the evaluation of the current position into the transposition table
            if (!inQSearch)
            {
                TT.zobristHash = m_board.ZobristKey;
                TT.evaluation = bestEvaluation;
                TT.move = bestMove;
                TT.depth = (sbyte)depth;
                if (bestEvaluation < startingAlpha)
                    TT.flag = 3; // Upperbound
                else if (bestEvaluation >= beta)
                {
                    TT.flag = 2; // Lowerbound
                    if (!bestMove.IsCapture) m_killerMoves[depth] = bestMove;
                }
                else
                    TT.flag = 1; // Exact
            }
            

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
                    if (piece.IsWhite != board.IsWhiteToMove)
                        pieceValue = -pieceValue;
        
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


        // Move ordering function --> significantly more tokens, but greatly improved efficiency!
        //--------------------------------------------------------------------------------
        void OrderMoves(ref Move[] moves, int depth)
        {
            int[] movePriorities = new int[moves.Length];
            for(int m = 0; m < moves.Length; m++) movePriorities[m] =  GetMovePriority(moves[m], depth);
            Array.Sort(movePriorities, moves);
            Array.Reverse(moves);
        }
        //--------------------------------------------------------------------------------
        int GetMovePriority(Move move, int depth)
        {
            int priority = 0;
            // Check for a transposition table move (likely a PV move)
            Transposition TT = m_TPTable[m_board.ZobristKey & 0x7FFFFF];
            if(TT.move == move && TT.zobristHash == m_board.ZobristKey) 
                priority += 100000;
            
            else if (move.IsCapture) // Order captures by MVV-LVA
                priority =  1000 + 10 * (int)move.CapturePieceType - (int)move.MovePieceType;
            
            
            else if (depth >= 0 && move.Equals(m_killerMoves[depth])) // Order killer moves
                priority =  1;

            return priority;
        }
        //--------------------------------------------------------------------------------


        // Move ordering function [OLD]
        //--------------------------------------------------------------------------------
        // Move[] OrderMoves(Move[] moves)
        // {
        //     List<Move> captureMoves = new List<Move>();
        //     List<Move> promotionMoves = new List<Move>();
        //     List<Move> otherMoves = new List<Move>();

        //     foreach (Move m in moves)
        //     {
        //         if (m.IsCapture) { 
        //             captureMoves.Add(m);
        //         }
        //         else if (m.IsPromotion) {
        //             promotionMoves.Add(m);
        //         }
        //         else otherMoves.Add(m);
        //     }
        
        //     return captureMoves.Concat(promotionMoves).Concat(otherMoves).ToArray();
        // }
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