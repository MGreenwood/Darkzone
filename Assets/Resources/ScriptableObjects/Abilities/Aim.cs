using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ranged/Aim")]
public class Aim : Ability
{
    Camera cam;

    const int fovDelta = 20;

    private void OnEnable()
    {
        cam = Camera.main;
    }
    public override bool Cast()
    {
        cam.fieldOfView -= fovDelta;

       
        return true;
    }

    IEnumerator WaitForMouseUp()
    {
        
        yield return new WaitForEndOfFrame();
    }
}
