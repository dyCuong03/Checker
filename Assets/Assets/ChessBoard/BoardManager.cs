using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : CloneMonoBehaviour
{
    public static BoardManager instance;
    [SerializeField] protected MaterialManager materialManager;
    protected Material _material;
    public int TILE_COUNT_X = 8;
    public int TILE_COUNT_Y = 8;
    [SerializeField] protected GameObject tilePref;
    [SerializeField] protected GameObject piecePref;
    protected GameObject[,] tiles;
    public GameObject[,] Tiles
    {
        get { return tiles; }
    }
    protected ChessPiece[,] pieces;
    public ChessPiece[,] Pieces
    {
        get { return pieces; }
        set { pieces = value; }
    }
    private Camera currentCamera;
    private Vector3Int currentHover;
    protected Vector3Int mouseOver;
    public Vector3Int MouseOver
    {
        get { return mouseOver; }
    }
    protected override void Awake()
    {
        base.Awake();
        if (BoardManager.instance != null) Debug.LogError("Only 1 BoardManager allow to exist");
        BoardManager.instance = this;
        this.GenerateAllBroad(TILE_COUNT_X,TILE_COUNT_Y);
    }

    protected override void Update()
    {
        base.Update();
        this.RayCast();
        //this.HoverTranslateTile();
    }
    protected virtual void RayCast()
    {
        if(!currentCamera){
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile","Hover"))){
            Vector3Int hitPos = LookupTileIndex(info.transform.gameObject);
            if(currentHover == -Vector3Int.one){
                currentHover = hitPos;
                tiles[hitPos.x, hitPos.z].layer = LayerMask.NameToLayer("Hover");
            }
            if(currentHover != hitPos){
                tiles[currentHover.x, currentHover.z].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPos;
                tiles[hitPos.x, hitPos.z].layer = LayerMask.NameToLayer("Hover");
            }
            this.MouseOverPos(hitPos);
        } else {
            if(currentHover != -Vector3Int.one){
                tiles[currentHover.x, currentHover.z].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector3Int.one;
            }
            this.MouseOverPos(new Vector3Int(-1,-1,-1));
        }
    }
    protected virtual void MouseOverPos(Vector3Int hitPos){
            mouseOver.x = hitPos.x;
            mouseOver.y = hitPos.y;
            mouseOver.z = hitPos.z;
    }
    protected virtual Vector3Int LookupTileIndex(GameObject hitInfo){
        for(int i = 0; i < TILE_COUNT_X; i++)
            for(int j = 0; j < TILE_COUNT_Y; j++)
                if(tiles[i,j] == hitInfo)
                    return new Vector3Int(i,0,j);
        return -Vector3Int.one;
    }
    protected virtual void GenerateAllBroad(int tileCountX, int tileCountY){
        tiles = new GameObject[TILE_COUNT_X,TILE_COUNT_Y];
        pieces = new ChessPiece[TILE_COUNT_X,TILE_COUNT_Y];
        Material white = materialManager.White_Material;
        Material black = materialManager.Black_Material;

        // Generate board
        for(int i = 0 ; i < tileCountX; i++){
            for(int j = 0; j < tileCountY; j++){
                if ((i + j) % 2 == 0){
                    this._material = black;
                } else {
                    this._material = white;
                }
                tiles[i,j] = this.GenerateTile(i,j);
            }
        }
        // Generate black pieace
        for(int j = 0; j < 3; j++){
            bool oldRow = (j % 2 == 0);
            for(int i = 0; i < 8; i += 2){
                pieces[(oldRow)?i:i+1,j] = GeneratePiece(black,(oldRow)?i:i+1,j, TeamType.Black,  ChessPieceType.Men);
            }
        }
        // Generate white pieace
        for(int j = 7; j > 4; j--){
            bool oldRow = (j % 2 == 0);
            for(int i = 0; i < 8; i += 2){
                pieces[(oldRow)?i:i+1,j] = GeneratePiece(white,(oldRow)?i:i+1,j,TeamType.White, ChessPieceType.Men);
            }
        }
    }
    protected virtual GameObject GenerateTile(int xPos, int yPos)
    {
        Vector3 spawPos = new Vector3(xPos, 0, yPos);
        GameObject _tile = Instantiate(tilePref, spawPos, Quaternion.identity) as GameObject;
        _tile.transform.parent = this.transform;
        MeshRenderer renderer = _tile.GetComponent<MeshRenderer>();
        renderer.material = this._material;
        _tile.layer = LayerMask.NameToLayer("Tile");
        return _tile;
    }
    
    protected virtual ChessPiece GeneratePiece(Material _material, int xPos, int yPos, TeamType _team, ChessPieceType chessPieceType)
    {
        Vector3Int spawnPos = new Vector3Int(xPos, 1, yPos);
        ChessPiece _piece = Instantiate(piecePref.transform).GetComponent<ChessPiece>();
        _piece.team = _team;
        _piece.chessPieceType = chessPieceType;
        _piece.transform.position = spawnPos;
        MeshRenderer renderer = _piece.GetComponent<MeshRenderer>();
        renderer.material = _material;
        _piece.transform.parent = this.transform;
        return _piece;
    }
}
