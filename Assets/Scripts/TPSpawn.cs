using UnityEngine;

public class TPSpawn : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform spawn;
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            Debug.Log("Key R pressed");
            rb.transform.position = spawn.position;
            rb.transform.rotation = spawn.rotation;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
