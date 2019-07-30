using System.Collections;
using System.Collections.Generic;
using RBS;
using RBS.Messages;
using UnityEngine;
using UnityEngine.XR.iOS;

public class GoalPlacer : MonoBehaviour
{
    bool isPlaced = false;
    // ゴールオブジェクト
    public GameObject goalPositionPrefab;
    // ゴール方向オブジェクト
    public GameObject goalRotationPrefab;
    // ゴール位置
    GameObject goalPosition;
    // ゴール姿勢
    GameObject goalRotation;
    // ゴール位置のPublisher
    RBPublisher<RBS.Messages.geometry_msgs.PoseStamped> goalPub;

    void HitTest(ARPoint arPoint)
    {
        List<ARHitTestResult> hitResults = UnityARSessionNativeInterface
            .GetARSessionNativeInterface()
            .HitTest(arPoint, ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);

        // ロボットを検出するオブジェクトと検出したオブジェクトにPrefabを重ねて表示するオブジェクト
        GameObject generateRobotAnchor = GameObject.Find("GenerateRobotAnchor");
        GameObject robotAnchorGO = generateRobotAnchor.GetComponent<GenerateRobotAnchor>().objectAnchorGO;
        // 平面とあたっている かつ ロボットを検出している場合
        if (hitResults.Count > 0 && robotAnchorGO != null)
        {
            // 1回目のタッチ
            if (!isPlaced)
            {
                // 前回指定したゴール位置を削除
                Destroy(goalPosition);
                Destroy(goalRotation);

                goalPosition = Instantiate(goalPositionPrefab);
                goalPosition.transform.position = UnityARMatrixOps.GetPosition(hitResults[0].worldTransform);
                goalPosition.transform.rotation = UnityARMatrixOps.GetRotation(hitResults[0].worldTransform);
                this.isPlaced = true;
            }
            // 2回目のタッチ
            else
            {
                goalRotation = Instantiate(goalRotationPrefab);
                // 1回目にタッチした位置に対する、2回目にタッチした位置の方向を計算
                Vector3 goalVec = goalPosition.transform.position;
                Vector3 hitVec = UnityARMatrixOps.GetPosition(hitResults[0].worldTransform);
                Quaternion goalQua = Quaternion.LookRotation(hitVec - goalVec);
                goalRotation.transform.position = goalVec;
                goalRotation.transform.rotation = goalQua;
                // y軸成分以外の回転を除去
                Vector3 goalEuler = goalQua.eulerAngles;
                goalQua = Quaternion.Euler(0, goalEuler.y, 0);

                // Websocketがつながっている
                // Goal位置をPublishする
                if (RBSocket.Instance.IsConnected)
                {
                    ///////// ROS のメッセージ送信 /////////
                    // PoseStamped (Header, Pose)
                    RBS.Messages.geometry_msgs.PoseStamped msg = new RBS.Messages.geometry_msgs.PoseStamped();

                    // Header
                    RBS.Messages.std_msgs.Header header = new RBS.Messages.std_msgs.Header();
                    //header.frame_id = "base_footprint";
                    header.frame_id = "unity";
                    msg.header = header;

                    // Pose
                    RBS.Messages.geometry_msgs.Pose pose = new RBS.Messages.geometry_msgs.Pose();
                    // position
                    RBS.Messages.geometry_msgs.Point position = new RBS.Messages.geometry_msgs.Point();
                    position.x = goalVec.z;
                    position.y = -goalVec.x;
                    position.z = 0;
                    pose.position = position;
                    // orientation
                    RBS.Messages.geometry_msgs.Quaternion orientation = new RBS.Messages.geometry_msgs.Quaternion();
                    orientation.x = goalQua.z;
                    orientation.y = -goalQua.x;
                    orientation.z = goalQua.y;
                    orientation.w = -goalQua.w;
                    pose.orientation = orientation;
                    msg.pose = pose;

                    goalPub.publish(msg);
                }

                this.isPlaced = false;
            }
        }
    }

    void Start()
    {
        goalPub = new RBPublisher<RBS.Messages.geometry_msgs.PoseStamped>("/move_base_simple/goal");
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
                ARPoint arPoint = new ARPoint
                {
                    x = screenPosition.x,
                    y = screenPosition.y
                };

                // 平面との当たり判定
                HitTest(arPoint);
            }
        }
    }
}
