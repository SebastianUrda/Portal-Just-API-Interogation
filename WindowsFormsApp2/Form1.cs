using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {

        PortalWS.Query service= new PortalWS.Query();
        public Form1()
        {
            InitializeComponent();
            comboBox1.DataSource = Enum.GetValues(typeof(PortalWS.CategorieCaz));
        }


        private void generateButton_Click(object sender, EventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<PortalWS.Dosar> rezultateFinale = new List<PortalWS.Dosar>();
            if (dateTimePicker1.Value > dateTimePicker2.Value)
            {
                MessageBox.Show("A doua data trebuie sa fie mai mica decta prima!");
            }
            else
            {
                string fileName = "Dosare " + comboBox1.Text +" "+ dateTimePicker1.Value.ToShortDateString().Replace("/", ".") + "-" + dateTimePicker2.Value.ToShortDateString().Replace("/", ".") + ".csv";
                richTextBox1.AppendText("A inceput cautarea\n");
                if (!CreateHeader(fileName))
                {
                    MessageBox.Show("A aparut o eroare in creerea fisierului!");
                }
                Task.Factory.StartNew(() => appendDosarToFile(watch, rezultateFinale, fileName));
            }

        }

        private void appendDosarToFile(System.Diagnostics.Stopwatch watch, List<PortalWS.Dosar> rezultateFinale, string fileName)
        {
            int counter = 0;
            foreach (PortalWS.Institutie institutie in (PortalWS.Institutie[])Enum.GetValues(typeof(PortalWS.Institutie)))
            {
                PortalWS.Dosar[] ret = new PortalWS.Dosar[0];
                try
                {
                    ret = service.CautareDosare(null, null, null, institutie, Convert.ToDateTime(dateTimePicker1.Value.ToShortDateString()), Convert.ToDateTime(dateTimePicker2.Value.ToShortDateString()));
                }
                catch (Exception exception)
                {
                    String errorMessage = "Eroare in cautarea institutiei: " + institutie.ToString(); 
                    richTextBox1.Invoke(new Action<string>(AppendText), errorMessage + "\n");
                }

                if (ret == null)
                {

                }
                else
                {
                    string selectedcategroieCaz = "Toate";
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        selectedcategroieCaz = comboBox1.Text;
                    }));
                    foreach (PortalWS.Dosar dosar in ret)
                    {
                        if (selectedcategroieCaz.Equals("Toate"))
                        {
                            rezultateFinale.Add(dosar);
                            if (!AddRecord(dosar, fileName))
                            {
                                richTextBox1.Invoke(new Action<string>(AppendText), "Eroare in adaugarea dosarului " + dosar.numar);
                            }
                        }
                        else
                        {
                            if (selectedcategroieCaz.Equals(dosar.categorieCaz.ToString()))
                            {
                                rezultateFinale.Add(dosar);
                                if (!AddRecord(dosar, fileName))
                                {
                                    richTextBox1.Invoke(new Action<string>(AppendText), "Eroare in adaugarea dosarului " + dosar.numar);
                                }
                            }
                        }
                    }
                }

                if (counter != rezultateFinale.Count)
                {
                    int rez = rezultateFinale.Count - counter;
                    richTextBox1.Invoke(new Action<string>(AppendText), "S-au gasit in: " + institutie + " " + rez + " rezultate\n");
                    counter = rezultateFinale.Count;
                }

            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            MessageBox.Show("Fisierul a fost create cu success!\n Contine " + rezultateFinale.Count + " rezultate\nA durat " + elapsedMs / 1000 + " secunde.");
        }

        private void AppendText(string obj)
        {
            richTextBox1.AppendText(obj);
        }

        private Boolean CreateHeader(string filePath)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filePath, true))
            {
                try
                {
                    string header = "Instanta,";
                    header += "Numar,";
                    header += "Date,";
                    header += "Materie,";
                    header += "Obiect,";
                    header += "Debitor,";
                    header += "Creditor,";
                    header += "Alte parti,";
                    file.WriteLine(header);
                }
                catch (Exception ex)
                {
                    Console.Error.Write(ex);
                    return false;
                }
               
            }
            return true;
        }

        private Boolean AddRecord(PortalWS.Dosar dosar, string filePath)
        {
            try
            {
                string toWrite = dosar.institutie.ToString()+",";
                if (dosar.numar != null)
                {
                    toWrite += dosar.numar + ",";
                }
                else
                {
                    toWrite += ",";
                }
                if (dosar.data != null)
                {
                    toWrite += dosar.data.ToShortDateString() + ",";      
                }
                else
                {
                    toWrite += ",";
                }
                if (dosar.categorieCazNume != null)
                {
                    toWrite += dosar.categorieCazNume.Replace(",", " ") + ",";
                }
                else
                {
                    toWrite += ",";
                }
                if (dosar.obiect != null)
                {
                    toWrite += dosar.obiect.Replace(",", " ") + ",";
                }
                else
                {
                    toWrite += ",";
                }
                if (dosar.parti!=null)
                {
                    List<PortalWS.DosarParte> debitori = new List<PortalWS.DosarParte>();
                    List<PortalWS.DosarParte> creditori = new List<PortalWS.DosarParte>();
                    List<PortalWS.DosarParte> alteParti = new List<PortalWS.DosarParte>();
                    foreach (PortalWS.DosarParte parte in dosar.parti)
                    {
                        
                        if ("Debitor".Equals(parte.calitateParte))
                        {
                            debitori.Add(parte);
                        }
                        if ("Creditor".Equals(parte.calitateParte))
                        {
                            creditori.Add(parte);
                        }
                        else
                        {
                            alteParti.Add(parte);
                        }
                        
                    }
                    string toWriteDebitori = "";
                    foreach(PortalWS.DosarParte parte in debitori)
                    {
                        toWriteDebitori += parte.nume.Replace(",", " ") + ";";
                    }
                    string toWriteCreditori = "";
                    foreach (PortalWS.DosarParte parte in creditori)
                    {
                        toWriteCreditori += parte.nume.Replace(",", " ") + ";";
                    }
                    string toWriteAletParti = "";
                    foreach (PortalWS.DosarParte parte in alteParti)
                    {
                        toWriteAletParti += parte.nume.Replace(",", " ") + ";";
                    }
                    toWrite += toWriteDebitori + "," + toWriteCreditori + "," + toWriteAletParti;
                }
                using (System.IO.StreamWriter file=new System.IO.StreamWriter(@filePath, true,Encoding.Default))
                {
                    file.WriteLine(toWrite);
                }
            }
            catch(Exception ex)
            {
                Console.Error.Write(ex);
                return false;
            }
            return true;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }
    }
}
