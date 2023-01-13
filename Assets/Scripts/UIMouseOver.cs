using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMouseOver : MonoBehaviour
{
    // A reference to the UIManager singleton
    public UIManager uiManager;

    // A flag to track whether the pointer is currently over the object
    private bool pointerOver = false;

    void Update()
    {
        // Get the current position of the pointer
        Vector2 pointerPosition = Input.mousePosition;

        // Use a raycast to detect when the pointer intersects with the object
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the pointer has intersected with the object
            if (hit.collider == gameObject.GetComponent<Collider>())
            {
                // The pointer has intersected with the object
                if (!pointerOver)
                {
                    // The pointer was not previously over the object, so invoke the OnPointerEnter method
                    UIManager.instance.RegisterOverUI(true);
                    pointerOver = true;
                }
            }
            else
            {
                // The pointer has not intersected with the object
                if (pointerOver)
                {
                    // The pointer was previously over the object, so invoke the OnPointerExit method
                    UIManager.instance.RegisterOverUI(false); ;
                    pointerOver = false;
                }
            }
        }
    }
}
