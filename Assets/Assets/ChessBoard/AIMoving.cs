using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
public class AIMoving : CloneMonoBehaviour
{
    [SerializeField] protected ChessPieceMoving chessPieceMoving;
    public int score;

    protected override void Update()
    {
        base.Update();
        if(!chessPieceMoving.IsWhiteTurn){
            this.MakeAIMove();
        }
    }
    protected virtual void MakeAIMove()
    {
        // Thực hiện nước đi tốt nhất trên bảng cờ
        ChessPiece[,] board = CopyBoard(BoardManager.instance.Pieces);
        int depth = 3;
        Tuple<int, ChessPiece[,]> eval = MiniMax(board, depth, int.MinValue, int.MaxValue, false);
        PerformBestMove(eval.Item2);
        score = eval.Item1;
    }
protected virtual void PerformBestMove(ChessPiece[,] bestMove)
{
    Vector3Int startPos = -Vector3Int.one;
    Vector3Int endPos = -Vector3Int.one;
    if (chessPieceMoving.hasMovedPiece) return;
    Debug.Log(score.ToString());
    foreach(ChessPiece piece in BoardManager.instance.Pieces){
            if(piece != null){
                if(piece.team == TeamType.Black){
                    chessPieceMoving.GetMovableTilesForPiece(piece, Vector3Int.RoundToInt(piece.transform.position));
                        if (chessPieceMoving.CanCapture) {
                            startPos = Vector3Int.RoundToInt(chessPieceMoving.SelectedPiece.transform.position);
                            endPos = chessPieceMoving.capturePos;
                            if(chessPieceMoving.SelectedPiece.team == TeamType.Black){
                                chessPieceMoving.MovePiece(chessPieceMoving.SelectedPiece, startPos, endPos);
                                return;
                            }
                        } 
                }
            }
    }
    
    for (int x = 0; x < BoardManager.instance.TILE_COUNT_X; x++)
    {
        for (int y = 0; y < BoardManager.instance.TILE_COUNT_Y; y++)
        {
            if (BoardManager.instance.Pieces[x, y] == null && bestMove[x, y] != null)
            {
                endPos = new Vector3Int(x, 1, y);
            }
            else if (BoardManager.instance.Pieces[x, y] != null && bestMove[x, y] == null)
            {
                startPos = new Vector3Int(x, 1, y);
            }
        }
    }

    //Thực hiện nước đi tốt nhất
    if (startPos == -Vector3Int.one && endPos == -Vector3Int.one) return;
    chessPieceMoving.SelectedPiece = chessPieceMoving.GetChessPiece(startPos.x, startPos.z);
    if(chessPieceMoving.SelectedPiece.team == TeamType.Black){
        chessPieceMoving.MovePiece(chessPieceMoving.SelectedPiece, startPos, endPos);
    }
}
 // Hàm minimax để tìm nước đi tối ưu
protected virtual Tuple<int, ChessPiece[,]> MiniMax(ChessPiece[,] board, int depth, int alpha, int beta, bool maximizingPlayer)
{
    if (depth == 0 || IsTerminalNode(board))
    {
        return Tuple.Create(Evaluate(board), board);
    }

    Tuple<int, ChessPiece[,]> bestMove = null;

    if (maximizingPlayer)
    {
        int maxEval = int.MinValue;
        foreach (ChessPiece[,] childBoard in GetChildNodes(board))
        {
            Tuple<int, ChessPiece[,]> evalAndMove = MiniMax(childBoard, depth - 1, alpha, beta, false);
            int eval = evalAndMove.Item1;
            maxEval = Mathf.Max(maxEval, eval);
            alpha = Mathf.Max(alpha, eval);
            if (beta <= alpha)
                break;
            // Update best move if a better move is found
            if (maxEval == eval)
                bestMove = Tuple.Create(maxEval, childBoard);
        }
        return bestMove != null ? bestMove : Tuple.Create(maxEval, board); // Return the best move found
    }
    else
    {
        int minEval = int.MaxValue;
        foreach (ChessPiece[,] childBoard in GetChildNodes(board))
        {
            Tuple<int, ChessPiece[,]> evalAndMove = MiniMax(childBoard, depth - 1, alpha, beta, true);
            int eval = evalAndMove.Item1;
            minEval = Mathf.Min(minEval, eval);
            beta = Mathf.Min(beta, eval);
            if (beta <= alpha)
                break;

            // Update best move if a better move is found
            if (minEval == eval)
                bestMove = Tuple.Create(minEval, childBoard);
            
        }
        return bestMove != null ? bestMove : Tuple.Create(minEval, board); // Return the best move found
    }
}

protected virtual List<ChessPiece[,]> GetChildNodes(ChessPiece[,] board)
{
    List<ChessPiece[,]> childNodes = new List<ChessPiece[,]>();

    // Lặp qua từng ô trên bảng cờ
    for (int x = 0; x < BoardManager.instance.TILE_COUNT_X; x++)
    {
        for (int y = 0; y < BoardManager.instance.TILE_COUNT_Y; y++)
        {
            ChessPiece piece = board[x, y];
            // Kiểm tra xem quân cờ thuộc đội của máy không
            if (piece != null && piece.team == TeamType.Black)
            {
                // Tạo các nước đi có thể từ quân cờ hiện tại
                List<ChessPiece[,]> possibleMoves = GeneratePossibleMoves(CopyBoard(board), piece, x, y);
                // Thêm các nước đi có thể vào danh sách các trạng thái con
                childNodes.AddRange(possibleMoves);
            }
        }
    }
    return childNodes;
}

protected virtual List<ChessPiece[,]> GeneratePossibleMoves(ChessPiece[,] board, ChessPiece piece, int x, int y)
{
    // Logic để tạo ra các nước đi có thể từ quân cờ hiện tại
    Vector3Int piecePosition = new Vector3Int(x, 1, y);
    
    // Tính toán vị trí các ô xung quanh
    List<Vector3Int> adjacentPositionsForMen = new List<Vector3Int>()
    {
        new Vector3Int(-1, 0, 1), // Top left
        new Vector3Int(1, 0, 1),  // Top right
    };
    List<Vector3Int> adjacentPositionsForKing = new List<Vector3Int>()
    {
        adjacentPositionsForMen[0],
        adjacentPositionsForMen[1],
        new Vector3Int(-1, 0, -1), // Bottom left
        new Vector3Int(1, 0, -1)   // Bottom right
    };
    if(piece.chessPieceType == ChessPieceType.Men){
        return GetCanMoveAllOfPieceList(adjacentPositionsForMen, piecePosition, board, piece, x, y);
    } else if(piece.chessPieceType == ChessPieceType.King){
        return GetCanMoveAllOfPieceList(adjacentPositionsForKing, piecePosition, board, piece, x, y);
    }

    return null;
}
    protected virtual List<ChessPiece[,]> GetCanMoveAllOfPieceList(List<Vector3Int> adjacentPositions, Vector3Int piecePosition, ChessPiece[,] board, ChessPiece piece, int x, int y){
        List<ChessPiece[,]> possibleMoves = new List<ChessPiece[,]>();
        foreach (Vector3Int direction in adjacentPositions){
            Vector3Int newPos = piecePosition + direction;
            // Kiểm tra xem ô mới có trên bảng không
            if (chessPieceMoving.CheckOutBoard(newPos.x, newPos.z)){
            // Kiểm tra xem ô mới có trống không
                if (chessPieceMoving.IsEmptyTile(board,newPos.x, newPos.z)){
                    possibleMoves.Add(GetChessPiece(board, newPos, piece, x, y));
                }   
                else {
                // Kiểm tra xem quân cờ trên ô mới có thuộc đối phương không
                    var newCapturePos = chessPieceMoving.CheckCaptureFromPosition(board,piece, newPos, true);
                    if (newCapturePos != -Vector3Int.one){
                        possibleMoves.Add(GetChessPiece(board, newPos, piece, x, y));
                    }
                }
            }
        }
        return possibleMoves;
    }
    protected virtual ChessPiece[,] GetChessPiece(ChessPiece[,] copyBoard ,Vector3Int newPos, ChessPiece piece, int x , int y){
        if(newPos != -Vector3Int.one && copyBoard[newPos.x, newPos.z] == null){

            if(chessPieceMoving.CanCapturePiece(copyBoard,piece,newPos.x,newPos.z, x, y).Item2 != -Vector3.one){
                ChessPiece despawnPiece = chessPieceMoving.CanCapturePiece(copyBoard,piece,newPos.x,newPos.z, x, y).Item1;
                Vector3Int despawnPiecePos = chessPieceMoving.CanCapturePiece(copyBoard,piece,newPos.x,newPos.z, x, y).Item2;
                chessPieceMoving.DespawnChessPiece(copyBoard, despawnPiece, despawnPiecePos, true);
                var newCapturePos = chessPieceMoving.CheckCaptureFromPosition(copyBoard,piece, newPos, true);
                copyBoard[newCapturePos.x, newCapturePos.z] = copyBoard[x,y];
                copyBoard[x,y] = null;
                return copyBoard;
            }
            copyBoard[newPos.x, newPos.z] = copyBoard[x,y];
            copyBoard[x,y] = null;
            return copyBoard;
        }
        return copyBoard;
    }

protected virtual bool IsTerminalNode(ChessPiece[,] board)
{
    // Finish Game
    foreach (ChessPiece piece in board)
    {
        if (piece != null && piece.team == TeamType.White)
        {
            return false; 
        }
    }
    return true;
}

protected virtual int Evaluate(ChessPiece[,] board)
{
int score = 0;

    // Đánh giá số quân cờ của mỗi đội
    int blackPieces = 0;
    int whitePieces = 0;

    for (int i = 0; i < BoardManager.instance.TILE_COUNT_X; i++)
    {
        for (int j = 0; j < BoardManager.instance.TILE_COUNT_Y; j++)
        {
            ChessPiece currentPiece = board[i, j];
            if (currentPiece != null)
            {
                if (currentPiece.team == TeamType.Black)
                {
                    blackPieces++;
                }
                else if (currentPiece.team == TeamType.White)
                {
                    whitePieces++;
                }

                // Đánh giá điểm số của từng quân cờ
                int pieceValue = 0;
                if (currentPiece.chessPieceType == ChessPieceType.Men)
                {
                    pieceValue = 10; // Điểm số cho quân Men
                }
                else if (currentPiece.chessPieceType == ChessPieceType.King)
                {
                    pieceValue = 50; // Điểm số cho quân King
                }

                // Áp dụng điểm số theo màu quân cờ
                if (currentPiece.team == TeamType.Black)
                {
                    score += pieceValue;
                }
                else if (currentPiece.team == TeamType.White)
                {
                    score -= pieceValue;
                }
            }
        }
    }

    // Đánh giá số lượng quân cờ của mỗi đội
    int pieceDifference = blackPieces - whitePieces;

    // Đánh giá số điểm dựa trên sự chênh lệch về số lượng quân cờ
    score += pieceDifference * 5;

    return score;
}
 protected virtual ChessPiece[,] CopyBoard(ChessPiece[,] board)
{
    ChessPiece[,] copy = new ChessPiece[BoardManager.instance.TILE_COUNT_X, BoardManager.instance.TILE_COUNT_Y];
    for (int i = 0; i < BoardManager.instance.TILE_COUNT_X; i++)
    {
        for (int j = 0; j < BoardManager.instance.TILE_COUNT_Y; j++)
        {
            // Tạo một bản sao của từng quân cờ và thêm vào bảng mới
            if (board[i, j] != null)
            {
                copy[i, j] = board[i, j];
            }
        }
    }
    return copy;    
}
}

