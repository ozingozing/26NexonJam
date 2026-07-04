using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Threading;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;
    Grid grid;

    void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        // 중요:
        // grid.NodeFromWorldPoint 내부에서 transform.position 등을 쓸 가능성이 있으므로
        // 이 부분은 메인 스레드에서 먼저 처리합니다.
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        Thread pathThread = new Thread(() =>
        {
            try
            {
                FindPath(startNode, targetNode);
            }
            catch (Exception e)
            {
                requestManager.FinishedProcessingPath(
                    new Vector3[0],
                    false,
                    -1,
                    e.ToString()
                );
            }
        });

        pathThread.IsBackground = true;
        pathThread.Priority = System.Threading.ThreadPriority.BelowNormal;
        pathThread.Start();
    }

    void FindPath(Node startNode, Node targetNode)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        startNode.parent = startNode;

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour =
                        currentNode.gCost +
                        GetDistance(currentNode, neighbour) +
                        neighbour.movementPenalty;

                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }

        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }

        sw.Stop();

        // 여기서는 콜백을 직접 실행하지 않습니다.
        // Thread에서 Unity 오브젝트를 건드리지 않도록 Manager에 결과만 넘깁니다.
        requestManager.FinishedProcessingPath(
            waypoints,
            pathSuccess,
            sw.ElapsedMilliseconds
        );
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(
                path[i - 1].gridX - path[i].gridX,
                path[i - 1].gridY - path[i].gridY
            );

            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }

            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }

        return 14 * dstX + 10 * (dstY - dstX);
    }
}