using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ChessPieceMoving : CloneMonoBehaviour
{
    [SerializeField] protected MaterialManager materialManager;
    protected ChessPiece selectedPiece;
    protected Vector3Int startDrag;
    protected Vector3Int endDrag;
    private List<GameObject> movableTiles = new List<GameObject>();
    protected bool isWhiteTurn = true;
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
    private ChessPiece p;
    private GameObject tile;

    protected virtual void GetPiece(Vector3Int _startPos)
    {
        p = BoardManager.instance.Pieces[_startPos.x, _startPos.z];
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
        this.ClearMovableTiles();
            int xRight = (int)startPos.x + 1;
            int xLeft = (int)startPos.x - 1;
            int y = 0;
        switch (chessPiece.team){
            case TeamType.Black:
                y = (int)startPos.z + 1;
                break;
            case TeamType.White:
                y = (int)startPos.z - 1;
                break;
        }
        this.CheckAndAddMovableTile(xRight, y);
        this.CheckAndAddMovableTile(xLeft, y);
    }

    protected virtual void CheckAndAddMovableTile(int x, int y) {
        if(this.CanCapturePiece(x,y)){
            int nextX = x + (x - Mathf.RoundToInt(selectedPiece.transform.position.x));
            int nextY = y + (y - Mathf.RoundToInt(selectedPiece.transform.position.z));
            this.ChangeMovableTile(nextX, nextY);
            return;
        }
        if (this.IsEmptyTile(x, y)) {
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
            return BoardManager.instance.Pieces[x, y] == null;
        }
        return false;
    }

    protected virtual bool CanCapturePiece(int x, int y) {
        if (this.CheckOutBoard(x,y)) {
            ChessPiece piece = BoardManager.instance.Pieces[x, y];
            if (piece != null && piece.team != selectedPiece.team) {
                // Kiểm tra ô chéo tiếp theo của quân cờ
                int nextX = x + (x - Mathf.RoundToInt(selectedPiece.transform.position.x));
                int nextY = y + (y - Mathf.RoundToInt(selectedPiece.transform.position.z));
                if (this.CheckOutBoard(nextX,nextY)) {
                    ChessPiece nextPiece = BoardManager.instance.Pieces[nextX, nextY];
                    if (nextPiece == null) {
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
        }

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
        BoardManager.instance.Pieces[x1,y1] = null;
        BoardManager.instance.Pieces[x2,y2] = selectedPiece;
        selectedPiece.transform.position = endDrag;
        isWhiteTurn = !isWhiteTurn;
    }
    protected virtual bool CheckOutBoard(int x, int y){
        return x >= 0 && x < BoardManager.instance.TILE_COUNT_X 
        && y >= 0 && y < BoardManager.instance.TILE_COUNT_Y;
    }
}