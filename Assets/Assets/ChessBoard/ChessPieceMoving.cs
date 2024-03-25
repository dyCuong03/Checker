using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ChessPieceMoving : CloneMonoBehaviour
{
    protected ChessPiece selectedPiece;
    protected Vector3Int startDrag;
    protected Vector3Int endDrag;
    protected List<GameObject> tileCanMove;
    protected bool isWhiteTurn = true;
    private TeamType currentTeam = TeamType.White;
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Update()
    {
        base.Update();
        if(Input.GetMouseButtonDown(0)){
            this.SelectPiece(BoardManager.instance.MouseOver);
        }
        if(Input.GetMouseButtonUp(0)){
            this.MovePiece(startDrag.x, startDrag.z, BoardManager.instance.MouseOver.x, BoardManager.instance.MouseOver.z);
        }
    }

    protected virtual void SelectPiece(Vector3Int _mousePos)
    {
        if(_mousePos.x < 0 
        || _mousePos.x >= BoardManager.instance.Pieces.Length 
        || _mousePos.z < 0 
        || _mousePos.z >= BoardManager.instance.Pieces.Length) return;

        ChessPiece p = BoardManager.instance.Pieces[_mousePos.x,_mousePos.z];
        if(p !=null){
            selectedPiece = p;
            startDrag = _mousePos;
        }
    }

    protected virtual void MovePiece(int x1, int y1, int x2, int y2)
    {
        if(selectedPiece == null) return;
        if (!this.CanMove(x2, y2)) return;
        startDrag = new Vector3Int(x1,1,y1);
        endDrag = new Vector3Int(x2,1,y2);
        BoardManager.instance.Pieces[x1,y1] = null;
        BoardManager.instance.Pieces[x2,y2] = selectedPiece;
        selectedPiece.transform.position = endDrag;    
    }

    protected virtual bool CanMove(int x, int y)
    {
        if (x < 0 
        || x >= BoardManager.instance.TILE_COUNT_X 
        || y < 0 
        || y >= BoardManager.instance.TILE_COUNT_Y)
            return false;
        if (IsEmptyTile(x,y))
            return true;
        return false;
    }
    
    protected virtual bool IsEmptyTile(int x, int y){
        return BoardManager.instance.Pieces[x, y] == null;
    }
}