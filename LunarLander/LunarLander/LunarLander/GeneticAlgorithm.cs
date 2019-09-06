using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarLander
{
    public class GeneticAlgorithm
    {
        public static int GenerationSize = 1000;
        public static int ReproductionSize = 200;
        public static int MaxIterations = 1000;
        public static double MutationRate = 0.2;

        //velicina turnira za turnirsku selekciju
        public static int TournamentSize = 10;

        public static double GetRandomDouble(double min, double max)
        {
            Random random = new Random();
            return min + (random.NextDouble() * (max - min));
        }

        public static List<int> GetNRandomIntegers(int count, int maxValue, int minValue = 0)
        {
            HashSet<int> candidates = new HashSet<int>();
            Random r = new Random();
            while (candidates.Count < count)
            {
                // May strike a duplicate.
                candidates.Add(r.Next(minValue, maxValue));
            }

            return candidates.ToList();
        }

        //Selekcija ruletska- vece sanse da u reprodukciji ucestvuju jedinke koje su vise prilagodjenje
        //verovatnoca da jedinka i ucestvuje u reprodukciji je f(i)/suma po j f(j)
        public static SpaceShip RouletteSelction(List<SpaceShip> population)
        {

            double totalFitness = population.Sum(x => x.GAFitnessFunction);
            double selectedValue = GetRandomDouble(0, totalFitness);

            double currentSum = 0;

            for (int i = 0; i < GenerationSize; i++)
            {
                currentSum += population[i].GAFitnessFunction;
                //vraca se prva jedinka koja ispuni uslov 
                if (currentSum > selectedValue)
                    return population[i];
            }

            return null;
        }


        //pobednik turnira je jedinka sa najboljom prilagodjenoscu
        //sto je veca velicina turnira nekvalitetne jedinke imaju manje sanse da budu izabrane
        public static SpaceShip TournamentSelction(List<SpaceShip> population)
        {

            List<int> tournamet = GetNRandomIntegers(GeneticAlgorithm.TournamentSize, GeneticAlgorithm.GenerationSize);
            float max = 0;
            int maxIndex = 0;
            foreach (int i in tournamet)
            {
                if (population[i].GAFitnessFunction > max)
                {
                    max = population[i].GAFitnessFunction;
                    maxIndex = i;
                }
            }

            return population[maxIndex];

        }

        //jednopoziciono ukrstanje sa nasumicnom tackom
        public static List<SpaceShip> Crossover(SpaceShip parent1, SpaceShip parent2)
        {

            //public List<Tuple<string, int>> run
            List<SpaceShip> children = new List<SpaceShip>(2);
            Random r = new Random();
            int breakPoint = r.Next(1, SpaceShip.ChromosomeSize - 1);

            List<Tuple<string, int>> child1Run = parent1.run.Take(breakPoint).ToList().Concat(parent2.run.Skip(breakPoint).ToList()).ToList();
            List<Tuple<string, int>> child2Run = parent2.run.Take(breakPoint).ToList().Concat(parent1.run.Skip(breakPoint).ToList()).ToList();
            SpaceShip child1 = new SpaceShip(child1Run);
            SpaceShip child2 = new SpaceShip(child2Run);

            children.Add(child1);
            children.Add(child2);
            return children;
        }

        //jednopoziciono ukrstanje sa nasumicnom tackom
        public static List<SpaceShip> CrossoverTwoPoints(SpaceShip parent1, SpaceShip parent2)
        {

            //public List<Tuple<string, int>> run
            List<SpaceShip> children = new List<SpaceShip>(2);
            List<int> breakPoints = GetNRandomIntegers(2, SpaceShip.ChromosomeSize - 1, 1);
            breakPoints.Sort();

            List<Tuple<string, int>> child1Run = parent1.run.Take(breakPoints[0]).ToList().Concat(parent2.run.Skip(breakPoints[0]).Take(breakPoints[1]- breakPoints[0]).ToList()).ToList().Concat(parent1.run.Skip(breakPoints[1]).ToList()).ToList();
            List<Tuple<string, int>> child2Run = parent2.run.Take(breakPoints[0]).ToList().Concat(parent1.run.Skip(breakPoints[0]).Take(breakPoints[1]- breakPoints[0]).ToList()).ToList().Concat(parent2.run.Skip(breakPoints[1]).ToList()).ToList();
            SpaceShip child1 = new SpaceShip(child1Run);
            SpaceShip child2 = new SpaceShip(child2Run);

            children.Add(child1);
            children.Add(child2);
            return children;
        }

        //Mutacija treba da spreci da jedinke iz populacije postanu suvise slicne i da pomogne u obnavljanju genetskog materijala
        //ukoliko u jednoj generaciji sve jedinke imaju istu vrednost jednog gena, onda taj gen samo ukrstanjem nikada ne bi mogao da se promeni
        //omogucavaju razmatranje novih delova prostrora pretrage u nadi da ce se naici na globalni ekstremum
        //Ukoliko je verovatnoca mutacije velika, onda usmeravanje pretrage postaje preslabo i ona pocinje da lici na slucajnu pretragu

        public static SpaceShip Mutate(SpaceShip ship)
        {
            Random r = new Random();
            double randomValue = r.NextDouble();
            if (randomValue < MutationRate)
            {
                int randomIndex = r.Next(SpaceShip.ChromosomeSize);
                int actionR = r.Next(4);
                if (actionR == 0)
                {
                    Tuple<string, int> newRun = new Tuple<string, int>("NT", r.Next(100, 400));
                    ship.run[randomIndex] = newRun;
                }
                if (actionR == 1)
                {
                    Tuple<string, int> newRun = new Tuple<string, int>("RL", r.Next(10, 25));
                    ship.run[randomIndex] = newRun;
                }

                if (actionR == 2)
                {
                    Tuple<string, int> newRun = new Tuple<string, int>("RR", r.Next(10, 25));
                    ship.run[randomIndex] = newRun;
                }
                if (actionR == 3)
                {
                    Tuple<string, int> newRun = new Tuple<string, int>("TH", r.Next(10, 25));
                    ship.run[randomIndex] = newRun;
                }
            }
            return ship;
        }

        //kreiranje nove generacije od selektovanih jedinki
        public static List<SpaceShip> CreateGeneration(List<SpaceShip> selectedPopulation)
        {
            int generationSize = 0;
            List<SpaceShip> newGeneration = new List<SpaceShip>();
            Random r = new Random();
            while (generationSize < GenerationSize)
            {

                List<SpaceShip> parents = selectedPopulation.OrderBy(x => r.Next()).Take(2).ToList();
                SpaceShip p1 = parents[0]; SpaceShip p2 = parents[1];


                //dobijaju se deca ukrstanjem
                List<SpaceShip> children = Crossover(p1, p2);
                SpaceShip c1 = children[0]; SpaceShip c2 = children[1];

                //deca mutiraju
                c1 = Mutate(c1); c2 = Mutate(c2);

                //dodaju se u generaciju
                newGeneration.Add(c1); newGeneration.Add(c2);
                generationSize += 2;
            }
            return newGeneration;
        }

        public static List<SpaceShip> CreateInitialGeneration()
        {
            List<SpaceShip> initialPopulation = new List<SpaceShip>();
            Random r = new Random();
            while (initialPopulation.Count < GeneticAlgorithm.GenerationSize)
            {
                List<Tuple<string, int>> currRun = new List<Tuple<string, int>>();
                for (int j = 0; j < 20; j++)
                {
                    int actionR = r.Next(4);
                    if (actionR == 0)
                        currRun.Add(new Tuple<string, int>("NT", r.Next(100, 400)));
                    if (actionR == 1)
                        currRun.Add(new Tuple<string, int>("RL", r.Next(10, 25)));
                    if (actionR == 2)
                        currRun.Add(new Tuple<string, int>("RR", r.Next(10, 25)));
                    if (actionR == 3)
                        currRun.Add(new Tuple<string, int>("TH", r.Next(10, 25)));
                }
                initialPopulation.Add(new SpaceShip(currRun));
            }

            return initialPopulation;
        }

    }
}
