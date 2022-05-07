using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ProcesessSimulator.Utilities;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Compilador
{
    public partial class Principal : Form
    {
        int BLOCKED_TIME = 8; //Constante para el tiempo de bloqueo
        _Process nullProcess; //Proceso nulo para mantener en ejecucion al procesador
        bool nullProcessIndicator = false;
        //int MEMORY = 5; // Procesos maximos en memoria
        //Clase generadora de procesos aleatorios
        ProcessGenerator generator = new ProcessGenerator();
        int quantum = 0;
        int currentQuantum = 0;
        int tamMarco = 4;

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
             
            nullProcess = new _Process("NULL", 0, 99999, "NULL", 0, 0, 0);

            //tabOutputs.Enabled = false;
            btnIni.Enabled = false;
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
            paintPages(p, Color.Green, Color.White);

            if (p.id != 0) { execution[getIndexOfProcessList(p)] = p; }

            textID.Text = p.id.ToString();
            textOperation.Text = p.operation;
            textTMax.Text = p.maxTime.ToString();
            textTExecuted.Text = p.elapsedTme.ToString();
            textMem.Text = p.memory.ToString();
            textMarcos.Text = getNeededMarcos(p).ToString();
            

            textID.Update();
            textOperation.Update();
            textTMax.Update();
            textTExecuted.Update();
            textNuevos.Update();
            textMem.Update();
            textMarcos.Update();
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
            textMem.Text = "*";
            textMarcos.Text = "*";

            textTTrans.Update();
            textTRes.Update();
            textID.Update();
            textOperation.Update();
            textTMax.Update();
            textTExecuted.Update();
            textQuantum.Update();
            textMem.Update();
            textMarcos.Update();
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
                if (table.Rows[i].Cells[0].Value.ToString() == p.id.ToString())
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
            for (int i = 0; i < execution.Count; i++)
            {
                if (execution[i].added == false)
                {
                    if (isThereMemory(execution[i]))
                    {
                        addPagesToMem(execution[i]);

                        //Tiempo de llegada
                        execution[i].calculeTimers(timeAcum);

                        addToWorkingTable(execution[i]);
                        execution[i].added = true;

                        tableNews.Rows.RemoveAt(getNumRow(execution[i], tableNews)); //Se elimina el proceso de la tabla de nuevos
                        //tableNews.Update();
                        textNuevos.Text = countNews().ToString();
                    }
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
            row.Cells[3].Value = p.memory;
            row.Cells[4].Value = getNeededMarcos(p);
            tableNews.Rows.Add(row);
            //tableNews.Update();
        }

        private void addToBlockTable(_Process p) //Bloqueados
        {
            p.state = "BLOQUEADO";
            p.calculeTimers(timeAcum);
            paintPages(p, Color.DarkRed, Color.White);
            execution[getIndexOfProcessList(p)] = p;
            DataGridViewRow row = (DataGridViewRow)tableBlock.Rows[0].Clone();
            if (p != null)
            {
                row.Cells[0].Value = p.id;
                row.Cells[1].Value = p.maxTime;
                row.Cells[2].Value = p.elapsedTme;
                row.Cells[3].Value = p.operation;
                row.Cells[4].Value = p.blockTime;
                row.Cells[5].Value = p.memory;
                tableBlock.Rows.Add(row);
                //tableBlock.Update();
            }
        }

        private void addToWorkingTable(_Process p) // Listos
        {
            p.state = "LISTO";
            p.calculeTimers(timeAcum);
            paintPages(p, Color.LightGreen, Color.Black);
            execution[getIndexOfProcessList(p)] = p;
            DataGridViewRow row = (DataGridViewRow)tableWorking.Rows[0].Clone();
            row.Cells[0].Value = p.id;
            row.Cells[1].Value = p.maxTime;
            row.Cells[2].Value = p.elapsedTme;
            row.Cells[3].Value = p.operation;
            row.Cells[4].Value = p.memory;
            row.Cells[5].Value = getNeededMarcos(p);
            tableWorking.Rows.Add(row);
            //tableWorking.Update();
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
            //tableBCP.Update();
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
            //tableComplete.Update();
        }

        private void addToSuspTable(_Process p)
        {
            p.state = "SUSPENDIDO";
            p.calculeTimers(timeAcum);
            execution[getIndexOfProcessList(p)] = p;
            DataGridViewRow row = (DataGridViewRow)tableSusp.Rows[0].Clone();
            if (p != null)
            {
                row.Cells[0].Value = p.id;
                row.Cells[1].Value = p.maxTime;
                row.Cells[2].Value = p.elapsedTme;
                row.Cells[3].Value = p.memory;
                row.Cells[4].Value = getNeededMarcos(p);
                tableSusp.Rows.Add(row);
                //tableBlock.Update();
            }
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
                    removePages(Currentprocess); //Remover paginas del proceso de la memoria

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
            else if (e.KeyChar == 'a' || e.KeyChar == 'A')
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
            else if (e.KeyChar == 's' || e.KeyChar == 'S') //Sale a suspendido
            {
                if (tableBlock.Rows.Count > 1)
                {
                    _Process auxProcess = getProcess((int)tableBlock.Rows[0].Cells[0].Value);
                    int index = getIndexOfProcessList(auxProcess);

                    execution[index].suspendido = true;
                    removePages(execution[index]);
                    tableBlock.Rows.RemoveAt(0);

                    addToSuspTable(execution[index]);
                    tableSusp.Update();
                    writeDocument();
                }
            }
            else if (e.KeyChar == 'r' || e.KeyChar == 'R') //Regresa
            {
                if (tableSusp.Rows.Count > 1)
                {
                    _Process auxProcess = getProcess((int)tableSusp.Rows[0].Cells[0].Value);
                    int index = getIndexOfProcessList(auxProcess);

                    execution[index].suspendido = false;
                    tableSusp.Rows.RemoveAt(0);
                    tableSusp.Update();
                    writeDocument();

                    if (isThereMemory(execution[index]))
                    {
                        addPagesToMem(execution[index]);
                        addToWorkingTable(execution[index]);
                    }
                    else
                    {
                        execution[index].added = false;
                        addToNewsTable(execution[index]);
                    }

                    if (textID.Text == "0") //Si el id es 0 significa que hay un proceso null
                    {
                        nullProcessIndicator = false;
                        paintPages(nullProcess, Color.DodgerBlue, Color.White);
                        timerProcesess.Stop(); //Se detiene el timer de procesos para que se detenga el proceso null
                        timerLot.Start(); //Al comenzar el timer de lotes/memoria se reasignara el currentProcess, ya no sera null
                    }
                }
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

                        if (textID.Text == "0") //Si el id es 0 significa que el proceso es null
                        {
                            nullProcessIndicator = false;
                            paintPages(nullProcess, Color.DodgerBlue, Color.White);
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
                removePages(Currentprocess); //Remover paginas del proceso de la memoria
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
                if (tableBlock.Rows.Count > 1 || tableSusp.Rows.Count > 1) //Si todavia hay procesos en bloqueado o suspendido se agregara un proceso nulo
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
            File.WriteAllText("Suspendidos.txt", "");
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


        /*** Paginacion ***/
        private void addPagesToMem(_Process p)
        {
            int mem = p.memory;
            int marcos = mem / tamMarco;
            int barIncrement = 100 / tamMarco;
            ProgressBar auxBar;

            if (mem % tamMarco > 0)
            {
                marcos++;
                for (int i = 0; i < marcos; i++)
                {
                    for (int j = 1; j < layMem.RowCount; j++)
                    {
                        if (layMem.GetControlFromPosition(1, j).Text.Equals("-"))
                        {
                            //Se agrega la pagina con valor 5
                            auxBar = (ProgressBar)layMem.GetControlFromPosition(2, j);
                            layMem.GetControlFromPosition(1, j).Text = p.id.ToString();
                            layMem.GetControlFromPosition(1, j).Update();
                            if (i == marcos-1) //Ultimo elemento
                            {
                                //Se agrega la pagina con valor del residuo
                                auxBar.Value = barIncrement * (mem % tamMarco);
                            }
                            else{
                                auxBar.Value = barIncrement * tamMarco;
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < marcos; i++)
                {
                    for (int j = 1; j < layMem.RowCount; j++)
                    {
                        if (layMem.GetControlFromPosition(1, j).Text.Equals("-"))
                        {
                            //Se agrega la pagina con valor 5
                            auxBar = (ProgressBar)layMem.GetControlFromPosition(2, j);
                            layMem.GetControlFromPosition(1, j).Text = p.id.ToString();
                            layMem.GetControlFromPosition(1, j).Update();
                            auxBar.Value = barIncrement * tamMarco;
                            break;
                        }
                    }
                }
            }

        }

        private int getNeededMarcos(_Process p)
        {
            int mem = p.memory;
            int marcos = mem / tamMarco;

            if (mem % tamMarco > 0)
            {
                marcos++;
            }

            return marcos; 
        }

        private bool isThereMemory(_Process p)
        {
            int marcosCont = 0;
            int marcos = getNeededMarcos(p);

            for (int i = 1; i < layMem.RowCount; i++)
            {
                if (layMem.GetControlFromPosition(1, i).Text.Equals("-"))
                {
                    marcosCont++;
                }
                if (marcosCont == marcos)
                {
                    return true;
                }
            }
            return false;
        }

        private void removePages(_Process p)
        {
            ProgressBar auxBar;
            for (int i = 1; i < layMem.RowCount; i++)
            {
                if (layMem.GetControlFromPosition(1, i).Text.Equals(p.id.ToString()))
                {
                    auxBar = (ProgressBar)layMem.GetControlFromPosition(2, i);

                    layMem.GetControlFromPosition(1, i).Text = "-";
                    layMem.GetControlFromPosition(1, i).BackColor = Color.LightGreen;
                    layMem.GetControlFromPosition(1, i).ForeColor = Color.Black;
                    auxBar.Value = 0;
                }
            }
        }

        private void paintPages(_Process p, Color back, Color fore)
        {
            for (int i = 1; i < layMem.RowCount; i++)
            {
                if (layMem.GetControlFromPosition(1, i).Text.Equals(p.id.ToString()))
                {
                    layMem.GetControlFromPosition(1, i).BackColor = back;
                    layMem.GetControlFromPosition(1, i).ForeColor = fore;
                }
            }
        }

        /*** Escritura de documento ***/
        private void writeDocument()
        {
            string input = "";
            string[] lines = new string[tableSusp.Rows.Count - 1];
            _Process auxProcess;
            _Process p;
            int index;

            File.WriteAllText("Suspendidos.txt", input);
            for (int i = 0; i < tableSusp.Rows.Count - 1; i++)
            {
                auxProcess = getProcess((int)tableSusp.Rows[i].Cells[0].Value);
                index = getIndexOfProcessList(auxProcess);
                p = execution[index];

                input = "ID: " + p.id + " \nEstado: " + p.state + "\nOperacion: " + p.operation + "\nTamaño: " + p.memory
                            + "\nT Max: " + p.maxTime + "\nTranscurrido: " + p.elapsedTme + "\n\n";

                lines[i] = input;
            }
            File.WriteAllLines("Suspendidos.txt", lines);
        }

    }
}
