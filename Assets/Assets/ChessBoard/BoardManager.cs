using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : CloneMonoBehaviour
{
    [SerializeField] protected MaterialManager materialManager;
    protected Material _material;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    [SerializeField] protected GameObject tilePref;
    protected GameObject[,] tiles;
    protected ChessPiece[,] pieces;
    protected ChessPiece selectedPiece;
    protected Vector3Int startDrag;
    protected Vector3Int endDrag;
    [SerializeField] protected GameObject piecePref;
    private Camera currentCamera;
    private Vector3Int currentHover;
    private Vector3Int mouseOver;
    protected override void Awake()
    {
        base.Awake();
        this.GenerateAllBroad(TILE_COUNT_X,TILE_COUNT_Y);
    }

    protected override void Update()
    {
        base.Update();
        this.Raycast();
        //this.HoverTranslateTile();
        if(Input.GetMouseButtonDown(0)){
            this.SelectPiece(mouseOver.x,mouseOver.z);
            Debug.Log(selectedPiece.name + selectedPiece.transform.position);
            Debug.Log(pieces);
        }
        if(Input.GetMouseButtonUp(0)){
            this.MovePiece(startDrag.x, startDrag.z, mouseOver.x, mouseOver.z);
        }
    }
    protected virtual void SelectPiece(int x , int y)
    {
        if(x < 0 || x >= pieces.Length || y < 0 || y >= pieces.Length) return;

        ChessPiece p = pieces[x,y];
        if( p !=null){
            selectedPiece = p;
            startDrag = mouseOver;
        }
    }
    protected virtual void MovePiece(int x1, int z1, int x2, int z2)
    {
        if(x2 < 0 || z2 < 0) return;
        startDrag = new Vector3Int(x1,1,z1);
        endDrag = new Vector3Int(x2,1,z2);
        selectedPiece = pieces[x1,z1];
        selectedPiece.transform.position = endDrag;      
    }
    protected virtual void HoverTranslateTile()
    {
        foreach(GameObject tile in tiles){
            if(tile.layer == LayerMask.NameToLayer("Hover")){
                MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
                Material defaultMaterial = renderer.material;
                if(defaultMaterial == null)
                    continue;
        
            renderer.material = materialManager.HoverTile;

            if(tile.layer != LayerMask.NameToLayer("Hover")){
                renderer.material = defaultMaterial;
            }
        }           
    }

    }
    protected virtual void Raycast()
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
        _piece.xPos = spawnPos.x;
        _piece.yPos = spawnPos.z;
        _piece.transform.position = spawnPos;
        MeshRenderer renderer = _piece.GetComponent<MeshRenderer>();
        renderer.material = _material;
        _piece.transform.parent = this.transform;
        return _piece;
    }
}
