using System;

namespace ProcesessSimulator.Utilities
{
    class _Process
    {
        public string programerName;
        public int id = 0;
        
        public string operation;
        public int n1;
        public int n2;
        public float result = 999.999f;
        public int numLote = 0;
        public bool isError = false;
        public bool executed = false;
        public bool added = false;
        public int elapsedTme = 0;
        public int blockTime = 0;
        public string state = "NUEVO";

        /*Tiempos*/
        public int maxTime = -1;
        public int TLlegada = -1;
        private bool indicatorTLlegada = false;
        public int TFinal = -1;
        public int TRetorno = -1;
        public int TRespuesta = -1;
        private bool indicatorTRespuesta = false;
        public int TEspera = -1;
        public int TServicio = -1;

        /*
         if (p.maxTime == -1) { row.Cells[4].Value = "null"; } else { row.Cells[4].Value = p.maxTime; }
            if (p.TLlegada == -1) { row.Cells[5].Value = "null"; } else { row.Cells[5].Value = p.TLlegada; }
            if (p.TFinal == -1) { row.Cells[6].Value = "null"; } else { row.Cells[6].Value = p.TFinal; }
            if (p.TRetorno == -1) { row.Cells[7].Value = "null"; } else { row.Cells[7].Value = p.TRetorno; }
            if (p.TRespuesta == -1) { row.Cells[8].Value = "null"; } else { row.Cells[8].Value = p.TRespuesta; }
            if (p.TEspera == -1) { row.Cells[9].Value = "null"; } else { row.Cells[9].Value = p.TEspera; }
            if (p.TServicio == -1) { row.Cells[10].Value = "null"; } else { row.Cells[10].Value = p.TServicio; }
         */


        public _Process(String name, int id, int maxT, String op, int n1, int n2)
        {
            this.programerName = name;
            this.id = id;
            this.maxTime = maxT;
            this.operation = op;
            this.n1 = n1;
            this.n2 = n2;

            //setOperation();
        }

        public void setOperation()
        {
            if (operation == "Suma")
            {
                setSum();
            }
            else if (operation == "Resta")
            {
                setResta();
            }
            else if (operation == "Multiplicacion")
            {
                setMult();
            }
            else if (operation == "Division")
            {
                setDiv();
            }
            else if (operation == "Residuo")
            {
                setResiduo();
            }
        }

        public string getOperationSymbol()
        {
            if (operation == "Suma")
            {
                return "+";
            }
            else if (operation == "Resta")
            {
                return "-";
            }
            else if (operation == "Multiplicacion")
            {
                return "*";
            }
            else if (operation == "Division")
            {
                return "/";
            }
            else if (operation == "Residuo")
            {
                return "%";
            }
            return "NULL";
        }

        public void calculeTimers(int global)
        {
            if (state == "NUEVO")
            {
                
            }
            else if (state == "LISTO")
            {
                setTLlegada(global);
                setTServicio();
                TEspera = global - TLlegada - TServicio;
            }
            else if (state == "EJECUCION")
            {
                setTRespuesta(global);
                setTServicio();
                TEspera = global - TLlegada - TServicio;
            }
            else if (state == "BLOQUEADO")
            {
                setTServicio();
                TEspera = global - TLlegada - TServicio;
            }
            else if (state == "TERMINADO")
            {
                setTFinal(global);
                setTRetorno();
                setTServicio();
                setTEspera();
            }

        }

        /*      Tiempos     */
        public void setTLlegada(int time) // Cuando entra listos
        {
            if (indicatorTLlegada == false)
            {
                TLlegada = time;
                indicatorTLlegada = true;
            }
        }

        public void setTFinal(int time) // Cuando entra a terminados
        {
            TFinal = time;
        }
        public void setTRetorno() // Cuando entra a terminados
        {
            TRetorno = TFinal - TLlegada;
        }

        public void setTRespuesta(int time) // La primera vez que se ejecuta
        {
            if (indicatorTRespuesta == false)
            {
                TRespuesta = time - TLlegada;
                indicatorTRespuesta = true;
            }
        }

        public void setTServicio() // Cuando entra a terminados
        {
            TServicio = elapsedTme;
        }

        public void setTEspera() // Cuando entra a terminados
        {
            TEspera = TRetorno - TServicio;
        }

        public void setTEsperaBCP(int global) // Cuando entra a terminados
        {
            if (state == "LISTO" || state == "EJECUCION" || state == "BLOQUEADO")
            {
                setTServicio();
                TEspera = global - TLlegada - TServicio;
            }
        }
        /*****************/
        public void setNumLote(int lote)
        {
            this.numLote = lote;
        }

        public void setDiv()
        {
            result = (float)n1 / (float)n2;
        }

        public void setMult()
        {
            result = n1 * n2;
        }

        public void setSum()
        {
            result = n1 + n2;
        }

        public void setResta()
        {
            result = n1 - n2;
        }

        public void setResiduo()
        {
            result = (float)n1 % (float)n2;
        }

    }
}
