using UnityEngine;

public class EffectComponent : MonoBehaviour
{
    [HideInInspector]
    // ReSharper disable once InconsistentNaming
    public GameObject GameObject;

    [SerializeField] private float lifeSpan = 50f;
    private float timer;
    
    public float x = 0.1f;
    public float y = 0.1f;

    public Transform _transform;
    private SpriteRenderer spriteRenderer;

    public SpriteRenderer SpriteRenderer
    {
        get => spriteRenderer;
    }
    
    private void Awake()
    {
        _transform = transform;
        GameObject = gameObject;
        GameObject.SetActive(false);
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = lifeSpan;
    }
    
    private void Update()
    {
        var position = _transform.position;
        position.x += x * Time.deltaTime;
        position.y += y * Time.deltaTime;
        //
        // if (Vector3.SqrMagnitude(position - GameController.Instance.Position) > 90f) //recycle based on square magnitude from game controller object
        // {
        //     GameController.Instance.Recycle(this);
        // }

        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            timer = lifeSpan;
            GameController.Instance.Recycle(this);
        }
     

        _transform.position = position;
        
        
    }
}