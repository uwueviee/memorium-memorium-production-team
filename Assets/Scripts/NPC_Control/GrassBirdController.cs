﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBirdController : MonoBehaviour
{
    public EntityController entityController;

    [SerializeField] private Vector3 origin;
    [SerializeField] private Vector3 boxSize = new Vector3(5,5,5);
    [SerializeField] private bool lockOnYAxis = true;

    [SerializeField] private float maxWait = 10;
    [SerializeField] private float minWait = 2;

    private bool canMove = true;

    private void Start()
    {
        StartCoroutine(MoveToRandom());
    }

    private Vector3 getRandomPoint()
    {
        float x = Random.Range(-boxSize.x, boxSize.x);
        float y = lockOnYAxis ? 0 : Random.Range(-boxSize.y, boxSize.y);
        float z = Random.Range(-boxSize.z, boxSize.z);
        return new Vector3(x, y, z) + origin;
    }

    private IEnumerator MoveToRandom()
    {
        while (canMove)
        {
            entityController.MoveToPoint(getRandomPoint());
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
        }
    }
}