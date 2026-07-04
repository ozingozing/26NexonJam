using UnityEngine;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    Queue<PathResult> pathResultQueue = new Queue<PathResult>();

    PathRequest currentPathRequest;

    static PathRequestManager instance;
    Pathfinding pathfinding;

    bool isProcessingPath;

    void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    void Update()
    {
        while (true)
        {
            PathResult result;

            lock (pathResultQueue)
            {
                if (pathResultQueue.Count == 0)
                {
                    break;
                }

                result = pathResultQueue.Dequeue();
            }

            if (!string.IsNullOrEmpty(result.errorMessage))
            {
                UnityEngine.Debug.LogError(result.errorMessage);
            }
            else if (result.success)
            {
                //UnityEngine.Debug.Log("Path found: " + result.elapsedMilliseconds + " ms");
            }

            // 콜백은 메인 스레드에서 실행됩니다.
            result.callback(result.path, result.success);

            isProcessingPath = false;
            TryProcessNext();
        }
    }

    public static void RequestPath(
        Vector3 pathStart,
        Vector3 pathEnd,
        Action<Vector3[], bool> callback
    )
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);

        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;

            pathfinding.StartFindPath(
                currentPathRequest.pathStart,
                currentPathRequest.pathEnd
            );
        }
    }

    public void FinishedProcessingPath(
        Vector3[] path,
        bool success,
        long elapsedMilliseconds = -1,
        string errorMessage = null
    )
    {
        PathResult result = new PathResult(
            path,
            success,
            currentPathRequest.callback,
            elapsedMilliseconds,
            errorMessage
        );

        lock (pathResultQueue)
        {
            pathResultQueue.Enqueue(result);
        }
    }

    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest(
            Vector3 _start,
            Vector3 _end,
            Action<Vector3[], bool> _callback
        )
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
        }
    }

    struct PathResult
    {
        public Vector3[] path;
        public bool success;
        public Action<Vector3[], bool> callback;
        public long elapsedMilliseconds;
        public string errorMessage;

        public PathResult(
            Vector3[] _path,
            bool _success,
            Action<Vector3[], bool> _callback,
            long _elapsedMilliseconds,
            string _errorMessage
        )
        {
            path = _path;
            success = _success;
            callback = _callback;
            elapsedMilliseconds = _elapsedMilliseconds;
            errorMessage = _errorMessage;
        }
    }
}