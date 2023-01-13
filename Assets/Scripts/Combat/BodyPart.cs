using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    public enum AreaType { HEAD, TORSO, LIMB, WEAK_SPOT }
    [SerializeField]
    private AreaType areaType;


}
