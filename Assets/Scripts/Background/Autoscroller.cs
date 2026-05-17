using System.Runtime.Serialization;
using UnityEngine;

public class Autoscroller : MonoBehaviour
{
    public float scrollSpeed = 2f;
    [SerializeField] private float scrollDirection = 1f; // 1 for down, -1 for up
    public Transform cameraTarget;
    private Vector3 initialOffset;

    void Awake()
    {
        if (cameraTarget == null && Camera.main != null)
        {
            cameraTarget = Camera.main.transform;
        }
    }

    void Start()
    {
        if (cameraTarget != null)
        {
            initialOffset = transform.position - cameraTarget.position;
        }
    }

    void Update()
    {
        Vector3 basePosition = transform.position;
        if (cameraTarget != null)
        {
            basePosition = cameraTarget.position + initialOffset;
        }

        Vector3 scroll = Vector3.down * scrollSpeed * Time.deltaTime * scrollDirection;
        transform.position = basePosition + scroll;
    }
}
