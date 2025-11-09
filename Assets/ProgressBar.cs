using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ProgressBar : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject owner;
    public Vector2 offset = new(0, 0);
    public float progress;
    private Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        mainCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (owner != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(owner.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, screenPos, mainCamera, out Vector2 localPoint);
            (transform as RectTransform).localPosition = localPoint + offset;

        }
        slider.value = progress;
    }
}
