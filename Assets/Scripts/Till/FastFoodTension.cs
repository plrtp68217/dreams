using System.Collections;
using TMPro;
using UnityEngine;

public class FastFoodTension : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private FastFoodServiceView _view;
    [SerializeField] private InputService _input;
    [SerializeField] private Player _player;
    [SerializeField] private SpriteRenderer _clown;
    [SerializeField] private TextMeshProUGUI _caption;
    [SerializeField] private TextMeshProUGUI _levelLabel;
    [SerializeField] private Vector2 _interval = new Vector2(35f, 55f);
    [SerializeField, Min(1f)] private float _duration = 4.5f;
    private float _elapsed;
    private float _remaining;
    private int _visits;
    private Coroutine _appearance;
    public int Level => Mathf.Clamp(1 + Mathf.FloorToInt(_elapsed / 90f), 1, 3);

    private void Awake()
    {
        _remaining = Random.Range(_interval.x, _interval.y);
        _clown.enabled = false;
        _caption.gameObject.SetActive(false);
    }
    private void OnEnable() => LocalizedDialogueSystem.LanguageChanged += RefreshCaption;
    private void RefreshCaption()
    {
        if (_visits > 0) _caption.text = FacelessOrderService.Text("service_clown_" + ((_visits - 1) % 3));
    }
    private void Update()
    {
        if (!_service.IsActive || _service.IsCompleted || _service.IsServing || !_player.IsAlive)
        {
            if (_appearance != null) StopCoroutine(_appearance);
            _appearance = null;
            _clown.enabled = false;
            _caption.gameObject.SetActive(false);
            _levelLabel.text = "";
            return;
        }
        if (Time.timeScale <= 0) return;
        _elapsed += Time.deltaTime;
        _remaining -= Time.deltaTime;
        _levelLabel.text = FacelessOrderService.Text("service_tension") + " " + Level + "/3";
        if (_remaining <= 0 && _appearance == null && !_view.IsOpen && !_input.IsBlocked)
            _appearance = StartCoroutine(Appear());
    }
    private IEnumerator Appear()
    {
        _visits++;
        _caption.text = FacelessOrderService.Text("service_clown_" + ((_visits - 1) % 3));
        _caption.gameObject.SetActive(true);
        var camera = Camera.main;
        float halfWidth = camera.orthographicSize * camera.aspect;
        Vector3 target = new Vector3(camera.transform.position.x + halfWidth * .8f, -.74f, -2f);
        target.x = Mathf.Clamp(target.x, _player.transform.position.x - 1.1f, _player.transform.position.x + 1.1f);
        Vector3 from = target + Vector3.right * .35f;
        _clown.transform.position = from;
        _clown.flipX = _player.transform.position.x < target.x;
        _clown.enabled = true;
        float elapsed = 0;
        while (elapsed < _duration)
        {
            if (Time.timeScale <= 0f) { yield return null; continue; }
            elapsed += Time.deltaTime;
            _clown.transform.position = Vector3.Lerp(from, target, Mathf.Clamp01(elapsed / .5f));
            if (_view.IsOpen || _input.IsBlocked) break;
            yield return null;
        }
        _clown.enabled = false;
        _caption.gameObject.SetActive(false);
        _remaining = Random.Range(_interval.x, _interval.y) / Mathf.Sqrt(Level);
        _appearance = null;
    }
    private void OnDisable()
    {
        LocalizedDialogueSystem.LanguageChanged -= RefreshCaption;
        if (_appearance != null) StopCoroutine(_appearance);
        _appearance = null;
        if (_clown != null) _clown.enabled = false;
        if (_caption != null) _caption.gameObject.SetActive(false);
    }
}
