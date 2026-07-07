using UnityEngine;

public class MinimapCameraMover : MonoBehaviour
{
    public Transform target;
    public float height = 50f;
    //public bool followTargetHeight = true; 
    public bool smoothFollow = true;
    public float smoothSpeed = 5f;

    private Vector3 targetPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target == null) return;
        SnapToTarget();

    }
    private void LateUpdate()
    {
        if (target == null) return;

        //targetPos = target.position;
        //targetPos.y += height;
        targetPos = target.position;
        Quaternion targetRot = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        //targetRot = target.rotation;
        
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
            transform.rotation = targetRot;
        }
        else
            transform.position = targetPos;

        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    void SnapToTarget()
    {
        if (target == null) return;

        Vector3 snapPos = target.position;
        snapPos.y = target.position.y + height;
        transform.position = snapPos;
    }
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        SnapToTarget();
    }
}
