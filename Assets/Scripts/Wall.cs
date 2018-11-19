using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] 
	string m_layerHitName = "CarCollider";

	// Ako nesto dotakne zid provjeri ako je auto i ako je onda pokreni funkciju
    private void OnCollisionEnter(Collision col) // Once anything hits the wall
    {
        if (col.gameObject.layer == LayerMask.NameToLayer(m_layerHitName))
        	col.transform.GetComponent<Car>().WallHit();
    }
}
