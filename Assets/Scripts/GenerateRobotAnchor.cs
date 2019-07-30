using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class GenerateRobotAnchor : MonoBehaviour 
{

	[SerializeField]
	private ARReferenceObjectAsset referenceObjectAsset;

	[SerializeField]
	private GameObject prefabToGenerate;

	public GameObject objectAnchorGO;

    // オブジェクトが検出されているかどうか
    public bool isDetected = false;
    // オブジェクトの位置・姿勢
    public Vector3 position;
    public Quaternion rotation;

	// Use this for initialization
	void Start () {
		UnityARSessionNativeInterface.ARObjectAnchorAddedEvent += AddObjectAnchor;
		UnityARSessionNativeInterface.ARObjectAnchorUpdatedEvent += UpdateObjectAnchor;
		UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveObjectAnchor;

	}

	void AddObjectAnchor(ARObjectAnchor arObjectAnchor)
	{
		Debug.Log ("object anchor added");
		if (arObjectAnchor.referenceObjectName == referenceObjectAsset.objectName) {
			position = UnityARMatrixOps.GetPosition (arObjectAnchor.transform);
			rotation = Quaternion.AngleAxis(180f, new Vector3(0.0f, 1.0f, 0.0f))
                        * UnityARMatrixOps.GetRotation (arObjectAnchor.transform);

			objectAnchorGO = Instantiate<GameObject> (prefabToGenerate, position, rotation);
            isDetected = true;
		}
	}

	void UpdateObjectAnchor(ARObjectAnchor arObjectAnchor)
	{
		Debug.Log ("object anchor added");
		if (arObjectAnchor.referenceObjectName == referenceObjectAsset.objectName) {
            position = UnityARMatrixOps.GetPosition(arObjectAnchor.transform);
            rotation = Quaternion.AngleAxis(180f, new Vector3(0.0f, 1.0f, 0.0f))
                        * UnityARMatrixOps.GetRotation(arObjectAnchor.transform);

            objectAnchorGO.transform.position = position;
            objectAnchorGO.transform.rotation = rotation;
            isDetected = true;
		}

	}

	void RemoveObjectAnchor(ARImageAnchor arImageAnchor)
	{
		Debug.Log ("object anchor removed");
		if (objectAnchorGO) {
			GameObject.Destroy (objectAnchorGO);
            isDetected = false;
        }

	}

	void OnDestroy()
	{
		UnityARSessionNativeInterface.ARObjectAnchorAddedEvent -= AddObjectAnchor;
		UnityARSessionNativeInterface.ARObjectAnchorUpdatedEvent -= UpdateObjectAnchor;
		UnityARSessionNativeInterface.ARImageAnchorRemovedEvent -= RemoveObjectAnchor;

	}

	// Update is called once per frame
	void Update () {

	}
}
