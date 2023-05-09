using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class IKSettingBody : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField, Range(10, 120)] float FrameRate;
    public List<Transform> BoneList = new List<Transform>();
    [SerializeField] string Data_Path;
    [SerializeField] string File_Name;
    [SerializeField] int startFrame;
    [SerializeField] int Data_Size;
    GameObject FullbodyIK;
    Vector3[] points = new Vector3[17];
    Vector3 pointDifference = new Vector3();
    Vector3[] NormalizeBone = new Vector3[12];
    float[] BoneDistance = new float[12];
    float Timer;
    int[, ] joints = new int[, ] { { 0, 1 }, { 1, 2 }, { 2, 3 }, { 0, 4 }, { 4, 5 }, { 5, 6 }, { 0, 7 }, { 7, 8 }, { 8, 9 }, { 9, 10 }, { 8, 11 }, { 11, 12 }, { 12, 13 }, { 8, 14 }, { 14, 15 }, { 15, 16 } };
    int[, ] BoneJoint = new int[, ] { { 0, 2 }, { 2, 3 }, { 0, 5 }, { 5, 6 }, { 0, 9 }, { 9, 10 }, { 9, 11 }, { 11, 12 }, { 12, 13 }, { 9, 14 }, { 14, 15 }, { 15, 16 } };
    int[, ] NormalizeJoint = new int[, ] { { 0, 1 }, { 1, 2 }, { 0, 3 }, { 3, 4 }, { 0, 5 }, { 5, 6 }, { 5, 7 }, { 7, 8 }, { 8, 9 }, { 5, 10 }, { 10, 11 }, { 11, 12 } };
    // int[, ] joints = new int[, ] { { 0, 2 }, { 2, 3 }, { 0, 5 }, { 5, 6 }, { 0, 7 }, { 7, 8 }, { 8, 9 }, { 9, 10 }};
    // int[, ] BoneJoint = new int[, ] {{ 0, 2 }, { 2, 3 }, { 0, 5 }, { 5, 6 }, { 0, 9 }, { 9, 10 }};
    // int[, ] NormalizeJoint = new int[, ] { { 0, 1 }, { 1, 2 }, { 0, 3 }, { 3, 4 }, { 0, 5 }, { 5, 6 }};
    int NowFrame = 0;
    Vector3 initPosition;
    float initX;
    float initY;
    float initZ;
    Vector3 positionOffset;
    int NormalizeBoneLen = 12;
    public bool ableArms = false;
    List<float[]> Allpoints;
    int NumberOfFiles;
    void Start()
    {
        NormalizeBoneLen = 12;
        BoneJoint = new int[, ] { { 0, 2 }, { 2, 3 }, { 0, 5 }, { 5, 6 }, { 0, 9 }, { 9, 10 }, { 9, 11 }, { 11, 12 }, { 12, 13 }, { 9, 14 }, { 14, 15 }, { 15, 16 } };
        NormalizeJoint = new int[, ] { { 0, 1 }, { 1, 2 }, { 0, 3 }, { 3, 4 }, { 0, 5 }, { 5, 6 }, { 5, 7 }, { 7, 8 }, { 8, 9 }, { 5, 10 }, { 10, 11 }, { 11, 12 } };
        
        Allpoints = new List<float[]>();

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + Data_Path);
        var fi = di.EnumerateFiles("*.txt");
        NumberOfFiles = fi.Count();
        GetAllPoints();

        IKFind();
    }
    void Update()
    {
        Timer += Time.deltaTime;
        if (Timer > (1 / FrameRate))
        {
            Timer = 0;
            if(ableArms)
            {
                NormalizeBoneLen = 12;
                BoneJoint = new int[, ] { { 0, 2 }, { 2, 3 }, { 0, 5 }, { 5, 6 }, { 0, 9 }, { 9, 10 }, { 9, 11 }, { 11, 12 }, { 12, 13 }, { 9, 14 }, { 14, 15 }, { 15, 16 } };
                NormalizeJoint = new int[, ] { { 0, 1 }, { 1, 2 }, { 0, 3 }, { 3, 4 }, { 0, 5 }, { 5, 6 }, { 5, 7 }, { 7, 8 }, { 8, 9 }, { 5, 10 }, { 10, 11 }, { 11, 12 } };
            }
            else
            {
                NormalizeBoneLen = 6;
                BoneJoint = new int[, ] {{ 0, 2 }, { 2, 3 }, { 0, 5 }, { 5, 6 }, { 0, 9 }, { 9, 10 }};
                NormalizeJoint = new int[, ] { { 0, 1 }, { 1, 2 }, { 0, 3 }, { 3, 4 }, { 0, 5 }, { 5, 6 }};
            }
            for (int i = 0; i < 17; i++)
            {
                points[i] = new Vector3(
                    Allpoints[NowFrame*3][i],
                    Allpoints[NowFrame*3+1][i],
                    Allpoints[NowFrame*3+2][i]
                    );
            }
            for (int i = 0; i < NormalizeBoneLen; i++)
            {
                NormalizeBone[i] = (points[BoneJoint[i, 1]] - points[BoneJoint[i, 0]]).normalized;
            }
            NowFrame++;
            PointUpdate();
        }

        IKSet();
    }
    void GetAllPoints()
    {
        StreamReader fi = null;
        // loop until read all files in folder
        for (int files = 0; files < NumberOfFiles; files++)
        {
            fi = new StreamReader(Application.dataPath + Data_Path + File_Name + (files + startFrame).ToString() + ".txt");
            string all = fi.ReadToEnd();
            string[] axis = all.Split(']');
            float[] x = axis[0].Replace("[", "").Replace(" ", "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
            float[] y = axis[1].Replace("[", "").Replace(" ", "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
            float[] z = axis[2].Replace("[", "").Replace(" ", "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
        
            for (int i = 0; i < 17; i++)
            {
                float x1 = x[i];
                float y1 = y[i];
                float z1 = z[i];
                x[i] = x1 - x[0] - initX;
                y[i] = x1 - y[0] - initY;
                z[i] = z1 - z[0] - initZ;
            }
            Allpoints.Add(x);
            Allpoints.Add(y);
            Allpoints.Add(z);
            fi.Close();
        }
    }
    void IKFind()
    {
        FullbodyIK = gameObject.transform.Find("FullBodyIK").gameObject;
        if (FullbodyIK)
        {
            for (int i = 0; i < Enum.GetNames(typeof(OpenPoseRef)).Length; i++)
            {
                Transform obj = RecursiveFindChild(FullbodyIK.transform, Enum.GetName(typeof(OpenPoseRef), i));
                if (obj)
                {
                    BoneList.Add(obj);
                }
            }
            for (int i = 0; i < NormalizeBoneLen; i++)
            {
                BoneDistance[i] = Vector3.Distance(BoneList[NormalizeJoint[i, 0]].position, BoneList[NormalizeJoint[i, 1]].position);
            }
        }
        initPosition = BoneList[0].position;
    }
    void IKSet()
    {
        // BoneList[0].position = BoneList[0].position;
        BoneList[0].position = (points[0] * 0.004f) + initPosition;
        // BoneList[0].position = Vector3.Scale(initPosition,-(points[0]*0.00f));
        for (int i = 0; i < NormalizeBoneLen; i++)
        {
            BoneList[NormalizeJoint[i, 1]].position = Vector3.Lerp(
                BoneList[NormalizeJoint[i, 1]].position,
                BoneList[NormalizeJoint[i, 0]].position + BoneDistance[i] * NormalizeBone[i], 0.05f
            );
            DrawLine(BoneList[NormalizeJoint[i, 0]].position + Vector3.right, BoneList[NormalizeJoint[i, 1]].position + Vector3.right, Color.red);
        }
        Vector3 hipRot = (points[0]+points[1]+points[4]).normalized;
        FullbodyIK.transform.forward = Vector3.Lerp(FullbodyIK.transform.forward, new Vector3(hipRot.x, hipRot.y, hipRot.z), 0.1f);
        for (int i = 0; i < joints.Length / 2; i++)
        {
            DrawLine(points[joints[i, 0]] * 0.005f + Vector3.right - new Vector3(-1f, -0.25f, 1f), points[joints[i, 1]]  * 0.005f + Vector3.right - new Vector3(-1f, -0.25f, 1f), Color.blue);
        }
        pointDifference = points[0];
    }
    void DrawLine(Vector3 s, Vector3 e, Color c)
    {
        Debug.DrawLine(s, e, c);
    }
    
    Transform RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if(child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = RecursiveFindChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

}

enum OpenPoseRef
{
    Hips,
    LeftKnee,
    LeftFoot,
    RightKnee,
    RightFoot,
    Neck,
    Head,
    RightArm,
    RightElbow,
    RightWrist,
    LeftArm,
    LeftElbow,
    LeftWrist,
};
enum NormalizeBoneRef
{
    Hip2RightKnee,
    RightKnee2RightFoot,
    Hip2LeftKnee,
    LeftKnee2LeftFoot,
    Hip2Neck,
    Neck2Head,
    // Neck2RightArm,
    // RightArm2RightElbow,
    // RightElbow2RightWrist,
    // Neck2LeftArm,
    // LeftArm2LeftElbow,
    // LeftElbow2LeftWrist
};

