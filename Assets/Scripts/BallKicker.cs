using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallKicker : MonoBehaviour
{
    [Header("Kick Settings")]
    [SerializeField] private float flightDuration = 1.2f;
    [SerializeField] private float arcHeight = 3f;

    [Header("Events")]
    public UnityEvent<Transform> OnKickStarted;
    public UnityEvent<Transform> OnBallArrived;

    // one coroutine tracked PER ball
    private readonly Dictionary<Transform, Coroutine> activeKicks = new Dictionary<Transform, Coroutine>();

    public void KickToGoal(Transform ball, Transform goal)
    {
        if (ball == null || goal == null) return;

        if (activeKicks.TryGetValue(ball, out Coroutine existing) && existing != null)
            StopCoroutine(existing);

        activeKicks[ball] = StartCoroutine(FlyRoutine(ball, goal.position));
    }

    private IEnumerator FlyRoutine(Transform ball, Vector3 targetPos)
    {
        OnKickStarted?.Invoke(ball);

        Vector3 startPos = ball.position;
        float elapsed = 0f;

        while (elapsed < flightDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flightDuration);

            // straight-line interpolation on X/Z, parabolic arc on Y
            Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = arcHeight * 4f * t * (1f - t); // simple parabola, peaks at t = 0.5
            flatPos.y = Mathf.Lerp(startPos.y, targetPos.y, t) + arc;

            ball.position = flatPos;
            yield return null;
        }

        ball.position = targetPos;
        activeKicks.Remove(ball);
        OnBallArrived?.Invoke(ball);
    }
}