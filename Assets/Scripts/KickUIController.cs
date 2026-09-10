using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KickUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallManager ballManager;
    [SerializeField] private BallKicker ballKicker;
    [SerializeField] private List<Transform> goals = new List<Transform>();

    [Header("UI")]
    [SerializeField] private Button kickButton;
    [SerializeField] private Button autoKickButton;

    [Header("Effects")]
    [SerializeField] private ParticleSystem confettiPrefab;
    [SerializeField] private float confettiDestroyDelay = 3f;

    private void OnEnable()
    {
        ballManager.OnNearbyBallChanged.AddListener(HandleNearbyBallChanged);
        kickButton.onClick.AddListener(HandleKickClicked);
        autoKickButton.onClick.AddListener(HandleAutoKickClicked);
        ballKicker.OnKickStarted.AddListener(HandleKickStarted);
        ballKicker.OnBallArrived.AddListener(HandleBallArrived);

        kickButton.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ballManager.OnNearbyBallChanged.RemoveListener(HandleNearbyBallChanged);
        kickButton.onClick.RemoveListener(HandleKickClicked);
        autoKickButton.onClick.RemoveListener(HandleAutoKickClicked);
        ballKicker.OnKickStarted.RemoveListener(HandleKickStarted);
        ballKicker.OnBallArrived.RemoveListener(HandleBallArrived);
    }

    private void HandleKickStarted(Transform ball)
    {
        // ball is now mid-air — Auto Kick disable
        ballManager.MarkBallInFlight(ball);
    }

    private void HandleBallArrived(Transform ball)
    {
        // once a ball reaches a goal, blacklist from auto kick
        ballManager.ClearBallInFlight(ball);
        ballManager.MarkBallInGoal(ball);

        SpawnConfetti(ball.position);
    }

    private void SpawnConfetti(Vector3 position)
    {
        if (confettiPrefab == null) return;

        ParticleSystem instance = Instantiate(confettiPrefab, position, Quaternion.identity);
        instance.Play();
        Destroy(instance.gameObject, confettiDestroyDelay);
    }

    private void HandleNearbyBallChanged(Transform nearbyBall)
    {
        kickButton.gameObject.SetActive(nearbyBall != null);
    }

    private void HandleKickClicked()
    {
        Transform ball = ballManager.GetNearestBall();
        if (ball == null) return;

        Transform goal = FindNearestGoal(ball.position);
        if (goal == null) return;

        ballKicker.KickToGoal(ball, goal);
    }

    private void HandleAutoKickClicked()
    {
        Transform ball = ballManager.GetFarthestBall();
        if (ball == null) return;

        Transform goal = FindNearestGoal(ball.position);
        if (goal == null) return;

        ballKicker.KickToGoal(ball, goal);
    }

    private Transform FindNearestGoal(Vector3 fromPosition)
    {
        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Transform goal in goals)
        {
            if (goal == null) continue;

            float dist = Vector3.Distance(fromPosition, goal.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = goal;
            }
        }

        return nearest;
    }
}