using DG.Tweening;
using UnityEngine;

public class FlowerComponent : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private Transform _transform;
    [SerializeField] private float rotationSpeed = 1f;

    
    // Start is called before the first frame update
    void Start()
    {
        _transform = transform;
        var startPosition = _transform.position;
        var newPosition = startPosition;
        newPosition.y -= 7f;
        _transform.position = newPosition;
        _transform.DOMove(startPosition, 60f);
    }

    // Update is called once per frame
    void Update()
    {
        _transform.Rotate(0, 0, Time.deltaTime * rotationSpeed);
    }
}