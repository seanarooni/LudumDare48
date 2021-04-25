using System.Collections;
using System.Collections.Generic;
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
    }

    // Update is called once per frame
    void Update()
    {
        _transform.Rotate(0, 0, Time.deltaTime * rotationSpeed);
    }
}
