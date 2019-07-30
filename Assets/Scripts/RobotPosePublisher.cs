using System.Collections;
using System.Collections.Generic;
using RBS;
using RBS.Messages;
using UnityEngine;

public class RobotPosePublisher : MonoBehaviour
{
    RBPublisher<RBS.Messages.geometry_msgs.PoseStamped> robotPosePub;

    GameObject generateRobotAnchor;
    GenerateRobotAnchor robotAnchorScript;

    bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {
        // ロボット位置のPublisher
        robotPosePub = new RBPublisher<RBS.Messages.geometry_msgs.PoseStamped>("unity/robot_pose");
        // オブジェクトトラッキングを行うオブジェクトとそのスクリプトを取得
        generateRobotAnchor = GameObject.Find("GenerateRobotAnchor");
        robotAnchorScript = generateRobotAnchor.GetComponent<GenerateRobotAnchor>();
    }

    // Update is called once per frame
    void Update()
    {
        // 初期化されていなかったら初期化（unity座標系の真下にロボットがあると仮定）
        if (!initialized)
        {
            // PoseStamped (Header, Pose)
            RBS.Messages.geometry_msgs.PoseStamped msg = new RBS.Messages.geometry_msgs.PoseStamped();
            if (RBSocket.Instance.IsConnected)
            {
                robotPosePub.publish(msg);
                initialized = true;
            }
        }
        // ロボットが検出されていたら
        if (robotAnchorScript.isDetected)
        {
            ///////// ROS のメッセージ送信 /////////
            // PoseStamped (Header, Pose)
            RBS.Messages.geometry_msgs.PoseStamped msg = new RBS.Messages.geometry_msgs.PoseStamped();

            // Header
            RBS.Messages.std_msgs.Header header = new RBS.Messages.std_msgs.Header();
            header.frame_id = "unity";
            msg.header = header;

            // Pose (position, orientation)
            RBS.Messages.geometry_msgs.Pose pose = new RBS.Messages.geometry_msgs.Pose();
            RBS.Messages.geometry_msgs.Point position = new RBS.Messages.geometry_msgs.Point();
            RBS.Messages.geometry_msgs.Quaternion orientation = new RBS.Messages.geometry_msgs.Quaternion();
            // position
            position.x = robotAnchorScript.position.z;
            position.y = -robotAnchorScript.position.x;
            position.z = robotAnchorScript.position.y;
            pose.position = position;
            // orientation
            orientation.x = robotAnchorScript.rotation.z;
            orientation.y = -robotAnchorScript.rotation.x;
            orientation.z = robotAnchorScript.rotation.y;
            orientation.w = -robotAnchorScript.rotation.w;
            pose.orientation = orientation;

            msg.pose = pose;
            // ROSとの接続が確率されていたらPublish
            if (RBSocket.Instance.IsConnected)
            {
                robotPosePub.publish(msg);
                robotAnchorScript.isDetected = false;
            }
        }
    }
}
