using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;
using DG.Tweening;
using Perform = UtageExtensions.UtageCharacterUtility.Perform;
using Face = UtageExtensions.UtageCharacterUtility.Face;
using Emoji = UtageExtensions.UtageCharacterUtility.Emoji;
#if UNITY_EDITOR
using UnityEditor.Animations;
using UnityEditor;
#endif

public class UtageCharacter : MonoBehaviour, ISerializationCallbackReceiver
{
    public enum MoveType
    {
        Walk,
        Run
    }
    [System.Serializable]
    public struct MovePointInfo
    {
        public bool needLocateStart;
        public MoveType MoveType;
        public Transform StartPoint;
        public Transform EndPoint;
    }
    public static List<string> CHARACTERIDS;
    [InfoBox("Enable this component when this npc need to recevice command from utage system.")]
    public UtageCharacterRegisterBoard board;
#if UNITY_EDITOR
    [ListToPopup(typeof(UtageCharacter), "CHARACTERIDS")]
#endif
    public string CharacterName;
    public bool UseDefaultAnim;
    [ShowIf("UseDefaultAnim")]
    public string DefaultAnimState;
    [SerializeField] float walkSpd = 1, runSpd = 1;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform emojiPivot;
    [SerializeField] private UtageCharactorMovementHandler moveHandler;
    //[ReadOnly] [SerializeField] EmojiHUD emojiHandler;
    [SerializeField] string[] loopStates;
    public Animator Anim => animator;
    public NavMeshAgent Agent => agent;
    public float WalkSpd => walkSpd;
    public float RunSpd => runSpd;

