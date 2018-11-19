using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
	//instanca EvolutionManagera
    public static EvolutionManager Singleton = null;

	[SerializeField]
	int m_carCount = 100; 
	int m_generationCount = 0;
	int m_bestFitness = -1;

    [SerializeField] 
	GameObject m_carPrefab; 
    [SerializeField] 
	Text m_generationNumberText;
	[SerializeField] 
	Text m_fitnessNumberText;
	[SerializeField] 
	Text m_NumberCarCountText;
	public Slider m_slider;

    List<Car> m_cars = new List<Car>(); 
	NeuralNetwork m_bestNeuralNetwork = null; 
    
	public GameObject m_endPicture;

	// Samo da preko UI-a mozemo mijenjat br auta koji se spawnaju
	public void Update()
	{
		m_carCount = (int)m_slider.value;
		m_NumberCarCountText.text = "Broj auta: " +(int)m_slider.value;
		if(m_bestFitness >= 133)
			m_endPicture.gameObject.SetActive (true);
		
		if (Input.GetKey ("escape"))
			Application.Quit ();

	}


    // Kod starta pazimo da imamo samo prvu instancu pokrenutu. Postavimo najbolju mrezu na nasumicnu novu mrezu i pokrenemo novu generaciju
    private void Start()
    {
        if (Singleton == null) 
            Singleton = this; 
        else
            gameObject.SetActive(false);

		m_endPicture.gameObject.SetActive (false);
		m_bestNeuralNetwork = new NeuralNetwork(Car.m_nextNetwork); 

        StartGeneration(); 
    }



    // Povecamo broj generacije i ispisemo sve podatke na UI. Pobrinemo se da jedan od auta ima postavnu najbolju mrezu. 
	//Instanciramo sve aute, dodamo ih na listu i kopiramo mrezu na ostale aute.
    void StartGeneration ()
    {
		m_generationCount++; // Increment the generation count
		m_generationNumberText.text = "Generation: " + m_generationCount; // Update generation text
		m_fitnessNumberText.text = "Best fitness: " + m_bestFitness; 

		for (int i = 0; i < m_carCount; i++)
        {
            if (i == 0)
				Car.m_nextNetwork = m_bestNeuralNetwork; // Make sure one car uses the best network
            else
            {
				Car.m_nextNetwork = new NeuralNetwork(m_bestNeuralNetwork); // Clone the best neural network and set it to be for the next car
				Car.m_nextNetwork.Mutate(); // Mutate it
            }

			m_cars.Add(Instantiate(m_carPrefab, transform.position, Quaternion.identity, transform).GetComponent<Car>()); // Instantiate a new car and add it to the list of cars
        }
    }

    // Pozivamo kad se auto unisti -> maknemo ga s liste zivih auta, provjerimo fitnesse najboljeg i tog auta i mjenjamo ako je potrebno, 
	// a ako vise nema auta stvori novu generaciju.
    public void CarDistroyed(Car DeadCar, int Fitness)
    {
		m_cars.Remove(DeadCar);
        Destroy(DeadCar.gameObject); 

		if (Fitness > m_bestFitness) 
        {
			m_bestNeuralNetwork = DeadCar.m_network; 
			m_bestFitness = Fitness; 
        }

		if (m_cars.Count <= 0) 
            StartGeneration(); 
    }
}
