using UnityEngine;

public class QuadHP : MonoBehaviour
{

    public Transform fillQuad;
    public Transform objectToFollow;
    public Vector3 offset = new Vector3(0, 1, 0);

    private Vector3 fillFullScale;
    private float maxHealth;
    private float curHealth;
    private Material fillMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (fillQuad != null)
        {
            fillFullScale = fillQuad.localScale;
            fillMaterial = fillQuad.GetComponent<Renderer>().material;
        }    
    }
    public void Set(float maxHp, Transform target)
    {
        maxHealth = maxHp;
        objectToFollow = target;
    }
    public void UpdateHealth(float curHp)
    {
        //curHealth = curHp;
        curHealth = Mathf.Clamp(curHp, 0, maxHealth);
        float p = curHealth / maxHealth;

        if (fillQuad != null)
        {
            Vector3 newscale = fillFullScale;
            newscale.x = fillFullScale.x + p;
            fillQuad.localScale = newscale;
        }

        if (fillMaterial != null)
        {
            if (p < 0.5f)
            {
                fillMaterial.color = Color.Lerp(Color.red, Color.yellow, p * 2f);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
