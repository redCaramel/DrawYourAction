using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SpriteOutline))]
public class BreakableObject : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;
    private SpriteOutline outline;
    private SpriteRenderer sprite;
    private Collider2D col;
    public bool isBroken = false;
    [SerializeField] private GameObject wall;
    void Awake()
    {
        outline = GetComponent<SpriteOutline>();
        col = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
        outline.outlineSize = 2;
        sprite.sprite = sprites[0];
    }

    public void Break()
    {
        outline.outlineSize = 0;
        sprite.sprite = sprites[1];
        col.enabled = false;
        wall.SetActive(false);
        AudioManager.instance.PlaySFX(SFXType.tree);
    }
}
