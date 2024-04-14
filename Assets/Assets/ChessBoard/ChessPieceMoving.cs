using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChessPieceMoving : CloneMonoBehaviour
{
    [SerializeField] protected MaterialManager materialManager;
    [SerializeField] protected Mesh mesh_King;
    [SerializeField] protected UIManager uIManager;
    protected ChessPiece selectedPiece;
    public ChessPiece SelectedPiece{
        get{ return selectedPiece; }
        set{ selectedPiece = value; }
    }
    protected Vector3Int startDrag;
    protected Vector3Int endDrag;
    private ChessPiece p;
    private GameObject tile;
    protected bool canCapture = false;
    public bool CanCapture
    {
        get { return canCapture; }
        set { canCapture = value; }
    }
    private List<GameObject> movableTiles = new List<GameObject>();
    protected bool isWhiteTurn = true;
    public bool IsWhiteTurn
    {
        get { return isWhiteTurn; }
        set { isWhiteTurn = value; }
    }
    protected ChessPiece chessPieceDespawn = null;
    private Camera currentCamera;
    [SerializeField] protected GameObject black_SpawnPos;
    [SerializeField] protected GameObject white_SpawnPos;
    protected bool turnLogDisplayed = false;
    public bool TurnLogDisplayed
    {
        get { return turnLogDisplayed; }
        set { turnLogDisplayed = value; }
    }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(0))
        {
            this.MovePiece(BoardManager.instance.MouseOver);
            this.SelectPiece(BoardManager.instance.MouseOver);
        }
        if (!turnLogDisplayed)
        {
            if (isWhiteTurn)
            {
                uIManager.SetText("White Turn!");
            }
            else
            {
                uIManager.SetText("Black Turn!");
            }
            this.CheckCapture();
            turnLogDisplayed = true;
        }

    }
    protected virtual void SelectPiece(Vector3Int _mousePos)
    {
        this.ResetTiles();
        if (!this.CheckOutBoard(_mousePos.x, _mousePos.z)) return;
        this.GetPiece(_mousePos);
    }

    public virtual void ResetTiles()
    {
        foreach (GameObject tile in BoardManager.instance.Tiles)
        {
            MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
            if (tile.layer == LayerMask.NameToLayer("Hover"))
            {
                renderer.material = materialManager.Black_Material;
                tile.layer = LayerMask.NameToLayer("Tile");
            }
        }
        this.ClearMovableTiles();
    }

    protected virtual void GetPiece(Vector3Int _startPos)
    {
        // p = BoardManager.instance.Pieces[_startPos.x, _startPos.z];
        p = this.GetChessPiece(_startPos.x, _startPos.z);
        if (p != null)
        {
            if ((isWhiteTurn && p.team == TeamType.White) || (!isWhiteTurn && p.team == TeamType.Black))
            {
                this.selectedPiece = p;
                this.startDrag = _startPos;
                tile = BoardManager.instance.Tiles[_startPos.x, _startPos.z];
                Renderer renderer = tile.GetComponent<MeshRenderer>();
                renderer.material = materialManager.HoverTile;
                tile.layer = LayerMask.NameToLayer("Hover");
                this.ClearMovableTiles();
                this.GetMovableTilesForPiece(p, _startPos);
            }
            else
            {
                this.startDrag = -Vector3Int.one;
                return;
            }
        }
        else
        {
            this.startDrag = -Vector3Int.one;
            return;
        }
    }
    public virtual void GetMovableTilesForPiece(ChessPiece chessPiece, Vector3Int startPos)
    {
        int xRight = (int)startPos.x + 1;
        int xLeft = (int)startPos.x - 1;
        int y = 0;
        if (chessPiece.chessPieceType == ChessPieceType.Men)
        {
            switch (chessPiece.team)
            {
                case TeamType.Black:
                    y = (int)startPos.z + 1;
                    break;
                case TeamType.White:
                    y = (int)startPos.z - 1;
                    break;
            }
            this.CheckMovableTile(xRight, xLeft, y);

        }
        else if (chessPiece.chessPieceType == ChessPieceType.King)
        {
            y = (int)startPos.z - 1;
            this.CheckMovableTile(xRight, xLeft, y);

            y = (int)startPos.z + 1;
            this.CheckMovableTile(xRight, xLeft, y);
        }
    }

    protected virtual void CheckMovableTile(int xRight, int xLeft, int y)
    {
        if (this.CanCapturePiece(xRight, y) || this.CanCapturePiece(xLeft, y))
        {
            this.canCapture = true;
        }
        this.AddMovableTile(xRight, y);
        this.AddMovableTile(xLeft, y);
    }
    
    public Vector3Int capturePos;
    protected virtual void AddMovableTile(int x, int y)
    {
        if (this.CanCapturePiece(x, y))
        {
            this.canCapture = true;
            int nextX = x + (x - Mathf.RoundToInt(selectedPiece.transform.position.x));
            int nextY = y + (y - Mathf.RoundToInt(selectedPiece.transform.position.z));
            this.ChangeMovableTile(nextX, nextY);
            capturePos = new Vector3Int(nextX, 1, nextY);
            return;
        }
        if (this.IsEmptyTile(x, y) && !canCapture)
        {
            this.ChangeMovableTile(x, y);
        }
    }

    protected virtual void ChangeMovableTile(int x, int y)
    {
        GameObject tile = BoardManager.instance.Tiles[x, y];
        movableTiles.Add(tile);
        Renderer renderer = tile.GetComponent<Renderer>();
        renderer.material = materialManager.HoverTile;
        tile.layer = LayerMask.NameToLayer("Movable");
    }

    public virtual bool IsEmptyTile(int x, int y)
    {
        if (this.CheckOutBoard(x, y))
        {
            //return BoardManager.instance.Pieces[x,y] = null;
            return this.GetChessPiece(x, y) == null;
        }
        return false;
    }

    public virtual bool CanCapturePiece(int x, int y)
    {
        if (this.CheckOutBoard(x, y))
        {
            // ChessPiece piece = BoardManager.instance.Pieces[x, y];
            ChessPiece piece = this.GetChessPiece(x, y);
            if (piece != null && piece.team != selectedPiece.team)
            {
                // Kiểm tra ô chéo tiếp theo của quân cờ
                int nextX = x + (x - Mathf.RoundToInt(selectedPiece.transform.position.x));
                int nextY = y + (y - Mathf.RoundToInt(selectedPiece.transform.position.z));
                if (this.CheckOutBoard(nextX, nextY))
                {
                    ChessPiece nextPiece = this.GetChessPiece(nextX, nextY);
                    if (nextPiece == null)
                    {
                        this.chessPieceDespawn = piece;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public virtual Tuple<ChessPiece, Vector3Int> CanCapturePiece(ChessPiece[,] board, ChessPiece currentPiece, int x, int y, int currentX, int currentY)
    {
        ChessPiece chessPiece = null;
        if (this.CheckOutBoard(x, y))
        {
            ChessPiece piece = board[x,y];
            if (piece != null && piece.team != currentPiece.team)
            {
                // Kiểm tra ô chéo tiếp theo của quân cờ
                int nextX = x + (x - Mathf.RoundToInt(currentX));
                int nextY = y + (y - Mathf.RoundToInt(currentY));
                if (this.CheckOutBoard(nextX, nextY))
                {
                    ChessPiece nextPiece = board[nextX, nextY];
                    if (nextPiece == null)
                    {
                        return Tuple.Create(piece, new Vector3Int(x, 1, y));
                    }
                }
            }
        }
        return Tuple.Create(chessPiece, -Vector3Int.one);
    }

    protected virtual void ClearMovableTiles()
    {
        foreach (var tile in movableTiles)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            renderer.material = materialManager.Black_Material;
            tile.layer = LayerMask.NameToLayer("Tile");
        }
        movableTiles.Clear();
    }
    // Move Piece
    protected virtual void MovePiece(Vector3Int start)
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Movable")))
        {
            Vector3Int hitPos = BoardManager.instance.LookupTileIndex(info.transform.gameObject);
            hitPos.y = 1;
            this.MovePiece(this.selectedPiece, startDrag, hitPos);
        }
    }
    protected virtual void CheckCapture()
    {
        for (int x = 0; x < BoardManager.instance.TILE_COUNT_X; x++)
        {
            for (int y = 0; y < BoardManager.instance.TILE_COUNT_Y; y++)
            {
                ChessPiece currentPiece = GetChessPiece(x, y);
                // Nếu quân cờ hiện tại thuộc đội của bên đang có lượt chơi
                if (currentPiece != null && currentPiece.team == (isWhiteTurn ? TeamType.White : TeamType.Black))
                {
                    this.CheckPieceCapture(currentPiece);
                }
            }
        }
    }
    public virtual void CheckPieceCapture(ChessPiece currentPiece)
    {
        Vector3Int piecePosition = Vector3Int.RoundToInt(currentPiece.transform.position);

        Vector3Int topLeftPosition = piecePosition + new Vector3Int(-1, 0, 1);
        Vector3Int topRightPosition = piecePosition + new Vector3Int(1, 0, 1);
        Vector3Int bottomLeftPosition = piecePosition + new Vector3Int(-1, 0, -1);
        Vector3Int bottomRightPosition = piecePosition + new Vector3Int(1, 0, -1);
        if (currentPiece.chessPieceType == ChessPieceType.King)
        {
            this.CheckCaptureFromPosition(currentPiece, topLeftPosition);
            this.CheckCaptureFromPosition(currentPiece, topRightPosition);
            this.CheckCaptureFromPosition(currentPiece, bottomLeftPosition);
            this.CheckCaptureFromPosition(currentPiece, bottomRightPosition);
            return;
        }
        switch (currentPiece.team)
        {
            case TeamType.Black:
                this.CheckCaptureFromPosition(currentPiece, topLeftPosition);
                this.CheckCaptureFromPosition(currentPiece, topRightPosition);
                break;
            case TeamType.White:
                this.CheckCaptureFromPosition(currentPiece, bottomLeftPosition);
                this.CheckCaptureFromPosition(currentPiece, bottomRightPosition);
                break;
        }
    }
    public virtual Vector3Int CheckCaptureFromPosition(ChessPiece currentPiece, Vector3Int targetPos)
    {
        if (this.CheckOutBoard(targetPos))
        {
            ChessPiece targetPiece = this.GetChessPiece(targetPos.x, targetPos.z);
            if (targetPiece != null && targetPiece.team != currentPiece.team)
            {
                Vector3Int nextPos = targetPos + (targetPos - Vector3Int.RoundToInt(currentPiece.transform.position));
                if (this.CheckOutBoard(nextPos) && this.IsEmptyTile(nextPos.x, nextPos.z))
                {
                    this.SelectPiece(Vector3Int.RoundToInt(currentPiece.gameObject.transform.position));
                    return  Vector3Int.RoundToInt(currentPiece.gameObject.transform.position);
                }
            } else return new Vector3Int(-1,-1,-1);
        }
        return new Vector3Int(-1,-1,-1);
    }
    protected virtual void BecomeAKingType(Vector3Int _endPos)
    {
        if (_endPos == null || selectedPiece == null) return;
        if (this.selectedPiece.team == TeamType.Black && _endPos.z == BoardManager.instance.TILE_COUNT_Y - 1)
        {
            this.BecomeAKingType(this.selectedPiece);
        }
        else if (this.selectedPiece.team == TeamType.White && _endPos.z == 0)
        {
            this.BecomeAKingType(this.selectedPiece);
        }
    }
    protected virtual void BecomeAKingType(ChessPiece chessPiece)
    {
        MeshCollider meshCollider = chessPiece.GetComponent<MeshCollider>();
        MeshFilter mesh = chessPiece.GetComponent<MeshFilter>();
        meshCollider.sharedMesh = mesh_King;
        mesh.mesh = mesh_King;
        chessPiece.chessPieceType = ChessPieceType.King;
    }
    public bool hasMovedPiece = false;
    public virtual void MovePiece(ChessPiece _selectedPiece,Vector3Int _start, Vector3Int _end)
    {
        hasMovedPiece = true;
        this.BecomeAKingType(_end);
        if (_selectedPiece == null) return;
        int x1 = _start.x;
        int y1 = _start.z;
        int x2 = _end.x;
        int y2 = _end.z;
        startDrag = _start;
        endDrag = _end;
        // this.SetChessPiece(null, x1, y1);
        // this.SetChessPiece(_selectedPiece, x2, y2);
        // _selectedPiece.transform.position = this.endDrag;
        // isWhiteTurn = !isWhiteTurn;
        // if(chessPieceDespawn != null){
        //     this.DespawnChessPiece(BoardManager.instance.Pieces, chessPieceDespawn, chessPieceDespawn.gameObject.transform.position);
        // }
        // uIManager.CheckWinCondition();
        // turnLogDisplayed = false;
        // ResetTiles();
        StartCoroutine(MoveToTargetThenDoSomething(_selectedPiece.transform,endDrag, _selectedPiece,x1,x2,y1,y2));
    }
    IEnumerator MoveToTargetThenDoSomething(Transform targetTransform, Vector3Int targetPosition,ChessPiece _selectedPiece, int x1 , int x2, int y1, int y2)
    {
        yield return StartCoroutine(MoveToTarget(targetTransform, targetPosition));
        this.SetChessPiece(null, x1, y1);
        this.SetChessPiece(_selectedPiece, x2, y2);
        if(chessPieceDespawn != null){
            this.DespawnChessPiece(BoardManager.instance.Pieces, chessPieceDespawn, chessPieceDespawn.gameObject.transform.position);
        }
        isWhiteTurn = !isWhiteTurn;
        uIManager.CheckWinCondition();
        turnLogDisplayed = false;
        ResetTiles();
        hasMovedPiece = false;
    }
    IEnumerator MoveToTarget(Transform targetTransform, Vector3Int targetPosition)
    {
        while (Vector3.Distance(targetTransform.position, targetPosition) > 0.01f)
        {
            targetTransform.position = Vector3.Lerp(targetTransform.position, targetPosition, 10f * Time.deltaTime);

            yield return null;
        }
        targetTransform.position = targetPosition;
    }
    public virtual void DespawnChessPiece(ChessPiece[,] board, ChessPiece _chessPieceDespawn, Vector3 _chessPieceDespawnPos)
    {
        if (canCapture){
            Vector3 chessPieceCapture = _chessPieceDespawnPos;
            int x = (int)chessPieceCapture.x;
            int y = (int)chessPieceCapture.z;
            //BoardManager.instance.Pieces[x,y] = null;
            board[x,y] = null;
            SpawnChessPieceCaptured(board,_chessPieceDespawn);
            this.canCapture = false;
            if (CanCapturePiece(this.selectedPiece)){
                isWhiteTurn = !isWhiteTurn;
            }
        }
    }
    protected virtual bool CheckMovableTileBool(int xRight, int xLeft, int y)
    {
        if (this.CanCapturePiece(xRight, y) || this.CanCapturePiece(xLeft, y))
        {
            this.canCapture = true;
            return true;
        }
        this.AddMovableTile(xRight, y);
        this.AddMovableTile(xLeft, y);
        return false;
    }
    protected virtual bool CanCapturePiece(ChessPiece currentPiece)
    {
        Vector3 startPos = currentPiece.gameObject.transform.position;
        int xRight = (int)startPos.x + 1;
        int xLeft = (int)startPos.x - 1;
        int y = 0;
        if (currentPiece.chessPieceType == ChessPieceType.Men)
        {
            switch (currentPiece.team)
            {
                case TeamType.Black:
                    y = (int)startPos.z + 1;
                    break;
                case TeamType.White:
                    y = (int)startPos.z - 1;
                    break;
            }
            if (this.CheckMovableTileBool(xRight, xLeft, y)) return true;

        }
        else if (currentPiece.chessPieceType == ChessPieceType.King)
        {
            y = (int)startPos.z - 1;
            if (this.CheckMovableTileBool(xRight, xLeft, y)) return true;

            y = (int)startPos.z + 1;
            if (this.CheckMovableTileBool(xRight, xLeft, y)) return true;
        }
        return false;
    }

    public Vector3 newPos = new Vector3();
    public virtual void SpawnChessPieceCaptured(ChessPiece[,] board , ChessPiece chessPiece)
    {
        Vector3 spawnPosition = Vector3.zero;
        if (chessPiece.team == TeamType.White)
        {
            spawnPosition = white_SpawnPos.transform.position + newPos;
        }
        else if (chessPiece.team == TeamType.Black)
        {
            spawnPosition = black_SpawnPos.transform.position + newPos;
        }

        Collider[] colliders = Physics.OverlapSphere(spawnPosition, 0.1f);
        int numberOfPiecesAtPosition = 0;

        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.GetComponent<ChessPiece>() != null && collider.GetComponent<ChessPiece>() != chessPiece)
                {
                    numberOfPiecesAtPosition++;
                }
            }
        }
        newPos += new Vector3(numberOfPiecesAtPosition * 0.6f, 0f, 0f);
        spawnPosition += new Vector3(numberOfPiecesAtPosition * 0.6f, 0f, 0f);

        float x = chessPiece.gameObject.transform.position.x;
        float y = chessPiece.gameObject.transform.position.z;
        board[(int)x, (int)y] = null;
        chessPiece.gameObject.transform.position = spawnPosition;
    }

    public virtual bool CheckOutBoard(int x, int y)
    {
        return x >= 0 && x < BoardManager.instance.TILE_COUNT_X
        && y >= 0 && y < BoardManager.instance.TILE_COUNT_Y;
    }
    public virtual bool CheckOutBoard(Vector3Int vector3Int)
    {
        return vector3Int.x >= 0 && vector3Int.x < BoardManager.instance.TILE_COUNT_X
        && vector3Int.z >= 0 && vector3Int.z < BoardManager.instance.TILE_COUNT_Y;
    }
    public virtual ChessPiece GetChessPiece(int x, int y)
    {
        return BoardManager.instance.Pieces[x, y];
    }
    public virtual void SetChessPiece(ChessPiece newChess, int x, int y)
    {
        BoardManager.instance.Pieces[x, y] = newChess;
    }
}