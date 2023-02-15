using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class IKSettingArms : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField, Range(10, 120)] float FrameRate;
    public List<Transform> BoneList = new List<Transform>();
    [SerializeField] string Data_Path;
    [SerializeField] string File_Name;
    [SerializeField] int startFrame;
    [SerializeField] int Data_Size;
    GameObject FullbodyIK;
    Vector3[] points = new Vector3[38];
    Vector3[] NormalizeBone = new Vector3[20];
    float[] BoneDistance = new float[12];
    float Timer;
    // int[, ] joints = new int[, ] { { 0, 1 }, { 1, 2 }, { 2, 3 }, { 0, 4 }, { 4, 5 }, { 5, 6 }, { 0, 7 }, { 7, 8 }, { 8, 9 }, { 9, 10 }, { 8, 11 }, { 11, 12 }, { 12, 13 }, { 8, 14 }, { 14, 15 }, { 15, 16 } };
    // int[, ] BoneJoint = new int[, ] { { 0, 2 }, { 2, 3 }, { 0, 5 }, { 5, 6 }, { 0, 9 }, { 9, 10 }, { 9, 11 }, { 11, 12 }, { 12, 13 }, { 9, 14 }, { 14, 15 }, { 15, 16 } };
    // int[, ] NormalizeJoint = new int[, ] { { 0, 1 }, { 1, 2 }, { 0, 3 }, { 3, 4 }, { 0, 5 }, { 5, 6 }, { 5, 7 }, { 7, 8 }, { 8, 9 }, { 5, 10 }, { 10, 11 }, { 11, 12 } };
    int[, ] joints = new int[, ] { { 3, 4 }, { 4, 5 }, { 5, 6 }, { 3, 7 }, { 7, 8 }, { 8, 9 }, { 3, 10 }, { 10, 11 }, { 11, 12 }, { 3, 13 }, { 13, 14 }, { 14, 15 }, { 3, 16 }, { 16, 17 }, { 17, 18 },
                                   { 21, 22 }, { 22, 23 }, { 23, 24 }, { 21, 25 }, { 25, 26 }, { 26, 27 }, { 21, 28 }, { 28, 29 }, { 29, 30 }, { 21, 31 }, { 31, 32 }, { 29, 30 }, { 21, 34 }, { 34, 35 }, { 35, 36 }  };
    int[, ] BoneJoint = new int[, ] { { 3, 5 }, { 4, 6 }, { 3, 8 }, { 7, 9 }, { 3, 11 }, { 10, 12 }, { 3, 14 }, { 13, 15 }, { 3, 17 }, { 16, 18 },
                                      { 21, 23 }, { 22, 24 }, { 21, 26 }, { 25, 27 }, { 21, 29 }, { 28, 30 }, { 21, 32 }, { 31, 33 }, { 21, 35 }, { 34, 36 }};
    int[, ] NormalizeJoint = new int[, ] { { 0, 1 }, { 0, 2 }, { 0, 3 }, { 0, 4 }, { 0, 5 }, 
                                           { 6, 7 }, { 6, 8 }, { 6, 9 }, { 6, 10 }, { 6, 11 }};
    // int[,] NormalizeJoint = new int[,] { { 0, 1 }, { 1, 2 }, { 0, 3 }, { 3, 4 }, { 0, 5 }, { 5, 6 }, { 5, 10 }, { 10, 11 }, { 11, 12 }, { 5, 7 }, { 7, 8 }, { 8, 9 } };
    int NowFrame = 0;
    void Start()
    {
        PointUpdate();
    }
    void Update()
    {
        Timer += Time.deltaTime;
        if (Timer > (1 / FrameRate))
        {
            Timer = 0;
            PointUpdate();
        }
        if (!FullbodyIK)
        {
            IKFind();
        }
        else
        {
            IKSet();
        }
    }
    void PointUpdate()
    {
        StreamReader fi = null;
        if (NowFrame < Data_Size)
        {
            fi = new StreamReader(Application.dataPath + Data_Path + File_Name + (NowFrame + startFrame).ToString() + ".txt");
            NowFrame++;
            string all = fi.ReadToEnd();
            string[] axis = all.Split(']');
            float[] x = axis[0].Replace("[", "").Replace(" ", "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
			float[] y = axis[1].Replace("[", "").Replace(" ", "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
			float[] z = axis[2].Replace("[", "").Replace(" ", "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
            // float[] x = axis[0].Replace("[", "").Replace(Environment.NewLine, "").Split(' ').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
            // float[] y = axis[2].Replace("[", "").Replace(Environment.NewLine, "").Split(' ').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
            // float[] z = axis[1].Replace("[", "").Replace(Environment.NewLine, "").Split(' ').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
            for (int i = 0; i < 38; i++)
            {
                points[i] = new Vector3(x[i], z[i], y[i]);
            }

            for (int i = 0; i < 10; i++)
            {
                // NormalizeBone[i] = (points[BoneJoint[i, 1]] - points[BoneJoint[i, 0]]).normalized;
                NormalizeBone[i] = (points[BoneJoint[i, 1]] - points[BoneJoint[i, 0]]).normalized;
                // 최대값 최소값 찾아놓기
            }
            // 관절 전체에 대해 normalize 연산 실행
        }
    }
    void IKFind()
    {
        FullbodyIK = GameObject.Find("FullBodyIK");
        if (FullbodyIK)
        {
            for (int i = 0; i < Enum.GetNames(typeof(OpenPosehandsRef)).Length; i++)
            {
                Transform obj = GameObject.Find(Enum.GetName(typeof(OpenPosehandsRef), i)).transform;
                if (obj)
                {
                    BoneList.Add(obj);
                }
            }
            for (int i = 0; i < Enum.GetNames(typeof(NormalizeBonehandsRef)).Length; i++)
            {
                BoneDistance[i] = Vector3.Distance(BoneList[NormalizeJoint[i, 0]].position, BoneList[NormalizeJoint[i, 1]].position);
            }
        }
    }
    void IKSet()
    {
        for (int i = 0; i < 10; i++)
        {
            BoneList[NormalizeJoint[i, 1]].position = Vector3.Lerp(
                BoneList[NormalizeJoint[i, 1]].position,
                BoneList[NormalizeJoint[i, 0]].position + BoneDistance[i] * NormalizeBone[i], 0.05f
                // BoneList[NormalizeJoint[i, 0]].position + BoneDistance[i] * NormalizeBone[i], 0.05f
            );
            DrawLine(BoneList[NormalizeJoint[i, 0]].position + Vector3.right, BoneList[NormalizeJoint[i, 1]].position + Vector3.right, Color.red);
        }
        for (int i = 0; i < joints.Length / 2; i++)
        {
            DrawLine(points[joints[i, 0]] * 1.0f + new Vector3(1000, -100, 0), points[joints[i, 1]] * 1.0f + new Vector3(1000, -100, 0), Color.blue);
        }
    }
    void DrawLine(Vector3 s, Vector3 e, Color c)
    {
        Debug.DrawLine(s, e, c);
    }
}


enum OpenPosehandsRef
{
    //12개
    RightWrist,
    RightThumb,
    RightIndex,
    RightMiddle,
    RightRing,
    RightLittle,

    LeftWrist,
    LeftThumb,
    LeftIndex,
    LeftMiddle,
    LeftRing,
    LeftLittle,


};

enum NormalizeBonehandsRef
{
    RightWrist2RightThumb,
    RightWrist2RightIndex,
    RightWrist2RightMiddle,
    RightWrist2RightRing,
    RightWrist2RightLittle,
    LeftWrist2LeftThumb,
    LeftWrist2LeftIndex,
    LeftWrist2LeftMiddle,
    LeftWrist2LeftRing,
    LeftWrist2LeftLittle,
};