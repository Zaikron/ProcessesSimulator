using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ProcesessSimulator.Utilities;
using System.Diagnostics;

namespace Compilador
{
    public partial class Principal : Form
    {
        int BLOCKED_TIME = 8; //Constante para el tiempo de bloqueo
        _Process nullProcess; //Proceso nulo para mantener en ejecucion al procesador
        bool nullProcessIndicator = false;
        int MEMORY = 5; // Procesos maximos en memoria
        //Clase generadora de procesos aleatorios
        ProcessGenerator generator = new ProcessGenerator();
        int quantum = 0;
        int currentQuantum = 0;

        //Procesos en ejecucion
        List<_Process> execution = new List<_Process>();

        //TIMER
        _Process Currentprocess;
        int elapsedCurrentTime = 0;
        int timeAcum = 0;
        bool pauseIndicator = false;
        bool cooldownkey = false;

        public Principal()
        {
            InitializeComponent();
            tableWorking.RowHeadersVisible = false;
            tableComplete.RowHeadersVisible = false;
            tableNews.RowHeadersVisible = false;
            tablesFocus(true);

            nullProcess = new _Process("NULL", 0, 99999, "NULL", 0, 0);

            //tabOutputs.Enabled = false;
            btnIni.Enabled = false;
        }

        private void datos(List<_Process> p)
        {
            for (int i = 0; i < p.Count; i++)
            {
                Debug.WriteLine("id: " + p[i].id);
                Debug.WriteLine("added: " + p[i].added);
                Debug.WriteLine("executed: " + p[i].executed);
                Debug.WriteLine("tMax: " + p[i].maxTime);
                Debug.WriteLine("tT: " + p[i].elapsedTme);
                Debug.WriteLine("\n");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e){}
        private void Form1_Load(object sender, EventArgs e){}


        private void btnIni_Click(object sender, EventArgs e)
        {
            tabOutputs.SelectedIndex = 0;
            tablesFocus(false);
            cBoxProcesess.Enabled = false;
            fieldCantProcesess.Enabled = false;
            fieldQuantum.Enabled = false;
            btnAddAll.Enabled = false;

            quantum = int.Parse(fieldQuantum.Text);
            textID.Text = execution[0].id.ToString();
            addWorkingLot(); //Comprobar si no hay filas, y si no pues se agrega el primer lote
            timerBlock.Start(); //Inicia el timer de Bloqueados
            timerLot.Start(); // Inicio del timer de Lotes
        }

        private void setProcessTexts(_Process p) //Actualiza los textos del proceso actual
        {
            p.state = "EJECUCION";
            p.calculeTimers(timeAcum);

            if (p.id != 0) { execution[getIndexOfProcessList(p)] = p; }

            textID.Text = p.id.ToString();
            textOperation.Text = p.operation;
            textTMax.Text = p.maxTime.ToString();
            //textLotes.Text = (generator.totalLotes - p.numLote + 1).ToString(); //+1 para que concuerden
            //textCurrentLot.Text = p.numLote.ToString();
            textTExecuted.Text = p.elapsedTme.ToString();
            

            textID.Update();
            textOperation.Update();
            textTMax.Update();
            //textLotes.Update();
            //textCurrentLot.Update();
            textTExecuted.Update();
            textNuevos.Update();
        }

        private void setTimerTexts(_Process p, int j) //Actualiza los textos de los tiempos
        {
            textTTrans.Text = p.elapsedTme.ToString();
            textTRes.Text = (p.maxTime - j).ToString();
            textAcum.Text = timeAcum.ToString();
            textQuantum.Text = currentQuantum.ToString();

            textTTrans.Update();
            textTRes.Update();
            textAcum.Update();
            textQuantum.Update();
        }

        private void resetTexts()
        {
            textID.Text = "0";
            textOperation.Text = "*";
            textTMax.Text = "*";
            textTExecuted.Text = "*";
            textTTrans.Text = "0";
            textTRes.Text = "*";
            textQuantum.Text = "0";

            textTTrans.Update();
            textTRes.Update();
            textID.Update();
            textOperation.Update();
            textTMax.Update();
            textTExecuted.Update();
            textQuantum.Update();
        }

        private void tablesFocus(bool mode)
        {
            tableBlock.Enabled = mode;
            tableComplete.Enabled = mode;
            tableWorking.Enabled = mode;
            tableNews.Enabled = mode;
            tabOutputs.Enabled = mode;
        }

        private int getNumRow(_Process p, DataGridView table) //Obtener el numero de la fila de una tabla por el id del proceso
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {   
                //  Recorrido de las filas, por el valor de la primara columna
                if (table.Rows[i].Cells[0].Value == p.id.ToString())
                {
                    return i;
                }
            }
            return 0;
        }

