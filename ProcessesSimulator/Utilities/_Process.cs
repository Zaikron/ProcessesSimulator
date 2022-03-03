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
        public float result;
        public int numLote = 0;
        public bool executed = false;
        public bool added = false;
        public int elapsedTme = 0;
        public int blockTime = 0;

        /*Tiempos*/
        public int maxTime = 0;
        public int TLlegada = 0;
        public int TFinal = 0;
        public int TRetorno = 0;
        public int TRespuesta = 0;
        private bool indicatorTRespuesta = false;
        public int TEspera = 0;
        public int TServicio = 0;


        public _Process(String name, int id, int maxT, String op, int n1, int n2)
        {
            this.programerName = name;
            this.id = id;
            this.maxTime = maxT;
            this.operation = op;
            this.n1 = n1;
            this.n2 = n2;

            setOperation();
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

        /*      Tiempos     */
        public void setTLlegada(int time) // Cuando entra listos
        {
            TLlegada = time;
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
