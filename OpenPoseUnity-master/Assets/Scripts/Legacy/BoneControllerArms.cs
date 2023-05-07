using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class BoneControllerArms : MonoBehaviour
{
	[SerializeField] Animator animator;
	[SerializeField, Range(10, 120)] float frameRate;
	[SerializeField] string dataPath;
	[SerializeField] string fileName;
	[SerializeField] int startFrame;
	[SerializeField] int dataSize;
	public List<Transform> boneList = new List<Transform>();
	public float initX;
	public float initY;
	public float initZ;

	Vector3[] points = new Vector3[38];
	Quaternion[] initRot;
	Vector3 initPosition;
	Vector3 positionOffset;
    Quaternion[] initInv; //Inverse
    
	int [] bones = new int[34] {1, 2, 3, 4, 5, 3, 7, 8, 3, 10, 11, 3, 13, 14, 3, 16, 17, 19, 20, 21, 22, 23, 21, 25, 26, 21, 28, 29, 21, 31, 32, 21, 34, 35};
	int[] child_bones = new int[34] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };

	// int [] bones = new int[6] {1, 2, 3, 19, 20, 21};
	// int[] child_bones = new int[6] {2, 3, 4, 20, 21, 22};

	// int [] bones = new int[3] {3, 2, 1};
	// int[] child_bones = new int[3] {4,3,2};

	int boneNum = 39;
	int numberOfPoints = 38;

	int[,] joints = new int[,]
	{ 
		{1, 2}, {2, 3}, {3, 4}, {4, 5}, {5, 6}, {3, 7}, {7, 8}, {8, 9}, {3, 10}, {10, 11}, {11, 12}, {3, 13}, {13, 14}, {14, 15}, {3, 16}, {16, 17}, {17, 18}, {19, 20}, {20, 21}, {21, 22}, {22, 23}, {23, 24}, {21, 25}, {25, 26}, {26, 27}, {21, 28}, {28, 29}, {29, 30}, {21, 31}, {31, 32}, {32, 33}, {21, 34}, {34, 35}, {35, 36}
	};

	float timer;
	int nowFrame = 0;
	
	void Start()
	{
		GetBones();
		PointUpdate();
	}

	void Update()
	{
		PointUpdateByTime();
		SetBoneRot();
	}
	
	void GetBones()
	{
		initRot = new Quaternion[boneNum];
		initInv = new Quaternion[boneNum];
		//0
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.Chest));
		//원래 왼손 먼저
		//1,2,3
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightHand));
		//4,5,6
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightThumbProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightThumbDistal));
		//7,8,9
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightIndexProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightIndexDistal));
		//10,11,12
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal));
		//13,14,15
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightRingProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightRingDistal));
		//16,17,18
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightLittleProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightLittleDistal));
		//19,20,21
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftHand));
		//22,23,24
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal));
		//25,26,27
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal));
		//28,29,30
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal));
		//31,32,33
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftRingProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftRingDistal));
		//34,35,36
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal));
		//37
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.Head));

		Vector3 init_forward = TriangleNormal(points[1], points[19], points[0]);
		initInv[0] = Quaternion.Inverse(Quaternion.LookRotation(init_forward));

		initPosition = boneList[0].position;
		initRot[0] = boneList[0].rotation;

		for (int i = 0; i < bones.Length; i++)
		{
			int b = bones[i];
			int cb = child_bones[i];


			initRot[b] = boneList[b].rotation;

			initInv[b] = Quaternion.Inverse(Quaternion.LookRotation(boneList[b].position - boneList[cb].position, init_forward));
			// Debug.Log($"{initRot[b]},{initInv[b]}");
		}
	}
	void PointUpdate()
	{
		if (nowFrame < dataSize)
		{
			StreamReader fi = new StreamReader(Application.dataPath + dataPath + fileName + (nowFrame + startFrame).ToString() + ".txt");
			nowFrame++;
			string all = fi.ReadToEnd();
			if (all != "0")
			{
				string[] axis = all.Split(']');
				float[] x = axis[0].Replace("[", "").Replace(" ", "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
				float[] y = axis[1].Replace("[", "").Replace(" ", "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
				float[] z = axis[2].Replace("[", "").Replace(" ", "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
				// float[] x = axis[0].Replace("[", "").Replace("\r\n", "").Replace("\n", "").Split(' ').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
				// float[] y = axis[2].Replace("[", "").Replace("\r\n", "").Replace("\n", "").Split(' ').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
				// float[] z = axis[1].Replace("[", "").Replace("\r\n", "").Replace("\n", "").Split(' ').Where(s => s != "").Select(f => float.Parse(f)).ToArray();

				if (nowFrame == 0)
                {
					int idx = 0;
                    positionOffset = new Vector3(x[idx] - initX, y[idx] - initY, z[idx] - initZ);
                }
				for (int i = 0; i < numberOfPoints; i++)
				{
					//Todo : swap z and y when create txt file
					points[i] = new Vector3(x[i], z[i], -y[i]) - positionOffset; 
				}
			}
			else
			{
				Debug.Log("All Data 0");
			}
		}
	}
	void PointUpdateByTime()
	{
		timer += Time.deltaTime;
		if (timer > (1 / frameRate))
		{
			timer = 0;
			PointUpdate();
		}
	}
	Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 d1 = b - a;
		Vector3 d2 = c - a;

		Vector3 dd = Vector3.Cross(d1, d2);
		dd.Normalize();

		return dd;
	}
	void SetBoneRot()
	{
		Vector3[] now_pos = points;
		Vector3 pos_forward = TriangleNormal(now_pos[1], now_pos[19], now_pos[0]);

		for (int i = 0; i < bones.Length; i++)
		{
			int b = bones[i];
			int cb = child_bones[i];
			// Debug.Log($"{i},{b},{cb}");
			// Debug.Log($"{boneList[b].rotation = (Quaternion.LookRotation(now_pos[b] - now_pos[cb], pos_forward) * initInv[b] * initRot[b])}");
			boneList[b].rotation = (Quaternion.LookRotation(now_pos[b] - now_pos[cb], pos_forward) * initInv[b] * initRot[b]);
		}

		for (int i = 0; i<34; i++)
		{
			DrawLine(points[joints[i, 0]] + new Vector3(-1, 0.8f, 0), points[joints[i, 1]] + new Vector3(-1, 0.8f, 0), Color.blue);
			DrawRay(points[joints[i, 0]] + new Vector3(-1, 0.8f, 0), boneList[i].right * 0.01f, Color.magenta);
			DrawRay(points[joints[i, 0]] + new Vector3(-1, 0.8f, 0), boneList[i].forward * 0.01f, Color.green);
			DrawRay(points[joints[i, 0]] + new Vector3(-1, 0.8f, 0), boneList[i].up * 0.01f, Color.cyan);

			// DrawLine(points[joints[i, 0]] * 0.001f + new Vector3(-1, 0.8f, 0), points[joints[i, 1]] * 0.001f + new Vector3(-1, 0.8f, 0), Color.blue);
			// DrawRay(points[joints[i, 0]] * 0.001f + new Vector3(-1, 0.8f, 0), boneList[i].right * 0.01f, Color.magenta);
			// DrawRay(points[joints[i, 0]] * 0.001f + new Vector3(-1, 0.8f, 0), boneList[i].forward * 0.01f, Color.green);
			// DrawRay(points[joints[i, 0]] * 0.001f + new Vector3(-1, 0.8f, 0), boneList[i].up * 0.01f, Color.cyan);
		}
	}
	void DrawLine(Vector3 s, Vector3 e, Color c)
	{
		Debug.DrawLine(s, e, c);
	}
	void DrawRay(Vector3 s, Vector3 d, Color c)
	{
		Debug.DrawRay(s, d, c);
	}
}