        private int getIndexOfProcessList(_Process p) //Se obtiene el indice en la lista del procesos
        {
            for (int i = 0; i < generator.processes.Count; i++)
            {
                if (p.id == generator.processes[i].id)
                {
                    return i;
                }
            }
            return -1;
        }

        private void addWorkingLot() //Se agregan los procesos de un lote a la tabla, por separado
        {
            int processInMemory = (tableWorking.Rows.Count - 1) + (tableBlock.Rows.Count - 1);
            if (textID.Text != "0") { processInMemory++; }
            Debug.WriteLine("Process: " + processInMemory);

            if (processInMemory < MEMORY)
            {
                for (int i = 0; i < execution.Count; i++)
                {
                    if (execution[i].added == false && processInMemory < MEMORY)
                    {
                        //Tiempo de llegada
                        execution[i].calculeTimers(timeAcum);

                        addToWorkingTable(execution[i]);
                        execution[i].added = true;

                        tableNews.Rows.RemoveAt(getNumRow(execution[i], tableNews)); //Se elimina el proceso de la tabla de nuevos
                        tableNews.Update();
                        textNuevos.Text = countNews().ToString();
                    }
                    processInMemory = (tableWorking.Rows.Count - 1) + (tableBlock.Rows.Count - 1);
                    if (textID.Text != "0") { processInMemory++; }
                }
            }
        }

        private _Process getProcess(int id)
        {
            for (int i = 0; i < execution.Count; i++)
            {
                if (id == execution.ElementAt(i).id)
                {
                    return execution.ElementAt(i);
                }
            }
            return null;
        }

        private void fillNewsTable() {
            for (int i = 0; i < execution.Count; i++)
            {
                addToNewsTable(execution[i]);
            }
        }

        private void fillBCPTable()
        {
            for (int i = 0; i < execution.Count; i++)
            {
                addToBCPTable(execution[i]);
            }
        }

        private void addToNewsTable(_Process p) // Nuevos
        {
            p.state = "NUEVO";
            p.calculeTimers(timeAcum);
            execution[getIndexOfProcessList(p)] = p;
            DataGridViewRow row = (DataGridViewRow)tableNews.Rows[0].Clone();
            row.Cells[0].Value = p.id;
            row.Cells[1].Value = p.maxTime;
            row.Cells[2].Value = p.operation;
            tableNews.Rows.Add(row);
            tableNews.Update();
        }

        private void addToBlockTable(_Process p) //Bloqueados
        {
            p.state = "BLOQUEADO";
            p.calculeTimers(timeAcum);
            execution[getIndexOfProcessList(p)] = p;
            DataGridViewRow row = (DataGridViewRow)tableBlock.Rows[0].Clone();
            if (p != null)
            {
                row.Cells[0].Value = p.id;
                row.Cells[1].Value = p.maxTime;
                row.Cells[2].Value = p.elapsedTme;
                row.Cells[3].Value = p.operation;
                row.Cells[4].Value = p.blockTime;
                tableBlock.Rows.Add(row);
                tableBlock.Update();
            }
        }

        private void addToWorkingTable(_Process p) // Listos
        {
            p.state = "LISTO";
            p.calculeTimers(timeAcum);
            execution[getIndexOfProcessList(p)] = p;
            DataGridViewRow row = (DataGridViewRow)tableWorking.Rows[0].Clone();
            row.Cells[0].Value = p.id;
            row.Cells[1].Value = p.maxTime;
            row.Cells[2].Value = p.elapsedTme;
            row.Cells[3].Value = p.operation;
            tableWorking.Rows.Add(row);
            tableWorking.Update();
        }

