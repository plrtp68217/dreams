using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class BrokenFlickerLight2D : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private Light2D _targetLight;
    [SerializeField] private bool _isBroken = true;

    [Header("Параметры мерцания")]
    [SerializeField][Range(0.01f, 2f)] private float _minFrequency = 0.05f;
    [SerializeField][Range(0.01f, 2f)] private float _maxFrequency = 0.3f;
    [SerializeField][Range(0f, 1f)] private float _minIntensity = 0f;
    [SerializeField][Range(0f, 2f)] private float _maxIntensity = 1f;

    private float _originalIntensity;
    private Coroutine _flickerCoroutine;

    private void Start()
    {
        if (_targetLight == null)
            _targetLight = GetComponent<Light2D>();

        if (_targetLight == null)
        {
            Debug.LogError($"Нет компонента Light2D на объекте {gameObject.name}!");
            return;
        }

        _originalIntensity = _targetLight.intensity;

        if (_isBroken)
            StartFlicker();
    }

    private void OnValidate()
    {
        if (_targetLight == null)
            _targetLight = GetComponent<Light2D>();
    }

    private void OnDisable()
    {
        if (_flickerCoroutine != null)
            StopCoroutine(_flickerCoroutine);
    }

    public void StartFlicker()
    {
        if (_flickerCoroutine != null)
            return;

        _flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    public void StopFlicker()
    {
        if (_flickerCoroutine != null)
        {
            StopCoroutine(_flickerCoroutine);
            _flickerCoroutine = null;
        }

        if (_targetLight != null)
        {
            _targetLight.intensity = _originalIntensity;
            _targetLight.enabled = true;
        }
    }

    public void SetBroken(bool broken)
    {
        _isBroken = broken;

        if (_isBroken)
            StartFlicker();
        else
            StopFlicker();
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            float delay = Random.Range(_minFrequency, _maxFrequency);
            float targetIntensity;

            if (Random.value < 0.3f)
                targetIntensity = 0f;
            else
                targetIntensity = Random.Range(_minIntensity, _maxIntensity);

            _targetLight.intensity = targetIntensity;
            _targetLight.enabled = targetIntensity > 0.01f;

            yield return new WaitForSeconds(delay);
        }
    }
}