using UnityEngine;

public class Mission1_GetToTarget : MonoBehaviour, MissionManagerInterface
{
    private bool cleared = false;
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            cleared = true;
            Debug.Log("asdf");
            
        }
    }
    public bool isClear()
    {
        return cleared;
    }
}

// Attatch this Script to Target Colliders
