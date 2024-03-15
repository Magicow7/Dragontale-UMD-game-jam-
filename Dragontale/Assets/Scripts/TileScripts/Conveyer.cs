using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyer : TileDependent
{
    public float conveyerTime;
    // Start is called before the first frame update
    private GameObject movingPrefab;
    private GameObject movingInstance;
    private float moveTime;
    private Vector3 startPos;
    private Vector3 endPos;
    private BaseTile nextTile;
    private bool moving = false;


    public override void placeOn(TileDependent newOnTop){
        actionsPerformed++;
        StartCoroutine(moveCoroutine(newOnTop));            
        if(actionsPerformed >= 50){
            actionsPerformed = 0;
        }
    }

    public override void pickUpOff(TileDependent removedFromTop){
        Debug.Log("conveyer off");
    }

    private IEnumerator moveCoroutine(TileDependent cooking){
        int currActionsPerformed = actionsPerformed;
        yield return new WaitForSeconds(actionTime);
        if(actionsPerformed == currActionsPerformed){
            nextTile = GridHandler.instance.getClosestTile(transform.position + transform.forward);
            if(nextTile.topTiles.Count == 0){
                StartCoroutine(moveCoroutine(cooking));          
                yield break;
            }else{
                if(Vector3.Distance(cooking.dependentTile.baseTile.position, nextTile.position) < 1.2 &&
                cooking.possiblePlacementTiles.Contains(nextTile.getTopTile().prefab)){
                    movingPrefab = cooking.dependentTile.prefab;
                    startPos = cooking.transform.position;
                    endPos = nextTile.getTopTile().dependent.transform.position 
                    + nextTile.getTopTile().dependent.holderPosition
                    + cooking.positionOffset;
                    moveTime = 0;
                    movingInstance = Instantiate(movingPrefab, cooking.transform.position, Quaternion.identity);
                    movingInstance.GetComponent<TileDependent>().purelyVisual = true;
                    moving = true;
                    GridHandler.instance.removeTopTile(cooking.dependentTile);
                }else{
                    StartCoroutine(moveCoroutine(cooking));   
                    Debug.Log("tile full or incompatible");
                }
            }
            
            
        }
    }

    public void Update(){
        if(moving){
            moveTime += Time.deltaTime;
            movingInstance.transform.position = Vector3.Lerp(startPos,endPos, moveTime);
            if(moveTime >= conveyerTime){
                moving = false;
                Destroy(movingInstance);
                GridHandler.instance.placeTile(nextTile,movingPrefab,false);
            }
        }
        
    }
}
