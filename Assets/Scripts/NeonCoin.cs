using UnityEngine;
public class NeonCoin : MonoBehaviour
{
    public float spinSpeed = 100f;
    void Update() { transform.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.World); }
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NeonPlayer>())
        {
            if (NeonMazeGame.Instance) NeonMazeGame.Instance.AddCoin();
            Destroy(gameObject);
        }
    }
}
