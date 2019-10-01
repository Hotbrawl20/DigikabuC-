
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace Digikabu
{
    enum tabs
    {
        termin,
        stundenplan,
        schulaufgabenplan,
        entschuldigung,
        fehlzeiten,
        einstellungen
    }
    
    public partial class Form1 : Form
    {
        private string[] stundenArray = new string[10];
        private static readonly HttpClient client = new HttpClient();
        bool loggedin = false;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Button1_Click(object sender, EventArgs e) //Login Button
        {

            if (usernamebox.Text != "" && passwordbox.Text != "")
            {
                var values = new Dictionary<string, string>
            {
            { "UserName", usernamebox.Text },
            { "Password", passwordbox.Text }
            };

                FormUrlEncodedContent content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("https://digikabu.de/Login/Proceed", content);

                var responseString = await response.Content.ReadAsStringAsync();


                if (responseString.Contains("Falscher Benutzername"))
                {
                    MessageBox.Show("Falscher Benutzername und/oder Passwort!", "Fehler!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                }
                else
                {
                    logout.Enabled = true;
                    login.Enabled = false;
                    MessageBox.Show("Erfolgreich Eingeloggt!", "Erfolg!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    comboBox1.Enabled = true;
                    comboBox1.Visible = true;
                    tabControl1.Visible = true;
                    comboBox1.SelectedItem = "Termine"; //termine = 0, rest jada jada
                    usernamebox.Enabled = false;
                    passwordbox.Enabled = false;
                    loggedin = true;
                    TerminFenster();
                }


            }
            else
            {
                MessageBox.Show("Bitte geben Sie Benutzername und Passwort an!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void Button2_Click(object sender, EventArgs e) //Logout Button
        { //toDo relogin falls ausgelogt
            var values = new Dictionary<string, string>
            {

            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://digikabu.de/Logout", content);

            var responseString = await response.Content.ReadAsStringAsync();



            if (responseString.Contains("abgemeldet"))
            {

                MessageBox.Show("Erfolgreich Abgemeldet!", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                logout.Enabled = false;
                login.Enabled = true;
                un.Visible = false;
                schuelertxt.Visible = false;
                kl.Visible = false;
                klassetxt.Visible = false;
                comboBox1.Enabled = false;
                tabControl1.Visible = false;
                comboBox1.Visible = false;
                usernamebox.Enabled = true;
                passwordbox.Enabled = true;
                loggedin = false;
            }
            else
            {
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }


        }
        static string fix(string toFix)
        {
            string ret = string.Empty;
            if (toFix.Contains("&#x"))
            {
                ret = toFix;
                ret = ret.Replace("&#xFC;", "ü");
                ret = ret.Replace("&#xDF;", "ß");
                ret = ret.Replace("&#xF6;", "ö");
                ret = ret.Replace("&#xE4;", "ä");
            }
            else
            {
                ret = toFix;
            }
            return ret;
        }
        private async void gettermine()
        {
            var values = new Dictionary<string, string>
            {

            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://digikabu.de/Main", content);

            var responseString = await response.Content.ReadAsStringAsync();
            string[] info = new string[2];
            bool nextIsIgnore = false, nextIsMessage = false;
            foreach (string s in responseString.Split('<'))
            {

                if (s.Contains("td"))
                {
                    var trim = s.Trim();
                    if (trim.Contains("white-space"))
                    {
                        var x = trim.Split('>');
                        info[0] = fix(x[1]) + ": ";
                        nextIsIgnore = true;
                    }
                    else if (nextIsIgnore)
                    {
                        nextIsMessage = true;
                        nextIsIgnore = false;
                    }
                    else if (nextIsMessage)
                    {
                        var x = trim.Split('>');
                        nextIsMessage = false;
                        info[1] = fix(x[1]);
                        listBox1.Items.Add(info[0] + info[1]);
                        listBox1.Items.Add("");
                    }
                }
            }
        }
        private async void TerminFenster()
        {
            var values = new Dictionary<string, string>
            {

            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://digikabu.de/Main", content);

            var responseString = await response.Content.ReadAsStringAsync();
            if (!responseString.Contains("<input class=\"form - control\" type=\"password\" id=\"Password\" name=\"Password\" />"))
            {


                foreach (string s in responseString.Split('>'))
                {
                    //Content = Text
                    //Visibility = Visible
                    //Visibility.Visible = true
                    if (s.Contains(")</span"))
                    {
                        string[] split = s.Split(' ');
                        un.Text = fix(split[0]) + " " + fix(split[1]);
                        string klasse = split[2].Trim(new char[] { '(', ')' });
                        string[] klassesplit = klasse.Split(')');
                        kl.Text = klassesplit[0];
                        schuelertxt.Visible = true;
                        un.Visible = true;
                        klassetxt.Visible = true;
                        kl.Visible = true;
                    }

                }
                InsertStunden(null, DateTime.Now, true);
                


            }
            else
            {
                relog();
            }
        }
        private async void getSchulaufgaben(ListBox lt)
        {
            string datesave = string.Empty;

            int counter = 0;
            var values = new Dictionary<string, string>
            {

            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://digikabu.de/SchulaufgabenPlan", content);

            var responseString = await response.Content.ReadAsStringAsync();
            List<string> termine = new List<string>();

            string[] split = responseString.Split('>');

            foreach (string s in split)
            {

                switch (counter)
                {
                    case 6:

                        datesave = s.Replace(".</strong", "");

                        break;
                    case 1:
                        if (s != "</td")
                        {
                            termine.Add(datesave + ": " + s.Replace("</td", ""));
                        }
                        break;
                    default:
                        break;
                }
                counter--;
                if (s.Contains("tr class=\"\""))
                {
                    counter = 8;
                }
            }
            foreach(string s in termine)
            {
                lt.Items.Add(s);
            }
        }
        private async void relog()
        {
            if (usernamebox.Text != "" && passwordbox.Text != "")
            {
                var values = new Dictionary<string, string>
            {
            { "UserName", usernamebox.Text },
            { "Password", passwordbox.Text }
            };

                FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                HttpResponseMessage response = null;


                response = await client.PostAsync("https://digikabu.de/Login/Proceed", content);
                var responseString = await response.Content.ReadAsStringAsync();



                var values2 = new Dictionary<string, string>
                {

                };
                content = new FormUrlEncodedContent(values2);
               
                responseString = await response.Content.ReadAsStringAsync();
            }
        }
        private async void FehlzeitenFenster()
        {
            //nach folgenden string wird für ganztags gesucht
            //style = "color:blue;font-weight:bold"
            var response = await client.GetAsync("https://digikabu.de/Fehlzeiten");
            var responsestring = await response.Content.ReadAsStringAsync();
            int z = 0; //0 für ganztag, 1 für stundenweise
            string[] stunden = new string[2];
            foreach (string s in responsestring.Split(' '))
            {
                if (s.Contains("style=\"color:blue;font-weight:bold\">"))
                {
                    stunden[z] = s;
                    z++;
                }
            }
            string[] ganztags1 = stunden[0].Split('>');//nummer = 2, {nummer}</span

            string[] ganztags = ganztags1[1].Split('<');
            ganz.Text = ganztags[0];
            string[] sweise1 = stunden[1].Split('>');//nummer = 2, {nummer}</span
            string[] sweise = sweise1[1].Split('<');
            stundenw.Text = sweise[0];
        }
        private async void Einstellung()
        {
            var response = await client.GetAsync("https://digikabu.de/Einstellungen");
            var responsestring = await response.Content.ReadAsStringAsync();
            foreach (string s in responsestring.Split('>'))
            {
                if (s.Contains("email1"))
                {
                    string[] split = s.Split('=');
                    split[4] = split[4].Replace("\"", "");
                    split[4] = split[4].Replace("/", "");
                    split[4] = split[4].Replace(" ", "");
                    if (split[4] == "")
                    {
                        email1.Text = "";
                    }
                    else
                    {
                        email1.Text = split[4];
                    }
                }
                if (s.Contains("email2"))
                {
                    string[] split = s.Split('=');
                    split[4] = split[4].Replace("\"", "");
                    split[4] = split[4].Replace("/", "");
                    split[4] = split[4].Replace(" ", "");
                    if (split[4] == "")
                    {
                        email2.Text = "";
                    }
                    else
                    {
                        email2.Text = split[4];
                    }

                }
            }

        }
        private void StundenplanFenster()
        {
            //variable datum_montag
            DateTime datum_montag = StartingDateOfWeek(DateTime.Now), datum_freitag = datum_montag.AddDays(4);

            //TODO: Montag bis Freitag datum montagdatum, freitagdatum labels
            montagdatum.Text = datum_montag.ToString("dd.MM.yyyy");
            freitagdatum.Text = datum_freitag.ToString("dd.MM.yyyy");

            //TODO: Fachausgabe a tag
            #region Fächer
            #region Montag
            InsertStunden(montaglist, datum_montag,false);
            #endregion
            #region Dienstag
            InsertStunden(dienstaglist, datum_montag.AddDays(1), false);
            #endregion
            #region Mittwoch
            InsertStunden(mittwochlist, datum_montag.AddDays(2), false);
            #endregion
            #region Donnerstag
            InsertStunden(donnerstaglist, datum_montag.AddDays(3), false);
            #endregion
            #region Freitag
            InsertStunden(freitaglist, datum_montag.AddDays(4), false);
            #endregion
            #endregion
        }
        private string[] returnThisShit(string[] array)
        {
            return array;
        }
        
        private async void InsertStunden(ListBox listbox, DateTime date, bool array)
        {
            var response = await client.GetAsync("https://digikabu.de/Main?date=" + date.ToString("yyyy-MM-dd"));
            var responsestring = await response.Content.ReadAsStringAsync();
            string[] stunden = new string[10];
            int fach = 0;
            int fach2 = 0;
            string fachsave = string.Empty;
            foreach (string s in responsestring.Split('<'))
            {

                if (s.Contains("svg x="))
                {
                    string[] split = s.Split(' ');
                    string[] fach1 = split[2].Split('\'');
                    string[] fachx = split[4].Split('\'');

                    fach2 = Convert.ToInt32(fachx[1]) / 60;
                    fach = Convert.ToInt32(fach1[1]) / 60;
                }

                if (s.Contains("text-anchor='middle'"))
                {
                    string[] split = s.Split('>');
                    fachsave = split[1];
                    if (fachsave == "RK" || fachsave == "RV")
                    {
                        fachsave = "Religion";
                    }
                    if (fach2 < 2)
                    {
                            stunden[fach] = fachsave;
                        
                    }
                    else
                    {
                        if (fach2 == 2)
                        {
                            switch (fach)
                            {
                                case 0:
                                    stunden[0] = fachsave;
                                    stunden[1] = fachsave;
                                    break;
                                case 1:
                                    stunden[1] = fachsave;
                                    stunden[2] = fachsave;
                                    break;
                                case 2:
                                    stunden[2] = fachsave;
                                    stunden[3] = fachsave;
                                    break;
                                case 3:
                                    stunden[3] = fachsave;
                                    stunden[4] = fachsave;
                                    break;
                                case 4:
                                    stunden[4] = fachsave;
                                    stunden[5] = fachsave;
                                    break;
                                case 5:
                                    stunden[5] = fachsave;
                                    stunden[6] = fachsave;
                                    break;
                                case 6:
                                    stunden[6] = fachsave;
                                    stunden[7] = fachsave;
                                    break;
                                case 7:
                                    stunden[7] = fachsave;
                                    stunden[8] = fachsave;
                                    break;
                                case 8:
                                    stunden[8] = fachsave;
                                    stunden[9] = fachsave;
                                    break;
                                case 9:
                                    stunden[9] = fachsave;
                                    break;
                            }
                        }
                        else
                        {
                            switch (fach)
                            {
                                case 0:
                                    stunden[0] = fachsave;
                                    stunden[1] = fachsave;
                                    stunden[2] = fachsave;
                                    break;
                                case 1:
                                    stunden[1] = fachsave;
                                    stunden[2] = fachsave;
                                    stunden[3] = fachsave;
                                    break;
                                case 2:
                                    stunden[2] = fachsave;
                                    stunden[3] = fachsave;
                                    stunden[4] = fachsave;
                                    break;
                                case 3:
                                    stunden[3] = fachsave;
                                    stunden[4] = fachsave;
                                    stunden[5] = fachsave;
                                    break;
                                case 4:
                                    stunden[4] = fachsave;
                                    stunden[5] = fachsave;
                                    stunden[6] = fachsave;
                                    break;
                                case 5:
                                    stunden[5] = fachsave;
                                    stunden[6] = fachsave;
                                    stunden[7] = fachsave;
                                    break;
                                case 6:
                                    stunden[6] = fachsave;
                                    stunden[7] = fachsave;
                                    stunden[8] = fachsave;
                                    break;
                                case 7:
                                    stunden[7] = fachsave;
                                    stunden[8] = fachsave;
                                    stunden[9] = fachsave;
                                    break;
                                case 8:
                                    stunden[8] = fachsave;
                                    stunden[9] = fachsave;
                                    break;
                                case 9:
                                    stunden[9] = fachsave;
                                    break;
                            }
                        }

                    }
                }

            }
            if (array)
            {
                Fach1.Text = stunden[0];
                Fach2.Text = stunden[1];
                Fach3.Text = stunden[2];
                Fach4.Text = stunden[3];
                Fach5.Text = stunden[4];
                Fach6.Text = stunden[5];
                Fach7.Text = stunden[6];
                Fach8.Text = stunden[7];
                Fach9.Text = stunden[8];
                Fach10.Text = stunden[9];
            }
            else
            {
                foreach (string s in stunden)
                {
                    string edit_s = s;
                    if (edit_s == null)
                    {
                        edit_s = "";
                    }
                    listbox.Items.Add(edit_s);
                }
            }
            
        }
        private DateTime StartingDateOfWeek(DateTime date)
        {
            DateTime usedDate;
            int dateAdjustment = 0;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dateAdjustment = 1;
                    break;
                case DayOfWeek.Monday:
                    dateAdjustment = 0;
                    break;
                case DayOfWeek.Tuesday:
                    dateAdjustment = -1;
                    break;
                case DayOfWeek.Wednesday:
                    dateAdjustment = -2;
                    break;
                case DayOfWeek.Thursday:
                    dateAdjustment = -3;
                    break;
                case DayOfWeek.Friday:
                    dateAdjustment = -4;
                    break;
                case DayOfWeek.Saturday:
                    dateAdjustment = 2;
                    break;
                default:
                    break;
            }
            usedDate = date.AddDays(Convert.ToDouble(dateAdjustment));
            return usedDate;
        }
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            relog();
            if (comboBox1.SelectedIndex == 0)
            {
                tabControl1.SelectedIndex = 0;
                TerminFenster();
                listBox1.Items.Clear();
                gettermine();
            }
            if (comboBox1.SelectedIndex == 1)
            {
                tabControl1.SelectedIndex = 1;
                montaglist.Items.Clear();
                dienstaglist.Items.Clear();
                mittwochlist.Items.Clear();
                donnerstaglist.Items.Clear();
                freitaglist.Items.Clear();
                StundenplanFenster();
            }
            if (comboBox1.SelectedIndex == 2)
            {
                tabControl1.SelectedIndex = 2;
                listBox2.Items.Clear();
                getSchulaufgaben(listBox2);
            }
            if (comboBox1.SelectedIndex == 3)
            {
                tabControl1.SelectedIndex = 3;
                
            }
            if (comboBox1.SelectedIndex == 4)
            {
                tabControl1.SelectedIndex = 4;
                FehlzeitenFenster();
            }
            if (comboBox1.SelectedIndex == 5)
            {
                tabControl1.SelectedIndex = 5;
                Einstellung();
            }
        }
        

        //Diese Methode wird ausgeführt, wenn eine neue Stunde anfängt
        private void Stunde_Tick(object sender, EventArgs e)
        {
            string uStart = "20:42", ausgabe = string.Empty; // Unterrichtsstart, Ausgabe
            int stdDauer = 45, pDauer = 15, pPos = 2, stdAnz = 10; // Stundendauer, Pausendauer, Pausenposition(nach 2. Std), Maximale Stundenanz (für uns 10)

            DateTime jetzt = /*Convert.ToDateTime("8:30")*/DateTime.Now;
            List<DateTime> uhrZeiten = new List<DateTime>();

            uhrZeiten.Add(Convert.ToDateTime(uStart));
            for (int i = 0; i <= stdAnz; i++)
            {
                if (i == pPos)
                {
                    uhrZeiten.Add(uhrZeiten[i].AddMinutes(pDauer));
                }
                else
                {
                    uhrZeiten.Add(uhrZeiten[i].AddMinutes(stdDauer));
                }
            }
            for (int i = 0; i < uhrZeiten.Count - 1; i++)
            {
                if (jetzt >= uhrZeiten[i] && jetzt < uhrZeiten[i + 1])
                {
                    if (i < pPos)
                    {
                        ausgabe = $"{i + 1}";
                    }
                    else if (i == pPos)
                    {
                        ausgabe = "Pause";
                    }
                    else if (i > pPos)
                    {
                        ausgabe = $"{i}";
                    }// ToDo wenn man nur 9 Stunden hat steht wahrscheinlich in der 10. Std immer noch '10.' Std statt 'Frei' da

                }
            }

            //hier label highliten
            ResetHighlight();
            switch (ausgabe)
            {
                case "1":
                    if(Fach1.Text != string.Empty)
                    {
                        panel1.BackColor = Color.LightGray;
                        Stunde1.BackColor = Color.LightGray;
                        Fach1.BackColor = Color.LightGray;
                    }
                    break;
                case "2":
                    if (Fach2.Text != string.Empty)
                    {
                        panel2.BackColor = Color.LightGray;
                        Stunde2.BackColor = Color.LightGray;
                        Fach2.BackColor = Color.LightGray;
                    }
                    break;
                case "3":
                    if (Fach3.Text != string.Empty)
                    {
                        panel3.BackColor = Color.LightGray;
                        Stunde3.BackColor = Color.LightGray;
                        Fach3.BackColor = Color.LightGray;
                    }
                    break;
                case "4":
                    if (Fach4.Text != string.Empty)
                    {
                        panel4.BackColor = Color.LightGray;
                        Stunde4.BackColor = Color.LightGray;
                        Fach4.BackColor = Color.LightGray;
                    }
                    break;
                case "5":
                    if (Fach5.Text != string.Empty)
                    {
                        panel5.BackColor = Color.LightGray;
                        Stunde5.BackColor = Color.LightGray;
                        Fach5.BackColor = Color.LightGray;
                    }
                    break;
                case "6":
                    if (Fach6.Text != string.Empty)
                    {
                        panel6.BackColor = Color.LightGray;
                        Stunde6.BackColor = Color.LightGray;
                        Fach6.BackColor = Color.LightGray;
                    }
                    break;
                case "7":
                    if (Fach7.Text != string.Empty)
                    {
                        panel7.BackColor = Color.LightGray;
                        Stunde7.BackColor = Color.LightGray;
                        Fach7.BackColor = Color.LightGray;
                    }
                    break;
                case "8":
                    if (Fach8.Text != string.Empty)
                    {
                        panel8.BackColor = Color.LightGray;
                        Stunde8.BackColor = Color.LightGray;
                        Fach8.BackColor = Color.LightGray;
                    }
                    break;
                case "9":
                    if (Fach9.Text != string.Empty)
                    {
                        panel9.BackColor = Color.LightGray;
                        Stunde9.BackColor = Color.LightGray;
                        Fach9.BackColor = Color.LightGray;
                    }
                    break;
                case "10":
                    if (Fach10.Text != string.Empty)
                    {
                        panel10.BackColor = Color.LightGray;
                        Stunde10.BackColor = Color.LightGray;
                        Fach10.BackColor = Color.LightGray;
                    }
                    break;
                case "Pause":
                    Pause.BackColor = Color.LightGray;
                    break;
            }
        }
        private void ResetHighlight()
        {
            panel1.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel2.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel3.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel4.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel5.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel6.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel7.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel8.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel9.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel10.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde1.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde2.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde3.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde4.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde5.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde6.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde7.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde8.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde9.BackColor = Color.FromKnownColor(KnownColor.Control);
            Stunde10.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach1.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach2.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach3.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach4.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach5.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach6.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach7.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach8.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach9.BackColor = Color.FromKnownColor(KnownColor.Control);
            Fach10.BackColor = Color.FromKnownColor(KnownColor.Control);
            Pause.BackColor = Color.FromKnownColor(KnownColor.Control);
        }

        private void Credits_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Nico Haberkorn\nJulian Bergmann", "Credits", MessageBoxButtons.OK);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
        }
    }
    

}

