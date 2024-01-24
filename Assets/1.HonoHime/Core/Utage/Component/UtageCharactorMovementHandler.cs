using System.Linq;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine.AI;
using MovePointInfo = UtageCharacter.MovePointInfo;
using MoveType = UtageCharacter.MoveType;
using System;

[ExecuteAlways]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(UtageCharacter))]
public class UtageCharactorMovementHandler : MonoBehaviour
{
    public enum Mode
    {
        Runtime,
        EditMode,
        SliderMode
    }
    public Mode mode;

    [SerializeField] UtageCharacter character;
    [SerializeField] Animator anim;
    [SerializeField] float localRotScale = 1;
    [SerializeField] float editMoveAnimLerpSpd = 2;

    [HideIf("IsPathNull")]
    [SerializeField][Range(0.0f, 1.0f)]
    [OnValueChanged("SliderValueUpdate")]
    float slider;
    float progress;
    public MovePointInfo[] PointInfos;

    private Vector3 desiredDir;
    private float desiredAngle;

#if UNITY_EDITOR
    private EditorCoroutine coroutine_move_edit, coroutine_rotate_edit;
#endif
    
    
    private Coroutine coroutine_move, coroutine_rotate;
    private bool rotating = false;
    private NavMeshPath path;
    private bool IsPathNull
    {
        get
        {
            return path == null || path.corners.Length <= 1;
        }
    }

    private void Start()
    {
        if (character == null)
            character = GetComponent<UtageCharacter>();
        if (anim == null)
        {
            if (character.Anim != null)
                anim = character.Anim;
            else
                anim = GetComponent<Animator>();
        }
    }
    private void Update()
    {
        if (Application.isPlaying)
        {
            //anim.Update(Time.deltaTime);
        }
        else if (mode == Mode.EditMode && !Application.isPlaying)
        {
            anim.Update(Time.deltaTime);
        }
    }
    private void OnAnimatorMove()
    {
        if (rotating)
        {
            transform.rotation *= Quaternion.LerpUnclamped(Quaternion.identity, anim.deltaRotation, localRotScale * (desiredAngle / 90));
            transform.position += anim.deltaPosition;
        }
    }
    private void SliderValueUpdate()
    {
        EasePath(slider, progress);
    }
    private void EasePath(float _slider, float _progress)
    {
        if (_slider <= 0 && _slider >= 1)
            return;
        if (path == null || path.corners.Length <= 1)
            return;

        float[] timeCosts = CalculateTimeCostPerSeq(path.corners, character.WalkSpd);
        float[] percentages = CalculatePercentageInOne(timeCosts);
        int curSeqIndex = FindPercentageSeqIndex(_progress, percentages);
        float currentProgress = CalculateCurrentPercentage(_progress, percentages);
        transform.position = Vector3.Lerp(path.corners[curSeqIndex], path.corners[curSeqIndex + 1], currentProgress);
        var animUpdateSpd = (slider - progress) * 5;
        anim.Update(animUpdateSpd);
        anim.SetFloat("Velocity", character.WalkSpd);
        progress = slider;
    }
    private float CalculateCurrentPercentage(float currentProgress, float[] percentages)
    {
        float currentPercentage = 0.0f;
        float accumulatedPercentage = 0.0f;

        for (int i = 0; i < percentages.Length; i++)
        {
            float segmentProgress = percentages[i];
            accumulatedPercentage += segmentProgress;

            if (currentProgress <= accumulatedPercentage)
            {
                float segmentStart = accumulatedPercentage - segmentProgress;
                float segmentEnd = accumulatedPercentage;
                currentPercentage = (currentProgress - segmentStart) / (segmentEnd - segmentStart);
                break;
            }
        }

        return currentPercentage;
    }
    private int FindPercentageSeqIndex(float _targetPercentage, float[] percentages)
    {
        float sum = 0.0f;
        for (int i = 0; i < percentages.Length; i++)
        {
            sum += percentages[i];
            if (_targetPercentage <= sum)
            {
                return i;
            }
        }
        return percentages.Length - 1;
    }
    private float[] CalculatePercentageInOne(float[] values)
    {
        float totalT = 0.0f;
        Array.ForEach(values, value => totalT += value);
        float[] percentages = new float[values.Length];
        for (int i = 0; i < percentages.Length; i++)
        {
            percentages[i] = values[i] / totalT;
        }
        return percentages;
    }
    private float[] CalculateTimeCostPerSeq(Vector3[] _points, float _moveSpeed)
    {
        int numSeqs = _points.Length - 1;
        float[] moveTimes = new float[numSeqs];
        for (int i = 0; i < numSeqs; i++)
        {
            moveTimes[i] = CalculateMoveTime(_points[i], _points[i + 1], _moveSpeed);
        }
        return moveTimes;
    }
    private float CalculateMoveTime(Vector3 _pointA, Vector3 _pointB, float _moveSpd)
    {
        float dis = Vector3.Distance(_pointA, _pointB);
        float timeCost = dis / _moveSpd;
        return timeCost;
    }
    [Button]
    public void SetMove(int _index)
    {
        if (PointInfos.Length <= _index || _index <0)
        {
            Debug.LogError("Index assigned error.");
            return;
        }
        character.SetMove(PointInfos[_index]);
    }
    public void SetMove(int _index, MoveType _type, bool _reLocate)
    {
        MovePointInfo info = new MovePointInfo();
        info.StartPoint = PointInfos[_index].StartPoint;
        info.EndPoint = PointInfos[_index].EndPoint;
        info.MoveType = _type;
        info.needLocateStart = _reLocate;
        CalculatePath(_index);
        if (_reLocate)
            character.Agent.transform.position = path.corners.FirstOrDefault();

        character.SetMove(info);
    }
    public void SetMoveResetPos(int _index, MoveType _type, bool relocate = true)
    {
        //Debug.LogError("SetMoveResetPos");
        MovePointInfo info = new MovePointInfo();
        info.StartPoint = PointInfos[_index].StartPoint;
        info.EndPoint = PointInfos[_index].EndPoint;
        info.MoveType = _type;
        if (relocate)
            transform.position = info.StartPoint.position;
        CalculatePath(info);
        info.StartPoint.position = path.corners.FirstOrDefault();
        info.EndPoint.position = path.corners.Last();
        if (relocate)
        {
            transform.position = info.StartPoint.position;
            transform.rotation = info.StartPoint.rotation;
        }

        //if (!IsNeedTurnRotate(path.corners[1], info))
        //{
            if (Application.isPlaying)
            {
                character.SetMoveResetStartPoint(info);
            }
            else
            {
#if UNITY_EDITOR
                coroutine_move_edit = EditorCoroutineUtility.StartCoroutine(MoveSimulateCoroutine(info.MoveType), this);
#endif
            }
        //}
    }

