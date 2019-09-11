using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarLander
{
    public class GeneticAlgorithm
    {
        public static int GenerationSize;
        public static int ReproductionSize;
        public static int MaxIterations;
        public static double MutationRate;
        public static string SelectionMethod;
        public static string CrossoverMethod;
        public static int ElitePopulation;
        public static int TournamentSize;
        public static float MapStartX, MapStartY;

        public static void SetParameters(int generationSize, int reproductionSize, int maxIterations, double mutationRate, string selection, string crossover,float InMapStartX,float InMapStartY, int elite, int tsize)
        {
            GenerationSize = generationSize;
            ReproductionSize = reproductionSize;
            MaxIterations = maxIterations;
            MutationRate = mutationRate;
            SelectionMethod = selection;
            CrossoverMethod = crossover;
            MapStartX = InMapStartX;
            MapStartY = InMapStartY;
            ElitePopulation = elite;
            TournamentSize = tsize;
        }

        //velicina turnira za turnirsku selekciju
        

        public static double GetRandomDouble(double min, double max)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return min + (random.NextDouble() * (max - min));
        }

        public static List<int> GetNRandomIntegers(int count, int maxValue, int minValue = 0)
        {
            HashSet<int> candidates = new HashSet<int>();
            Random r = new Random(Guid.NewGuid().GetHashCode());
            while (candidates.Count < count)
            {
                // May strike a duplicate.
                candidates.Add(r.Next(minValue, maxValue));
            }

            return candidates.ToList();
        }

        //Selekcija ruletska- vece sanse da u reprodukciji ucestvuju jedinke koje su vise prilagodjenje
        //verovatnoca da jedinka i ucestvuje u reprodukciji je f(i)/suma po j f(j)
        public static SpaceShip RouletteSelection(List<SpaceShip> population, List<int> selectedIndex)
        {

            double totalFitness = population.Sum(x => x.GAFitnessFunction);
            

            double currentSum = 0;
            double selectedValue = 0;
            

            while(true) {
                selectedValue = GetRandomDouble(0, totalFitness);
                currentSum = 0;
                for (int i = 0; i < GenerationSize; i++)
                {
                    currentSum += population[i].GAFitnessFunction;
                    //vraca se prva jedinka koja ispuni uslov 
                    if (currentSum > selectedValue)
                    {
                        //i vec postoji
                        if(selectedIndex.IndexOf(i)>=0)
                        {
                            break;
                        }
                        else
                        {
                            selectedIndex.Add(i);
                            return population[i];
                        }
                        
                    }                      
                }
            }
        }


        //pobednik turnira je jedinka sa najboljom prilagodjenoscu
        //sto je veca velicina turnira nekvalitetne jedinke imaju manje sanse da budu izabrane
        public static SpaceShip TournamentSelection(List<SpaceShip> population, List<int> selectedIndex)
        {

            List<int> tournamet;
            while (true)
            {
                tournamet = GetNRandomIntegers(GeneticAlgorithm.TournamentSize, GeneticAlgorithm.GenerationSize);
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
                if (selectedIndex.IndexOf(maxIndex) < 0)
                {
                    selectedIndex.Add(maxIndex);
                    return population[maxIndex];
                }
                    

            }
            

            

        }

        //jednopoziciono ukrstanje sa nasumicnom tackom
        public static List<SpaceShip> CrossoverOnePoint(SpaceShip parent1, SpaceShip parent2)
        {

            //public List<Tuple<string, int>> run
            List<SpaceShip> children = new List<SpaceShip>(2);
            Random r = new Random(Guid.NewGuid().GetHashCode());
            int breakPoint = r.Next(1, SpaceShip.ChromosomeSize - 1);

            List<Tuple<string, int>> child1Run = parent1.run.Take(breakPoint).ToList().Concat(parent2.run.Skip(breakPoint).ToList()).ToList();
            List<Tuple<string, int>> child2Run = parent2.run.Take(breakPoint).ToList().Concat(parent1.run.Skip(breakPoint).ToList()).ToList();
            SpaceShip child1 = new SpaceShip(child1Run,GeneticAlgorithm.MapStartX, GeneticAlgorithm.MapStartY);
            SpaceShip child2 = new SpaceShip(child2Run, GeneticAlgorithm.MapStartX, GeneticAlgorithm.MapStartY);

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
            SpaceShip child1 = new SpaceShip(child1Run, GeneticAlgorithm.MapStartX, GeneticAlgorithm.MapStartY);
            SpaceShip child2 = new SpaceShip(child2Run, GeneticAlgorithm.MapStartX, GeneticAlgorithm.MapStartY);

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
            Random r = new Random(Guid.NewGuid().GetHashCode());
            double randomValue = r.NextDouble();
            if (randomValue < MutationRate)
            {
                int randomIndex = r.Next(SpaceShip.ChromosomeSize);
                //Console.WriteLine(randomIndex);
                int actionR = r.Next(4);
                //Console.WriteLine("akcija" + actionR);
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


        /* def selection(self, chromosomes):
         selected = []

         # Bira se self.reproduction_size hromozoma za reprodukciju
         # Selekcija moze biti ruletska ili turnirska
         for i in range(self.reproduction_size) :
             if self.selection_type == 'roulette':
                 selected.append(self.roulette_selection(chromosomes))
             elif self.selection_type == 'tournament':
                 selected.append(self.tournament_selection(chromosomes))

         # Vracaju se izabrani hromozomi za repodukciju
         return selected*/
        public static List<SpaceShip> Selection(List<SpaceShip> currentPopulation)
        {
            List<SpaceShip> selected = new List<SpaceShip>();
            List<int> selectedIndex = new List<int>();
            for (int i=0; i<ReproductionSize; i++)
            {
                if(SelectionMethod=="roulette")
                {
                    selected.Add(RouletteSelection(currentPopulation, selectedIndex ));
                }
                else if (SelectionMethod == "tournament")
                {
                    selected.Add(TournamentSelection(currentPopulation, selectedIndex));
                }
            }
            return selected;
        }

        public static List<SpaceShip> CreateInitialGeneration()
        {
            List<SpaceShip> initialPopulation = new List<SpaceShip>();
            Random r = new Random(Guid.NewGuid().GetHashCode());
            while (initialPopulation.Count < GeneticAlgorithm.GenerationSize)
            {
                List<Tuple<string, int>> currRun = new List<Tuple<string, int>>();
                for (int j = 0; j < SpaceShip.ChromosomeSize; j++)
                {
                    int actionR = r.Next(3);
                    if (actionR == 0)
                        currRun.Add(new Tuple<string, int>("NT", r.Next(100, 200)));
                    if (actionR == 1)
                    {
                        string rtSt;
                        int rot = r.Next(2);
                        if (rot == 0)
                            rtSt = "RL";
                        else
                            rtSt = "RR";
                        currRun.Add(new Tuple<string, int>(rtSt, r.Next(10, 25)));
                    }
                    if (actionR == 2)
                        currRun.Add(new Tuple<string, int>("TH", r.Next(10, 25)));
                }
                initialPopulation.Add(new SpaceShip(currRun, GeneticAlgorithm.MapStartX, GeneticAlgorithm.MapStartY));
            }

            return initialPopulation;
        }


        //kreiranje nove generacije od selektovanih jedinki
        public static List<SpaceShip> CreateGeneration(List<SpaceShip> selectedPopulation)
        {
            int generationSize = 0;
            List<SpaceShip> newGeneration = new List<SpaceShip>();
            Random r = new Random(Guid.NewGuid().GetHashCode());
            while (generationSize < GenerationSize-ElitePopulation)
            {

                List<SpaceShip> parents = selectedPopulation.OrderBy(x => r.Next()).Take(2).ToList();
                SpaceShip p1 = parents[0]; SpaceShip p2 = parents[1];
                //dobijaju se deca ukrstanjem
                List<SpaceShip> children = new List<SpaceShip>();
                if (CrossoverMethod=="onepoint")
                    children = CrossoverOnePoint(p1, p2);
                else if (CrossoverMethod == "twopoints")
                    children = CrossoverTwoPoints(p1, p2);
                SpaceShip c1 = children[0]; SpaceShip c2 = children[1];

                //deca mutiraju
                c1 = Mutate(c1); c2 = Mutate(c2);

                //dodaju se u generaciju
                newGeneration.Add(c1); newGeneration.Add(c2);
                generationSize += 2;
            }
            return newGeneration;
        }

        
        //
        public static void Optimize()
        {

        }
    }
}