        private void addToBCPTable(_Process p) //BCP
        {
            p.setTEsperaBCP(timeAcum);
            execution[getIndexOfProcessList(p)] = p;

            DataGridViewRow row = (DataGridViewRow)tableBCP.Rows[0].Clone();
            row.Cells[0].Value = p.id;
            row.Cells[1].Value = p.n1 + " " + p.getOperationSymbol() + " " + p.n2;
            if (p.isError == false)
            {
                if (p.result == 999.999f)
                {
                    row.Cells[2].Value = "null";
                }
                else
                {
                    row.Cells[2].Value = p.result;
                }
            }
            else
            {
                row.Cells[2].Value = "ERROR";
            }
            row.Cells[3].Value = p.state;
            /*Tiempos*/
            if (p.maxTime == -1) { row.Cells[4].Value = "null"; } else { row.Cells[4].Value = p.maxTime; }
            if (p.TLlegada == -1) { row.Cells[5].Value = "null"; } else { row.Cells[5].Value = p.TLlegada; }
            if (p.TFinal == -1) { row.Cells[6].Value = "null"; } else { row.Cells[6].Value = p.TFinal; }
            if (p.TRetorno == -1) { row.Cells[7].Value = "null"; } else { row.Cells[7].Value = p.TRetorno; }
            if (p.TRespuesta == -1) { row.Cells[8].Value = "null"; } else { row.Cells[8].Value = p.TRespuesta; }
            if (p.TEspera == -1) { row.Cells[9].Value = "null"; } else { row.Cells[9].Value = p.TEspera; }
            if (p.TServicio == -1) { row.Cells[10].Value = "null"; } else { row.Cells[10].Value = p.TServicio; }

            tableBCP.Rows.Add(row);
            tableBCP.Update();
        }

        private void addToCompleteTable(_Process p) //Completados
        {
            p.state = "TERMINADO";
            p.setOperation();
            p.calculeTimers(timeAcum);

            execution[getIndexOfProcessList(p)] = p;

            DataGridViewRow row = (DataGridViewRow)tableComplete.Rows[0].Clone();
            row.Cells[0].Value = p.id;
            row.Cells[1].Value = p.n1 + " " + p.getOperationSymbol() + " " + p.n2;
            if (p.isError == false){
                row.Cells[2].Value = p.result;
            }
            else{
                row.Cells[2].Value = "ERROR";
            }
            /*Tiempos*/
            row.Cells[3].Value = p.maxTime;
            row.Cells[4].Value = p.TLlegada;
            row.Cells[5].Value = p.TFinal;
            row.Cells[6].Value = p.TRetorno;
            row.Cells[7].Value = p.TRespuesta;
            row.Cells[8].Value = p.TEspera;
            row.Cells[9].Value = p.TServicio;

            tableComplete.Rows.Add(row);
            tableComplete.Update();
        }

        private void addToProcesessBox(_Process p)
        {
            cBoxProcesess.Items.Add("ID: " + p.id + " | Tiempo: " +  p.maxTime + " | Operacion: " + p.operation);
        }

        private void btnAddAll_Click(object sender, EventArgs e)
        {
            resetAll();
            //Gracias al objeto generator se crean todos los procesos con valores aleatorios
            execution = generator.generate(int.Parse(fieldCantProcesess.Text)); // es una variable auxiliar para trabajar con los procesos
            fillComboBoxProcesses(); //Se llena el combo box con los procesos generados
            fillNewsTable();
            textNuevos.Text = countNews().ToString();
            //datos(execution);
        }

