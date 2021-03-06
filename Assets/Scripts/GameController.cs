using UnityEngine;
using System.Collections.Generic;
using AudioFramework;
using TMPro;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

//recycle based on square magnitude from game controller 
public class GameController : MonoBehaviour
{
    private Camera _camera;

    [Header("shake")] [SerializeField] private float shakeDuration = 0.8f;
    [SerializeField] private float shakeStrength = 1f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;
    [SerializeField] private bool shakeSnapping = false;
    [SerializeField] private bool shakeFade = true;

    [Header("victory screen")] [SerializeField]
    private float victoryScreenCameraYValue = -11f;

    [SerializeField] private float victoryScreenScrollSpeed = 5f;

    [SerializeField] private TextMeshPro victoryScreenFishHitLabel;
    
    public static GameController Instance;

    private Transform _transform; 

    public Vector3 Position => _transform.position;

    [SerializeField] private TextMeshProUGUI depthText;

    [SerializeField] private float depthLevel1 = 0f;
    [SerializeField] private float depthLevel2 = 2f;
    [SerializeField] private float depthLevel3 = 5f;
    [SerializeField] private float depthLevel4 = 8f;
    [SerializeField] private float finishDepth = 16f;
    
    [SerializeField] private float level1Timer = 3.5f; 
    [SerializeField] private float level2Timer = 2.5f; 
    [SerializeField] private float level3Timer = 3f; 
    [SerializeField] private float level4Timer = 2f;
    [SerializeField] private float finishTimer = 10f;
    private float fishTimer;

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

    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    [SerializeField] private float fishSpawnYPosition;
    [SerializeField] private float fishYTarget;
    [SerializeField] private float fishVerticalMoveTime = 2f;
    [SerializeField] private FishComponent fishPrefab;

    [SerializeField] private float fishYHoverTarget;
    [SerializeField] private float fishYHoverVariance;

    [SerializeField] private float timeUntilFishHover = 1f;
    private int fishCollisions;
    
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
        fishTimer = level1Timer;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Assert(_spriteRenderer!= null);
        _camera = Camera.main;
        
        Debug.Assert(victoryScreenFishHitLabel != null);
        Debug.Assert(tutorialCanvas != null);
    }

    private bool _tutorial = true;
    [SerializeField] private GameObject tutorialCanvas;
    private void Tutorial()
    {
        Time.timeScale = 0f;
        AudioManager.Instance.StartGameMusic();
        tutorialCanvas.SetActive(true);
        
        // DOTween.To(() => _camera.backgroundColor, x =>_camera.backgroundColor = x, Color.black, 45f);

    }
    

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            SceneManager.LoadScene(0);
            return;
        }
        
        Tutorial();
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log($"collided with {other.collider.name}");

        if (onVictoryConditionMet)
            return;
        
        //have the fish dart off in the opposite direction of the collision

        var collisionX = other.GetContact(0).point.x;
        var fishX = other.transform.position.x;

        
        
        other.collider.enabled = false;
        fishCollisions++;

        Collision();
        if (fishX > collisionX)
        {
            DOTween.Kill(other.collider.transform);
            other.collider.transform.DOMoveX(10f, 0.5f).SetEase(Ease.InBack);
        }
        else
        {
            DOTween.Kill(other.collider.transform);
            other.collider.transform.DOMoveX(-10f, 0.5f).SetEase(Ease.InBack);
        }
    }

    private void Collision()
    {
        AudioManager.Instance.PlayCollisionSound();
        _camera.transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness, shakeSnapping, shakeFade);
    }

    private bool _paused;
    private bool onVictoryConditionMet;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_WEBGL
            _paused = !_paused;
            Time.timeScale = _paused ? 0f : 1f;
