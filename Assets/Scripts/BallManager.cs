using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private List<Transform> balls = new List<Transform>();

    [Header("Nearby Detection (for Kick button)")]
    [SerializeField] private float nearbyThreshold = 2.5f;
    [SerializeField] private float checkInterval = 0.1f; // seconds between distance checks

    [Header("Events")]
    public UnityEvent<Transform> OnNearbyBallChanged;
    public Transform NearestBall { get; private set; }
    public bool HasNearbyBall => NearestBall != null;

    private float timer;
    private readonly HashSet<Transform> ballsInGoal = new HashSet<Transform>(); 
    private readonly HashSet<Transform> ballsInFlight = new HashSet<Transform>(); 

    private void Reset()
    {
        AutoFindBalls();
    }

    private void Update()
    {
        if (player == null || balls.Count == 0) return;

        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        UpdateNearestBall();
    }

    private void UpdateNearestBall()
    {
        Transform closest = null;
        float closestDist = float.MaxValue;

        foreach (Transform ball in balls)
        {
            if (ball == null) continue;

            float dist = Vector3.Distance(player.position, ball.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = ball;
            }
        }

        bool isNearby = closest != null && closestDist <= nearbyThreshold;
        Transform newNearest = isNearby ? closest : null;

        if (newNearest != NearestBall)
        {
            NearestBall = newNearest;
            OnNearbyBallChanged?.Invoke(NearestBall);
        }
    }

    public Transform GetNearestBall()
    {
        Transform closest = null;
        float closestDist = float.MaxValue;

        foreach (Transform ball in balls)
        {
            if (ball == null) continue;

            float dist = Vector3.Distance(player.position, ball.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = ball;
            }
        }

        return closest;
    }

    public Transform GetFarthestBall()
    {
        Transform farthest = null;
        float farthestDist = -1f;

        foreach (Transform ball in balls)
        {
            if (ball == null || ballsInGoal.Contains(ball) || ballsInFlight.Contains(ball)) continue;

            float dist = Vector3.Distance(player.position, ball.position);
            if (dist > farthestDist)
            {
                farthestDist = dist;
                farthest = ball;
            }
        }

        return farthest;
    }

    public void MarkBallInGoal(Transform ball)
    {
        if (ball != null) ballsInGoal.Add(ball);
    }

    public void MarkBallInFlight(Transform ball)
    {
        if (ball != null) ballsInFlight.Add(ball);
    }

    public void ClearBallInFlight(Transform ball)
    {
        if (ball != null) ballsInFlight.Remove(ball);
    }

    public void ClearScoredBalls()
    {
        ballsInGoal.Clear();
        ballsInFlight.Clear();
    }

    [ContextMenu("Auto Find Balls By Tag")]
    private void AutoFindBalls()
    {
        balls.Clear();
        GameObject[] found = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject go in found)
            balls.Add(go.transform);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, nearbyThreshold);
    }
#endif
}