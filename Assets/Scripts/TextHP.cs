using TMPro;
using UnityEngine;

public class TextHP : MonoBehaviour
{
    public TextMeshPro hpText;
    public GameObject objectToFollow;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    public void UpdateText(float maxHp, float curHp)
    {
        float p = curHp / maxHp;
        hpText.text = $"{curHp}";
        if (p > 0.5f)
        {
            hpText.color = Color.green;
        }
        else
        {
            hpText.color = Color.red;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (objectToFollow != null)
        {
            transform.position = objectToFollow.transform.position + offset;
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
            }
        }
    }
}
