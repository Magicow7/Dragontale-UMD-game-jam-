using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the meat of how each tile works.
public abstract class TileDependent : MonoBehaviour
{
    public TopTile dependentTile;
    //position to offset by from holderposition of prev in stack
    public Vector3 positionOffset;
    //position to hold next in stack
    public Vector3 holderPosition;
    
    public bool interactable;
    public bool moveable;
    public bool bottomTile = false;
    public List<GameObject>possiblePlacementTiles;

    public TileActions performedAction;
    public float actionTime = 5;
    public int actionsPerformed = 0;



    public List<TileActions>reactsToActions;
    // corrosponeds to the list in reacts to actions
    public List<GameObject>transformations;

    public Dictionary<TileActions, GameObject> transformationDict;

    [Header("DO NOT CHANGE IN INSPECTOR")]
    public bool purelyVisual;

    public void Start(){
        if(purelyVisual){
            return;
        }
        transformationDict = new Dictionary<TileActions, GameObject>();
        if(reactsToActions.Count != transformations.Count){
            Debug.Log("Not all actions corrospond to a transformation");
        }else{
            for(int i = 0; i < reactsToActions.Count; i++){
                transformationDict.Add(reactsToActions[i], transformations[i]);
            }        
        }
        
    }

    //interact is unused for now
    public virtual void interact(){
        //rotates
        transform.Rotate(new Vector3(0,90,0));
        Debug.Log("no interaction set");
    }

    public virtual void placeOn(TileDependent newOnTop){
        Debug.Log("object placed on top");
        if(performedAction != null && newOnTop.reactsToActions.Contains(performedAction)){
            actionsPerformed++;
            StartCoroutine(cookCoroutine(newOnTop));            
            if(actionsPerformed >= 50){
                actionsPerformed = 0;
            }
            Debug.Log("started action");
        }else{
            Debug.Log("not actionable");
        }
    }

    public virtual void pickUpOff(TileDependent removedFromTop){
        //disregards current action
        actionsPerformed++;
        Debug.Log("Object removed from top");
    }
    private IEnumerator cookCoroutine(TileDependent cooking){
        int currActionsPerformed = actionsPerformed;
        yield return new WaitForSeconds(actionTime);
        if(actionsPerformed == currActionsPerformed){
            GameObject newPrefab = cooking.transformationDict[performedAction];
            Debug.Log(cooking.dependentTile);
            BaseTile based = cooking.dependentTile.baseTile;
            GridHandler.instance.removeTopTile(cooking.dependentTile);
            GridHandler.instance.placeTile(based,newPrefab,false);
        }
    }
}


