using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LunarLander;
namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGetRandomDouble()
        {
            for(int i=0; i<100; i++)
            {
                double minValue = 0.1;
                double maxValue = 2.3;
                double value = LunarLander.GeneticAlgorithm.GetRandomDouble(minValue, maxValue);
                Assert.IsTrue(value >= minValue && value < maxValue); 
            }
            
        }

        [TestMethod]
        public void TestGetNRandomIntegers()
        {
           
            int minValue = 2;
            int maxValue = 150;
            List<int> values = GeneticAlgorithm.GetNRandomIntegers(100, maxValue, minValue);

            foreach(int v in values)
            {
                Assert.IsTrue(v >= minValue && v < maxValue);
            } 

        }

        [TestMethod]
        public void TestRouletteSelction()
        {

            List<SpaceShip> initialGeneration = GeneticAlgorithm.CreateInitialGeneration();
            SpaceShip s=GeneticAlgorithm.RouletteSelection(initialGeneration);

        }


        [TestMethod]
        public void TestTournamentSelction()
        {

            List<SpaceShip> initialGeneration = GeneticAlgorithm.CreateInitialGeneration();
            SpaceShip s = GeneticAlgorithm.TournamentSelection(initialGeneration);

        }

        [TestMethod]
        public void TCrossover()
        {

            List<SpaceShip> initialGeneration = GeneticAlgorithm.CreateInitialGeneration();
  
            List<SpaceShip> children = GeneticAlgorithm.CrossoverOnePoint(initialGeneration[0], initialGeneration[1]);

        }

        [TestMethod]
        public void TCrossoverTwoPoints()
        {

            List<SpaceShip> initialGeneration = GeneticAlgorithm.CreateInitialGeneration();

            List<SpaceShip> children = GeneticAlgorithm.CrossoverTwoPoints(initialGeneration[0], initialGeneration[1]);

        }

        [TestMethod]
        public void TMutate()
        {

            List<SpaceShip> initialGeneration = GeneticAlgorithm.CreateInitialGeneration();

            SpaceShip children = GeneticAlgorithm.Mutate(initialGeneration[0]);

        }


        





    }
}