        private void btnIni_KeyPress(object sender, KeyPressEventArgs e)
        {
            textKey.Text = char.ToUpper(e.KeyChar).ToString();
            if (e.KeyChar == 'i' || e.KeyChar == 'I')
            {
                if (pauseIndicator == false && nullProcessIndicator == false && cooldownkey == true)
                {
                    timerProcesess.Stop();
                    currentQuantum = 0;
                    elapsedCurrentTime = 0; //Reset del tiempo
                    addToBlockTable(getProcess(Currentprocess.id));
                    resetTexts();
                    timerLot.Start();
                    cooldownkey = false;
                }
            }
            else if (e.KeyChar == 'e' || e.KeyChar == 'E')
            {
                if (pauseIndicator == false && nullProcessIndicator == false && cooldownkey == true)
                {
                    timerProcesess.Stop();
                    currentQuantum = 0;
                    elapsedCurrentTime = 0; //Reset del tiempo
                    execution[getIndexOfProcessList(Currentprocess)].isError = true; //Se establece como error
                    addToCompleteTable(getProcess(Currentprocess.id));

                    resetTexts();
                    timerLot.Start();
                    cooldownkey = false;
                }
            }
            else if (e.KeyChar == 'p' || e.KeyChar == 'P')
            {
                pauseIndicator = true;
                timerProcesess.Stop();
                timerLot.Stop();
                timerBlock.Stop();
            }
            else if (e.KeyChar == 'c' || e.KeyChar == 'C')
            {
                tabOutputs.SelectedIndex = 0;
                pauseIndicator = false;

                tableBCP.Rows.Clear();
                tableBCP.Refresh();
                //Se inicia el timer de procesos para comience en el ultimo estado, pues las variables no se resetean
                timerProcesess.Start();
                timerBlock.Start();
            }
            else if (e.KeyChar == 'n' || e.KeyChar == 'N')
            {
                if (pauseIndicator == false && cooldownkey == true)
                {
                    _Process p = generator.generateProcess();
                    
                    execution.Add(p);
                    addToNewsTable(p);
                    textNuevos.Text = countNews().ToString();

                    cooldownkey = false;
                }
            }
            else if (e.KeyChar == 't' || e.KeyChar == 'T')
            {
                fillBCPTable();

                tabOutputs.SelectedIndex = 1;
                pauseIndicator = true;
                timerProcesess.Stop();
                timerLot.Stop();
                timerBlock.Stop();
            }
        }


        //Timers

        private void timeBlock_Tick(object sender, EventArgs e)
        {
            if (tableBlock.Rows.Count > 1)
            {
                for (int i = 0; i < tableBlock.Rows.Count - 1; i++)
                {
                    _Process auxProcess = getProcess((int)tableBlock.Rows[i].Cells[0].Value);
                    int index = getIndexOfProcessList(auxProcess);

                    if (execution[index].blockTime > 0)
                    {
                        execution[index].blockTime--;
                        tableBlock.Rows[i].Cells[4].Value = execution[index].blockTime;
                        tableBlock.Update();
                    }
                    else
                    {
                        tableBlock.Rows.RemoveAt(0); //Se elimina la primera posicion de la tabla
                        addToWorkingTable(execution[index]);

                        if (textID.Text == "0") //Si el id es 0 significa que el proceso el null
                        {
                            nullProcessIndicator = false;
                            timerProcesess.Stop(); //Se detiene el timer de procesos para que se detenga el proceso null
                            timerLot.Start(); //Al comenzar el timer de lotes/memoria se reasignara el currentProcess, ya no sera null
                            
                        }
                    }

                }
            }
        }

        private void timerProcesess_Tick(object sender, EventArgs e)
        {
            addWorkingLot(); //Comprobar si no hay filas, y si no pues se agrega el primer lote
            //Se ejecutara hasta que se complete el tiempo maximo
            elapsedCurrentTime = Currentprocess.elapsedTme; //Obtengo el tiempo del proceso actual sino empezara siempre de 0
            if (elapsedCurrentTime < Currentprocess.maxTime)
            {
                elapsedCurrentTime++;
                timeAcum++;
                if (nullProcessIndicator == false)
                {
                    if (currentQuantum < quantum)
                    {
                        // Se establece el tiempo transcurrido en el objeto de proceso actual
                        execution[getIndexOfProcessList(Currentprocess)].elapsedTme = elapsedCurrentTime;
                        currentQuantum++;
                        setTimerTexts(Currentprocess, elapsedCurrentTime); //Actualizar los tiempos del proceso y tiempo total
                    }
                    else
                    {
                        addToWorkingTable(Currentprocess);
                        currentQuantum = 0;
                        timerProcesess.Stop(); //Se detiene el timer de los procesos
                        timerLot.Start(); // y se vuelve a iniciar el timer de los lotes
                    }
                }
                else
                {
                    currentQuantum = 0;
                    setTimerTexts(Currentprocess, elapsedCurrentTime); //Actualizar los tiempos del proceso y tiempo total
                }
                cooldownkey = true;
            }
            else //Cuando termine el tiempo maximo
            {
                currentQuantum = 0;
                elapsedCurrentTime = 0; //Reset del tiempo
                execution[getIndexOfProcessList(Currentprocess)].executed = true;
                addToCompleteTable(Currentprocess); //Se agrega a la tabla de procesos terminados
                resetTexts();
                timerProcesess.Stop(); //Se detiene el timer de los procesos
                timerLot.Start(); // y se vuelve a iniciar el timer de los lotes
            }
            
        }

