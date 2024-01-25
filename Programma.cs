//2.3 forward checking part 2   

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

class Programma
{
    static void Main3()
    {
        int[,] sudoku = new int[9, 9];

        //We maken een array van lijsten voor elk blokje. Hierin wordt bijgehouden welke waarden uitgesloten worden,
        //zodat deze terug in het domein gezet kunnen worden als het domein leeg raakt
        //en er dus een waardetoekenning in een eerder blokje veranderd moet worden.
        List<int>[,] uitgesloten = new List<int>[9, 9];

        List<int>[,] domeinen = new List<int>[9, 9];

        List<Point> punten = new List<Point>();                                                                 //Dit is een lijst met punten die bijhoudt in welke volgorde je de sudoku vakjes aan het invullen bent. Dit is belangrijk omdat we bij de most-constrained-variable (MCV) heuristiek niet meer van links boven naar
                                                                                                                //rechts onder werken, maar kriskras door de sudoku wandelen. Als bij een random vakje geconcludeert wordt dat je terug moet naar het vorige vakje, moet wel bijgehouden zijn wat het vorige vakje was.

        //Alles gebeurt in de functie import()
        import();

        void import()
        {
            int sudoku_nr = 0;
            string[] cijfers;
            string regel;
            char[] separators = { ' ' };

            StreamReader sr = new StreamReader("../../../TextFile1.txt");

            while ((regel = sr.ReadLine()) != null)
            {
                cijfers = regel.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (cijfers.Length == 81)
                {
                    //start de timer
                    sudoku_nr++;
                    double totalTime = 0;
                    var timer = System.Diagnostics.Stopwatch.StartNew();

                    vul_sudoku(cijfers);
                    maak_domeinen();

                    printSudoku(sudoku);
                    Console.WriteLine();

                    bool sudoku_niet_opgelost = true;
                    while (sudoku_niet_opgelost)                                                                //Totdat de sudoku opgelost is moeten we forward checking blijven gebruiken totdat alle vakjes correct ingevuld zijn in de sudoku
                    {
                        Point coordinaten = vind_vakje();                                                       //Voordat vakjes van de sudoku ingevuld kunnen worden, moeten we eerst achterhalen in welke volgorde we ze moeten invullen. In dit deel
                                                                                                                //van de opdracht moest dat gedaan worden volgens de most-constrained-variable (MCV) heuristiek. Hier is de functie vind_vakje verantwoordelijk voor
                        if (coordinaten.X != -1)                                                                //Vind_vakje returnt een Punt met een X en Y coordinaat. Zolang de coordinaten niet -1 zijn, weten we dat de sudoku nog niet opgelost is,
                                                                                                                //en moeten we waarden gaan invullen voor het gevonde vakje. In de functie vind_vakje zal duidelijk worden waarom -1 betekent dat de sudoku opelost is.
                        {
                            punten.Add(coordinaten);                                                            //Het vakje dat ingevuld gaat worden wordt toegevoegd aan de lijst, zodat we bijhouden in welke volgorde we welke punten bijlangs gaan
                            forward_checking(coordinaten.X, coordinaten.Y);                                     //Forward checking wordt gebruikt om het vakje waar we zijn aangebroken in te vullen
                        }
                        if (coordinaten.X == -1)                                                                //Als de coordinaten -1 zijn weten we dat de sudoku opgelost is en moeten we de while loop uit zodat de sudoku geprint kan worden
                            sudoku_niet_opgelost = false;
                    }

                    //Visualisatie
                    timer.Stop();
                    totalTime += Math.Round(timer.Elapsed.TotalMilliseconds);

                    //Nadat de hele sudoku correct is ingevuld, wordt de sudoku geprint
                    printSudoku(sudoku);
                    Console.WriteLine("Sudoku nummer " + sudoku_nr + ". Gevonden in " + totalTime + "ms");
                    Console.WriteLine();

                    punten.Clear();
                }
            }
        }

        //De sudoku wordt ingevuld met de vastgestelde waarden, de rest krijgt waarde 0
        void vul_sudoku(string[] cijfers)
        {
            int index = 0;
            for (int j = 0; j < 9; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    sudoku[i, j] = int.Parse(cijfers[index]);
                    domeinen[i, j] = new List<int>();                   //De domein-lijsten worden geïnitieerd
                    uitgesloten[i, j] = new List<int>();                //De lijsten die bijhouden welke waarden worden uitgesloten worden geïnitieerd
                    index++;
                }
            }
        }

