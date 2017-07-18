using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
* Auto-generated code below aims at helping you parse
* the standard input according to the problem statement.
**/
public class Player
{
    public static void Main(string[] args)
    {
        GameMap myGameMap = new GameMap();
        //int iddd = 0;
        string[] inputs;
        int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
        Console.Error.WriteLine(factoryCount);
        int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
        Console.Error.WriteLine(linkCount);
        for (int i = 0; i < linkCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int factory1 = int.Parse(inputs[0]);
            int factory2 = int.Parse(inputs[1]);
            int distance = int.Parse(inputs[2]);

            Console.Error.WriteLine(string.Join(" ", inputs));
            myGameMap.AddFactoryInformation(factory1, factory2, distance);
        }

        myGameMap.Init();

        // game loop
        while (true)
        {
            myGameMap.AddTurn();
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
            Console.Error.WriteLine(entityCount);
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int arg1 = int.Parse(inputs[2]);
                int arg2 = int.Parse(inputs[3]);
                int arg3 = int.Parse(inputs[4]);
                int arg4 = int.Parse(inputs[5]);
                int arg5 = int.Parse(inputs[6]);

                Console.Error.WriteLine(string.Join(" ", inputs));
                switch (entityType)
                {
                    case "FACTORY":
                        myGameMap.UpdateFactoryInformation(entityId, arg1, arg2, arg3, arg4, arg5);
                        break;
                    case "TROOP":
                        myGameMap.AddTroopInformation(arg1, arg2, arg3, arg4, arg5);
                        break;
                    case "BOMB":
                        break;
                }
            }

            string instruction = myGameMap.GenerateNextInstruction();

            Console.WriteLine(instruction);
        }
    }

    public class GameMap
    {
        public Dictionary<int, Factory> factories;

        public int Turn { get; set; }

        public int BombQuantity { get; set; }

        public List<Bomb> bombsTracker;

        public Factory FactoryBoombed = null;

        public GameMap()
        {
            factories = new Dictionary<int, Factory>();
            Turn = 0;
            BombQuantity = 2;
            bombsTracker = new List<Bomb>();
        }

        public void AddTurn()
        {
            this.Turn++;
            foreach (Bomb bomb in bombsTracker)
            {
                if (bomb.Distance > 1)
                {
                    bomb.Distance--;
                }
                else
                {
                    bomb.bombEffectTurns--;
                }
            }
            this.CleanGameInformation();
        }

        public void AddFactoryInformation(int factoryId1, int factoryId2, int distance)
        {
            Factory factory1 = null;
            if (this.factories.ContainsKey(factoryId1))
            {
                factory1 = this.factories[factoryId1];
            }
            else
            {
                factory1 = new Factory(factoryId1);
                this.factories[factoryId1] = factory1;
            }

            Factory factory2 = null;
            if (this.factories.ContainsKey(factoryId2))
            {
                factory2 = this.factories[factoryId2];
            }
            else
            {
                factory2 = new Factory(factoryId2);
                this.factories[factoryId2] = factory2;
            }

            Link.CreateLinkInformation(factory1, factory2, distance);
        }

        public void UpdateFactoryInformation(int factoryId, int possession, int cyborgs, int productivity, int arg4, int arg5)
        {
            Factory myFactory = this.factories[factoryId];

            myFactory.Update(possession, cyborgs, productivity);
            //Console.Error.WriteLine(myFactory.ToString());
        }

        public void AddTroopInformation(int possession, int fromFactoryId, int toFactoryId, int cyborgs, int distance)
        {
            Factory myFactory = this.factories[toFactoryId];

            myFactory.AddTroop(possession, fromFactoryId, cyborgs, distance);
            //Console.Error.WriteLine(myFactory.ToString());
        }

        public void AddBombInformation(int possession, int fromFactoryId, int toFactoryId, int distance)
        {
            if (toFactoryId != -1)
            {
                Factory myFactory = this.factories[toFactoryId];

                myFactory.AddBomb(possession, fromFactoryId, distance);
            }
            //Console.Error.WriteLine(myFactory.ToString());
        }

        public void Init()
        {
            foreach (Factory factory in this.factories.Values)
            {
                foreach (Link link in factory.Links)
                {
                    Factory otherFactory = link.GetOtherFactory(factory);

                    var results = findBestWay(factory, otherFactory);
                    //factory.GetLinkForOtherFactory();
                    //pouetBestFactoryWayToFactory1

                    if (factory.FactoryId == 3 && otherFactory.FactoryId == 4)
                    {
                        int j = 0;
                    }

                    int distance = 0;
                    int i = 0;
                    var oldFactory = this.factories[results[i++]];
                    do
                    {
                        var nextFactory = this.factories[results[i++]];
                        distance += oldFactory.GetLinkForOtherFactory(nextFactory).Distance;
                        oldFactory = nextFactory;
                    } while (i < results.Count);

                    link.BestFactoryWayToFactory1 = results.ElementAt(1);
                    link.BestFactoryWayToFactory2 = results.ElementAt(results.Count - 2);
                    //Console.Error.WriteLine(link);
                }
            }
        }

        public void CleanGameInformation()
        {
            foreach (Factory factory in this.factories.Values)
            {
                factory.CleanWarEvents();
            }

            bombsTracker = bombsTracker.Where(bomb => bomb.bombEffectTurns != 0).ToList();
        }

        public string GenerateNextInstruction()
        {
            var myFactories = factories.Values.Where(factory => factory.Possession == 1 && factory.Cyborgs > 1).OrderByDescending(factory => factory.Cyborgs);
            //var otherFactories = myFactories.SelectMany(factory => factory.Links).Select(link => link.GetOtherFactory(factory)).Where(otherFactory => otherFactory.Possession != 1));

            int waitingCyborgs = myFactories.Sum(factory => factory.Cyborgs); //pourquoi - nb Factories

            var selected = new List<Tuple<Factory, Factory, double, Tuple<int, int>, int>>();

            foreach (Factory factory in myFactories)
            {
                foreach (Link link in factory.Links)
                {
                    Factory otherFactory = link.GetOtherFactory(factory);
                    var futurFactory = otherFactory.CalculatePossession();
                    double bonus = 1;
                    if (otherFactory.Possession != 1) bonus = 1;
                    if (futurFactory.Item1 != 1)
                    {
                        double attractivity = (bonus) * (otherFactory.Productivity * otherFactory.Productivity) / (1 + futurFactory.Item2 * link.Distance / 4);

                        selected.Add(new Tuple<Factory, Factory, double, Tuple<int, int>, int>(factory, otherFactory, attractivity, futurFactory, link.Distance));
                        Console.Error.WriteLine(selected.LastOrDefault());
                    }
                }
            }

            var attacks = selected.Where(t => t.Item3 > 0);
            //attacks = selected.;
            //var attacks = selected.OrderByDescending(c => c.Item3).ThenByDescending(c => c.Item1.Cyborgs);

            List<string> instructions = new List<string>();
            var myBoomers = factories.Values.Where(factory => factory.Possession == 1);
            var myTargets = factories.Values.Where(factory => factory.Possession == -1 && factory.Productivity > 0 && factory.Cyborgs > 0).OrderByDescending(factory => factory.Productivity * factory.Cyborgs);
            var ennemyFactory = myTargets.FirstOrDefault();
            var myFactory = myBoomers.FirstOrDefault();
            if (ennemyFactory != null)//ennemyFactory
            {
                foreach (Factory atarget in myTargets)
                {
                    var conquest = ennemyFactory.CalculatePossession();

                    if (FactoryBoombed != ennemyFactory && conquest.Item1 * conquest.Item2 < 0)
                    {
                        //rechercher
                        int distRef = myFactory.GetLinkForOtherFactory(ennemyFactory).Distance;
                        foreach (Factory aFactory in myBoomers)
                        {
                            if (distRef > aFactory.GetLinkForOtherFactory(ennemyFactory).Distance)
                            {
                                distRef = aFactory.GetLinkForOtherFactory(ennemyFactory).Distance;
                                myFactory = aFactory;
                            }
                        }
                        instructions.Add("BOMB " + myFactory.FactoryId + " " + atarget.FactoryId);
                        this.BombQuantity--;
                        FactoryBoombed = ennemyFactory;
                    }
                }
            }
            if (attacks != null)
            {
                List<Tuple<int, int, int>> primalOrders = new List<Tuple<int, int, int>>();
                foreach (var attack in attacks)
                {
                    if (attack.Item1.Cyborgs > 1)
                    {
                        int compensateProductivity = 0;
                        if (attack.Item2.Possession != 0)
                        {
                            compensateProductivity = attack.Item5 + 1;
                        }

                        if (attack.Item2.Productivity > 0)
                        {
                            compensateProductivity += attack.Item2.Productivity * 2;
                        }
                        Link tmpLink = attack.Item1.GetLinkForOtherFactory(attack.Item2);
                        int idToattack = attack.Item1.GetLinkForOtherFactory(attack.Item2).GetBestFactoryWay(attack.Item1);
                        double nbItemTofight = Math.Min((attack.Item4.Item2 + 1 + compensateProductivity), (attack.Item1.Cyborgs)) * 1.5;
                        primalOrders.Add(new Tuple<int, int, int>(attack.Item1.FactoryId, idToattack, (int)nbItemTofight));
                        //instructions.Add("MOVE " + attack.Item1.FactoryId + " " + idToattack + " " + Math.Min((attack.Item4.Item2 + 1 + compensateProductivity), (attack.Item1.Cyborgs - 1)));
                        //}
                    }
                }

                var finalOrders = primalOrders.GroupBy(t => t.Item1 + " " + t.Item2)
                    .Select(g => g.Aggregate((a, b) => new Tuple<int, int, int>(a.Item1, a.Item2, Math.Max(a.Item3, b.Item3)))).OrderBy(t => t.Item3);

                //                    .Aggregate((a.Item3, b.Item3) => a.Item3);
                //finalOrder = primalOrders.GroupBy(t => new Tuple<int, int, int>(t.Item1, t.Item2, 0)); // .Aggregate((a, b) => new Tuple<int, int, int>(a.Item1, a.Item2, a.Item3 + b.Item3)); //.GroupBy(t => new Tuple<int, int>(t.Item1, t.Item2));
                //.Select(y => y.Item3).Aggregate((a, b) => a + b);

                //var finalOrder = primalOrders.GroupBy(y => new Tuple<int, int>(y.Item1, y.Item2)).Where(g => g.Count() > 1).Select(g => Tuple.Create(g.Item1,y.Item2,y.Item3).Aggregate((a, b) => a + b);
                primalOrders.OrderBy(c => c.Item1 + " " + c.Item2);
                //Tuple<int, int, int> finalOrder = new Tuple<int, int, int>(1,1, 0);
                foreach (var finalOrder in finalOrders)
                {
                    instructions.Add("MOVE " + finalOrder.Item1 + " " + finalOrder.Item2 + " " + finalOrder.Item3);
                    //finalOrder.
                }
                //instructions.Add(order);

            }

            var myBestFactories = factories.Values.Where(factory => factory.Possession == 1 && factory.Cyborgs > 20 && factory.Productivity < 3);

            foreach (Factory bestFactory in myBestFactories)
            {
                instructions.Add("INC " + bestFactory.FactoryId);
            }

            if (instructions.Count == 0)
            {
                instructions.Add("WAIT");
            }

            return string.Join(";", instructions);
        }

        private List<int> findBestWay(Factory from, Factory to)
        {
            List<int> partOne = new List<int>();
            List<int> partTwo = new List<int>();
            List<int> list = new List<int>();
            Factory pivot = null;
            int distRef = from.GetLinkForOtherFactory(to).Distance;
            foreach (Factory actualFactory in this.factories.Values)
            {
                if (actualFactory.FactoryId != from.FactoryId && actualFactory.FactoryId != to.FactoryId && (from.GetLinkForOtherFactory(actualFactory).Distance + to.GetLinkForOtherFactory(actualFactory).Distance) < distRef)
                {
                    pivot = actualFactory;
                    partOne = findBestWay(from, pivot);
                    partTwo = findBestWay(pivot, to);
                }
            }
            if (partOne.Count() == 0)
            {
                partOne.Add(from.FactoryId);
                if (pivot != null) partOne.Add(pivot.FactoryId);
            }
            if (partTwo.Count() == 0)
            {
                partOne.Add(to.FactoryId);
            }
            else
            {
                partTwo.RemoveAt(0);
                partOne.AddRange(partTwo);
            }
            return partOne;
        }
    }
    public class Precogs
    {
        List<int> Turns = null;
        List<int> Troops = null;
        Factory MyFac { get; set; }
        public int HowMany(int aTurn)
        {
            int increment = 0, turnBefore = 0, nbCyborgs = 0;
            foreach (int turn in Turns)
            {
                if (turn <= aTurn)
                {
                    increment = turn - turnBefore;
                    nbCyborgs += increment;
                    turnBefore = turn;
                }
                else break;
            }
            return nbCyborgs;
        }
        public int HowMany()
        {
            int increment = 0, turnBefore = 0, nbCyborgs = 0;
            foreach (int turn in Turns)
            {
                increment = turn - turnBefore;
                nbCyborgs += increment;
                turnBefore = turn;

            }
            return nbCyborgs;
        }
    }
    public class Factory
    {
        public int FactoryId { get; private set; }
        //blic Precogs precog;
        public int Possession { get; set; }
        public int Cyborgs { get; set; }
        public int Productivity { get; private set; }
        public int Boomed { get; private set; }
        public List<Link> Links { get; private set; }
        public List<WarEvent> WarEvents { get; private set; }

        public Factory(int factoryId)
        {
            this.FactoryId = factoryId;
            this.Cyborgs = 0;
            this.Productivity = 0;
            this.Possession = 0;
            this.Boomed = 0;
            this.Links = new List<Link>();
            this.WarEvents = new List<WarEvent>();
        }

        public void CleanWarEvents()
        {
            this.WarEvents = new List<WarEvent>();
        }

        public Link GetLinkForOtherFactory(Factory otherFactory)
        {
            foreach (Link link in Links)
            {
                Factory aFactory = link.GetOtherFactory(this);
                if (aFactory.FactoryId == otherFactory.FactoryId)
                {
                    return link;
                }
            }
            return null;
        }
        // renvoyer une liste en lieux et place => tour d'arrivé, futurPossession, futurCyborg... 
        //ajouter la production de cyborg par tour peux-être plus facile ensuite à manipuler.=> attention donc si l'on modifi des ordres de conquête ou de défense
        public List<Tuple<int, int, int>> CalculatePossessionbyTurn()
        {
            int futurPossession = this.Possession;
            int futurCyborgs = this.Cyborgs;
            int turnToAdd = this.Boomed;
            int oldDistance = 0;
            int returnDist = 0;
            int returnPossession = 0;
            int returnCyborgs = 0;
            List<Tuple<int, int, int>> turns = new List<Tuple<int, int, int>>();
            foreach (WarEvent warEvent in this.WarEvents.OrderBy(t => t.Distance))
            {
                turnToAdd = warEvent.Distance - turnToAdd;
                futurCyborgs += this.Productivity * Math.Max(turnToAdd, 0);
                if (warEvent.Possession == futurPossession)
                {
                    futurCyborgs = warEvent.CalculateReinforcement(futurCyborgs);
                }
                else
                {
                    futurCyborgs = warEvent.CalculateDestruction(futurCyborgs);
                    if (futurCyborgs < 0)
                    {
                        futurPossession = warEvent.Possession;
                        futurCyborgs = -futurCyborgs;
                    }
                }
                if (oldDistance == warEvent.Distance || returnDist==0)
                {
                    returnDist += warEvent.Distance;
                    returnPossession = futurPossession;
                    returnCyborgs = futurCyborgs;
                }
                turns.Add(new Tuple<int, int, int>(returnDist, returnPossession, returnCyborgs));
            }
            return turns;
        }

        public Tuple<int, int> CalculatePossession()
        {
            int futurPossession = this.Possession;
            int futurCyborgs = this.Cyborgs;
            int turnToAdd = this.Boomed;
            foreach (WarEvent warEvent in this.WarEvents.OrderBy(t => t.Distance))
            {
                turnToAdd = warEvent.Distance - turnToAdd;
                futurCyborgs += this.Productivity * Math.Max(turnToAdd, 0);
                if (warEvent.Possession == futurPossession)
                {
                    futurCyborgs = warEvent.CalculateReinforcement(futurCyborgs);
                }
                else
                {
                    futurCyborgs = warEvent.CalculateDestruction(futurCyborgs);
                    if (futurCyborgs < 0)
                    {
                        futurPossession = warEvent.Possession;
                        futurCyborgs = -futurCyborgs;
                    }
                }
            }

            return new Tuple<int, int>(futurPossession, futurCyborgs);
        }

        public void RegisterLink(Link alink)
        {
            Links.Add(alink);
        }
       
        public void Update(int possession, int cyborgs, int productivity)
        {
            this.Possession = possession;
            this.Cyborgs = cyborgs;
            if (this.Productivity > productivity)
            {
                if (this.Boomed == 0)
                {
                    this.Boomed = 5;
                }
                else
                {
                    this.Boomed--;
                }
            }
            else
            {
                this.Productivity = productivity;
            }
        }

        public void AddTroop(int possession, int fromFactoryId, int cyborgs, int distance)
        {
            this.WarEvents.Add(new Troop(possession, fromFactoryId, cyborgs, distance));
        }

        public void AddBomb(int possession, int fromFactoryId, int distance)
        {
            this.WarEvents.Add(new Bomb(possession, fromFactoryId, distance));
        }

        public override string ToString()
        {
            return "Factory " + this.FactoryId + ": poss=" + Possession + ", prod=" + Productivity + ", count=" + Cyborgs;

        }

    }

    public class Link
    {
        public int Distance { get; private set; }
        public int DistShort { get; private set; }
        Factory factory1;
        Factory factory2;
        public int BestFactoryWayToFactory1 { get; set; }
        public int BestFactoryWayToFactory2 { get; set; }

        private Link(Factory factory1, Factory factory2, int distance)
        {
            this.Distance = distance;
            this.factory1 = factory1;
            this.factory2 = factory2;

        }

        public Factory GetOtherFactory(Factory aFactory)
        {
            if (aFactory.FactoryId == factory1.FactoryId)
            {
                return factory2;
            }
            else
            {
                return factory1;
            }
        }
        public int GetBestFactoryWay(Factory aFactory)
        {
            if (aFactory.FactoryId == factory1.FactoryId)
            {
                return BestFactoryWayToFactory2;
            }
            else
            {
                return BestFactoryWayToFactory1;
            }
        }

        public static void CreateLinkInformation(Factory factory1, Factory factory2, int distance)
        {
            Link newLink = new Link(factory1, factory2, distance);
            factory1.RegisterLink(newLink);
            factory2.RegisterLink(newLink);
        }

        public override string ToString()
        {
            return "Link:  distance=" + this.Distance + ", fact1=" + factory1.FactoryId + ", fact2=" + factory2.FactoryId + ", BestTo1=" + BestFactoryWayToFactory1 + ", BestTo2=" + BestFactoryWayToFactory2;
        }
    }

    public abstract class WarEvent
    {
        public int Possession { get; set; }
        public int FromFactory { get; set; }
        public int Distance { get; set; }
        public bool ToDelete { get; set; }
        public int type { get; set; }


        public WarEvent(int possession, int fromFractory, int distance)
        {
            this.Possession = possession;
            this.FromFactory = fromFractory;
            this.Distance = distance;
        }

        public abstract int CalculateDestruction(int cyborgsInFactory);
        public abstract int CalculateReinforcement(int cyborgsInFactory);
    }

    public class Troop : WarEvent
    {

        public int Cyborgs { get; set; }


        public Troop(int possession, int fromFractory, int cyborgs, int distance)
            : base(possession, fromFractory, distance)
        {
            this.Cyborgs = cyborgs;
            this.ToDelete = true;
            this.type = 0;
        }

        public override int CalculateDestruction(int cyborgsInFactory)
        {
            return cyborgsInFactory - this.Cyborgs;
        }

        public override int CalculateReinforcement(int cyborgsInFactory)
        {
            return cyborgsInFactory + this.Cyborgs;
        }

        public override string ToString()
        {
            return "Troop:  possession=" + this.Possession + ", fromFactory=" + this.FromFactory + ", Cyborgs=" + this.Cyborgs + ", distance=" + this.Distance;
        }
    }



   
    public class Bomb : WarEvent
    {
        public int bombEffectTurns;

        public Bomb(int possession, int fromFractory, int distance)
            : base(possession, fromFractory, distance)
        {
            this.bombEffectTurns = 5;
            this.ToDelete = false;
            this.type = 1;
        }

        public void Tick()
        {
            if (Distance > 1)
            {
                Distance--;
            }
            else
            {
                bombEffectTurns--;

                if (bombEffectTurns == 0)
                {
                    this.ToDelete = true;
                }
            }

        }

        public override int CalculateDestruction(int cyborgsInFactory)
        {
            return Math.Min(10, cyborgsInFactory / 2);
        }

        public override int CalculateReinforcement(int cyborgsInFactory)
        {
            return cyborgsInFactory;
        }

        public override string ToString()
        {
            return "Bomb:  possession=" + this.Possession + ", fromFactory=" + this.FromFactory + ", distance=" + this.Distance;
        }
    }
}


