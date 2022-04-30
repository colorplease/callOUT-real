using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SpookyWhiteMan : MonoBehaviour
{
    [SerializeField]Transform target;
    [SerializeField]MultiAimConstraint sus;
    
    // Start is called before the first frame update
    void Awake()
    {
        target = GameObject.Find("LocalGamePlayer").transform;
        sus.data.sourceObjects = new WeightedTransformArray{new WeightedTransform(target,1)};
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
