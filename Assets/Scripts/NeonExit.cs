using UnityEngine;
public class NeonExit : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<NeonPlayer>() && NeonMazeGame.Instance)
            NeonMazeGame.Instance.Finish(true,"EXIT REACHED!");
    }
}
