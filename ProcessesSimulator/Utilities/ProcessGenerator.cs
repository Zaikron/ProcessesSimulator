using System;
using System.Collections.Generic;

namespace ProcesessSimulator.Utilities
{
    class ProcessGenerator
    {
        public List<_Process> processes = new List<_Process>();
        public const int maxProcesses = 5; //Maximos procesos por lote
        public int totalLotes = 0; // Numero de lotes actuales
        public int currentNumProcess = 0; // Numero de procesos actuales
        public int actualID = 0;
        public ProcessGenerator()
        {

        }

        public List<_Process> generate(int numProcesses)
        {
            for (int i = 0; i < numProcesses; i++)
            {
                _Process p;
                p = new _Process("Anthony Sandoval", generateID(), generateTime(),
                            generateOperation(), generateNum1(), generateNum2());
                p.setNumLote(getNumLote());
                processes.Add(p);
            }
            return processes;
        }

        public _Process generateProcess()
        {
            _Process p;
            p = new _Process("Anthony Sandoval", generateID(), generateTime(),
                        generateOperation(), generateNum1(), generateNum2());
            p.setNumLote(getNumLote());

            return p;
        }

        private int generateID()
        {
            actualID++;
            return actualID;
        }

        private int generateTime()
        {
            Random r = new Random();
            return r.Next(6, 17);
        }

        private string generateOperation()
        {
            Random r = new Random();
            int selector = r.Next(1, 6);
            if (selector == 1)
            {
                return "Suma";
            }
            else if(selector == 2)
            {
                return "Resta";
            }
            else if (selector == 3)
            {
                return "Multiplicacion";
            }
            else if (selector == 4)
            {
                return "Division";
            }
            else
            {
                return "Residuo";
            }
        }

        private int generateNum1()
        {
            Random r = new Random();
            return r.Next(1, 50);
        }

        private int generateNum2()
        {
            Random r = new Random();
            return r.Next(1, 50);
        }



        private int getNumLote()
        {
            if (currentNumProcess < maxProcesses)
            {
                if (totalLotes == 0)
                {
                    totalLotes = 1;
                }
                currentNumProcess++;
                return totalLotes;
            }
            else
            {
                currentNumProcess = 1;
                totalLotes++;
                return totalLotes;
            }
        }

    }
}
