using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class BoneController : MonoBehaviour
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

	Vector3[] points = new Vector3[17];
	Vector3[] normalizeBone = new Vector3[12];
	Quaternion[] initRot;
	Vector3 initPosition;
	Vector3 positionOffset;
    Quaternion[] initInv; //Inverse
	int[] bones = new int[16] {0, 0, 7, 8, 8, 8, 9, 10, 1, 2, 4, 5, 11, 12, 14, 15};
    int[] childBones = new int[16] {1, 4, 0, 11, 14, 7, 8, 9, 2, 3, 5, 6, 12, 13, 15, 16};
	int boneNum = 19;
	float scaleRatio = 0.005f;
    float healPosition = 0.005f;
    float headAngle = -55f;
	

	float timer;
	int[,] joints = new int[,]
	{ { 0, 1 }, { 1, 2 }, { 2, 3 }, { 0, 4 }, { 4, 5 }, { 5, 6 }, { 0, 7 }, { 7, 8 }, { 8, 9 }, { 9, 10 }, { 8, 11 }, { 11, 12 }, { 12, 13 }, { 8, 14 }, { 14, 15 }, { 15, 16 }
	};
	int[,] boneJoint = new int[,]
	{ { 0, 2 }, { 2, 3 }, { 0, 5 }, { 5, 6 }, { 0, 7 }, { 7, 8 }, { 8, 9 }, { 9, 10 }, { 9, 12 }, { 12, 13 }, { 9, 15 }, { 15, 16 }
	};
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

		boneList.Add(animator.GetBoneTransform(HumanBodyBones.Hips));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftFoot));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightFoot));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.Spine));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.Chest));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.Neck));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.Head));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.RightHand));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
		boneList.Add(animator.GetBoneTransform(HumanBodyBones.LeftHand));

		Vector3 init_forward = TriangleNormal(points[7],points[4],points[0]);
		initInv[0] = Quaternion.Inverse(Quaternion.LookRotation(init_forward));

		initPosition = boneList[0].position;
        initRot[0] = boneList[0].rotation;
        for (int i = 0; i < bones.Length; i++) {
            int b = bones[i];
            int cb = childBones[i];
        
           
            initRot[b] = boneList[b].rotation;
            
            initInv[b] = Quaternion.Inverse(Quaternion.LookRotation(boneList[b].position - boneList[cb].position,init_forward));
            // Debug.Log($"{initRot[b]},{initInv[b]}");
		}
	}
	void PointUpdate()
	{
		if (nowFrame < dataSize)
		{
			StreamReader fi = new StreamReader(Application.dataPath + dataPath + fileName + (nowFrame + startFrame).ToString() + ".txt");
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
				for (int i = 0; i < 17; i++)
				{
					points[i] = new Vector3(x[i], y[i], z[i]) - positionOffset; 
				}
				for (int i = 0; i < 12; i++)
				{
					normalizeBone[i] = (points[boneJoint[i, 1]] - points[boneJoint[i, 0]]).normalized;
				}
			}
			else
			{
				Debug.Log("All Data 0");
			}
			nowFrame++;
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
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }
	
	static Vector3 ThoraxCalc(Vector3 a1, Vector3 b1)
	{		
	Vector3 t = (a1-b1)/2;
	return t;
	}
	
	static Vector3 SpineCalc(Vector3 a2, Vector3 b2, Vector3 c2)
	{
		Vector3 s2 = (b2-c2)/2;
	Vector3 s = (a2-s2)/2;
	return s;
	}

	void SetBoneRot()
	{
		Vector3[] now_pos = points;

        Vector3 pos_forward = TriangleNormal(now_pos[7], now_pos[4], now_pos[0]);
		// 캐릭터의 위치를 업데이트
        boneList[0].position = (now_pos[0] * scaleRatio) + new Vector3(initPosition.x, healPosition, initPosition.z);
        boneList[0].rotation = Quaternion.LookRotation(pos_forward) * initInv[0] * initRot[0];

		Vector3 tmp = new Vector3(-1, 0, 0);
        for (int i = 0; i < bones.Length; i++) {
            int b = bones[i];
            int cb = childBones[i];
            // Debug.Log($"{i},{b},{cb}");
			// Debug.Log($"{boneList[b].rotation = (Quaternion.LookRotation(now_pos[b] - now_pos[cb], pos_forward) * initInv[b] * initRot[b])}");
			boneList[b].rotation = (Quaternion.LookRotation(now_pos[b] - now_pos[cb], pos_forward) * initInv[b] * initRot[b]);

        }
		
        boneList[9].rotation = Quaternion.AngleAxis(headAngle, boneList[11].position - boneList[14].position) * boneList[9].rotation;
		
		for (int i = 0; i < 16; i++)
		{
			DrawLine(points[joints[i, 0]] * 0.001f + new Vector3(-1, 0.8f, 0), points[joints[i, 1]] * 0.001f + new Vector3(-1, 0.8f, 0), Color.blue);
			DrawRay(points[joints[i, 0]] * 0.001f + new Vector3(-1, 0.8f, 0), boneList[i].right * 0.01f, Color.magenta);
			DrawRay(points[joints[i, 0]] * 0.001f + new Vector3(-1, 0.8f, 0), boneList[i].forward * 0.01f, Color.green);
			DrawRay(points[joints[i, 0]] * 0.001f + new Vector3(-1, 0.8f, 0), boneList[i].up * 0.01f, Color.cyan);
		}
		for (int i = 0; i < 12; i++)
		{
			DrawRay(points[boneJoint[i, 0]] * 0.001f + new Vector3(1, 0.8f, 0), normalizeBone[i] * 0.1f, Color.green);
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