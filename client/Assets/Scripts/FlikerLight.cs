using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class FlikerLight : MonoBehaviour
{
    [SerializeField]
    Light2D flickeringLight;

    [SerializeField]
    private float _min;
    [SerializeField]
    private float _max;
    [SerializeField]
    private float _rmin;
    [SerializeField]
    private float _rmax;

    void Update()
    {
        float lickerIntensity = Random.Range(_min, _max);
        float lickerRange = Random.Range(_rmin, _rmax);
        flickeringLight.intensity = lickerIntensity;
        flickeringLight.pointLightInnerRadius = lickerRange;
    }
}