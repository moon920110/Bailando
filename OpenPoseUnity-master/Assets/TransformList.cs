using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformList : MonoBehaviour
{
    public GameObject m_Objects;
    void Start()
    {
        m_Objects = FindWithTag("rabbit");
        GetOrbits();
    }
    //return specific transform of the gameobject

    public Transform GetNeck()
    {
        var tmp = m_Objects.transform.GetChild(0);
        tmp = tmp.transform.GetChild(0);
        tmp = tmp.transform.GetChild(0);
        tmp = tmp.transform.GetChild(2);
        tmp = tmp.transform.GetChild(0);
        return tmp.transform;
    }
    public Transform GetSpine()
    {
        var tmp = m_Objects.transform.GetChild(0);
        tmp = tmp.transform.GetChild(0);
        tmp = tmp.transform.GetChild(0);
        tmp = tmp.transform.GetChild(2);
        return tmp.transform;
    }
    //Find gameobject with specific tag and actived
    public GameObject FindWithTag(string tag)
    {
        var objs = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objs)
        {
            if (obj.activeSelf)
            {
                return obj;
            }
        }
        return null;
    }

    //Get all cameraOrbit script
    public void GetOrbits()
    {
        List<GameObject> objs = new List<GameObject>();
        for (int i = 0; i<transform.childCount; i++)
        {
            objs.Add(transform.GetChild(i).gameObject);
        }

        foreach (GameObject obj in objs)
        {
            if(obj.tag == "MainCamera"|| obj.tag == "CameraWide")
            {
                var tmp = obj.GetComponent<CameraOrbit>();
                if (tmp != null)
                {
                    tmp.Targets[0] = GetNeck();
                }
            }
            if(obj.tag == "CameraUpper"||obj.tag == "CameraRight"||obj.tag == "CameraLeft")
            {
                var tmp = obj.GetComponent<CameraOrbit>();
                if (tmp != null)
                {
                    tmp.Targets[0] = GetSpine();
                }
                
            }
        }
    }
}