        private void timerLot_Tick(object sender, EventArgs e)
        {
            if (tableWorking.Rows.Count != 1) //Mientras existan filas en la tabla se ejecutara
            {
                String id = tableWorking[0, 0].Value.ToString(); //Se obtiene el id de la primera fila de la tabla
                _Process p = getProcess(int.Parse(id)); // Obtencion del procesos respecto al id
                execution[getIndexOfProcessList(p)].blockTime = BLOCKED_TIME; //Se establece el tiempo de bloqueo

                tableWorking.Rows.RemoveAt(0); //Se elimina la primera posicion de la tabla
                tableWorking.Update();
                
                setProcessTexts(p); //Actualizar textos con los datos del proceso
                
                Currentprocess = p;
                textNuevos.Text = countNews().ToString();
                cooldownkey = true;
                timerLot.Stop(); //Se detiene el timer de los lotes para que se ejecute el proceso
                timerProcesess.Start(); //Comienza el tiemer del proceso
                addWorkingLot(); //Comprobar si no hay filas, y si no pues se agrega el primer lote
            }
            else
            {
                if (tableBlock.Rows.Count > 1) //Si todavia hay procesos en bloqueado se agregara un proceso nulo
                {
                    nullProcessIndicator = true;
                    Currentprocess = nullProcess; //Al proceso actual se le asigna el proceso null
                    setProcessTexts(Currentprocess); //Actualizar textos con los datos del proceso
                    timerLot.Stop();
                    timerProcesess.Start(); //Se comienza en timer de procesos para comience con el proceso null
                }
                else //Cuando ya no exista ningun proceso
                {
                    //Cuando ya no existen procesos no ejecutados se detiene el timer
                    generator.totalLotes = 0; // Numero de lotes actuales
                    generator.currentNumProcess = 0; // Numero de procesos actuales

                    cBoxProcesess.Enabled = true;
                    fieldCantProcesess.Enabled = true;
                    fieldQuantum.Enabled = true;
                    btnIni.Enabled = false;
                    btnAddAll.Enabled = true;
                    cBoxProcesess.Items.Clear();
                    cBoxProcesess.Refresh();

                    timerLot.Stop();
                    cooldownkey = true;
                    tablesFocus(true);
                }
                
            }
            
        }

        private void resetAll()
        {
            generator = new ProcessGenerator();
            elapsedCurrentTime = 0;
            timeAcum = 0;
            btnIni.Enabled = true;
            cBoxProcesess.Enabled = true;

            tableComplete.Rows.Clear();
            tableComplete.Refresh();

            tableNews.Rows.Clear();
            tableNews.Refresh();

            cBoxProcesess.Items.Clear();
            cBoxProcesess.Refresh();
        }

        private void fillComboBoxProcesses()
        {
            for (int i = 0; i < generator.processes.Count; i++)
            {
                addToProcesessBox(generator.processes[i]);
            }
        }

        private int countNews()
        {
            int cont = 0;
            for (int i = 0;  i  < execution.Count; i++)
            {
                if (execution[i].added == false)
                {
                    cont++;
                }
            }
            return cont;
        }

        private void Principal_Click(object sender, EventArgs e)
        {
            btnIni.Focus();
        }
    }
}
