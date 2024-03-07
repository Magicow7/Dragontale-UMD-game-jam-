using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this script is a signleton that handles level gen and tile refrences
public class GridHandler : MonoBehaviour
{

    public static GridHandler instance;
    public List<List<BaseTile>> baseTiles = new List<List<BaseTile>>();
    public List<BaseTile> activeTiles = new List<BaseTile>();

    //make sure these are ints
    public Vector3 upperBounds;
    public Vector3 lowerBounds;
    public GameObject activePrefab;

    public int playableTiles = 5;

    public Vector2 directionalWeights = new Vector2(1,1);
    public int doubleAdjacentWeightBonus = 3;

    [Header("All Tile Prefabs")]
    public GameObject wall;
    public GameObject furnace;
    public GameObject ironOre;

    //private vars
    private Dictionary <BaseTile,int> tileWeights = new Dictionary <BaseTile,int>();
    // Start is called before the first frame update
    void Start()
    {
        //initiate singleton
        if(instance == null){
            instance = this;
        }
        //this code is pretty ineffecient. if it gets bad at load times, optimize this.
        spawnBaseTiles();
        activateTiles();
        listAllTiles();
        spawnWalls();
        spawnDefaults();
    }

    void spawnBaseTiles(){
        for(int i = (int)lowerBounds.z; i <= (int)upperBounds.z; i++){
            baseTiles.Add(new List<BaseTile>());
            for(int j = (int)lowerBounds.x; j <= (int)upperBounds.x; j++){
                baseTiles[i + (int)upperBounds.z].Add(new BaseTile(new Vector3(j,0,i)));
            }
        }
    }

    void activateTiles(){
        //activate centerTile
        BaseTile centerTile = getClosestTile(Vector3.zero);
        activateSingleTile(centerTile);
        //activate tiles in random patern
        while(activeTiles.Count < playableTiles){
            //apply weights
            foreach(List<BaseTile> L in baseTiles){
                foreach(BaseTile T in L){
                    if(T.usable == false){
                        tileWeights.Add(T, 0);
                        BaseTile temp = getClosestTile(T.position + new Vector3(0,0,1));
                        if(temp != T && temp.usable == true){
                            tileWeights[T] += (int)directionalWeights.y;
                        }
                        temp = getClosestTile(T.position + new Vector3(0,0,-1));
                        if(temp != T && temp.usable == true){
                            tileWeights[T] += (int)directionalWeights.y;
                        }
                        temp = getClosestTile(T.position + new Vector3(1,0,0));
                        if(temp != T && temp.usable == true){
                            tileWeights[T] += (int)directionalWeights.x;
                        }
                        temp = getClosestTile(T.position + new Vector3(-1,0,0));
                        if(temp != T && temp.usable == true){
                            tileWeights[T] += (int)directionalWeights.x;
                        }
                    }
                }
            }
            //pick tile
            List<BaseTile> possibleTiles = new List<BaseTile>();
            foreach(BaseTile T in tileWeights.Keys){
                //if not edge tile
                if(!(T.position.x == upperBounds.x || T.position.x == lowerBounds.x ||
                T.position.z == upperBounds.z || T.position.z == lowerBounds.z)){
                    //award extra weights for multiple adjacent active nodes 
                    int toAdd = tileWeights[T];
                    if(tileWeights[T] >= directionalWeights.x + directionalWeights.y){
                        toAdd += doubleAdjacentWeightBonus;
                    }
                    for(int i = 0; i < toAdd; i++){
                        possibleTiles.Add(T);
                    }
                }
                 
            }
            int choosenIndex = Random.Range(0,possibleTiles.Count);
            activateSingleTile(possibleTiles[choosenIndex]);
            tileWeights.Clear();
        }
    }

    void activateSingleTile(BaseTile tile){
        activeTiles.Add(tile);
        tile.usable = true;
    }

    void spawnWalls(){
        for(int i = 0; i < baseTiles.Count; i++){
            for(int j = 0; j < baseTiles[i].Count; j++){
                if(!baseTiles[i][j].usable){
                    if(getClosestTile(baseTiles[i][j].position + new Vector3 (0,0,1)).usable || 
                    getClosestTile(baseTiles[i][j].position + new Vector3 (0,0,-1)).usable ||
                    getClosestTile(baseTiles[i][j].position + new Vector3 (1,0,0)).usable ||
                    getClosestTile(baseTiles[i][j].position + new Vector3 (-1,0,0)).usable){
                        TopTile newTop = new TopTile(baseTiles[i][j], wall);
                        baseTiles[i][j].topTiles.Add(newTop);
                    }
                }
            }
        }
    }

    void spawnDefaults(){
        //spawn furnace
        for(int i = 0; i < 2; i++){
            BaseTile choosenTile = getRandomActiveTile(true);
            TopTile newFurnace = new TopTile(choosenTile, furnace);
            choosenTile.topTiles.Add(newFurnace);
        }

        

    }

    //bad recursion that might cause issue, but probably won't.
    BaseTile getRandomActiveTile(bool empty){
        int choosenIndex = Random.Range(0,activeTiles.Count);
        if(!empty || activeTiles[choosenIndex].topTiles.Count == 0){
            return activeTiles[choosenIndex];
        }else{
            return getRandomActiveTile(empty);
            
        }
        
    }
    void listAllTiles(){
        /*
        for(int i = 0; i < baseTiles.Count; i++){
            for(int j = 0; j < baseTiles[i].Count; j++){
                Debug.Log(baseTiles[i][j].position);
                //Instantiate(activePrefab, baseTiles[i][j].position, Quaternion.identity);
            }
        }
        */
        /*
        foreach(BaseTile t in activeTiles){
            Instantiate(activePrefab, t.position, Quaternion.identity);
        }
        */
        Debug.Log(getClosestTile(new Vector3(0.9f,0,0)).position);
    }

    //this work assuming 0,0 is center
    public BaseTile getClosestTile(Vector3 position){

        position.x = Mathf.Clamp(position.x, (int)lowerBounds.x, (int)upperBounds.x);
        position.z = Mathf.Clamp(position.z, (int)lowerBounds.z, (int)upperBounds.z);
        int tileX = (int)Mathf.Round(position.x) + (int)upperBounds.x;
        int tileZ = (int)Mathf.Round(position.z) + (int)upperBounds.z;

        return baseTiles[tileZ][tileX];

        //old brute force method, was pretty ineffecient
        /*
        BaseTile ReturnTile = baseTiles[0][0];
        float distanceToBeat = 100000;
        for(int i = 0; i < baseTiles.Count; i++){
            for(int j = 0; j < baseTiles[i].Count; j++){
                if(Vector3.Distance(baseTiles[i][j].position, position) < distanceToBeat){
                    ReturnTile = baseTiles[i][j];
                    distanceToBeat = Vector3.Distance(baseTiles[i][j].position, position);
                }
            }
        }
        return ReturnTile;
        */
        
    }

    public void removeTopTile(TopTile topTile){
        BaseTile based = topTile.baseTile;
        based.topTiles.Remove(topTile);
        if(based.topTiles.Count >= 1){based.topTiles[based.topTiles.Count -1].dependent.pickUpOff(topTile.dependent);}
        Destroy(topTile.dependent.gameObject);
    }

    public void placeTile(BaseTile selectedTile, GameObject heldTile, bool bottomTile){
        TopTile newTile = new TopTile(selectedTile, heldTile);
        selectedTile.topTiles.Add(newTile);
        if(!bottomTile){selectedTile.topTiles[selectedTile.topTiles.Count -2].dependent.placeOn(newTile.dependent);}
    }

}
