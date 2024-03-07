using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//each top tile has a prefab for what to place.
public class TopTile
{
    public BaseTile baseTile;
    public GameObject prefab;
    public TileDependent dependent;
    public TopTile(BaseTile baseTile, GameObject prefab){
        this.baseTile = baseTile;
        this.prefab = prefab;
        spawnPrefab();
    }

    public void spawnPrefab(){
        GameObject temp = GameObject.Instantiate(prefab, baseTile.position, Quaternion.identity);
        dependent = temp.GetComponent<TileDependent>();
        dependent.dependentTile = this;
        if(baseTile.topTiles.Count >= 1){
            //-1 works here because the current thing hasn't been added to the list at this time
            dependent.transform.position = baseTile.topTiles[baseTile.topTiles.Count - 1].dependent.transform.position +
            baseTile.topTiles[baseTile.topTiles.Count - 1].dependent.holderPosition + dependent.positionOffset;
        }else{
            dependent.transform.position = baseTile.position + dependent.positionOffset;
        }
    }

    public virtual void interact(){
        if(dependent.interactable){
            dependent.interact();
        }
    }

    //unused
    public void move(){
        if(dependent.moveable){
            Debug.Log("forgor to implement this");
        }
    }
}