    public Transform EmojiPivot{ get { return emojiPivot; } }
    private bool isRegisted = false;
    private Coroutine triggerAnimCoroutine;
    private MovePointInfo currentPointInfo;
    void Start()
    {
        if (string.IsNullOrEmpty(CharacterName))
        {
            Debug.LogError(gameObject.name + " character name is empty.");
            return;
        }
        if (!isRegisted)
        {
            UtageCharaterCommandHandler.Inst.RegistCharacter(this);
            //emojiHandler = UtageEmojiHUDManager.Inst.RegistHandler(this);
            isRegisted = true;
        }
        if (UseDefaultAnim && !string.IsNullOrEmpty(DefaultAnimState))
            animator.CrossFade(DefaultAnimState, 0.1f);
    }
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(CharacterName))
        {
            Debug.LogError(gameObject.name + " character name is empty.");
            return;
        }
        if (UtageCharaterCommandHandler.Inst != null && !isRegisted)
        {
            UtageCharaterCommandHandler.Inst.RegistCharacter(this);
            //emojiHandler = UtageEmojiHUDManager.Inst.RegistHandler(this);
            isRegisted = true;
        }
        if (UseDefaultAnim && !string.IsNullOrEmpty(DefaultAnimState))
            animator.CrossFade(DefaultAnimState, 0.1f);
    }
    private void Update()
    {
        if (Application.isPlaying && animator != null)
        {
            animator.SetFloat("Velocity", agent.velocity.magnitude);
        }
    }

    public void SetPerformAction(string _perform, bool _flag = true)
    {
        Perform perAct;
        if (Enum.TryParse(_perform, out perAct))
            SetAniamtorProcess<Perform>(perAct, _flag);
        else
            Debug.LogError($"PerformAction isn't included '{_perform}'");
    }
    [Button]
    public void SetFaceAction(string _face, bool _flag = true)
    {
        Face face;
        if (Enum.TryParse(_face, out face))
        {
            SetAniamtorProcess<Face>(face, _flag);
            animator.CrossFade(_face, 0.1f);
        }
        else
            Debug.LogError($"PerformAction isn't included '{_face}'");
    }
    public void Speak(float duration = 3.5f)
    {
        StartCoroutine(SpeakInDuration(duration));
    }
    public void SetSpeak(bool _flag)
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            return;
        animator.SetBool("Speak", _flag);
    }
    IEnumerator SpeakInDuration(float duration) 
    {
        animator.SetBool("Speak", true);
        yield return new WaitForSeconds(duration);
        animator.SetBool("Speak", false);
    }
    public void SetEmojiAction(string _emoji, float _duration = 5)
    {
        //Emoji emoji;
        //if (Enum.TryParse(_emoji, out emoji))
        //    emojiHandler.Show(emoji, _duration);
        //else
        //    Debug.LogError($"PerformAction isn't included '{emoji}'");
    }
    public void HideEmoji(bool force = false)
    {
        //if (force)
        //{

        //}
        //else
        //{
        //    emojiHandler.Hide();
        //}
    }

    [Button]
    public void CrossIndexAnim(int _index, float _duration, float _fadeOutT, float _transition = 0.1f, int _layer = 0) 
    {
        string actionName = "Action" + _index;
        animator.CrossFade(actionName, _transition, _layer);
        if (_duration < 0)
            return;
        triggerAnimCoroutine = StartCoroutine(WaitActionDuration(_duration, _fadeOutT));
    }
    public void OverrideFace(bool _flag, float _duration = 0)
    {
        float overrideValue = (_flag) ? 0 : 1;
        if (_duration <= 0)
        {
            animator.SetLayerWeight(1, overrideValue);
        }
        else
        {
            float w = animator.GetLayerWeight(1);
            DOTween.To(() => w, x=>w=x, overrideValue, _duration)
                .OnUpdate(() => 
                {
                    animator.SetLayerWeight(1, w);
                });
        }
    }
    [Button]
    public void SetAnimAction(string _action, bool _flag, float _duration, float _fadeOutT = 0.1f, float _transition = 0.1f, int _layer = 0)
    {
        bool isLoopAnim = false;
        if (loopStates != null && loopStates.Length > 0)
        {
            for (int i = 0; i < loopStates.Length; i++)
            {
                if (loopStates[i] == _action)
                {
                    isLoopAnim = true;
                    break;
                }
            }
        }
        if (_flag)
        {
            if (_layer == 1)
            {
                SetFaceAction(_action, _flag);
            }
            else
            {
                animator.CrossFade(_action, _transition, _layer);
            }

            if (_duration > 0.0f)
            {
                if (triggerAnimCoroutine != null)
                    StopCoroutine(triggerAnimCoroutine);
                triggerAnimCoroutine = StartCoroutine(WaitActionDuration(_duration, _fadeOutT, isLoopAnim));
            }
        }
        else
        {
            if (isLoopAnim)
            {
                animator.SetTrigger("LoopFinsh");
            }
            else
            {
                ResetAnim(_fadeOutT, _layer);
            }
        }
    }

    IEnumerator WaitActionDuration(float _duration, float _fadeOutT, bool _isLoop = false)
    {
        yield return new WaitForSeconds(_duration);
        if (_isLoop)
            animator.SetTrigger("LoopFinsh");
        else
            ResetAnim(_fadeOutT);

        triggerAnimCoroutine = null;
    }
    public void CrossAnim(int _index, int layer = 0, float transitionDurtation = 0.1f)
    {
        string actionName = "Action" + _index;
        animator.CrossFade(actionName, transitionDurtation, layer);
    }
    [Button]
    public void ResetAnim(float _fadeOutT = 0.1f, int layer = -1)
    {
        if (triggerAnimCoroutine != null)
        {
            StopCoroutine(triggerAnimCoroutine);
            triggerAnimCoroutine = null;
        }

        if (layer == -1 || layer == 0)
        {
            animator.CrossFade("Locomotion", _fadeOutT, 0);
        }
        if (layer == -1 || layer == 1)
        {
            animator.CrossFade("Default", _fadeOutT, 1);
            foreach (var par in animator.parameters)
            {
                if (par.name != "Speak")
                    SetParameter(par, false);
            }
        }
    }
    private void SetAniamtorProcess<T>(T perAct, bool _flag = false)
    {
        string perActName = Enum.GetName(typeof(T), perAct);
        if (perActName == "FaceDefault")
        {
            foreach (var par in animator.parameters)
            {
                if (par.name != "Speak")
                    SetParameter(par, false);
            }

            return;
        }
        foreach (var par in animator.parameters)
        {
            if (par.name == perActName)
            {
                SetParameter(par, _flag);
            }
        }
    }
    private void SetParameter(AnimatorControllerParameter _par, object value)
    {
        switch (_par.type)
        {
            case AnimatorControllerParameterType.Float:
                animator.SetFloat(_par.name, Convert.ToSingle(value));
                break;
            case AnimatorControllerParameterType.Int:
                animator.SetInteger(_par.name, Convert.ToInt32(value));
                break;
            case AnimatorControllerParameterType.Bool:
                animator.SetBool(_par.name, Convert.ToBoolean(value));
                break;
            case AnimatorControllerParameterType.Trigger:
                bool bTmp = Convert.ToBoolean(value);
                if (bTmp) animator.SetTrigger(_par.name);
                break;
            default:
                break;
        }
    }

    [Button]
    public void SetMove(MovePointInfo _info)
    {
        //Debug.LogError("SetMove");
        switch (_info.MoveType)
        {
            case MoveType.Walk:
                agent.speed = walkSpd;
                break;
            case MoveType.Run:
                agent.speed = runSpd;
                break;
            default:
                break;
        }
        currentPointInfo = _info;
        agent.SetDestination(_info.EndPoint.position);
    }
    public void SetMove(string _type, int _index)
    {
        if (moveHandler == null)
        {
            Debug.LogError($"{gameObject.name} don't have moveHandler. Can't SetMove by excel.");
            return;
        }
        MovePointInfo info = new MovePointInfo();
        if (_type == "Walk")
            info.MoveType = MoveType.Walk;
        else if (_type == "Run")
            info.MoveType = MoveType.Run;
        info.StartPoint = moveHandler.PointInfos[_index].StartPoint;
        info.EndPoint = moveHandler.PointInfos[_index].EndPoint;
        SetMoveResetStartPoint(info);
    }
