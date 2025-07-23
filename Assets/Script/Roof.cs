using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Roof : MonoBehaviour
{
    TilemapRenderer tilemapRenderer;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag ("hero"))
        {
            tilemapRenderer.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag ("hero"))
        {
            tilemapRenderer.enabled = true;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
       tilemapRenderer = GetComponent<TilemapRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
