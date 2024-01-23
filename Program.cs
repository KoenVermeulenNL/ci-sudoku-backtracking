
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Xml;

class Program
{
    static void Main()
    {
        int[,] sudoku = new int[9, 9];                                                  //De sudoku die ingevuld gaat worden
        int[,] statiche_sudoku = new int[9, 9];                                         //Een "statische" sudoku om bij te houden welke cijfers in de sudoku vanaf het begin al vaststaan

        backtracking();

        //De functie die verantwoordelijk is voor chronological backtracking
        void backtracking()
        {
            int index;
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
                    printSudoku(sudoku);                                                //Nadat de hele sudoku correct is ingevuld, wordt de sudoku geprint
                }
            }
        }

        //De fucnctie die de sudoku proint in een een simpele form 
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
}