    [ShowIf("mode", Mode.EditMode)]
    [Button]
    public void SimulateNavMove(int _index)
    {
        if (Application.isPlaying)
            return;

        if (PointInfos.Length <= _index || _index < 0)
        {
            Debug.LogError("Index assigned error.");
            return;
        }
        var startPos = Vector3.ProjectOnPlane(PointInfos[_index].StartPoint.position, Vector3.up);
        character.transform.position = startPos;
        character.transform.rotation = PointInfos[_index].StartPoint.rotation;
        SimulateNavMove(PointInfos[_index]);
    }
    private void SimulateNavMove(MovePointInfo _info)
    {
        if (Application.isPlaying)
            return;

        path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, _info.EndPoint.position, NavMesh.AllAreas, path);
        if (path.corners.Length >= 1)
        {
            if (!IsNeedTurnRotate(path.corners[1], _info))
            {
#if UNITY_EDITOR
                coroutine_move_edit = EditorCoroutineUtility.StartCoroutine(MoveSimulateCoroutine(_info.MoveType), this);
#endif
            }
        }
    }
    [Button]
    public void StopMove(bool relocate = true)
    {
        character.StopMove(relocate);
    }
    [Button]
    public void TurnRotate(float angle)
    {
        desiredDir = Quaternion.Euler(0, angle, 0) * transform.forward;
        //desiredDir = Vector3.ProjectOnPlane(desiredDir, Vector3.up);
        var signedAngle = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
        desiredAngle = Mathf.Abs(signedAngle);

        if (signedAngle < 0)
            anim.CrossFade("TurnLeft", 0.1f);
        else
            anim.CrossFade("TurnRight", 0.1f);

        if (Application.isPlaying)
        {
            rotating = true;
            coroutine_rotate = StartCoroutine(RotateCoroutine(desiredDir));
        }
        else
        {
            rotating = true;
#if UNITY_EDITOR
            coroutine_rotate_edit = EditorCoroutineUtility.StartCoroutine(RotateCoroutine(desiredDir), this);
#endif
        }
    }
    private bool IsNeedTurnRotate(Vector3 _targetPos, MovePointInfo _info)
    {
        bool needTurn = false;
        desiredDir = (_targetPos - transform.position).normalized;
        desiredDir = Vector3.ProjectOnPlane(desiredDir, Vector3.up);
        var signedAngle = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
        desiredAngle = Mathf.Abs(signedAngle);
        if (Mathf.Abs(signedAngle) > 5)
        {
            needTurn = true;
            if (signedAngle < 0)
                anim.CrossFade("TurnLeft", 0.1f);
            else
                anim.CrossFade("TurnRight", 0.1f);

            if (Application.isPlaying)
            {
                coroutine_rotate = StartCoroutine(RotateSimulateCoroutine(desiredDir, _info, true));
            }
            else
            {
#if UNITY_EDITOR
                coroutine_rotate_edit = EditorCoroutineUtility.StartCoroutine(RotateSimulateCoroutine(desiredDir, _info, true), this);
#endif
            }
        }

        Debug.LogError("IsNeedTurnRotate: " + needTurn);
        return needTurn;
    }
    [ShowIf("mode", Mode.SliderMode)]
    [Button]
    private void CalculatePath(int _index)
    {
        if (PointInfos.Length <= _index || _index < 0)
        {
            Debug.LogError("Index assigned error.");
            return;
        }
        CalculatePath(PointInfos[_index]);
    }
    private void CalculatePath(MovePointInfo _info)
    {
        path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, _info.EndPoint.position, NavMesh.AllAreas, path);
    }
    [Button]
    private void ClearPath()
    {
        path = new NavMeshPath();

#if UNITY_EDITOR
        if (coroutine_move_edit != null)
            EditorCoroutineUtility.StopCoroutine(coroutine_move_edit);
        if (coroutine_rotate_edit != null)
            EditorCoroutineUtility.StopCoroutine(coroutine_rotate_edit);
#endif
        if (coroutine_rotate != null)
            StopCoroutine(coroutine_rotate);
        if (coroutine_move != null)
            StopCoroutine(coroutine_move);

    }
    IEnumerator RotateSimulateCoroutine(Vector3 _dir, MovePointInfo _info, bool _moveAfterRotate)
    {
        Quaternion targetRot = Quaternion.LookRotation(_dir, Vector3.up);
        while (true)
        {
            Debug.DrawLine(transform.position, transform.position + _dir, Color.yellow);
            if (Vector3.Angle(transform.forward, _dir) < 4)
                break;
            yield return null;
        }
        transform.rotation = targetRot;

#if UNITY_EDITOR
        coroutine_rotate_edit = null;
        coroutine_rotate = null;
#endif

        rotating = false;

        if (_moveAfterRotate)
        {
            if (Application.isPlaying)
            {
                character.SetMoveResetStartPoint(_info);
                //StartCoroutine(MoveSimulateCoroutine(_type));
            }
            else
            {
#if UNITY_EDITOR
                coroutine_move_edit = EditorCoroutineUtility.StartCoroutine(MoveSimulateCoroutine(_info.MoveType), this);
#endif
            }
        }
    }
    IEnumerator RotateCoroutine(Vector3 _dir)
    {
        Quaternion targetRot = Quaternion.LookRotation(_dir, Vector3.up);
        while (true)
        {
            Debug.DrawLine(transform.position, transform.position + _dir, Color.yellow);
            if (Vector3.Angle(transform.forward, _dir) < 4)
                break;
            yield return null;
        }
        transform.rotation = targetRot;
        coroutine_rotate = null;
        rotating = false;
    }
    IEnumerator MoveSimulateCoroutine(MoveType _moveType)
    {
        int index = 1;
        float dis = 0;
        float spd = 0;
        while (true)
        {
            dis = Vector3.Distance(transform.position, path.corners[index]);
            if (dis <= character.Agent.stoppingDistance)
            {
                index++;
                if (index >= path.corners.Length)
                    break;
            }
            var faceDir = (path.corners[index] - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(faceDir);
            transform.position = Vector3.MoveTowards(transform.position, path.corners[index], Time.deltaTime * character.WalkSpd);

            spd = Mathf.Lerp(spd, character.WalkSpd, Time.deltaTime * editMoveAnimLerpSpd);
            anim.SetFloat("Velocity", spd);
            yield return null;
        }
        while (spd > 0)
        {
            spd = Mathf.Lerp(spd, 0, Time.deltaTime * editMoveAnimLerpSpd);
            anim.SetFloat("Velocity", spd);
            yield return null;
        }
        ClearPath();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (path != null && path.corners.Length > 0)
        {
            for (int i = 0; i < path.corners.Length; i++)
            {
                Gizmos.DrawWireSphere(path.corners[i], 0.2f);
                if (i < path.corners.Length - 1)
                    Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
            }
        }
    }
}
