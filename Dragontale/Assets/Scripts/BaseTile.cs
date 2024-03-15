using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this is the base for the tile system, each one holds a list ot TopTiles in a stack
public class BaseTile
{
    public Vector3 position;
    public bool usable = false;
    //when interacting, think of this as a stack, only the top tile can be interacted with.
    public List<TopTile> topTiles;
    public BaseTile(Vector3 position){
        this.position = position;
        topTiles = new List<TopTile>();
    }

    public TopTile getTopTile(int extra = 0){
        if(topTiles.Count == 0){
            return null;
        }
        return topTiles[topTiles.Count - (1+extra)];
    }
}
