using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rader : MonoBehaviour
{
    private Character owner;
    public List<Character> characters;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == owner.gameObject)
            return;
        Character targetCharacter = collision.GetComponent<Character>();
        if (targetCharacter is NPC)
        {
            if (!characters.Contains(targetCharacter))
                characters.Add(targetCharacter);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        Character targetCharacter = collision.GetComponent<Character>();
        if (targetCharacter is NPC)
        {
            if (characters.Contains(targetCharacter))
                characters.Remove(targetCharacter);
        }
    }


    void Awake()
    {
        owner = GetComponentInParent<Character>();
    }

    void OnEnable()
    {
        
    }

    void OnDisable() 
    {
        characters.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
