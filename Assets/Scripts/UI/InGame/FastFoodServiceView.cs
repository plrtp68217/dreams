using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class FastFoodServiceView : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private FastFoodInventory _inventory;
    [SerializeField] private InputService _input;
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _hud;
    [SerializeField] private GameObject _modal;
    [SerializeField] private TextMeshProUGUI _trayTitle;
    [SerializeField] private TextMeshProUGUI _pocketTitle;
    [SerializeField] private TextMeshProUGUI _ready;
    [SerializeField] private TextMeshProUGUI _prompt;
    [SerializeField] private UnityEngine.UI.Image[] _trayIcons;
    [SerializeField] private TextMeshProUGUI[] _slotLabels;
    [SerializeField] private UnityEngine.UI.Image _pocketIcon;
    [SerializeField] private UnityEngine.UI.Button[] _trayButtons;
    [SerializeField] private UnityEngine.UI.Button _pocketButton;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _message;
    [SerializeField] private UnityEngine.UI.Image _art;
    [SerializeField] private UnityEngine.UI.Button[] _actions;
    [SerializeField] private TextMeshProUGUI[] _actionLabels;
    [SerializeField] private UnityEngine.UI.Button _close;
    [SerializeField] private TextMeshProUGUI _closeLabel;
    [SerializeField] private GameObject _keypad;
    [SerializeField] private TMP_InputField _code;
    [SerializeField] private UnityEngine.UI.Button[] _digitButtons;
    [SerializeField] private GameObject _mixing;
    [SerializeField] private UnityEngine.UI.Slider[] _syrups;
    [SerializeField] private UnityEngine.UI.Button[] _syrupLess;
    [SerializeField] private UnityEngine.UI.Button[] _syrupMore;
    [SerializeField] private TextMeshProUGUI[] _syrupLabels;
    [SerializeField] private Sprite[] _stationArt;
    [SerializeField] private Sprite[] _oilFrames;
    [SerializeField] private Sprite[] _pourFrames;
    [SerializeField] private Sprite[] _mirrorFrames;
    [SerializeField] private Sprite[] _fridgeFrames;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _drainSound;
    [SerializeField] private AudioClip _pourSound;
    [SerializeField] private AudioClip _fridgeSound;
    [SerializeField] private AudioClip _pickupSound;
    [SerializeField, Min(.1f)] private float _animationDuration = 2.4f;

    private FastFoodServiceStation[] _stations;
    private FastFoodStationKind _station;
    private bool _wasBlocked;
    private bool _isInspecting;
    private bool _busy;
    private bool _audioPaused;
    private Coroutine _animation;
    private float _lastPrompt;
    private FastFoodOrderItem _inspectItem;
    private bool _inspectReward;
    public bool IsOpen => _modal != null && _modal.activeSelf;
    public bool IsBusy => _busy;

    private void Awake()
    {
        _modal.SetActive(false);
        _hud.SetActive(false);
        _close.onClick.AddListener(Close);
        for (int i = 0; i < _trayButtons.Length; i++)
        {
            int slot = i;
            _trayButtons[i].onClick.AddListener(() => Inspect((FastFoodOrderItem)slot));
        }
        _pocketButton.onClick.AddListener(InspectRecord);
        for (int i = 0; i < _digitButtons.Length; i++)
        {
            int digit = i;
            _digitButtons[i].onClick.AddListener(() =>
            {
                if (!CanUseUI) return;
                if (digit == 10) _code.text = "";
                else if (digit == 11) SubmitCode();
                else if (_code.text.Length < 4) _code.text += digit.ToString();
            });
        }
        _code.onSubmit.AddListener(_ => SubmitCode());
        foreach (var slider in _syrups) slider.onValueChanged.AddListener(_ => UpdateSyrups());
        for (int i = 0; i < _syrups.Length; i++)
        {
            int channel = i;
            _syrupLess[i].onClick.AddListener(() => { if (CanUseUI) _syrups[channel].value--; });
            _syrupMore[i].onClick.AddListener(() => { if (CanUseUI) _syrups[channel].value++; });
        }
    }

    private void OnEnable()
    {
        _service.Changed += Refresh;
        _inventory.Changed += Refresh;
        LocalizedDialogueSystem.LanguageChanged += Refresh;
    }
    private void Start()
    {
        _stations = FindObjectsByType<FastFoodServiceStation>(FindObjectsSortMode.None);
        Refresh();
    }
    private void OnDisable()
    {
        _service.Changed -= Refresh;
        _inventory.Changed -= Refresh;
        LocalizedDialogueSystem.LanguageChanged -= Refresh;
        Close();
    }

    private bool CanUseUI => IsOpen && Time.timeScale > 0f && !_busy && _player.IsAlive;
    private static string T(string key) => FacelessOrderService.Text(key);
    public FastFoodServiceStation NearestStation()
    {
        if (_stations == null) return null;
        return _stations.Where(s => s != null && s.Available)
            .OrderBy(s => Mathf.Abs(s.transform.position.x - _player.transform.position.x)).FirstOrDefault();
    }
    public void ShowPrompt(FastFoodServiceStation station)
    {
        _prompt.text = "E — " + T("service_station_" + (int)station.Kind);
        _prompt.gameObject.SetActive(true);
        _lastPrompt = Time.time;
    }
    private void LateUpdate()
    {
        if (_prompt != null && (IsOpen || _input.IsBlocked || Time.timeScale <= 0 || Time.time - _lastPrompt > .08f))
            _prompt.gameObject.SetActive(false);
        if (_audio != null && _audio.isPlaying && Time.timeScale <= 0)
        { _audio.Pause(); _audioPaused = true; }
        else if (_audio != null && _audioPaused && Time.timeScale > 0)
        { _audio.UnPause(); _audioPaused = false; }
        if (IsOpen && !_player.IsAlive) Close();
    }
    private void Update()
    {
        if (Time.timeScale <= 0 || _input.IsBlocked || IsOpen) return;
        for (int i = 0; i < 4; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) Inspect((FastFoodOrderItem)i);
        if (Input.GetKeyDown(KeyCode.I) && _inventory.HasMedicalRecord) InspectRecord();
    }

    public void Open(FastFoodServiceStation station)
    {
        if (IsOpen || !_service.CanInteract || _input.IsBlocked || Time.timeScale <= 0) return;
        _station = station.Kind;
        _isInspecting = false;
        BeginModal();
        _message.text = "";
        RefreshStation();
    }
    private void BeginModal()
    {
        _wasBlocked = _input.IsBlocked;
        _input.Block();
        if (_player.Rigidbody != null) _player.Rigidbody.linearVelocity = Vector2.zero;
        _modal.SetActive(true);
        _prompt.gameObject.SetActive(false);
    }
    public void Close()
    {
        if (!IsOpen) return;
        if (_animation != null) StopCoroutine(_animation);
        _animation = null;
        _busy = false;
        _audioPaused = false;
        foreach (var slider in _syrups) if (slider != null) slider.interactable = true;
        if (_audio != null) _audio.Stop();
        _modal.SetActive(false);
        if (!_wasBlocked && _input != null) _input.Unblock();
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    public void Inspect(FastFoodOrderItem item)
    {
        if (!_inventory.Has(item) || IsOpen || _input.IsBlocked || Time.timeScale <= 0) return;
        _inspectItem = item;
        _inspectReward = false;
        _isInspecting = true;
        BeginModal();
        RefreshInspection();
    }
    public void InspectRecord()
    {
        if (!_inventory.HasMedicalRecord || IsOpen || _input.IsBlocked || Time.timeScale <= 0) return;
        _inspectReward = true;
        _isInspecting = true;
        BeginModal();
        RefreshInspection();
    }
    private void RefreshInspection()
    {
        ResetControls();
        _title.text = T(_inspectReward ? "service_record_title" : "service_item_" + (int)_inspectItem);
        _description.text = T(_inspectReward ? "service_record_text" : "service_item_description_" + (int)_inspectItem);
        _description.rectTransform.sizeDelta = new Vector2(350, 380);
        _art.sprite = _inspectReward ? _inventory.MedicalRecordIcon : _inventory.Icon(_inspectItem);
        _message.text = "";
    }

    public void Refresh()
    {
        if (_hud == null) return;
        _hud.SetActive(_service.IsActive || _service.IsCompleted);
        _trayTitle.text = T("service_tray") + " · " + _inventory.TrayCount + "/4";
        _pocketTitle.text = T("service_pocket");
        _ready.text = _service.IsCompleted ? T("service_complete") : _inventory.IsOrderReady ? T("service_ready") : "";
        for (int i = 0; i < 4; i++)
        {
            bool has = _inventory.Has((FastFoodOrderItem)i);
            _trayIcons[i].sprite = _inventory.Icon((FastFoodOrderItem)i);
            _trayIcons[i].enabled = has;
            _slotLabels[i].text = has ? (i + 1).ToString() : (i + 1) + "  ?";
            _trayButtons[i].interactable = has;
        }
        _pocketIcon.sprite = _inventory.MedicalRecordIcon;
        _pocketIcon.enabled = _inventory.HasMedicalRecord;
        _pocketButton.interactable = _inventory.HasMedicalRecord;
        if (IsOpen && !_busy)
        {
            if (_isInspecting) RefreshInspection(); else RefreshStation();
        }
    }

    private void ResetControls()
    {
        _keypad.SetActive(false);
        _mixing.SetActive(false);
        foreach (var button in _actions) { button.gameObject.SetActive(false); button.onClick.RemoveAllListeners(); }
        _closeLabel.text = T("service_close");
    }
    private void Action(int index, string label, Action action, bool enabled = true)
    {
        _actions[index].gameObject.SetActive(true);
        _actionLabels[index].text = T(label);
        _actions[index].interactable = enabled;
        _actions[index].onClick.AddListener(() => { if (CanUseUI) action(); });
    }
    private void Act(int index, string label, FacelessServiceAction action)
        => Action(index, label, () => Do(action), _service.CanPerform(action));

    private void RefreshStation()
    {
        ResetControls();
        _description.rectTransform.sizeDelta = new Vector2(350, 112);
        _title.text = T("service_station_" + (int)_station);
        _art.sprite = _stationArt[(int)_station];
        if (_station == FastFoodStationKind.Fryer && _service.OilDrained && _oilFrames.Length > 0) _art.sprite = _oilFrames[_oilFrames.Length - 1];
        if (_station == FastFoodStationKind.Dispenser && _service.DrinkReady && _pourFrames.Length > 0) _art.sprite = _pourFrames[_pourFrames.Length - 1];
        if (_station == FastFoodStationKind.Mirror && _service.MirrorLooked && _mirrorFrames.Length > 0) _art.sprite = _mirrorFrames[_mirrorFrames.Length - 1];
        if (_station == FastFoodStationKind.Fridge && _service.FridgeUnlocked && _fridgeFrames.Length > 0) _art.sprite = _fridgeFrames[_fridgeFrames.Length - 1];
        switch (_station)
        {
            case FastFoodStationKind.Fryer:
                _description.text = T(!_service.PowerOff ? "service_fryer_powered" : !_service.FuseRemoved ? "service_find_fuse" : !_service.OilDrained ? "service_drain_hint" : "service_fryer_empty");
                if (!_service.PowerOff) Act(0, "service_power_off", FacelessServiceAction.PowerOff);
                else if (!_service.FuseRemoved) Action(0, "service_find_fuse_button", Close);
                else if (!_service.OilDrained) Act(0, "service_drain", FacelessServiceAction.DrainOil);
                else Act(0, "service_take_burger", FacelessServiceAction.TakeBurger);
                break;
            case FastFoodStationKind.FuseBox:
                _description.text = T(_service.PowerOff ? "service_fuse_safe" : "service_fuse_danger");
                Act(0, "service_remove_fuse", FacelessServiceAction.RemoveFuse);
                break;
            case FastFoodStationKind.Fridge:
                _description.text = T(_service.FridgeUnlocked ? "service_fridge_open" : "service_fridge_locked");
                if (!_service.FridgeUnlocked) _keypad.SetActive(true);
                else Act(0, "service_take_fries", FacelessServiceAction.TakeFries);
                break;
            case FastFoodStationKind.Dispenser:
                var recipe = _service.Recipe;
                _description.text = T("service_recipe") + "\n" + recipe.x.ToString("0") + "% / " + recipe.y.ToString("0") + "% / " + recipe.z.ToString("0") + "%";
                _mixing.SetActive(!_service.DrinkReady && !_inventory.Has(FastFoodOrderItem.Drink));
                UpdateSyrups();
                if (_service.DrinkReady) Act(0, "service_take_drink", FacelessServiceAction.TakeDrink);
                else Action(0, "service_pour", Pour, _service.CanPerform(FacelessServiceAction.PourDrink));
                break;
            case FastFoodStationKind.Mirror:
                _description.text = T(_service.MirrorLooked ? "service_mirror_toy" : "service_mirror_dark");
                if (!_service.MirrorLooked) Act(0, "service_look", FacelessServiceAction.LookInMirror);
                else Act(0, "service_take_toy", FacelessServiceAction.TakeToy);
                break;
            case FastFoodStationKind.Guest:
                _description.text = T(_inventory.IsOrderReady ? "service_deliver_hint" : "service_missing_items");
                Action(0, "service_deliver", () => _service.BeginServing(), _inventory.IsOrderReady);
                break;
        }
    }

    private void SubmitCode()
    {
        if (!CanUseUI || _station != FastFoodStationKind.Fridge) return;
        if (_service.EnterFridgeCode(_code.text))
        {
            _message.text = T("service_code_ok");
            _animation = StartCoroutine(OpenFridge());
        }
        else { _message.text = T("service_code_wrong"); _code.text = ""; }
    }
    private void UpdateSyrups()
    {
        if (IsOpen && !_busy && _station == FastFoodStationKind.Dispenser) _message.text = "";
        for (int i = 0; i < 3; i++)
            _syrupLabels[i].text = T("service_syrup_" + i) + "  " + Mathf.RoundToInt(_syrups[i].value) + "%";
    }
    private void Pour()
    {
        var values = new Vector3(_syrups[0].value, _syrups[1].value, _syrups[2].value);
        if (!_service.CorrectRecipe(values)) { _message.text = T("service_recipe_wrong"); return; }
        Do(FacelessServiceAction.PourDrink);
    }
    private void Do(FacelessServiceAction action)
    {
        if (!_service.CanPerform(action)) return;
        if (action == FacelessServiceAction.DrainOil || action == FacelessServiceAction.PourDrink || action == FacelessServiceAction.LookInMirror)
        {
            _animation = StartCoroutine(Animate(action));
            return;
        }
        if (_service.Perform(action))
        {
            PlaySound(_pickupSound);
            _message.text = T(action == FacelessServiceAction.PowerOff ? "service_power_safe" : action == FacelessServiceAction.RemoveFuse ? "service_fuse_removed" : "service_added");
            RefreshStation();
        }
    }
    private IEnumerator Animate(FacelessServiceAction action)
    {
        _busy = true;
        foreach (var button in _actions) button.interactable = false;
        foreach (var slider in _syrups) slider.interactable = false;
        var frames = action == FacelessServiceAction.DrainOil ? _oilFrames : action == FacelessServiceAction.PourDrink ? _pourFrames : _mirrorFrames;
        PlaySound(action == FacelessServiceAction.DrainOil ? _drainSound : action == FacelessServiceAction.PourDrink ? _pourSound : null);
        _message.text = T(action == FacelessServiceAction.DrainOil ? "service_draining" : action == FacelessServiceAction.PourDrink ? "service_pouring" : "service_looking");
        float elapsed = 0f;
        while (elapsed < _animationDuration)
        {
            if (frames != null && frames.Length > 0)
                _art.sprite = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(elapsed / _animationDuration * frames.Length))];
            elapsed += Time.deltaTime;
            yield return null;
        }
        _busy = false;
        _animation = null;
        foreach (var slider in _syrups) slider.interactable = true;
        if (_audio != null) _audio.Stop();
        _service.Perform(action);
        _message.text = T("service_action_done");
        RefreshStation();
    }
    private IEnumerator OpenFridge()
    {
        _busy = true;
        foreach (var button in _actions) button.interactable = false;
        PlaySound(_fridgeSound);
        float elapsed = 0;
        while (elapsed < _animationDuration)
        {
            if (_fridgeFrames.Length > 0) _art.sprite = _fridgeFrames[Mathf.Min(_fridgeFrames.Length - 1, Mathf.FloorToInt(elapsed / _animationDuration * _fridgeFrames.Length))];
            elapsed += Time.deltaTime;
            yield return null;
        }
        _busy = false;
        _animation = null;
        if (_audio != null) _audio.Stop();
        RefreshStation();
    }
    private void PlaySound(AudioClip clip)
    {
        if (_audio == null || clip == null) return;
        _audio.Stop(); _audio.clip = clip; _audio.Play();
        _audioPaused = false;
    }
}
