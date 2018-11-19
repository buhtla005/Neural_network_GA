using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] 
	string m_layerHitName = "CarCollider";
    List<string> m_allGuids = new List<string>(); 


	// Na dodir sa zidom provjeri ako se radi o autu i ako da uzmi njegovu komponentu i preko toga dohvati
	// unikatni ID od auta. Ukoliko taj checkpoint nema ID auta na listi, dodaj taj auto na listu i povecaj 
	// njegov fitness
    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer(m_layerHitName))
        {
            Car component = col.transform.parent.GetComponent<Car>();
			string carGuid = component.m_theGuid;

            if (!m_allGuids.Contains(carGuid))
            {
                m_allGuids.Add(carGuid);
                component.CheckpointHit();
            }
        }
    }
}
