using System.Collections;
using UnityEngine;

// Персонаж выходит из точки появления, подходит, говорит свои фразы
// и уходит в точку выхода (или остаётся на месте, если она не задана)
public class NpcVisit : MonoBehaviour
{
    [SerializeField] private InputService _inputService;

    [Header("Персонаж")]
    [SerializeField] private Npc _npc;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _standPoint;

    [Tooltip("Если не задана, персонаж остаётся на месте")]
    [SerializeField] private Transform _exitPoint;

    [Header("Диалог")]
    [SerializeField] private Dialog _subtitles;
    [SerializeField] private string[] _phraseIds;
    [SerializeField] private float _delayBeforeDialog = 0.3f;

    public IEnumerator Play()
    {
        _npc.transform.position = _spawnPoint.position;
        _npc.gameObject.SetActive(true);

        yield return _npc.WalkTo(_standPoint.position);
        yield return new WaitForSeconds(_delayBeforeDialog);

        if (_phraseIds.Length > 0)
        {
            yield return DialogPhrases.Play(_subtitles, _inputService, _phraseIds);
        }

        if (_exitPoint != null)
        {
            yield return _npc.WalkTo(_exitPoint.position);
            _npc.gameObject.SetActive(false);
        }
    }
}
