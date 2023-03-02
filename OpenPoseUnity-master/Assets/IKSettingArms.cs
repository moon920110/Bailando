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
    Vector3[] NormalizeBone = new Vector3[16];
    float[] BoneDistance = new float[16];
    float Timer;
    int[,] joints = new int[,] { { 37, 1 }, { 1, 2 }, { 2, 3 },
                                 { 3, 4 }, { 4, 5 }, { 5, 6 }, { 3, 7 }, { 7, 8 }, { 8, 9 }, { 3, 10 }, { 10, 11 }, { 11, 12 }, { 3, 13 }, { 13, 14 }, { 14, 15 }, { 3, 16 }, { 16, 17 }, { 17, 18 },
                                 { 37, 19 }, { 19, 20 }, { 20, 21 },
                                 { 21, 22 }, { 22, 23 }, { 23, 24 }, { 21, 25 }, { 25, 26 }, { 26, 27 }, { 21, 28 }, { 28, 29 }, { 29, 30 }, { 21, 31 }, { 31, 32 }, { 29, 30 }, { 21, 34 }, { 34, 35 }, { 35, 36 }};

    int[,] BoneJoint = new int[,] {{ 37, 1 }, { 1, 2 }, { 2, 3 }, 
                                    { 3, 6 }, { 3, 9 }, { 3, 12 }, { 3, 15 }, { 3, 18 }, 
                                    { 37, 19 }, { 19, 20 }, { 20, 21 },
                                    { 21, 24 }, { 21, 27 }, { 21, 30 }, { 21, 33 }, { 21, 36 }};

    int[,] NormalizeJoint = new int[,] {{ 0, 1 }, { 1, 2 }, { 2, 3 }, 
                                        { 3, 4 }, { 3, 5 }, { 3, 6 }, {3, 7 }, { 3, 8 },
                                        { 0, 9 }, { 9, 10 }, { 10, 11 }, 
                                        { 11, 12 },{11,13 },{11, 14 },{11,15 },{11, 16 }  };
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
                points[i] = new Vector3(x[i], z[i], -y[i]);
            }
            for (int i = 0; i < 16; i++)
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
            for (int i = 0; i < 16; i++) //Enum.GetNames(typeof(NormalizeBonehandsRef)).Length
            {
                BoneDistance[i] = Vector3.Distance(BoneList[NormalizeJoint[i, 0]].position, BoneList[NormalizeJoint[i, 1]].position);
            }
        }
    }
    void IKSet()
    {
        for (int i = 0; i < 16; i++)
        {
            BoneList[NormalizeJoint[i, 1]].position = Vector3.Lerp(
                BoneList[NormalizeJoint[i, 1]].position,
                BoneList[NormalizeJoint[i, 0]].position + BoneDistance[i] * (animator.GetBoneTransform(HumanBodyBones.Hips).rotation * NormalizeBone[i]), 0.05f
           );
            // BoneList[NormalizeJoint[i, 1]].position += (BoneList[0].position - BoneList[NormalizeJoint[i, 1]].position) + BoneList[NormalizeJoint[i, 1]].position;
            // BoneListTmp[NormalizeJoint[i, 1]].RotateAround(animator.GetBoneTransform(HumanBodyBones.Hips).position,axis,(angle-previousAngle));
            

            DrawLine(BoneList[NormalizeJoint[i, 0]].position + Vector3.right, BoneList[NormalizeJoint[i, 1]].position + Vector3.right, Color.red);
        }
        // BoneList[1].position += new Vector3(0.1f, 0, 0);
        for (int i = 0; i < joints.Length / 2; i++)
        {
            DrawLine(points[joints[i, 0]] * 1.0f + new Vector3(1, 1, 1), points[joints[i, 1]] * 1.0f + new Vector3(1, 1, 1), Color.blue);
        }
    }
    void DrawLine(Vector3 s, Vector3 e, Color c)
    {
        Debug.DrawLine(s, e, c);
    }
}
enum OpenPosehandsRef
{
    //17개
    Neck,
    RightArm,
    RightElbow,
    RightWrist,
    RightThumb,
    RightIndex,
    RightMiddle,
    RightRing,
    RightLittle,
    LeftArm,
    LeftElbow,
    LeftWrist,
    LeftThumb,
    LeftIndex,
    LeftMiddle,
    LeftRing,
    LeftLittle,
    Head
};
enum NormalizeBonehandsRef
{
    RightArm2RightWrist,
    RightWrist2RightThumb,
    RightWrist2RightIndex,
    RightWrist2RightMiddle,
    RightWrist2RightRing,
    RightWrist2RightLittle,
    LeftArm2LeftWrist,
    LeftWrist2LeftThumb,
    LeftWrist2LeftIndex,
    LeftWrist2LeftMiddle,
    LeftWrist2LeftRing,
    LeftWrist2LeftLittle,
};