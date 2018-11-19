using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Car : MonoBehaviour
{
	Rigidbody TheRigidbody; 
	LineRenderer TheLineRenderer;

	public string m_theGuid { get; private set; }
	public int m_fitness { get; private set; } 

    [SerializeField] 
	LayerMask m_sensorMask;
    [SerializeField] 
	float m_fitnessUnchangedDie = 5;
	[SerializeField] 
	bool m_userInput = false;

	//Neural network koja se odnosi na sljedecu neuralnu mrezu koja se stavlja na sljedeci instancirani auto 
    public static NeuralNetwork m_nextNetwork = new NeuralNetwork(new uint[] { 6, 4, 3, 2 }, null);
	public NeuralNetwork m_network { get; private set; }




	// Ako dotaknem checkpoint povecaj fitness i ukoliko je br >= 133 naucio sam sve i dobivam certifikat koji izgleda ko jeftina verzija nase diplome
	public void CheckpointHit ()
	{
		m_fitness++;
	}



	// Kada auto pogodi zid javi EvolutionManageru da je auto unisten i postavi ga u inaktivno stanje
	public void WallHit()
	{
		EvolutionManager.Singleton.CarDistroyed(this, m_fitness);
		gameObject.SetActive(false); 
	}



	// Prije pocetka pokretanja dohvati sve komponente, dodaj unikatni ID, pobrini se da sljedeci auto nema istu mrezu, postavi duljinu zrake.
	// Pokreni korutinu i provjeri ako se stvari ne popravljaju
    private void Awake()
    {

		TheRigidbody = GetComponent<Rigidbody>();
		TheLineRenderer = GetComponent<LineRenderer>(); 

		m_theGuid = Guid.NewGuid().ToString();
        
		m_network = m_nextNetwork;
		m_nextNetwork = new NeuralNetwork(m_nextNetwork.Topology, null);

        StartCoroutine(IsNotImproving()); 
        TheLineRenderer.positionCount = 17;
    }



	// Provjeri svakih par sekundi ako se fitness promijenio i ako nije unisti auto 
	IEnumerator IsNotImproving ()
	{
		while(true)
		{
			int oldFitness = m_fitness;
			yield return new WaitForSeconds(m_fitnessUnchangedDie);
			if (oldFitness == m_fitness)
				WallHit(); 
		}
	}



	// Ako zelimo sami vozit kako bi pojednostavili ucenje onda koristimo klasicno kretanje preko Move funkcije tj. vertikalne i horizontalne osi, ako 
	// sve ostavimo racunalu onda pokrenemo funkciju za dohvacanje vrijednosti osi (GetNeuralInputAxis) koje onda proslijedimo na Move funkciju
    private void FixedUpdate()
    {
		if (m_userInput)
            Move(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));
        else 
        {
            float Vertical;
            float Horizontal;

            GetNeuralInputAxis(out Vertical, out Horizontal);
            Move(Vertical, Horizontal);
        }
    }



	// Pomicamo auto u trazenom smjeru
	public void Move (float v, float h)
	{
		TheRigidbody.velocity = transform.right * v * 4;
		TheRigidbody.angularVelocity = transform.up * h * 3;
	}



    // Stvori sve zrake, pusti ih kroz neuralnu mrezu i izlazne vrijednosti posalji na Move funkciju
	// Feed foward prosljeduje neural output na mrezu, a vrijednosti od 2 outputa daju vertikalnu i horizontalnu vrijednost
	// po kojoj se krece auto. Ukoliko su vrijednosti 0 samo salji ravno...
    void GetNeuralInputAxis (out float Vertical, out float Horizontal)
    {
		double[] NeuralInput = new double[m_nextNetwork.Topology[0]];
		float SqrtHalf = Mathf.Sqrt(0.5f);
        NeuralInput[0] = CastRay(transform.forward, Vector3.forward, 1) / 4;
        NeuralInput[1] = CastRay(-transform.forward, -Vector3.forward, 3) / 4;
        NeuralInput[2] = CastRay(transform.right, Vector3.right, 5) / 4;
        NeuralInput[3] = CastRay(-transform.right, -Vector3.right, 7) / 4;
        NeuralInput[4] = CastRay(transform.right * SqrtHalf + transform.forward * SqrtHalf, Vector3.right * SqrtHalf + Vector3.forward * SqrtHalf, 9) / 4;
        NeuralInput[5] = CastRay(transform.right * SqrtHalf + -transform.forward * SqrtHalf, Vector3.right * SqrtHalf + -Vector3.forward * SqrtHalf, 13) / 4;

        double[] NeuralOutput = m_network.FeedForward(NeuralInput);
        
        if (NeuralOutput[0] <= 0.25f)
            Vertical = -1;
        else if (NeuralOutput[0] >= 0.75f)
            Vertical = 1;
        else
            Vertical = 0;

        if (NeuralOutput[1] <= 0.25f)
            Horizontal = -1;
        else if (NeuralOutput[1] >= 0.75f)
            Horizontal = 1;
        else
            Horizontal = 0;

        if (Vertical == 0 && Horizontal == 0)
            Vertical = 1;
    }



    // Stvori zraku odredene duljine koja ce se vidjeti uz pomoc line renderera. Ako smo bacili zraku i pogodili 
	// vratimo udaljenost od tocke pogodka do auta. Ako nemamo pogodak vratimo udaljenost zrake
    double CastRay (Vector3 RayDirection, Vector3 LineDirection, int LinePositionIndex)
    {
		RaycastHit hit;
        float length = 4;

        if (Physics.Raycast(transform.position, RayDirection, out hit, length, m_sensorMask))
        {
            float distance = Vector3.Distance(hit.point, transform.position);
            TheLineRenderer.SetPosition(LinePositionIndex, distance * LineDirection);
            return distance;
        }
        else
        {
            TheLineRenderer.SetPosition(LinePositionIndex, LineDirection * length); 
            return length; 
        }
    }

}
