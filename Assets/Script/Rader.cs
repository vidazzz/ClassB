using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rader : MonoBehaviour
{
    private NPC owner;
    public List<Interactable> interactables; //视野中的可交互物体列表
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == owner.gameObject)
            return;
        Interactable targetInteractable = collision.GetComponent<Interactable>();
        if (!interactables.Contains(targetInteractable))
        {
            interactables.Add(targetInteractable);
        }      
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        Character targetCharacter = collision.GetComponent<Character>();
            if (interactables.Contains(targetCharacter))
                interactables.Remove(targetCharacter);
    }
    public bool IsInRader(Interactable target)
    {
        return interactables.Contains(target);
    }


    void Awake()
    {
        owner = GetComponentInParent<NPC>();
    }

    void OnEnable()
    {
        
    }

    void OnDisable() 
    {
        interactables.Clear();
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
