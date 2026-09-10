using System.Collections;
using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private BallKicker ballKicker;

    [Header("Top-Down Framing")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, 0f); 
    [SerializeField] private Vector3 fixedRotationEuler = new Vector3(90f, 0f, 0f); 
    [SerializeField] private float followSmoothTime = 0.2f;

    [Header("Return Timing")]
    [SerializeField] private float returnToPlayerDelay = 2f;

    private Transform currentTarget;
    private Vector3 velocity;
    private Coroutine returnCoroutine;

    private void Awake()
    {
        transform.rotation = Quaternion.Euler(fixedRotationEuler);
    }

    private void OnEnable()
    {
        currentTarget = player;

        ballKicker.OnKickStarted.AddListener(HandleKickStarted);
        ballKicker.OnBallArrived.AddListener(HandleBallArrived);
    }

    private void OnDisable()
    {
        ballKicker.OnKickStarted.RemoveListener(HandleKickStarted);
        ballKicker.OnBallArrived.RemoveListener(HandleBallArrived);
    }

    private void LateUpdate()
    {
        if (currentTarget == null) return;

        Vector3 desiredPosition = currentTarget.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, followSmoothTime);
    }

    private void HandleKickStarted(Transform ball)
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        currentTarget = ball;
    }

    private void HandleBallArrived(Transform ball)
    {
        if (ball != currentTarget) return;

        returnCoroutine = StartCoroutine(ReturnToPlayerAfterDelay(ball));
    }

    private IEnumerator ReturnToPlayerAfterDelay(Transform ball)
    {
        yield return new WaitForSeconds(returnToPlayerDelay);

        if (currentTarget == ball)
            currentTarget = player;

        returnCoroutine = null;
    }
}