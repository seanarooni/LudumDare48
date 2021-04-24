using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

//recycle based on square magnitude from game controller 
public class GameController : MonoBehaviour
{
    public static GameController Instance;

    private Transform _transform; 

    public Vector3 Position => _transform.position;

    [SerializeField] private float horizontalMoveSpeed = 0.1f;
    [SerializeField] private float maxHorizontalSpeed = 1000f;
    [SerializeField] private float friction = 0.1f;
    [SerializeField] private float acceleration = 0.5f;
    [SerializeField] private float currentSpeed;
    
    [SerializeField] private float descentSpeed = 0.1f;
    
    [SerializeField] private float depth;
    
    [SerializeField] private Sprite[] sprites;

    [SerializeField] private EffectComponent effectPrefab;
    private readonly Stack<EffectComponent> EffectPool = new Stack<EffectComponent>();

    private static readonly Vector3 Origin = new Vector3(0f, 0f, 0f);
        //sprite effect prefab
        //prefab recycler
        //direction randomizerddgameGame
        //layer randomizer
    //effect color randomizer / controller (trends darker as depth increases
    //higher layers move faster than lower / more negative layers
    
    //sort effects into back, front, or both 
    //alternately, make sorting layers be less opaque the higher they are
    
    //certain objects show up between certain depths
    //background gets dark as depth increases
    //bubbles go up
    
    /*MOVING
     * moving horizontally needs to have an acceleration component
     * 
     */

    //STRETCH GOALS
    //collision with certain fish
        //screen shake and a noise, thats it
    //walls or wrapping to limit player movement
    //a couple easter eggs, something about LD48
    //ability to input a random seed

    //at a certain depth it gets totally black and says "thanks for playing"
    
    /* SOUND AND MUSIC
     * main motif
     * research underwater musical tropes
     * certain effects or encounters can have sound effects or musical layers associated with them
     * 
     */

    public void Recycle(EffectComponent component)
    {
        component.GameObject.SetActive(false);
        component.SpriteRenderer.sprite = null;
        component.transform.position = Origin;
        EffectPool.Push(component);

    }
    
    private void Spawn()
    {
        EffectComponent effect;
        if (EffectPool.Count > 0)
        {
            effect = EffectPool.Pop();
            //get an effect
            // sprite.color = Random.ColorHSV();
        }
        else
        {
            effect = InstantiateEffect();
        }
        
        //chance to spawn 2 of the same kind, very slightly different xy, 1 layer apart

        
        effect._transform.position = _transform.position;
        effect.GameObject.SetActive(true);
        // var sprite = effect.GetComponent<SpriteRenderer>();
        effect.SpriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        var sorting = Random.Range(-5, 5);

        effect.x = Random.Range(-0.2f * (sorting + 10), 0.2f * (sorting + 10));
        effect.y = Random.Range(-0.2f * (sorting + 10), 0.2f * (sorting + 10));
        // effect.y = Random.Range(-0.2f, 0.2f);
        effect.SpriteRenderer.sortingOrder = sorting;

        var color = Random.ColorHSV();
        color.a = Map(sorting, -5f, 5f, 1f, 0f);
        effect.SpriteRenderer.color = color;
        
        if (Random.Range(0, 10) == 1)
        {
            EffectComponent second;
            if (EffectPool.Count > 0)
            {
                second = EffectPool.Pop();
                //get an effect
                // sprite.color = Random.ColorHSV();
            }
            else
            {
                second = InstantiateEffect();
            }
        
            //chance to spawn 2 of the same kind, very slightly different xy, 1 layer apart

        
            second._transform.position = _transform.position;
            second.GameObject.SetActive(true);
            // var sprite = effect.GetComponent<SpriteRenderer>();
            second.SpriteRenderer.sprite = effect.SpriteRenderer.sprite;
            // var sorting = Random.Range(-5, 5);

            second.x = effect.x + Random.Range(-0.1f, 0.1f);
            second.y = effect.y + Random.Range(-0.1f, 0.1f);
            // effect.y = Random.Range(-0.2f, 0.2f);
            second.SpriteRenderer.sortingOrder = sorting;

            color.a = Mathf.Max(0.2f, color.a - 0.1f);
            second.SpriteRenderer.color = color;

        }

    }

    private static float Map(float x, float iMin, float iMax, float oMin, float oMax)
    {
        return (x - iMin) * (oMax - oMin) / (iMax - iMin) + oMin;
    }

    private void Awake()
    {
        Instance = this;
        _transform = transform;

        Debug.Assert(effectPrefab != null);

    }

    private EffectComponent InstantiateEffect()
    {
        return Instantiate(effectPrefab, Origin, Quaternion.identity);
    }

    private void Start()
    {
        Screen.SetResolution(800, 600, true);

        for (var i = 0; i < 20; i++)
        {
            EffectPool.Push(InstantiateEffect());
        }
    }

    private void Update()
    {
        var direction = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            direction = -1;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            direction = 1;
        }

        if (direction == 0)
        {
            // Mathf.Lerp(currentSpeed, 0, currentSpeed / maxHorizontalSpeed);
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, friction);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, direction * maxHorizontalSpeed, acceleration);
        }

        var position = _transform.position;
        position.x += currentSpeed * Time.deltaTime;
        _transform.position = position;
        
            
        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Spawn();
        }
        #endif
    }
}