#else
Application.Quit();
#endif

        }
        

        if (_paused)
        {
            return;
        }

        if (_tutorial)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                //tutorial finished
                _tutorial = false;
                Time.timeScale = 1f;
                tutorialCanvas.SetActive(false);
            }
            else
            {
                return;
            }
        }

        if (onVictoryConditionMet)
        {
            // ReSharper disable once InvertIf
            if (Input.GetKeyDown(KeyCode.R))
            {
                AudioManager.Instance.GameMode();
                SceneManager.LoadScene(1);
            }
            return;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                AudioManager.Instance.GameMode();
                SceneManager.LoadScene(1);
            }
        }
        
        var direction = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (_transform.position.x > leftLimit.position.x)
                direction -= 1;
            _spriteRenderer.flipX = true;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (_transform.position.x < rightLimit.position.x)
                direction += 1;
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

        fishTimer -= Time.deltaTime;
        if (fishTimer < 0f)
        {
            fishTimer = currentDepthLevel switch
            {
                1 => fishTimer = level1Timer,
                2 => fishTimer = level2Timer,
                3 => fishTimer = level3Timer,
                _ => fishTimer = level4Timer
            };
            
            FishSpawner();
        }
        
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

    private void FishSpawner()
    {
        if (currentDepthLevel == 5) return;
        
        var position = new Vector3(Random.Range(leftLimit.position.x - 4f, rightLimit.position.x + 4f), fishSpawnYPosition, 0f);
        var fish = Instantiate(fishPrefab, position, quaternion.identity);

        fish.GetComponent<SpriteRenderer>().sprite = GetFishSprite();

        var sequence = DOTween.Sequence();
        var ft = fish.transform;
        var hover = fishYHoverTarget + Random.Range(0f, fishYHoverVariance);
        sequence.Append(ft.DOMoveY(hover,  timeUntilFishHover));
        sequence.Append(ft.DOMoveY(fishYTarget, fishVerticalMoveTime)
            .OnComplete(() => Destroy(fish.transform.gameObject)));
        sequence.Play();
    }

    private Sprite GetFishSprite()
    {
        var sprite = currentDepthLevel switch
        {
            0 => level1Fish[Random.Range(0, level1Fish.Length)],
            1 => level2Fish[Random.Range(0, level2Fish.Length)],
            2 => level3Fish[Random.Range(0, level3Fish.Length)],
            3 => level4Fish[Random.Range(0, level4Fish.Length)],
            _ => level4Fish[Random.Range(0, level4Fish.Length)]
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

                    // DOTween.To(() => _camera.backgroundColor, x =>_camera.backgroundColor = x, depthLevel2Color, depthLevel2);
                    
                    Debug.Log("achieved depth 1");
                }
                break;
            case 1:
                if (depth > depthLevel2)
                {
                    currentDepthLevel = 2;
                    _camera.backgroundColor = depthLevel3Color;
                    // DOTween.To(() => _camera.backgroundColor, x =>_camera.backgroundColor = x, depthLevel3Color, depthLevel3);

                    Debug.Log("achieved depth 2");

                }
                break;
            case 2:
                if (depth > depthLevel3)
                {
                    currentDepthLevel = 3;
                    _camera.backgroundColor = depthLevel4Color;
                    // DOTween.To(() => _camera.backgroundColor, x =>_camera.backgroundColor = x, depthLevel3Color, depthLevel4);

                    Debug.Log("achieved depth 3");

                }
                break;
            case 3:
                if (depth > depthLevel4)
                {
                    currentDepthLevel = 4;
                    _camera.backgroundColor = Color.black;
                    // DOTween.To(() => _camera.backgroundColor, x =>_camera.backgroundColor = x, Color.black, finishDepth);

                    Debug.Log($"achieved depth 4, fish collisions {fishCollisions.ToString()}");
                }
                break;
            case 4:
                if (depth > finishDepth)
                {
                    currentDepthLevel = 5;
                    VictoryScreen();
                    Debug.Log($"finishing depth, fish collisions {fishCollisions.ToString()}");
                }
                break;
            // default:
            //     Debug.LogWarning("out of depth range");
            //     break;
        }

    }

    private void VictoryScreen()
    {
        onVictoryConditionMet = true;
        victoryScreenFishHitLabel.text = "";
        _camera.transform.DOMoveY(victoryScreenCameraYValue, victoryScreenScrollSpeed).OnComplete(() =>
        {
            victoryScreenFishHitLabel.text = $"{fishCollisions.ToString()} fish collisions";
            // AudioManager.Instance.AmbienceMode();
        });
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