#if UNITY_EDITOR
    [Button]
    void GetLoopAnimStates()
    {
        List<string> stateList = new List<string>();
        if (animator != null && animator.runtimeAnimatorController is AnimatorController controller)
        {
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                AnimatorStateMachine stateMachine = layer.stateMachine;

                foreach (ChildAnimatorState state in stateMachine.states)
                {
                    AnimatorState animatorState = state.state;
                    if (string.IsNullOrEmpty(animatorState.tag))
                        continue;
                    if (!animatorState.tag.Contains("Loop") && !animatorState.tag.Contains("loop"))
                        continue;

                    stateList.Add(animatorState.name);
                    Debug.Log("Get Loop state: " + animatorState.name);
                }
            }
        }
        if (stateList.Count > 0)
            loopStates = stateList.ToArray();
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(gameObject);
    }
#endif
    public void SetMoveResetStartPoint(MovePointInfo _info)
    {
        //var startPoint = Vector3.ProjectOnPlane(_info.StartPoint.position, Vector3.up);
        //agent.transform.position = startPoint;
        //agent.transform.rotation = _info.StartPoint.transform.rotation;
        SetMove(_info);
    }
    public void StopMove(bool relocate = true)
    {
        if (relocate)
            agent.transform.position = currentPointInfo.EndPoint.position;

        agent.SetDestination(currentPointInfo.EndPoint.position);
    }

    private void OnDisable()
    {
        if (isRegisted)
        {
            UtageCharaterCommandHandler.Inst.UnregisterCharacter(this);
            isRegisted = false;
        }
    }
    private void OnDestroy()
    {
        if (isRegisted)
        {
            UtageCharaterCommandHandler.Inst.UnregisterCharacter(this);
            isRegisted = false;
        }
    }

    public void OnBeforeSerialize()
    {
        if (board != null && board.IDs != null && board.IDs.Length > 0)
        {
            CHARACTERIDS = board.IDs.ToList();
        }
        else
        {
            string[] str = { CharacterName };
            CHARACTERIDS = str.ToList();
        }
    }

    public void OnAfterDeserialize()
    {
        //throw new NotImplementedException();
    }
}