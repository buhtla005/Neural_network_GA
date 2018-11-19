using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class NeuralNetwork
{
	//ima svu topologiju neuralne mreze
	ReadOnlyCollection<UInt32> TheTopology;
	//sadrzi sve djelove neuralne mreze
	NeuralSection[] Sections; 
	//nasumicna instanca mreze koristena za mutaciju neuralne mreze
	Random TheRandomizer; 



	// Vraca topologiju u obliku polja
    public UInt32[] Topology
    {
        get
        {
            UInt32[] Result = new UInt32[TheTopology.Count];

            TheTopology.CopyTo(Result, 0);

            return Result;
        }
    }


    private class NeuralSection
    {
		//Sadrzi sve tezine djelova [i][j] gdje su i tezine iz neurona ulaznog layera a j iz neurona izlaza
        private double[][] Weights; 
		//Referenca na nasumicnu istancu neuralne mreze
        private Random TheRandomizer;


        //Pokreni neuralnu selekciju iz Topologije i seeda
        public NeuralSection(UInt32 InputCount, UInt32 OutputCount, Random Randomizer)
        {
            if (InputCount == 0)
                throw new ArgumentException("Ne postoji neural layer bez inputa.", "InputCount");
            else if (OutputCount == 0)
                throw new ArgumentException("Ne postoji neural layer bez outputa.", "OutputCount");
            else if (Randomizer == null)
                throw new ArgumentException("Randomizer nesmije bit null.", "Randomizer");


            TheRandomizer = Randomizer;

			//tezine se postave
            Weights = new double[InputCount + 1][]; // +1 for the Bias Neuron

            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = new double[OutputCount];

            // nasumicne tezine
            for (int i = 0; i < Weights.Length; i++)
                for (int j = 0; j < Weights[i].Length; j++)
                    Weights[i][j] = TheRandomizer.NextDouble() - 0.5f;
        }



       //Inicijalizira deep-copy neuralne mreze
        public NeuralSection(NeuralSection Main)
        {

            TheRandomizer = Main.TheRandomizer;

            Weights = new double[Main.Weights.Length][];

            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = new double[Main.Weights[0].Length];

            for (int i = 0; i < Weights.Length; i++)
                for (int j = 0; j < Weights[i].Length; j++)
                    Weights[i][j] = Main.Weights[i][j];
        }

		// Input -> output
        public double[] FeedForward(double[] Input)
        {
			
            if (Input == null)
                throw new ArgumentException("The input array cannot be set to null.", "Input");
            else if (Input.Length != Weights.Length - 1)
                throw new ArgumentException("The input array's length does not match the number of neurons in the input layer.", "Input");

            double[] Output = new double[Weights[0].Length];


            for (int i = 0; i < Weights.Length; i++)
                for (int j = 0; j < Weights[i].Length; j++)
                    if (i == Weights.Length - 1) // If is Bias Neuron
                        Output[j] += Weights[i][j]; // Then, the value of the neuron is equal to one
                    else
                        Output[j] += Weights[i][j] * Input[i];

            // aktivacijska funkcija
            for (int i = 0; i < Output.Length; i++)
                Output[i] = ReLU(Output[i]);

            // Return Output
            return Output;
        }

        //Mutiraj neuralnu selekciju
        public void Mutate (double MutationProbablity, double MutationAmount)
        {
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    if (TheRandomizer.NextDouble() < MutationProbablity)
                        Weights[i][j] = TheRandomizer.NextDouble() * (MutationAmount * 2) - MutationAmount;
                }
            }
        }

        //Aktivacijska funkcija
        private double ReLU(double x)
        {
            if (x >= 0)
                return x;
            else
                return x / 20;
        }
    }
    


    //Inicijalizira neuralnu mrezu iz topologije i seed-a. Provjeri uvijete, inicaliziraj nasumicni odabir 
    public NeuralNetwork (UInt32[] Topology, Int32? Seed = 0)
    {
        if (Topology.Length < 2)
            throw new ArgumentException("Neuralna mreza nesmije imati manje od 2 layera.", "Topology");

        for (int i = 0; i < Topology.Length; i++)
        {
            if(Topology[i] < 1)
                throw new ArgumentException("Layer neurona mora imati barem jedan neuron.", "Topology");
        }

        // Initialize Randomizer za instancu
        if (Seed.HasValue)
            TheRandomizer = new Random(Seed.Value);
        else
            TheRandomizer = new Random();

        // Postavi topologiju
        TheTopology = new List<uint>(Topology).AsReadOnly();

        // Inicijaliziraj selekciju
        Sections = new NeuralSection[TheTopology.Count - 1];

        // Postavi na selekciju
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i] = new NeuralSection(TheTopology[i], TheTopology[i + 1], TheRandomizer);
        }
    }

    

	/// Inicira nezavisnu deep-copy mrezu koju kloniramo
    public NeuralNetwork (NeuralNetwork Main)
    {
        TheRandomizer = new Random(Main.TheRandomizer.Next());
       
        TheTopology = Main.TheTopology;
   
        Sections = new NeuralSection[TheTopology.Count - 1];

        for (int i = 0; i < Sections.Length; i++)
            Sections[i] = new NeuralSection (Main.Sections[i]);
    }

   

	//Prosljedi unose kroz mrezu i dobi izlaze
	public double[] FeedForward(double[] Input)
    {

        if (Input == null)
            throw new ArgumentException("Polje unosa nesmije bit null.", "Input");
        else if (Input.Length != TheTopology[0])
            throw new ArgumentException("Velicina input polja se ne podudara sa velicinom neurona unosa.", "Input");

        double[] Output = Input;

        for (int i = 0; i < Sections.Length; i++)
            Output = Sections[i].FeedForward(Output);

        return Output;
    }



    // Vjerojatnost da ce se dogoditi mutacija i kolicina mutacije
    public void Mutate (double MutationProbablity = 0.3, double MutationAmount = 2.0)
    {
        for (int i = 0; i < Sections.Length; i++)
            Sections[i].Mutate(MutationProbablity, MutationAmount);
    }
}