        void maak_domeinen()
        {
            for (int j = 0; j < 9; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (sudoku[i, j] == 0)
                    {
                        for (int k = 1; k < 10; k++)
                        {
                            domeinen[i, j].Add(k);
                        }

                        //Alle domeinen worden knoopconsistent gemaakt
                        knoopconsistent(i, j);
                    }
                    else
                    {
                        //Domeinen van vakjes met vooraf vastgestelde waarden krijgen waarde -1 om ze te herkennen
                        domeinen[i, j].Add(-1);
                    }
                }
            }
        }

        void knoopconsistent(int i, int j)
        {
            //Met onderstaande variabelen herkennen we in welk blok het vakje zig bevindt
            int blok_x = (int)Math.Floor(Convert.ToDouble(i) / 3);
            int blok_y = (int)Math.Floor(Convert.ToDouble(j) / 3);

            //Alle waarden die al in het 3 bij 3 blok van het bekeken vakje zitten worden uit zijn domein gehaald
            for (int l = 0; l < 3; l++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int punt_x = blok_x * 3 + k;
                    int punt_y = blok_y * 3 + l;
                    int waarde = sudoku[punt_x, punt_y];

                    if (domeinen[i, j].Contains(waarde))
                    {
                        domeinen[i, j].Remove(waarde);
                    }
                }
            }

            //Hetzelfde geldt voor alle waarden in de rij en kolom van het vakje
            for (int k = 0; k < 9; k++)
            {
                int waarde_1 = sudoku[i, k];
                int waarde_2 = sudoku[k, j];
                if (domeinen[i, j].Contains(waarde_1))
                {
                    domeinen[i, j].Remove(waarde_1);
                }
                if (domeinen[i, j].Contains(waarde_2))
                {
                    domeinen[i, j].Remove(waarde_2);
                }
            }
        }

        //Deze functie is verantwoordelijk om te vinden welk volgende vakje we moeten invullen. Dit wordt bepaald volgens de most-constrained-variable (MCV) heuristiek. We doen dit
        //door de domeinen te checken van elk vakje en het vakje te kiezen met het kleinste domein (minste opties van mohelijke cijfers die ingevuld kunnen worden in een vakje).
        Point vind_vakje()
        {
            int domein_teller = 10;                                                                             //We moeten een startpunt hebben om de domeinen te vergelijken. We hebben 10 gekozen omdat de domeinen altijd van grootte 9 of kleiner zijn, dus er wordt altijd een vakje gekozen.
            int a = 0;
            int b = 0;
            int aantal_ingevuld = 0;                                                                            //Aantal ingevuld houdt bij hoeveel vakjes al correct ingevuld zijn. Op het moment dat deze counter 81 wordt, weten we dat de sudoku ophelost is. 

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (domein_teller > domeinen[x, y].Count && domeinen[x, y][0] != -1 && sudoku[x, y] == 0)   //Hier wordt gekozen welk vakje als volgende bezocht moet worden. Restrictie 1: alleen een vakje kiezen die een kleiner domein heeft dan het vorige vakje met het kleinste domein ((MCV) heuristiek).
                                                                                                                //Restructie 2: Alleen naar vakjes kijken die veranderdt mogen worden (vakjes met domeinen[x, y][0] == -1 zijn vakjes waar je niet aan mag zitten omdat die gegeven waarden hadden vanaf het begin).
                                                                                                                //Restrictie 3: Alleen vakjes kiezen die nog geen waarde hebben gekregen oftewel sudoku[x, y] == 0.
                    {
                        a = x;                                                                                  //Nieuwe coordinaten opslaan
                        b = y;
                        domein_teller = domeinen[x, y].Count;                                                   //Het kleinste domein tot nu toe updaten
                    }
                    if (sudoku[x, y] != 0)                                                                      //Als een vakje ingevuld is, tell dan een op bij aantal_ingevuld, zodat we weten wanneer de sudoku compleet ingevuld is
                    {
                        aantal_ingevuld++;
                    }
                    if (aantal_ingevuld == 81)                                                                  //Als de gehele sudoku ingevuld is, return -1,-1 als coordinaten
                    {
                        a = -1;
                        b = -1;
                    }
                }
            }
            return new Point(a, b);
        }

        //In deze functie krijgt een aangewezen vakje (bepaard door de coordinaten i en j die als argument meegegeven zijn aan de functie) een waarde door middel van een Forward Checking algoritme
        void forward_checking(int i, int j)
        {
            //Hiermee houden we bij of het invullen van een specifieke waarde er voor zorgt dat een domein van een ander nog oningevuld vakje leeg maakt (dit heet bij ons een illegale move)
            bool illegale_move = false;
            int waarde;

            //Een int die bijhoudt of de waarde die ingevuld is in een vakje niet leidt tot een illegale move. Zolang een vakje niet correct ingevuld is, blijft de int 0. Als de ingevulde waarde
            //geen ander domein 0 maakt, is het een legale move en wordt de int -1
            int correct = 0;  
            
            while (correct != -1)                                                   //Zolang het geen legale move is moeten we door proberen totdat we het vakje correct ingevuld hebben
            {
                //Zolang we niet te maken hebben met een vakje met een leeg domein blijven we het vakje invullen en domeinen updaten
                if (domeinen[i, j].Count > 0)
                {
                    if (domeinen[i, j][0] != -1)                                    //Alleen aan vakjes zitten die niet vaststaan vanaf het begin. In princiepe hoeft deze restrictie er niet in te staan, want hier wordt al op gefiltert in vind_vakje. 
                    {
                        illegale_move = false;

                        //De waarde die wordt toegekend is het laagste getal in het domein
                        waarde = domeinen[i, j][0];
                        sudoku[i, j] = waarde;

                        double blok_x = Math.Floor((double)i / 3);
                        double blok_y = Math.Floor((double)j / 3);

                        //Voor de andere vakjes in het blok van het vakje wordt de waarde uit het domein gehaald
                        for (int l = ((int)blok_y * 3); l < (3 + blok_y * 3); l++)
                        {
                            for (int k = ((int)blok_x * 3); k < (3 + blok_x * 3); k++)
                            {
                                //Onderstaande restrictie zorgt ervoor dat alleen domeinen van nog lege vakjes ge-update worden. Als het vakje in de punten lijst staan weten we dat we er al een keer langsgegaan zijn. 
                                if (!punten.Contains(new Point(k, l)))
                                {
                                    if (domeinen[k, l].Contains(waarde) && (l != j || k != i))
                                    {
                                        domeinen[k, l].Remove(waarde);

                                        //Als een domein door het verwijderen van de waarde leeg raakt hebben we te maken met een illegale move
                                        if (domeinen[k, l].Count == 0)
                                        {
                                            illegale_move = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (illegale_move)
                                break;
                        }

                        //Hetzelfde geldt voor de vakjes in dezelfde rij en colom.
                        //Om tijd te besparen doen we dit alleen als we niet al weten dat het een illegale move is
                        if (illegale_move == false)
                        {
                            for (int k = 0; k < 9; k++)
                            {
                                if (!punten.Contains(new Point(i, k)))
                                {
                                    if (domeinen[i, k].Contains(waarde) && k != j)
                                    {
                                        domeinen[i, k].Remove(waarde);
                                        if (domeinen[i, k].Count == 0)
                                        {
                                            illegale_move = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (illegale_move == false)
                        {
                            for (int k = 0; k < 9; k++)
                            {
                                if (!punten.Contains(new Point(k, j)))
                                {
                                    if (domeinen[k, j].Contains(waarde) && k != i)
                                    {
                                        domeinen[k, j].Remove(waarde);
                                        if (domeinen[k, j].Count == 0)
                                        {
                                            illegale_move = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (illegale_move == false)                         //Als we alle domeinen hebben gecheckt die beinvloed worden door de ingevulde waarde en er geen lege domeinen ontstaan is het een legale move en kunnen we de while loop uit
                        {
                            correct = -1;
                        }

                        //Als de move illegaal bleek te zijn: 
                        if (illegale_move == true)
                        {
                            sudoku[i, j] = 0;                   //De waarde toekenning wordt ongedaan gemaakt
                            terugzetten(i, j, waarde);          //De waarde wordt weer teruggezet in de domeinen van alle vakjes waaruit het verwijderd was door deze move
                            domeinen[i, j].Remove(waarde);      //De waarde wordt uit het domein van dit vakje verwijderd
                            uitgesloten[i, j].Add(waarde);      //De waarde wordt toegevoegd aan de lijst met uitgesloten waarden van dit vakje
                        }
                    }
                }

                //Als blijkt dat het domein van het vakje waar we mee bezig zijn leeg raakt (en er dus iets moet veranderen in de al ingevulde vakjes) gebeurt het volgende:
                else
                {
                    //Alle uitgesloten waarden worden teruggezet in het domein (aangezien we iets ervoor gaan veranderen en er dan opnieuw naar gaan kijken)
                    domeinen[i, j].AddRange(uitgesloten[i, j]);
                    uitgesloten[i, j].Clear();

                    //Omdat alle mogelijke waarden van een vakje uitgeprobeert zijn, maar geen enkele correct is, moeten we terug naar het vakje waar we waren voor het huidige vakje.
                    //Dit doen we door het huidige vakje te verwijderen uit de lijst en de coordinaten aan te passen, door het laatste punt van de lijst te gebruiken. 
                    //Het verwijderen van het huidige vakje uit de lijst kan omdat we de MCV heuristiek dynamisch toepassen. Op het moment dat een vakje correct ingevuld is, hoeven we niet meer te weten welk vakje erna kwam,
                    //omdat we opnieuw het vakje met het laagste domein moeten zoeken. 
                    if (punten.Count > 0)
                    {
                        punten.RemoveAt(punten.Count - 1);
                    }

                    if (punten.Count > 0)
                    {
                        i = punten[punten.Count - 1].X;
                        j = punten[punten.Count - 1].Y;
                    }

                    //Nu updaten we het domein van dit al ingevulde vakje waarvan we er dus achter gekomen zijn dat de ingevulde waarde niet kan. We doen hetzelfde als bij een illegale move
                    waarde = sudoku[i, j];
                    sudoku[i, j] = 0;
                    terugzetten(i, j, waarde);
                    domeinen[i, j].Remove(waarde);
                    uitgesloten[i, j].Add(waarde);
                }

            }
        }


        void terugzetten(int i, int j, int waarde)
        {
            double blok_x = Math.Floor((double)i / 3);
            double blok_y = Math.Floor((double)j / 3);

            //Voor de andere vakjes in het blok van het vakje wordt de waarde uit het domein gehaald
            for (int l = ((int)blok_y * 3); l < (3 + blok_y * 3); l++)
            {
                for (int k = ((int)blok_x * 3); k < (3 + blok_x * 3); k++)
                {
                    if (!punten.Contains(new Point(k, l)))
                    {
                        if (domeinen[k, l].Contains(waarde) == false && (l != j || k != i))
                        {
                            domeinen[k, l].Add(waarde);
                            knoopconsistent(k, l);
                            domeinen[k, l].Sort();

                        }
                    }
                }
            }

            for (int k = 0; k < 9; k++)
            {
                if (!punten.Contains(new Point(i, k)))
                {
                    if (domeinen[i, k].Contains(waarde) == false && k != j)
                    {
                        domeinen[i, k].Add(waarde);
                        knoopconsistent(i, k);
                        domeinen[i, k].Sort();

                    }
                }
            }
            for (int k = 0; k < 9; ++k)
            {
                if (!punten.Contains(new Point(k, j)))
                {
                    if (domeinen[k, j].Contains(waarde) == false && k != i)
                    {
                        domeinen[k, j].Add(waarde);
                        knoopconsistent(k, j);
                        domeinen[k, j].Sort();
                    }
                }
            }
        }

        void printSudoku(int[,] sudoku)
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Console.Write(sudoku[x, y] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}


