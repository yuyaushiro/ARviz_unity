using System.Collections;
using System.Collections.Generic;
using RBS;
using RBS.Messages;
using UnityEngine;

public class PathSubscriber : MonoBehaviour
{
    RBSubscriber<RBS.Messages.nav_msgs.Path> pathSub;

    [SerializeField]
    private GameObject pathPrefab;

    private LineRenderer lineRenderer;
    private List<GameObject> markers = new List<GameObject>();
    private List<RBS.Messages.geometry_msgs.Pose> path =
        new List<RBS.Messages.geometry_msgs.Pose>();

    int pathNum = 0;
    float lineHeight = 0.09f;

    // Start is called before the first frame update
    void Start()
    {
        pathSub = new RBSubscriber<RBS.Messages.nav_msgs.Path>("unity/plan", PathCB);
        lineRenderer = GetComponent<LineRenderer>();
    }

    void PathCB(RBS.Messages.nav_msgs.Path msg)
    {
        // subscribeしたpathの数が保持している数より少ないとき
        if (msg.poses.Length < path.Count)
        {
            // パーティクルの姿勢を保持
            for (int i = 0; i < msg.poses.Length; i++)
            {
                path[i] = msg.poses[i].pose;
            }
        }
        // subscribeしたpathの数が保持している数より多いとき
        else
        {
            // パーティクルの姿勢を保持
            for (int i = 0; i < path.Count; i++)
            {
                path[i] = msg.poses[i].pose;
            }
            // パーティクルの数が前回より増加していたら追加保持
            for (int i = pathNum; i < msg.poses.Length; i++)
            {
                path.Add(msg.poses[i].pose);
            }
        }
        // 現在のパーティクルの数を保持
        pathNum = msg.poses.Length;
    }

    // Update is called once per frame
    void Update()
    {
        // ライン描画
        if (pathNum >= 3)
        {
            if (!lineRenderer.enabled) lineRenderer.enabled = true;
            lineRenderer.SetVertexCount(pathNum);
            for (int i = 0; i < pathNum; i++)
            {
                lineRenderer.SetPosition(i, new Vector3((float)-path[i].position.y,
                                                        (float)path[i].position.z,
                                                        (float)path[i].position.x));
            }
        }
        else
        {
            if (lineRenderer.enabled) lineRenderer.enabled = false;
        }

        // 現在のpath数がマーカーの数より少ないとき
        if (pathNum <= markers.Count)
        {
            // マーカーの位置を更新
            for (int i = 0; i < pathNum; i++)
            {
                if (!markers[i].activeInHierarchy) markers[i].SetActive(true);
                markers[i].transform.position = new Vector3((float)-path[i].position.y,
                                                            (float)path[i].position.z + lineHeight,
                                                            (float)path[i].position.x);
            }
            // 現在のパーティクル数以上のマーカーは非表示
            for (int i = pathNum; i < markers.Count; i++)
            {
                if (markers[i].activeInHierarchy) markers[i].SetActive(false);
            }
        }
        // 現在のpath数がパーティクル数がマーカーの数より多いとき
        else
        {
            // マーカーの位置を更新
            for (int i = 0; i < markers.Count; i++)
            {
                if (!markers[i].activeInHierarchy) markers[i].SetActive(true);
                markers[i].transform.position = new Vector3((float)-path[i].position.y,
                                                            (float)path[i].position.z + lineHeight,
                                                            (float)path[i].position.x);
            }
            // 最新のパーティクル数がマーカーより多かったらマーカー生成
            for (int i = markers.Count; i < pathNum; i++)
            {
                markers.Add(Instantiate<GameObject>(pathPrefab,
                                                    new Vector3((float)-path[i].position.y,
                                                                (float)path[i].position.z + lineHeight,
                                                                (float)path[i].position.x),
                                                    Quaternion.identity));
            }
        }

    }
}

