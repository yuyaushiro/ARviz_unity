using System.Collections;
using System.Collections.Generic;
using RBS;
using RBS.Messages;
using UnityEngine;

public class FootprintSubscriber : MonoBehaviour
{
    RBSubscriber<RBS.Messages.geometry_msgs.PoseStamped> footSub;
    RBPublisher<RBS.Messages.std_msgs.Float32> debugPub;

    [SerializeField]
    private GameObject footPrefab;

    private GameObject generateRobotAnchor;
    private GenerateRobotAnchor robotScript;

    private GameObject marker;
    private RBS.Messages.geometry_msgs.Pose pose = 
        new RBS.Messages.geometry_msgs.Pose();

    //private GameObject generateRobotAnchor;
    //private GameObject robotAnchorGO;

    // Start is called before the first frame update
    void Start()
    {
        footSub = new RBSubscriber<RBS.Messages.geometry_msgs.PoseStamped>("unity/foot", footCB);
        debugPub = new RBPublisher<RBS.Messages.std_msgs.Float32>("unity/debug");
        generateRobotAnchor = GameObject.Find("GenerateRobotAnchor");
        robotScript = generateRobotAnchor.GetComponent<GenerateRobotAnchor>();
    }

    void footCB(RBS.Messages.geometry_msgs.PoseStamped msg)
    {
        pose = msg.pose;
    }

    // Update is called once per frame
    void Update()
    {
        RBS.Messages.std_msgs.Float32 msg = new RBS.Messages.std_msgs.Float32();
        msg.data = (float)(0);

        //GameObject generateRobotAnchor = GameObject.Find("GenerateRobotAnchor");

        //if (robotScript.isDetected)
        //{
        Vector3 position = new Vector3((float)-pose.position.y,
                                       (float)pose.position.z,
                                       (float)pose.position.x);
        Quaternion rotation = new Quaternion((float)-pose.orientation.y,
                                             (float)pose.orientation.z,
                                             (float)pose.orientation.x,
                                             (float)-pose.orientation.w);
        // マーカーが生成されていなかったら生成
        if (marker == null)
        {
            marker = Instantiate<GameObject>(footPrefab, position, rotation);
        }
        // マーカーが生成されていたら位置を更新
        else
        {
            marker.transform.position = position;
            marker.transform.rotation = rotation;
        }
        msg.data = (float)(1);
            //robotScript.isDetected = false;
        //}
        if (RBSocket.Instance.IsConnected) debugPub.publish(msg);
    }
}
