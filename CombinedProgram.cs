using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI_Practicum1
{
    internal class CombinedProgram
    {
        static bool displayMode = true;
        static bool withoutReductionAlgorithm = false;

        static void Main() {
            Console.WriteLine();
            Console.WriteLine("Press enter to start with CBT..");

            //when starting with "0" as input, the program runs the experiments and does not display the sudokus
            string mode = Console.ReadLine();
            displayMode = mode != "0";
            withoutReductionAlgorithm = false;
            CBT();      //backtracking: Program.cs

            Console.WriteLine();
            Console.WriteLine("Press enter to continue with FC..");
            Console.ReadLine();
            FC();       //forwardchecking: Programme.cs

            Console.WriteLine();
            Console.WriteLine("Press enter to continue with FC_MCV..");
            Console.ReadLine();
            FC_MCV();   //forwardchecking volgens most-constrained-variable (MCV) heuristiek: Programma.cs
        }

        //backtracking
        static void CBT()
        {
            int[,] sudoku = new int[9, 9];                                                  //De sudoku die ingevuld gaat worden
            int[,] statiche_sudoku = new int[9, 9];                                         //Een "statische" sudoku om bij te houden welke cijfers in de sudoku vanaf het begin al vaststaan

            backtracking();

            //De functie die verantwoordelijk is voor chronological backtracking
            void backtracking()
            {
                int index;
                int sudoku_nr = 0;
                string[] woorden;
                char[] separators = { ' ' };
                string regel;
                StreamReader sr = new StreamReader("../../../TextFile1.txt");

                //1 regel representeerd 1 sudoku
                //split de regel en zet het om naar een sudoku bord
                while ((regel = sr.ReadLine()) != null)
                {
                    woorden = regel.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (woorden.Length == 81)
                    {
                        //start de timer
                        sudoku_nr++;
                        double totalTime = 0;
                        var timer = System.Diagnostics.Stopwatch.StartNew();


                        index = 0;
                        for (int y = 0; y < 9; y++)                                         //In deze dubbele for-loop worden de sudokus ingevuld van de textfile
                            for (int x = 0; x < 9; x++)
                            {
                                sudoku[x, y] = int.Parse(woorden[index]);
                                statiche_sudoku[x, y] = int.Parse(woorden[index]);
                                index++;
                            }

                        //print de originele input
                        Console.WriteLine();
                        printSudoku(statiche_sudoku);                                       //hier wordt de oningevulde sudoku geprint
                        Console.WriteLine();



                        for (int y = 0; y < 9; y++)                                         //In deze dubbele for-loop worden alle vakjes een voor een ingevuld door middel van chronological backtracking
                            for (int x = 0; x < 9; x++)
                            {
                                if (statiche_sudoku[x, y] == 0)                             //Alleen vakjes gaan invullen die ingevuld mogen worden
                                {
                                    bool correct = false;
                                    while (correct == false)                                //Alleen doorgaan naar het volgende vakje als het huidige vakje goed ingevuld is. Als dit niet het geval is moeten we een vakje naar achteren (Daar is de functie block_achteruit voor)
                                    {
                                        if (sudoku[x, y] == 0)                              //Als het vakje waar we naar kijken nog nooit bekeken is moeten we van 1 beginnen. Zo niet moeten we beginnen bij de waarde van het vakje +1, omdat de waardes daarvoor al zijn geevalueerd (dit gebeurt bij de else part).
                                        {
                                            int ingevuld_nummer = 1;                                            //Beginnen bij 1
                                            bool nog_niet_correct = true;
                                            correct = invullen(ingevuld_nummer, nog_niet_correct, x, y);        //Als correct true is is er een waarde voor het het huidige vakje die de regels van sudoku niet schendt. Als correct false is is er geen correcte waarde van 1 t/m 9 en moeten we een vakje terug.  
                                            if (invullen(ingevuld_nummer, nog_niet_correct, x, y) == false)     //Hier gaan we doormiddel van de functie block_achteruit een waarde terug als er geen geldige waarde is voor het huidige vakje
                                            {
                                                Point aangepaste_coordinaten = block_achteruit(x, y);           //Een vakje achteruit gaan (het zou kunnen dat je meerdere vakjes achteruit gaat als je te maken hebt met een vakje waar een waarde in staat die niet verandert mag worden)
                                                x = aangepaste_coordinaten.X;                                   //Coordinaten worden aangepasst
                                                y = aangepaste_coordinaten.Y;
                                            }
                                        }
                                        else                                                //Als het huidige vakje niet de waarde 0 heeft hoeven we niet te beginnen vanaf 1, want sommige waarden zijn al uitgeprobeert. Daarom beginnnen we vanaf 1 + de waarde van het vakje.
                                        {
                                            int ingevuld_nummer = 1 + sudoku[x, y];                             //1 + de waarde van het huidige vakje wordt de nieuwe waarde
                                            bool nog_niet_correct = true;
                                            correct = invullen(ingevuld_nummer, nog_niet_correct, x, y);        //vanaf hier, zelfde concept als bij de if loop.
                                            if (invullen(ingevuld_nummer, nog_niet_correct, x, y) == false)
                                            {
                                                Point aangepaste_coordinaten = block_achteruit(x, y);
                                                x = aangepaste_coordinaten.X;
                                                y = aangepaste_coordinaten.Y;
                                            }

                                        }
                                    }
                                }
                            }
                        //Visualisatie
                        timer.Stop();
                        totalTime += Math.Round(timer.Elapsed.TotalMicroseconds);
                        printSudoku(sudoku);                                                //Nadat de hele sudoku correct is ingevuld, wordt de sudoku geprint
                        Console.WriteLine("CBT: Sudoku nummer " + sudoku_nr + ". Gevonden in " + totalTime + " microseconden");
                    }
                }
            }

            //De fucnctie die de sudoku proint in een een simpele form 
            void printSudoku(int[,] sudoku)
            {
                if (displayMode)
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

            //Een functie die ervoor kan zorgen dat je een block "achteruit" gaat in de sudoku.
            //Deze functie returnt de coordinaten van het nieuwe vakje.
            Point block_achteruit(int x, int y)
            {
                bool een_ronde = true;                                          //De while-loop moet gegarandeert een keer uitgvoerd worden, anders gaan we geen vakje achteruit
                if (x != 0 || y != 0)
                {
                    while (statiche_sudoku[x, y] != 0 || een_ronde == true)     //Als we een vakje achteruit zijn gegaan, maar we zijn aangekomen bij een vakje waar we niet aan mogen zitten (statische_sudoku[x,y] != 0), moeten we nog een vakje achteruit totdat statische_sudoku[x,y] == 0
                    {
                        if (x == 0)                                             //Als x = 0 zijn we bij de rand en moeten we een y omhoog en de x moet weer helemaal naar het meest rechter vakje
                        {
                            y = y - 1;
                            x = 8;
                        }
                        else                                                    //Als x != 0 hoeven we alleen een vakje naar links, ofterwel x = x - 1 
                        {
                            x = x - 1;
                        }
                        een_ronde = false;                                      //Ervoor zorgen dat de while-loop niet voor eeuwig doorgaat
                    }
                }
                return new Point(x, y);                                         //De coordinaten van het vakje waar we zijn aangekomen returnen zodat die gebruikt kunnen wornden
            }

            //Een functie die cijfers oplopend probeert in te vullen in een vakje totdat er een cijfer ingevuld is die niet tegen de regels ingaat van sudoku.
            //Als 1 t/m 9 niet mogelijk zijn zonder de regels van een sudoku te schenden returnt deze functie false, anders returnt die true
            bool invullen(int ingevuld_nummer, bool nog_niet_correct, int x, int y)
            {
                while (ingevuld_nummer != 10 && nog_niet_correct)               //Zolang we geen toegestane waarde hebben gevonden blijven we een nieuwe waarde uitproberen door er een bij op te tellen. Als we echter bij 10 aankomen moeten we stoppen, want je mag geen 10 invullen in een sudoku.
                {
                    nog_niet_correct = false;
                    sudoku[x, y] = ingevuld_nummer;                             //Ingevuld_nummer is 1 of 1 + huidige waarde van het vakje. Dit wordt meegegeven als argument voor de functie invullen.
                    for (int i = 0; i < 9; i++)                                 //Kijken of ingevuld_nummer mag in horizontale richting
                    {
                        if (sudoku[i, y] == ingevuld_nummer && x != i)          //Als we in horizontale richting dezelfde waarde tegenkomen (natuurlijk niet als hij zichzelf tegenkomt) is het een illegale move en moeten we de huidige waarde verhogen
                        {
                            ingevuld_nummer++;
                            nog_niet_correct = true;                            //Het vakje is nog niet correct ingevuld dus we moeten nog een ronde in de while loop 
                            i = 10;
                        }
                    }
                    if (nog_niet_correct == false)                              //Als we in horizontale richting geen dubbele waarden tegenkomen kijken we in verticale richting
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            if (sudoku[x, j] == ingevuld_nummer && y != j)      //Als we in verticale richting een illegale move tegenkomen moeten we de huidige waarde met 1 verhogen
                            {
                                ingevuld_nummer++;
                                nog_niet_correct = true;                        //Het vakje is nog niet correct ingevuld dus we moeten nog een ronde in de while loop 
                                j = 10;
                            }
                        }
                    }
                    if (nog_niet_correct == false)                              //Als alles goed gaat in horizontale en verticale richting, moeten we checken of er binnen de 3x3 blokken geen dubbele waarden staan
                    {
                        double n = Math.Floor((double)x / 3);
                        double m = Math.Floor((double)y / 3);
                        for (int a = ((int)n * 3); a < (3 + n * 3); a++)
                            for (int b = ((int)m * 3); b < (3 + m * 3); b++)
                            {
                                if (sudoku[a, b] == ingevuld_nummer && (a != x || b != y))      //Als we binnen een 3x3 block een illegale move tegenkomen, moeten we ingevuld_nummer met 1 verhogen
                                {
                                    ingevuld_nummer++;
                                    nog_niet_correct = true;
                                    a = 10; b = 10;
                                }
                            }
                    }
                }
                if (ingevuld_nummer == 10)                                      //Als geen enkele waarde van 1 t/m 9 mogelijk is, moeten we terugwerken in de sudoku, zodat we hier later terug kunnen komen en wel een correct cijfer kunnen invullen
                {
                    sudoku[x, y] = 0;                                           //Het huidige vakje wordt weer op nul gezet
                    return false;                                               //De functie returnt false om aan te geven dat we een vakje terug moeten en nog niet door kunnen
                }
                return true;                                                    //Als we een correcte waarde hubben gevonden returnt de functie true. Dit geeft aan dat we naar het volgende vakje kunnen. 
            }

        }

        //forwardchecking
        static void FC()
        {
            int[,] sudoku = new int[9, 9];

            //We maken een array van lijsten voor elk blokje. Hierin wordt bijgehouden welke waarden uitgesloten worden,
            // zodat deze terug in het domein gezet kunnen worden als het domein leeg raakt
            // en er dus een waardetoekenning in een eerder blokje veranderd moet worden.
            List<int>[,] uitgesloten = new List<int>[9, 9];

            List<int>[,] domeinen = new List<int>[9, 9];

            var reductionTimer = new System.Diagnostics.Stopwatch();

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
                        var timer = new System.Diagnostics.Stopwatch();
                        timer.Start();

                        //create new reductionAlgorithmTimer
                        reductionTimer.Restart();

                        vul_sudoku(cijfers);

                        maak_domeinen();
                        

                        printSudoku(sudoku);
                        Console.WriteLine();


                        forward_checking();

                        //Visualisatie
                        timer.Stop();
                        double totalTime = Math.Round(timer.Elapsed.TotalMicroseconds);
                        double reductionTime = Math.Round(reductionTimer.Elapsed.TotalMicroseconds);

                        //Nadat de hele sudoku correct is ingevuld, wordt de sudoku geprint
                        printSudoku(sudoku);
                        Console.WriteLine("FC: Sudoku nummer " + sudoku_nr + ". Gevonden in " + totalTime + " microseconden");
                        Console.WriteLine("Reduction time: " + reductionTime + " microseconden");
                        Console.WriteLine();
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
                        domeinen[i, j] = new List<int>();           //De domein-lijsten worden geïnitieerd
                        uitgesloten[i, j] = new List<int>();        //De lijsten die bijhouden welke waarden worden uitgesloten worden geïnitieerd
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
                reductionTimer.Start();
                //Met onderstaande variabelen herkennen we in welk blok het vakje zig bevindt
                int blok_x = (int)Math.Floor(Convert.ToDouble(i) / 3);
                int blok_y = (int)Math.Floor(Convert.ToDouble(j) / 3);

                //Alle waarden die al in het 3 bij 3 blok van het bekeken vakje zitten worden uit zijn domein gehaald.
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
                reductionTimer.Stop();
            }

            //In deze functie krijgen alle vakjes een waarde door middel van een Forward Checking algoritme
            void forward_checking()
            {
                //Met illegale_move houden we bij of het invullen van een specifieke waarde er voor zorgt dat een domein van een ander nog oningevuld vakje leeg maakt.
                // In dat geval is het een illegale move. 
                bool illegale_move = false;
                int waarde;

                for (int j = 0; j < 9; j++)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        //Zolang we niet te maken hebben met vakjes met lege domeinen blijven we ze invullen en domeinen updaten
                        if (domeinen[i, j].Count > 0)
                        {
                            //Voor vakjes die niet al vanaf het begin vast staan doen we het volgende
                            if (domeinen[i, j][0] != -1)
                            {
                                illegale_move = false;

                                //De waarde die wordt toegekend is het laagste getal in het domein
                                waarde = domeinen[i, j][0];
                                sudoku[i, j] = waarde;

                                int blok_x = (int)Math.Floor(Convert.ToDouble(i) / 3);
                                int blok_y = (int)Math.Floor(Convert.ToDouble(j) / 3);

                                //Voor de andere vakjes in het blok van het vakje wordt de waarde uit het domein gehaald
                                for (int l = j % 3; l < 3; l++)
                                {
                                    for (int k = 0; k < 3; k++)
                                    {
                                        //Onderstaande restrictie zorgt ervoor dat alleen domeinen van nog lege vakjes ge-update worden
                                        if ((l * 3 + k) > ((j % 3) * 3 + (i % 3)))
                                        {
                                            int punt_x = blok_x * 3 + k;
                                            int punt_y = blok_y * 3 + l;
                                            if (domeinen[punt_x, punt_y].Contains(waarde))
                                            {
                                                domeinen[punt_x, punt_y].Remove(waarde);

                                                //Als een domein door het verwijderen van de waarde leeg raakt hebben we te maken met een illegale move
                                                if (domeinen[punt_x, punt_y].Count == 0)
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

                                //Hetzelfde geldt voor de vakjes in dezelfde rij en kolom.
                                //Om tijd te besparen doen we dit alleen als we niet al weten dat het een illegale move is
                                if (illegale_move == false)
                                {
                                    for (int k = j + 1; k < 9; k++)
                                    {
                                        if (domeinen[i, k].Contains(waarde))
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
                                if (illegale_move == false)
                                {
                                    for (int k = i + 1; k < 9; k++)
                                    {
                                        if (domeinen[k, j].Contains(waarde))
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

                                //Als de move illegaal bleek te zijn:
                                if (illegale_move == true)
                                {

                                    sudoku[i, j] = 0;                   //De waarde toekenning wordt ongedaan gemaakt
                                    terugzetten(i, j, waarde);          //De waarde wordt weer teruggezet in de domeinen van alle vakjes waaruit het verwijderd was door deze move
                                    domeinen[i, j].Remove(waarde);      //De waarde wordt uit het domein van dit vakje verwijderd
                                    uitgesloten[i, j].Add(waarde);      //De waarde wordt toegevoegd aan de lijst met uitgesloten waarden van dit vakje

                                    //Door één vakje teug te springen en daarna meteen verder te gaan in de for-loop proberen we hetzelfde vakje opnieuw
                                    if (i > 0)
                                    {
                                        i -= 1;
                                    }

                                    else
                                    {
                                        j -= 1;
                                        i = 8;
                                    }
                                }
                            }

                            //Voor vakjes die al vanaf het begin vast staan (en dus -1 in het domein hebben)
                            else
                            {
                                //Als we vakjes aan het invullen zijn moeten deze vakjes overgeslagen worden. We doen dus niks en gaan verder in de for-loop
                                if (illegale_move == false)
                                { }

                                //Als een vakje waar we mee bezig zijn een leeg domein krijgt en we dus terug moeten werken (in dit geval staat
                                // illegale_move op true) worden de vaststaande vakjes op deze manier weer overgeslagen
                                else
                                {
                                    if (i > 1)
                                    {
                                        i -= 2;
                                    }

                                    else
                                    {
                                        j -= 1;
                                        i = 7;
                                    }
                                }
                            }
                        }

                        //Als blijkt dat het domein van het vakje waar we mee bezig zijn leeg raakt (en er dus iets moet veranderen in de al ingevulde vakjes)
                        // gebeurt het volgende
                        else
                        {
                            //Alle uitgesloten waarden worden teruggezet in het domein (aangezien we iets ervoor gaan veranderen en er dan opnieuw naar gaan kijken)
                            domeinen[i, j].AddRange(uitgesloten[i, j]);
                            uitgesloten[i, j].Clear();

                            //Onderstaande while-loop zorgt ervoor dat we een vakje terug gaan (hier is de variabele a voor), of meer als we bij 
                            // van tevoren vastgestelde vakjes komen (en de eerste waarde in het domein -1 is)
                            int a = 0;
                            while (a == 0 || domeinen[i, j][0] == -1)
                            {
                                a = 1;
                                if (i == 0)
                                {
                                    j -= 1;
                                    i = 8;
                                }
                                else
                                {
                                    i -= 1;
                                }
                            }

                            //Nu updaten we het domein van dit al ingevulde vakje waarvan we er dus achter gekomen zijn dat de ingevulde waarde niet kan.
                            // We doen hetzelfde als bij een illegale move
                            waarde = sudoku[i, j];
                            sudoku[i, j] = 0;
                            terugzetten(i, j, waarde);
                            domeinen[i, j].Remove(waarde);
                            uitgesloten[i, j].Add(waarde);

                            //Net als eerder gaan we één stapje terug maar door meteen verder te gaan in de for-loop behandelen we hetzelfde vakje opnieuw
                            if (i == 0)
                            {
                                j -= 1;
                                i = 8;
                            }
                            else
                            {
                                i -= 1;
                            }
                        }
                    }
                }
            }

            //Deze functie zet een waarde terug in de domeinen van de vakjes die deze waarden verloren door het toekennen ervan aan het vakje met [i, j]
            void terugzetten(int i, int j, int waarde)
            {
                int blok_x = (int)Math.Floor(Convert.ToDouble(i) / 3);
                int blok_y = (int)Math.Floor(Convert.ToDouble(j) / 3);

                //Net als eerder zorgen we ervoor dat alleen domeinen van nog lege vakjes ge - update worden
                for (int l = j % 3; l < 3; l++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if ((l * 3 + k) > ((j % 3) * 3 + (i % 3)))
                        {
                            int punt_x = blok_x * 3 + k;
                            int punt_y = blok_y * 3 + l;
                            if (domeinen[punt_x, punt_y].Contains(waarde) == false)
                            {
                                domeinen[punt_x, punt_y].Add(waarde);
                                knoopconsistent(punt_x, punt_y);        //Het domein wordt wel weer knoopconsistent gemaakt zodat geen waarde wordt toegevoegd die niet mag
                                domeinen[punt_x, punt_y].Sort();        //Het domein wordt gesorteerd zodat de laagste waarde vooraan blijft staan
                            }
                        }
                    }
                }

                //We doen hetzelfde voor de rij en kolom
                for (int k = j + 1; k < 9; k++)
                {
                    if (domeinen[i, k].Contains(waarde) == false)
                    {
                        domeinen[i, k].Add(waarde);
                        knoopconsistent(i, k);
                        domeinen[i, k].Sort();

                    }
                }
                for (int k = i + 1; k < 9; ++k)
                {
                    if (domeinen[k, j].Contains(waarde) == false)
                    {
                        domeinen[k, j].Add(waarde);
                        knoopconsistent(k, j);
                        domeinen[k, j].Sort();
                    }
                }
            }

            //Dit print een simpele vorm van de sudoku
            void printSudoku(int[,] sudoku)
            {
                if (displayMode)
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

        //forwardchecking volgens most-constrained-variable (MCV) heuristiek
        static void FC_MCV()
        {
            int[,] sudoku = new int[9, 9];

            //We maken een array van lijsten voor elk blokje. Hierin wordt bijgehouden welke waarden uitgesloten worden,
            //zodat deze terug in het domein gezet kunnen worden als het domein leeg raakt
            //en er dus een waardetoekenning in een eerder blokje veranderd moet worden.
            List<int>[,] uitgesloten = new List<int>[9, 9];

            List<int>[,] domeinen = new List<int>[9, 9];

            List<Point> punten = new List<Point>();                                                                 //Dit is een lijst met punten die bijhoudt in welke volgorde je de sudoku vakjes aan het invullen bent. Dit is belangrijk omdat we bij de most-constrained-variable (MCV) heuristiek niet meer van links boven naar
                                                                                                                    //rechts onder werken, maar kriskras door de sudoku wandelen. Als bij een random vakje geconcludeert wordt dat je terug moet naar het vorige vakje, moet wel bijgehouden zijn wat het vorige vakje was.

            var reductionTimer = new System.Diagnostics.Stopwatch();

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
                        var timer = new System.Diagnostics.Stopwatch();
                        timer.Start();

                        //create new reductionAlgorithmTimer
                        reductionTimer.Restart();


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
                        double totalTime = Math.Round(timer.Elapsed.TotalMicroseconds);
                        double reductionTime = Math.Round(reductionTimer.Elapsed.TotalMicroseconds);

                        //Nadat de hele sudoku correct is ingevuld, wordt de sudoku geprint
                        printSudoku(sudoku);
                        Console.WriteLine("FC_MCV: Sudoku nummer " + sudoku_nr + ". Gevonden in " + totalTime + " microseconden");
                        Console.WriteLine("Reduction time: " + reductionTime + " microseconden");
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
                reductionTimer.Start();
                //Met onderstaande variabelen herkennen we in welk blok het vakje zich bevindt
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
                reductionTimer.Stop();
            }

            //Deze functie is verantwoordelijk om te vinden welk volgende vakje we moeten invullen. Dit wordt bepaald volgens de most-constrained-variable (MCV) heuristiek. We doen dit
            //door de domeinen te checken van elk vakje en het vakje te kiezen met het kleinste domein (minste opties van mohelijke cijfers die ingevuld kunnen worden in een vakje).
            Point vind_vakje()
            {
                reductionTimer.Start();
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
                reductionTimer.Stop();
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
                if (displayMode)
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
    }
}
