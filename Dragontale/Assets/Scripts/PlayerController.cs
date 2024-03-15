using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//player controler, self explanatory
public class PlayerController : MonoBehaviour
{

    //public vars
    [Header("Movement Settings")]
    public float acceleration = 2.5f;
    //do not set this value above 1!!
    //0 is instant stop, 1 is natural deceleration
    public float deceleration = 0.1f;
    public float maxVelocity = 5;

    public float rotateSpeed = 2;

    [Header("Refrences")]
    public GameObject selector;

    //serializable private vars
    [SerializeField] private Rigidbody rb;

    //private vars
    private List<Tuple<Vector3, ForceMode>> QueuedForces = new List<Tuple<Vector3, ForceMode>>();
    private Vector2 inputVector = new Vector2(0,0);

    private float sqrMaxVelocity;

    private BaseTile selectedTile;

    public GameObject heldTile;

    private Vector3 lastInputDir = new Vector3(0,0,-1);

    // Start is called before the first frame update
    void Start()
    {

        rb = transform.GetComponent<Rigidbody>();
        SetMaxVelocity(maxVelocity);
    }

    void SetMaxVelocity(float maxVelocity){
    this.maxVelocity = maxVelocity;
    sqrMaxVelocity = maxVelocity * maxVelocity;
}

    // Update is called once per frame
    void Update()
    {
        //read inputs and apply force
        inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if(inputVector != Vector2.zero){
            Vector3 forceVector = new Vector3(inputVector.x,0,inputVector.y);
            QueuedForces.Add(new Tuple<Vector3,ForceMode>(forceVector*acceleration, ForceMode.Force));

            //face towards move direction
            var lookPos = transform.position + new Vector3(inputVector.x, 0, inputVector.y) - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotateSpeed); 
        }
        //clamp velocity
        if(rb.velocity.sqrMagnitude > sqrMaxVelocity){
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }

        //update selector pos
        
        if(inputVector != Vector2.zero){
            lastInputDir = inputVector;
            
        }
        Vector3 searchPos = transform.position + (new Vector3(lastInputDir.x, 0, lastInputDir.y));
        selectedTile = GridHandler.instance.getClosestTile(searchPos);
        Vector3 temp = selectedTile.position;
        selector.transform.position = new Vector3(temp.x, selector.transform.position.y, temp.z);
        

        //get input for moving tiles
        if(Input.GetKeyDown(KeyCode.Space)){
            if(heldTile == null){
                GrabTile();
            }else{
                PlaceTile();
            }
            
        }

        //get input for interacting with tiles, rotation for now.
        if(Input.GetKeyDown(KeyCode.E)){
            TopTile topTile = selectedTile.topTiles[selectedTile.topTiles.Count-1];
            if(topTile.dependent.interactable){
                topTile.dependent.interact();
            }
        }
        
    }

    //used for consistent physics applications
    void FixedUpdate(){
        foreach(Tuple<Vector3, ForceMode> v in QueuedForces){
            rb.AddForce(v.Item1, v.Item2);
        }
        QueuedForces.Clear();

        if(inputVector == Vector2.zero){
            //if(Mathf.Abs(rb.velocity.x) >  0.1f || Mathf.Abs(rb.velocity.z) >  0.1f){
                rb.velocity = new Vector3(rb.velocity.x*deceleration , rb.velocity.y, rb.velocity.z*deceleration);
            //}else{
                //rb.velocity = new Vector3(0,rb.velocity.y,0);
            //}
        }
    }

    void GrabTile(){
        if(selectedTile.topTiles.Count <= 0){
            return;
        }
        TopTile topTile = selectedTile.topTiles[selectedTile.topTiles.Count-1];
        if(topTile.dependent.moveable){
            heldTile = topTile.prefab;
            GridHandler.instance.removeTopTile(topTile);
        }
    }

    void PlaceTile(){
        if(selectedTile.usable){
            //bottom tile
            if(heldTile.GetComponent<TileDependent>().bottomTile && selectedTile.topTiles.Count == 0){
                GridHandler.instance.placeTile(selectedTile, heldTile, true);
                heldTile = null;
            //if top tile on stack is a possible host
            }else if(selectedTile.topTiles.Count >= 1 && heldTile.GetComponent<TileDependent>().possiblePlacementTiles.Contains(selectedTile.topTiles[selectedTile.topTiles.Count -1].prefab)){
                GridHandler.instance.placeTile(selectedTile, heldTile, false);
                heldTile = null;
            }
            
        }
       
    }
}
