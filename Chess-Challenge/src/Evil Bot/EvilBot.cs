using ChessChallenge.API;
using System;
using System.ComponentModel.DataAnnotations;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 325, 500, 900, 400 };

        public Move Think(Board board, Timer timer)
        {
            return Depth(board, timer, 3);
        }
        public Move Depth(Board board, Timer timer, int depth)
        {
            int factor = -1;
            if(board.IsWhiteToMove){
                factor = 1;
            }
            Move[] allMoves = board.GetLegalMoves();
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
            if(depth == 1){
                if (MoveIsCheckmate(board, moveToPlay)||timer.MillisecondsRemaining<1000)
                {
                    return moveToPlay;
                }
                board.MakeMove(moveToPlay);
                int score = 0;
                if(!board.IsDraw()){
                    score = factor*Score(board);
                }
                board.UndoMove(moveToPlay);
                foreach (Move move in allMoves)
                {
                    if (MoveIsCheckmate(board, move))
                    {
                        return move;
                    }
                    board.MakeMove(move);
                    if(!board.IsDraw()){
                        if(Score(board)*factor>score){
                            moveToPlay = move;
                            score = Score(board)*factor;
                        }
                    }
                    else{
                        if(0>score){
                            moveToPlay = move;
                            score = 0;
                        }
                    }
                    board.UndoMove(move);
                }
                return moveToPlay;
            }
            else{
                int score = -20000;
                foreach (Move move in allMoves)
                {
                    if (MoveIsCheckmate(board, move)||timer.MillisecondsRemaining<1000)
                    {
                        return move;
                    }
                    board.MakeMove(move);
                    if(!board.IsDraw()){
                        Move response = Depth(board, timer, depth-1);
                        if (MoveIsCheckmate(board, response))
                        {
                            if(-10000 > score){
                                moveToPlay = move;
                                score = -10000;
                            }
                        }
                        board.MakeMove(response);
                        if(!board.IsDraw()){
                            if(Score(board)*factor>score){
                                moveToPlay = move;
                                score = Score(board)*factor;
                            }
                        }
                        else{
                            if(0>score){
                                moveToPlay = move;
                                score = 0;
                            }
                        }
                        board.UndoMove(response);
                    }
                    else{
                        if(0>score){
                                moveToPlay = move;
                                score = 0;
                            }
                    }
                    board.UndoMove(move);
                }
                return moveToPlay;

            }
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }
        int Score(Board board)
        {
            int factor = -1;
            if(board.IsWhiteToMove){
                factor = 1;
            }
            if(board.IsInCheckmate()){
                return -10000*factor;
            }
            if(board.IsDraw()){
                return 0;
            }
            int score = 0;
            PieceList[] pieces = board.GetAllPieceLists();
            for(int i=0; i<pieces.Length; i++){
                if(pieces[i].IsWhitePieceList){
                    score += pieces[i].Count*pieceValues[(int)pieces[i].TypeOfPieceInList];
                }
                else{
                    score -= pieces[i].Count*pieceValues[(int)pieces[i].TypeOfPieceInList];
                }
            }
            score += 2*board.GetLegalMoves().Length*factor;
            Move[] caps = board.GetLegalMoves(true);
            foreach(Move cap in caps){
                double diff = pieceValues[(int)cap.CapturePieceType]-pieceValues[(int)cap.MovePieceType];
                score += (int) (25*factor*(Math.Sqrt(Math.Pow(diff/100,2)+4)+diff/100));
            }
            Move[] allMoves = board.GetLegalMoves();
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
            board.MakeMove(moveToPlay);
            if(!board.IsInCheckmate()){
                score -= (2*board.GetLegalMoves().Length+15*board.GetLegalMoves(true).Length)*factor;
            }
            board.UndoMove(moveToPlay);
            return score;
        }
    }
}