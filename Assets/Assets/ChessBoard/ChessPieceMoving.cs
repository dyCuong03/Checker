using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ChessPieceMoving : CloneMonoBehaviour
{
    [SerializeField] protected MaterialManager materialManager;
    [SerializeField] protected Mesh mesh_King;
    protected ChessPiece selectedPiece;
    protected Vector3Int startDrag;
    protected Vector3Int endDrag;
    private ChessPiece p;
    private GameObject tile;
    protected bool canCapture = false;
    private List<GameObject> movableTiles = new List<GameObject>();
    protected bool isWhiteTurn = true;
    protected ChessPiece chessPieceDespawn;
    [SerializeField] protected GameObject black_SpawnPos;
    [SerializeField] protected GameObject white_SpawnPos;
    private bool turnLogDisplayed = false;
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Update()
    {
        base.Update();
        if(Input.GetMouseButtonDown(0)){
            this.MovePiece(BoardManager.instance.MouseOver);
            this.SelectPiece(BoardManager.instance.MouseOver);
        }
        if (!turnLogDisplayed)
        {
            if (isWhiteTurn)
            {
            Debug.Log("Lượt chơi của quân cờ Trắng.");
            }
            else
            {
            Debug.Log("Lượt chơi của quân cờ Đen.");
            }

            turnLogDisplayed = true;
        }

    }

    protected virtual void SelectPiece(Vector3Int _mousePos)
    {
        this.ResetTiles();
        if(!this.CheckOutBoard(_mousePos.x, _mousePos.z)) return;
        this.GetPiece(_mousePos);
    }

    protected virtual void ResetTiles(){
        foreach(GameObject tile in BoardManager.instance.Tiles){
            MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
            if(tile.layer == LayerMask.NameToLayer("Hover")){
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
    protected virtual void GetMovableTilesForPiece(ChessPiece chessPiece, Vector3Int startPos) {
            int xRight = (int)startPos.x + 1;
            int xLeft = (int)startPos.x - 1;
            int y = 0;
        if(chessPiece.chessPieceType == ChessPieceType.Men){
            switch (chessPiece.team){
                case TeamType.Black:
                    y = (int)startPos.z + 1;
                    break;
                case TeamType.White:
                    y = (int)startPos.z - 1;
                    break;
            }
                this.CheckMovableTile(xRight, xLeft,y);

        } else if(chessPiece.chessPieceType == ChessPieceType.King){
                y = (int)startPos.z - 1;
                this.CheckMovableTile(xRight, xLeft,y);

                y = (int)startPos.z + 1;
                this.CheckMovableTile(xRight, xLeft,y);
        }
    }

    protected virtual void CheckMovableTile(int xRight, int xLeft, int y){
        if(this.CanCapturePiece(xRight,y) || this.CanCapturePiece(xLeft,y)){
            this.canCapture = true;
        }
        this.AddMovableTile(xRight, y);
        this.AddMovableTile(xLeft, y);
    }

    protected virtual void AddMovableTile(int x, int y) {
        if(this.CanCapturePiece(x,y)){
            this.canCapture = true;
            int nextX = x + (x - Mathf.RoundToInt(selectedPiece.transform.position.x));
            int nextY = y + (y - Mathf.RoundToInt(selectedPiece.transform.position.z));
            this.ChangeMovableTile(nextX, nextY);
            return;
        }
        if (this.IsEmptyTile(x, y) && !canCapture) {
            this.ChangeMovableTile(x, y);
        }
    }
    
    protected virtual void ChangeMovableTile(int x, int y){
        GameObject tile = BoardManager.instance.Tiles[x, y];
        movableTiles.Add(tile);
        Renderer renderer = tile.GetComponent<Renderer>();
        renderer.material = materialManager.HoverTile;
        tile.layer = LayerMask.NameToLayer("Movable");
    }

    protected virtual bool IsEmptyTile(int x, int y) {
        if (this.CheckOutBoard(x,y)) {
            //return BoardManager.instance.Pieces[x,y] = null;
            return this.GetChessPiece(x,y) == null;
        }
        return false;
    }

    protected virtual bool CanCapturePiece(int x, int y) {
        if (this.CheckOutBoard(x,y)) {
            // ChessPiece piece = BoardManager.instance.Pieces[x, y];
            ChessPiece piece = this.GetChessPiece(x,y);
            if (piece != null && piece.team != selectedPiece.team) {
                // Kiểm tra ô chéo tiếp theo của quân cờ
                int nextX = x + (x - Mathf.RoundToInt(selectedPiece.transform.position.x));
                int nextY = y + (y - Mathf.RoundToInt(selectedPiece.transform.position.z));
                if (this.CheckOutBoard(nextX,nextY)) {
                    ChessPiece nextPiece = BoardManager.instance.Pieces[nextX, nextY];
                    if (nextPiece == null) {
                        this.chessPieceDespawn = piece;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    protected virtual void ClearMovableTiles() {
        foreach (var tile in movableTiles) {
            Renderer renderer = tile.GetComponent<Renderer>();
            renderer.material = materialManager.Black_Material;
            tile.layer = LayerMask.NameToLayer("Tile");
        }
        movableTiles.Clear();
    }
    // Move Piece
    private Camera currentCamera;
    protected virtual void MovePiece(Vector3Int start)
    {
        if(!currentCamera){
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Movable"))){
            Vector3Int hitPos = BoardManager.instance.LookupTileIndex(info.transform.gameObject);
            hitPos.y = 1;
            this.MovePiece(startDrag, hitPos);
            this.BecomeAKingType(hitPos);
        }
    }

    protected virtual void BecomeAKingType(Vector3Int _endPos){
        if(this.selectedPiece.team == TeamType.Black && _endPos.z == BoardManager.instance.TILE_COUNT_Y - 1){
            this.BecomeAKingType(this.selectedPiece);
        } else if(this.selectedPiece.team == TeamType.White && _endPos.z == 0){
            this.BecomeAKingType(this.selectedPiece);
        }
    }
    protected virtual void BecomeAKingType(ChessPiece chessPiece){
        MeshCollider meshCollider = chessPiece.GetComponent<MeshCollider>();
        MeshFilter mesh = chessPiece.GetComponent<MeshFilter>();
        meshCollider.sharedMesh = mesh_King;
        mesh.mesh = mesh_King;
        chessPiece.chessPieceType = ChessPieceType.King;
    }
    protected virtual void MovePiece(Vector3Int _start, Vector3Int _end)
    {
        if(selectedPiece == null) return;
        int x1 = _start.x;
        int y1 = _start.z;
        int x2 = _end.x;
        int y2 = _end.z;
        startDrag = _start;
        endDrag = _end;
        // BoardManager.instance.Pieces[x1,y1] = null;
        // BoardManager.instance.Pieces[x2,y2] = this.selectedPiece;
        this.SetChessPiece(null,x1,y1);
        this.SetChessPiece(this.selectedPiece,x2,y2);
        selectedPiece.transform.position = this.endDrag;
        isWhiteTurn = !isWhiteTurn;
        this.DespawnChessPiece();
        this.CheckCanPieceCapture();
        turnLogDisplayed = false;
    }
    protected virtual void CheckCanPieceCapture()
    {
        if (selectedPiece == null || this.startDrag == null) return;
    
        Vector3Int piecePosition = Vector3Int.RoundToInt(selectedPiece.transform.position);
        
        Vector3Int topLeftPosition = piecePosition + new Vector3Int(-1, 0, 1);
        Vector3Int topRightPosition = piecePosition + new Vector3Int(1, 0, 1);

        Vector3Int bottomLeftPosition = piecePosition + new Vector3Int(-1, 0, -1);
        Vector3Int bottomRightPosition = piecePosition + new Vector3Int(1, 0, -1);
        
        if(!CheckOutBoard(topLeftPosition) || !CheckOutBoard(topRightPosition) 
        || !CheckOutBoard(bottomLeftPosition) || !CheckOutBoard(bottomRightPosition)) return;
        switch (selectedPiece.team){
            case TeamType.Black:
                this.CheckCanPieceCapture(topLeftPosition,bottomRightPosition);
                this.CheckCanPieceCapture(topRightPosition,bottomLeftPosition);
                break;
            case TeamType.White:
                this.CheckCanPieceCapture(bottomRightPosition,topLeftPosition);
                this.CheckCanPieceCapture(bottomLeftPosition,topRightPosition);
                break;
            }
    }
    protected virtual void CheckCanPieceCapture(Vector3Int targetPos){
        if(this.GetChessPiece(targetPos.x, targetPos.z).team != this.selectedPiece.team){
            this.SelectPiece(targetPos);
        }
    }
    protected virtual void CheckCanPieceCapture(Vector3Int targetPos, Vector3Int emptyPos){
        if(!this.IsEmptyTile(targetPos.x, targetPos.z) 
            && this.IsEmptyTile(emptyPos.x, emptyPos.z)){
                this.CheckCanPieceCapture(targetPos);
            }
    }
    protected virtual void DespawnChessPiece(){
        if(canCapture){
            Vector3 chessPieceCapture = chessPieceDespawn.gameObject.transform.position;
            int x = (int)chessPieceCapture.x;
            int y = (int)chessPieceCapture.z;
            //BoardManager.instance.Pieces[x,y] = null;
            this.SetChessPiece(null,x,y);
            this.SpawnChessPieceCaptured(chessPieceDespawn);
            this.canCapture = false;
            if(CanCapturePiece(this.selectedPiece)) {
                isWhiteTurn = !isWhiteTurn;
            }
        }
    }
    protected virtual bool CheckMovableTileBool(int xRight, int xLeft, int y){
        if(this.CanCapturePiece(xRight,y) || this.CanCapturePiece(xLeft,y)){
            this.canCapture = true;
            return true;
        }
        this.AddMovableTile(xRight, y);
        this.AddMovableTile(xLeft, y);
        return false;
    }
    protected virtual bool CanCapturePiece(ChessPiece currentPiece) {
        Vector3 startPos = currentPiece.gameObject.transform.position;
            int xRight = (int)startPos.x + 1;
            int xLeft = (int)startPos.x - 1;
            int y = 0;
        if(currentPiece.chessPieceType == ChessPieceType.Men){
            switch (currentPiece.team){
                case TeamType.Black:
                    y = (int)startPos.z + 1;
                    break;
                case TeamType.White:
                    y = (int)startPos.z - 1;
                    break;
            }
            if(this.CheckMovableTileBool(xRight, xLeft,y)) return true;

        } else if(currentPiece.chessPieceType == ChessPieceType.King){
                y = (int)startPos.z - 1;
            if(this.CheckMovableTileBool(xRight, xLeft,y)) return true;
            
                y = (int)startPos.z + 1;
            if(this.CheckMovableTileBool(xRight, xLeft,y)) return true;
                
        }
        return false;
    }

    protected Vector3 newPos = new Vector3();
    protected virtual void SpawnChessPieceCaptured(ChessPiece chessPiece){
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
        chessPiece.gameObject.transform.position = spawnPosition;
    }

    protected virtual bool CheckOutBoard(int x, int y){
        return x >= 0 && x < BoardManager.instance.TILE_COUNT_X 
        && y >= 0 && y < BoardManager.instance.TILE_COUNT_Y;
    }
    protected virtual bool CheckOutBoard(Vector3Int vector3Int){
        return vector3Int.x >= 0 && vector3Int.x < BoardManager.instance.TILE_COUNT_X 
        && vector3Int.z >= 0 && vector3Int.z < BoardManager.instance.TILE_COUNT_Y;
    }
    protected virtual ChessPiece GetChessPiece(int x, int y){
        return BoardManager.instance.Pieces[x,y];
    }
    protected virtual void SetChessPiece(ChessPiece newChess,int x, int y){
        BoardManager.instance.Pieces[x,y] = newChess;
    }
}