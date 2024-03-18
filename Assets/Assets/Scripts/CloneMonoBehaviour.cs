using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CloneMonoBehaviour : MonoBehaviour
{

    protected virtual void Reset()
    {
        this.LoadComponents();
        this.ResetValue();
    }


    protected virtual void Start()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    protected virtual void Update(){
        
    }

    protected virtual void Awake()
    {
        this.LoadComponents();
    }
    protected virtual void LoadComponents()
    {
        // For override
    }

    protected virtual void ResetValue()
    {
        // For override
    }

    protected virtual void OnEnable()
    {
        // For override
    }

}
