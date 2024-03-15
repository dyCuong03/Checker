using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MaterialManager : CloneMonoBehaviour
{
    [SerializeField] protected Material tileMaterial_White;
    public Material White_Material => tileMaterial_White;
    [SerializeField] protected Material tileMaterial_Black;
    public Material Black_Material => tileMaterial_Black;
}