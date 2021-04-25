using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

//recycle based on square magnitude from game controller 
public class GameController : MonoBehaviour
{
    private Camera _camera;

    public static GameController Instance;

    private Transform _transform; 

    public Vector3 Position => _transform.position;

    [SerializeField] private TextMeshProUGUI depthText;

    [SerializeField] private float depthLevel1 = 0f;
    [SerializeField] private float depthLevel2 = 2f;
    [SerializeField] private float depthLevel3 = 5f;
    [SerializeField] private float depthLevel4 = 8f;


    [SerializeField] private Color depthLevel1Color;
    [SerializeField] private Color depthLevel2Color;
    [SerializeField] private Color depthLevel3Color;
    [SerializeField] private Color depthLevel4Color;

    [SerializeField] private Sprite[] level1Fish;
    [SerializeField] private Sprite[] level2Fish;
    [SerializeField] private Sprite[] level3Fish;
    [SerializeField] private Sprite[] level4Fish;

    private int currentDepthLevel = 0;

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

    private static readonly Vector3 Origin = new Vector3(0f, -16f, 0f);
    
    private float timer;
    [SerializeField] private float spawnRate = 2f;

    private SpriteRenderer _spriteRenderer;
    
    //certain objects show up between certain depths
    //background gets dark as depth increases
    //bubbles go up
    
    /*MOVING
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

    private void Awake()
    {
        Instance = this;
        _transform = transform;

        Debug.Assert(effectPrefab != null);
        timer = spawnRate;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Assert(_spriteRenderer!= null);
        _camera = Camera.main;
    }
    

    private void Start()
    {
        Screen.SetResolution(800, 600, true);

        for (var i = 0; i < 20; i++)
        {
            EffectPool.Push(InstantiateEffect());
        }
        
        for (var i = 0; i < 10; i++)
        {
            Spawn();
        }

        _camera.backgroundColor = depthLevel1Color;

        
    }
    
    private void Update()
    {
        var direction = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            direction = -1;
            _spriteRenderer.flipX = true;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            direction = 1;
            _spriteRenderer.flipX = false;
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

        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            timer = spawnRate;
            Spawn();
        }
        
        HandleDepth();
        
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Spawn();
        }
        #endif
    }

    private Sprite GetFishSprite()
    {
        var sprite = currentDepthLevel switch
        {
            0 => level1Fish[Random.Range(0, level1Fish.Length)],
            1 => level2Fish[Random.Range(0, level2Fish.Length)],
            2 => level3Fish[Random.Range(0, level3Fish.Length)],
            3 => level4Fish[Random.Range(0, level4Fish.Length)],
            _ => throw new IndexOutOfRangeException()
        };

        return sprite;
    }

    private void HandleDepth()
    {
        depth += Time.deltaTime * descentSpeed;
        depthText.text = depth.ToString();

        switch (currentDepthLevel)
        {
            case 0:
                if (depth > depthLevel1)
                {
                    currentDepthLevel = 1;
                    _camera.backgroundColor = depthLevel2Color;
                    Debug.Log("achieved depth 1");
                }
                break;
            case 1:
                if (depth > depthLevel2)
                {
                    currentDepthLevel = 2;
                    _camera.backgroundColor = depthLevel3Color;
                    Debug.Log("achieved depth 2");

                }
                break;
            case 2:
                if (depth > depthLevel3)
                {
                    currentDepthLevel = 3;
                    _camera.backgroundColor = depthLevel4Color;
                    Debug.Log("achieved depth 3");

                }
                break;
            case 3:
                if (depth > depthLevel4)
                {
                    currentDepthLevel = 4;
                    _camera.backgroundColor = Color.black;
                    Debug.Log("achieved depth 4");

                }
                break;
            default:
                Debug.LogWarning("out of depth range");
                break;
        }

    }
    
    private EffectComponent InstantiateEffect()
    {
        return Instantiate(effectPrefab, Origin, Quaternion.identity);
    }
    
    
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

        var spawnPoint = _transform.position + Origin;
        var xSpawnPosition = Random.Range(-16f, 16f);
        spawnPoint.x += xSpawnPosition;
        effect._transform.position = spawnPoint;
        effect.GameObject.SetActive(true);
        // var sprite = effect.GetComponent<SpriteRenderer>();
        effect.SpriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        var sorting = Random.Range(-5, 5);

        if (xSpawnPosition < 0f)
        {
            effect.x = Random.Range(-0.02f * (sorting + 10), 0.2f * (sorting + 10));
        }
        else
        {
            effect.x = Random.Range(-0.2f * (sorting + 10), 0.02f * (sorting + 10));

        }
        // effect.x = Random.Range(-0.2f * (sorting + 10), 0.2f * (sorting + 10));
        effect.y = Random.Range(.1f * (sorting + 10), 0.2f * (sorting + 10));
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
            second.y = effect.y + Random.Range(.01f, 0.1f);
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


}