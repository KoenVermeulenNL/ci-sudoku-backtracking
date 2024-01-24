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
    static void Main()
    {
        int[,] sudoku = new int[9, 9];

        //Lijsten voor elk blokje waarin wordt bijgehouden welke waarden uitgesloten worden, zodat deze terug in het domein gezet kunnen worden als het 
        // domein leeg raakt en er dus een waardetoekenning in een eerder blokje veranderd moet worden.
        List<int>[,] uitgesloten = new List<int>[9, 9];

        List<int>[,] domeinen = new List<int>[9, 9];

        List<Point> punten = new List<Point>();


        //Alles gebeurt in de functie import()
        import();

        void import()
        {
            string[] cijfers;
            string regel;
            char[] separators = { ' ' };

            StreamReader sr = new StreamReader("../../../TextFile1.txt");

            while ((regel = sr.ReadLine()) != null)
            {
                cijfers = regel.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (cijfers.Length == 81)
                {
                    vul_sudoku(cijfers);
                    maak_domeinen();

                    printSudoku(sudoku);
                    Console.WriteLine();

                    bool sudoku_not_solved = true;
                    while (sudoku_not_solved)
                    {
                        Point coordinaten = vind_vakje();
                        if (coordinaten.X != -1)
                        {
                            punten.Add(coordinaten);
                            forward_checking(coordinaten.X, coordinaten.Y);
                        }
                        if (coordinaten.X == -1)
                            sudoku_not_solved = false;
                    }
                    printSudoku(sudoku);
                    Console.WriteLine();
                    punten.Clear(); 
                }
            }
        }

        void vul_sudoku(string[] cijfers)
        {
            int index = 0;
            for (int j = 0; j < 9; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    sudoku[i, j] = int.Parse(cijfers[index]);
                    domeinen[i, j] = new List<int>();
                    uitgesloten[i, j] = new List<int>();
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

            //Hetzelfde geldt voor alle waarden in de rij en colom van het vakje
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

        Point vind_vakje()
        {
            int domein_teller = 10;
            int a = 0;
            int b = 0;
            int aantal_ingevuld = 0;

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (domeinen[x, y].Count > 0)
                    {
                        if (domein_teller > domeinen[x, y].Count && domeinen[x, y][0] != -1 && sudoku[x, y] == 0)
                        {
                            a = x;
                            b = y;
                            domein_teller = domeinen[x, y].Count;
                        }
                        if (sudoku[x, y] != 0)
                        {
                            aantal_ingevuld++;
                        }
                        if (aantal_ingevuld == 81)
                        {
                            a = -1;
                            b = -1;
                        }
                    }
                }
            }
            return new Point(a, b);
        }

        //In deze functie krijgen alle vakjes een waarde door middel van een Forward Checking algoritme
        void forward_checking(int i, int j)
        {
            //Hiermee houden we bij of het invullen van een specifieke waarde er voor zorgt dat een domein van een ander nog oningevuld vakje leeg maakt
            bool illegale_move = false;
            int waarde;
            int punten_index = 0;
            bool stop_while_loop = false; 

            while (punten_index != -1) //klopt dit
            {
                    //Zolang we niet te maken hebben met vakjes met lege domeinen blijven we ze invullen en domeinen updaten
                    if (domeinen[i, j].Count > 0)
                    {
                        if (domeinen[i, j][0] != -1)
                        {
                            illegale_move = false;

                            //De waarde die wordt toegekend is het laagste getal in het domein
                            waarde = domeinen[i, j][0];
                            sudoku[i, j] = waarde;

                            double blok_x = Math.Floor((double)i / 3);                                                      //////////////////
                            double blok_y = Math.Floor((double)j / 3);                                                      ///////////////

                            //int blok_x = (int)Math.Floor(Convert.ToDouble(i) / 3);
                            //int blok_y = (int)Math.Floor(Convert.ToDouble(j) / 3);

                            //Voor de andere vakjes in het blok van het vakje wordt de waarde uit het domein gehaald
                            for (int l = ((int)blok_y * 3); l < (3 + blok_y * 3); l++)                                                              ///////////////
                            {
                                for (int k = ((int)blok_x * 3); k < (3 + blok_x * 3); k++)                                          ///////////////////////
                                {
                                    //Onderstaande restrictie zorgt ervoor dat alleen domeinen van nog lege vakjes ge-update worden
                                    if (!punten.Contains(new Point(k,l)))                                                                   ///////////////////
                                    {
                                        //int punt_x = blok_x * 3 + k;
                                        //int punt_y = blok_y * 3 + l;
                                        if (domeinen[k, l].Contains(waarde) && (l != j || k != i))                                      /////////////////
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
                                if (!punten.Contains(new Point(i, k)))                                                              /////////////////////////   
                                {
                                    if (domeinen[i, k].Contains(waarde) && k != j)                                                          /////////////////
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
                                if (!punten.Contains(new Point(k, j)))                                                  ////////////////////
                                {
                                    if (domeinen[k, j].Contains(waarde) && k != i)                                              /////////////////
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
                            if (illegale_move == false)
                            {
                                //nog_geen_waarde = false;
                                punten_index = -1;

                                //if (stop_while_loop)
                                   // punten_index = -1; 
                               // if (punten_index != -1 && (punten.Count - punten_index) >=0 && (punten.Count - punten_index) < punten.Count)
                                //{
                                //    i = punten[punten.Count - punten_index].X;
                                //    j = punten[punten.Count - punten_index].Y;
                                //}

                            }

                        //Als de move illegaal bleek te zijn 
                            if (illegale_move == true)
                            {
                                sudoku[i, j] = 0;
                                terugzetten(i, j, waarde);
                                domeinen[i, j].Remove(waarde);
                                uitgesloten[i, j].Add(waarde);

                                    /*if (i > 0)
                                    {
                                        i -= 1;
                                    }

                                    else
                                    {
                                        j -= 1;
                                        i = 8;
                                    }*/
                            }
                        }

                    /*else
                    {
                        if (illegale_move == true)
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
                        else { }
                    }*/
                    }

                    else
                    {
                        domeinen[i, j].AddRange(uitgesloten[i, j]);
                        uitgesloten[i, j].Clear();

                    /*int a = 0;
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
                    }*/
                    /*if (stop_while_loop == false)
                    {
                        punten_index += 1;
                    }
                    if (punten_index != -1 && (punten.Count - punten_index) >= 0 && (punten.Count - punten_index) < punten.Count)
                    {
                        i = punten[punten.Count - punten_index].X;
                        j = punten[punten.Count - punten_index].Y;

                    }

                    if (punten.Count > 0)                                                                                               ////////////
                    {
                        punten.RemoveAt(punten.Count - 1);
                        stop_while_loop = true;
                    }*/

                    if (punten.Count > 0)                                                                                               ////////////
                    {
                        punten.RemoveAt(punten.Count - 1);
                    }

                    if (punten.Count > 0)
                    {
                        i = punten[punten.Count - 1].X;
                        j = punten[punten.Count - 1].Y;

                    }

                        waarde = sudoku[i, j];
                        sudoku[i, j] = 0;
                        terugzetten(i, j, waarde);
                        domeinen[i, j].Remove(waarde);
                        uitgesloten[i, j].Add(waarde);


                        //nog_geen_waarde = false; 

                        /*if (i == 0)
                        {
                            j -= 1;
                            i = 8;
                        }
                        else
                        {
                            i -= 1;
                        }*/
                    }
                
            }
        }


        void terugzetten(int i, int j, int waarde)
        {
            double blok_x = Math.Floor((double)i / 3);
            double blok_y = Math.Floor((double)j / 3);

            //int blok_x = (int)Math.Floor(Convert.ToDouble(i) / 3);
            //int blok_y = (int)Math.Floor(Convert.ToDouble(j) / 3);

            //Voor de andere vakjes in het blok van het vakje wordt de waarde uit het domein gehaald
            for (int l = ((int)blok_y * 3); l < (3 + blok_y * 3); l++)
            {
                for (int k = ((int)blok_x * 3); k < (3 + blok_x * 3); k++)
                {
                    if (!punten.Contains(new Point(k, l)))                                                                                      ///////////////
                    {
                        if (domeinen[k, l].Contains(waarde) == false && (l != j || k != i))                                         ////////////////
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
                if (!punten.Contains(new Point(i, k)))                                                                          ///////////
                {
                    if (domeinen[i, k].Contains(waarde) == false && k != j)                                                 /////////////
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
                    if (domeinen[k, j].Contains(waarde) == false && k != i)                             ////////////////
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





