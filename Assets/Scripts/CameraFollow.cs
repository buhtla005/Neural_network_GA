using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Vector3 m_smoothPosVelocity;
    Vector3 m_smoothRotVelocity; 

	// Najbrzi auto je najbolji auto, dohvati njegove komponente
	// vrti kroz sve ostale i dohvacaj njihove komponente - ako netko ima bolji fitness od najboljeg on postaje najbolji
	// kamera glatko prelazi na njega

    void FixedUpdate ()
    {
        Car BestCar = transform.GetChild(0).GetComponent<Car>();
        for (int i = 1; i < transform.childCount; i++)
        {
            Car CurrentCar = transform.GetChild(i).GetComponent<Car>();
			if (CurrentCar.m_fitness > BestCar.m_fitness)
                BestCar = CurrentCar;
        }

        Transform BestCarCamera = BestCar.transform.GetChild(0);
        Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, BestCarCamera.position, ref m_smoothPosVelocity, 0.7f); 
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, Quaternion.LookRotation(BestCar.transform.position - Camera.main.transform.position), 0.1f);
    }
}