using System.Collections;
using System.Collections.Generic;
using RBS;
using RBS.Messages;
using UnityEngine;

public class LaserScanSubscriber : MonoBehaviour
{
    RBSubscriber<RBS.Messages.sensor_msgs.PointCloud> pcSub;

    [SerializeField]
    private GameObject scanPointPrefab;
    [SerializeField]
    private GameObject scanPointShadowPrefab;

    private List<GameObject> markers = new List<GameObject>();
    private List<GameObject> markers_shadow = new List<GameObject>();
    private List<RBS.Messages.geometry_msgs.Point32> scanPoints =
        new List<RBS.Messages.geometry_msgs.Point32>();

    bool isInstantiated = false;
    int laserNum = 0;
    float LRFHeight = 0.182f;

    void Start()
    {
        pcSub = new RBSubscriber<RBS.Messages.sensor_msgs.PointCloud>("unity/scan_pc", pointCloudCB);
    }

    void pointCloudCB(RBS.Messages.sensor_msgs.PointCloud msg)
    {
        // 最初のsubscribeは配列にadd
        if (laserNum == 0)
        {
            for (int i = 0; i < msg.points.Length; i++)
            {
                scanPoints.Add(msg.points[i]);
            }
            laserNum = msg.points.Length;
        }
        else
        {
            for (int i = 0; i < laserNum; i++)
            {
                scanPoints[i] = msg.points[i];
            }
        }
    }

    void Update()
    {
        // マーカーが生成されていないかつlaserNumが0じゃない場合生成
        if (!isInstantiated && laserNum != 0)
        {
            for (int i = 0; i < laserNum; i++)
            {
                markers.Add(Instantiate<GameObject>(scanPointPrefab,
                                                    new Vector3(-scanPoints[i].y, scanPoints[i].z, scanPoints[i].x),
                                                    Quaternion.identity));
                markers_shadow.Add(Instantiate<GameObject>(scanPointShadowPrefab,
                                                           new Vector3(-scanPoints[i].y, scanPoints[i].z - LRFHeight, scanPoints[i].x),
                                                           Quaternion.identity));
            }
            // マーカー生成完了
            isInstantiated = true;
        }

        // マーカーの位置を更新
        for (int i = 0; i < laserNum; i++)
        {
            markers[i].transform.position = new Vector3(-scanPoints[i].y,
                                                        scanPoints[i].z,
                                                        scanPoints[i].x);
            markers_shadow[i].transform.position = new Vector3(-scanPoints[i].y,
                                                               scanPoints[i].z - LRFHeight,
                                                               scanPoints[i].x);
        }
    }
}
