using System.Collections;
using System.Collections.Generic;
using RBS;
using RBS.Messages;
using UnityEngine;
using UnityEngine.XR.iOS;

public class GoalPlacerPrev: MonoBehaviour
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
        GameObject generateObjectAnchor = GameObject.Find("GenerateObjectAnchor");
        GameObject objectAnchorGO = generateObjectAnchor.GetComponent<GenerateRobotAnchor>().objectAnchorGO;
        // 平面とあたっている かつ ロボットを検出している場合
        if (hitResults.Count > 0 && objectAnchorGO != null)
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
                Vector3 p1 = goalPosition.transform.position;
                Vector3 p2 = UnityARMatrixOps.GetPosition(hitResults[0].worldTransform);
                Vector3 d = p2 - p1;
                float rad = Mathf.Atan2(d.z, d.x);
                float degree = rad * Mathf.Rad2Deg;

                // 上で計算した方向に三角を向けて配置
                goalRotation.transform.position = p1;
                goalRotation.transform.localEulerAngles = new Vector3(0, -degree, 0);

                // Websocketがつながっている
                // Goal位置をPublishする
                if (RBSocket.Instance.IsConnected)
                {
                    //////// ロボット座標系でのゴールの位置・姿勢を計算 ////////
                    // Unity座標系で見たロボットとゴールの相対位置
                    Vector3 robo2Goal = goalPosition.transform.position - objectAnchorGO.transform.position;
                    float dx = robo2Goal.x;
                    float dz = robo2Goal.z;
                    // ロボット座標系でのゴールの位置
                    UnityEngine.Quaternion robotQua = objectAnchorGO.transform.rotation;
                    float theta = -robotQua.eulerAngles.y * Mathf.Deg2Rad;  // 左ねじ回転なのでマイナス
                    Vector3 rGoalPosition = new Vector3(dx * Mathf.Cos(-theta) - dz * Mathf.Sin(-theta),
                                                        0,
                                                        dx * Mathf.Sin(-theta) + dz * Mathf.Cos(-theta));
                    // ロボット座標系でのゴールの姿勢
                    UnityEngine.Quaternion goalQua = goalRotation.transform.rotation;
                    UnityEngine.Quaternion rGoalRotation = goalQua * UnityEngine.Quaternion.Inverse(robotQua);
                    // 垂直z右手座標系に変換
                    UnityEngine.Quaternion rosGoalRotation = UnityEngine.Quaternion.Euler(0,
                                                                                          0,
                                                                                          -rGoalRotation.eulerAngles.y);

                    ///////// ROS のメッセージ送信 /////////
                    // PoseStamped (Header, Pose)
                    RBS.Messages.geometry_msgs.PoseStamped msg = new RBS.Messages.geometry_msgs.PoseStamped();

                    // Header
                    RBS.Messages.std_msgs.Header header = new RBS.Messages.std_msgs.Header();
                    header.frame_id = "base_footprint";
                    msg.header = header;
                    // msg.header.frame_id = "base_footprint";

                    // Pose
                    RBS.Messages.geometry_msgs.Pose pose = new RBS.Messages.geometry_msgs.Pose();
                    // position
                    RBS.Messages.geometry_msgs.Point position = new RBS.Messages.geometry_msgs.Point();
                    position.x = rGoalPosition.x;
                    position.y = rGoalPosition.z;
                    position.z = 0;
                    pose.position = position;
                    // orientation
                    RBS.Messages.geometry_msgs.Quaternion orientation = new RBS.Messages.geometry_msgs.Quaternion();
                    orientation.x = rosGoalRotation.x;
                    orientation.y = rosGoalRotation.y;
                    orientation.z = rosGoalRotation.z;
                    orientation.w = rosGoalRotation.w;
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
        if (Input.touchCount > 0 )
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
    }}
