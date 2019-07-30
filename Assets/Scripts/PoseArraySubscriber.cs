using System.Collections;
using System.Collections.Generic;
using RBS;
using RBS.Messages;
using UnityEngine;

public class PoseArraySubscriber : MonoBehaviour
{
    RBSubscriber<RBS.Messages.geometry_msgs.PoseArray> pcSub;

    [SerializeField]
    private GameObject particlePrefab;

    private List<GameObject> markers = new List<GameObject>();
    private List<RBS.Messages.geometry_msgs.Pose> particles =
        new List<RBS.Messages.geometry_msgs.Pose>();
        
    int particleNum = 0;

    // Start is called before the first frame update
    void Start()
    {
        pcSub = new RBSubscriber<RBS.Messages.geometry_msgs.PoseArray>("unity/particlecloud", ParticlecloudCB);
    }

    void ParticlecloudCB(RBS.Messages.geometry_msgs.PoseArray msg)
    {
        // subscribeしたパーティクルの数が保持している数より少ないとき
        if (msg.poses.Length < particles.Count)
        {
            // パーティクルの姿勢を保持
            for (int i = 0; i < msg.poses.Length; i++)
            {
                particles[i] = msg.poses[i];
            }
        }
        // subscribeしたパーティクルの数が保持している数より多いとき
        else
        {
            // パーティクルの姿勢を保持
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i] = msg.poses[i];
            }
            // パーティクルの数が前回より増加していたら追加保持
            for (int i = particleNum; i < msg.poses.Length; i++)
            {
                particles.Add(msg.poses[i]);
            }
        }
        // 現在のパーティクルの数を保持
        particleNum = msg.poses.Length;
    }

    // Update is called once per frame
    void Update()
    {
        // 現在のパーティクル数がマーカーの数より少ないとき
        if (particleNum <= markers.Count)
        {
            // マーカーの位置を更新
            for (int i = 0; i < particleNum; i++)
            {
                if (!markers[i].activeSelf) markers[i].SetActive(true);
                markers[i].transform.position = new Vector3((float)-particles[i].position.y,
                                                            (float)particles[i].position.z,
                                                            (float)particles[i].position.x);
                markers[i].transform.rotation = new Quaternion((float)-particles[i].orientation.y,
                                                               (float)particles[i].orientation.z,
                                                               (float)particles[i].orientation.x,
                                                               (float)-particles[i].orientation.w);
            }
            // 現在のパーティクル数以上のマーカーは非表示
            for (int i = particleNum; i < markers.Count; i++)
            {
                if (markers[i].activeSelf) markers[i].SetActive(false);
            }
        }
        // 現在のパーティクル数がマーカーの数より多いとき
        else
        {
            // マーカーの位置を更新
            for (int i = 0; i < markers.Count; i++)
            {
                if (!markers[i].activeSelf) markers[i].SetActive(true);
                markers[i].transform.position = new Vector3((float)-particles[i].position.y,
                                                            (float)particles[i].position.z,
                                                            (float)particles[i].position.x);
                markers[i].transform.rotation = new Quaternion((float)-particles[i].orientation.y,
                                                               (float)particles[i].orientation.z,
                                                               (float)particles[i].orientation.x,
                                                               (float)-particles[i].orientation.w);
            }
            // 最新のパーティクル数がマーカーより多かったらマーカー生成
            for (int i = markers.Count; i < particleNum; i++)
            {
                markers.Add(Instantiate<GameObject>(particlePrefab,
                                                    new Vector3((float)-particles[i].position.y,
                                                                (float)particles[i].position.z,
                                                                (float)particles[i].position.x),
                                                    new Quaternion((float)-particles[i].orientation.y,
                                                                   (float)particles[i].orientation.z,
                                                                   (float)particles[i].orientation.x,
                                                                   (float)-particles[i].orientation.w)));
            }
        }
    }
}
